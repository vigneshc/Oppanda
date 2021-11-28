using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace OppandaCoreLib.TwitterIntegration
{
    public class TwitterValidator : ITwitterValidator
    {
        const string Approve = "Approve";
        const string DisApprove = "DisApprove";

        private readonly TwitterConfig config;
        public TwitterValidator(TwitterConfig config){
            this.config = config;
        }

        public async Task<ProposalValidationRecord> GetProposalValidationRecordAsync(Proposal proposal, ulong? minTweetId)
        {
            string proposalId = proposal.Id;
            var validatorsHandle = proposal.ValidatorHandles;

            bool IsValidTweet(RawTweet tweet){
                return tweet.entities != null &&
                tweet.entities.hashtags.Any(hashtag => hashtag.text.Equals(proposalId)) && // has proposalId
                tweet.entities.hashtags.Any(
                    hashtag => hashtag.text.Equals(Approve, StringComparison.InvariantCulture) || hashtag.text.Equals(DisApprove, StringComparison.InvariantCulture)); // has either approve or disapprove
            }

            bool IsApproved(RawTweet approvalTweet){
                var approvedHashtag = approvalTweet.entities.hashtags
                        .Select(hashtag => hashtag.text)
                        .Where(hashtag => hashtag.Equals(Approve, StringComparison.InvariantCulture) || hashtag.Equals(DisApprove, StringComparison.InvariantCulture))
                        .OrderByDescending(h => h)
                        .FirstOrDefault();
                return approvedHashtag != null && approvedHashtag.Equals(Approve, StringComparison.InvariantCultureIgnoreCase);
            }
            
            var result = new ProposalValidationRecord(){
                ProposalId = proposalId,
                LastUpdated = System.DateTime.MinValue
            };
            
            // collect validation record for each handle.
            List<ValidatorRecord> validatorRecords = new List<ValidatorRecord>();
            foreach(var validatorHandle in validatorsHandle.Distinct()){
                RawTweet approvalTweet = JsonConvert.DeserializeObject<RawTweet[]>((await this.GetTweetsResponseAsync(validatorHandle, minTweetId)))
                .Where(tweet => IsValidTweet(tweet))
                .OrderByDescending(tweet => tweet.id)
                .FirstOrDefault();
                
                if(approvalTweet != null){
                    ValidatorRecord validatorRecord = new ValidatorRecord(){
                        ValidatorHandle = validatorHandle,
                        ValidationRecordId = approvalTweet.id.ToString(),
                        ApprovalDate = new DateTime(approvalTweet.GetTimestamp()),
                        Approved = IsApproved(approvalTweet)
                    };
                    validatorRecords.Add(validatorRecord);
                }
            }

            result.ValidationRecords = validatorRecords.ToArray();
            return result;
        }
        

        // GET https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name=twitterapi&count=2
        // https://developer.twitter.com/en/docs/twitter-api/v1/tweets/timelines/api-reference/get-statuses-user_timeline
        private async Task<string> GetTweetsResponseAsync(string handle, ulong? sinceId){
            const string oauth_version = "1.0";
            const string oauth_signature_method = "HMAC-SHA1";
            const string UserTimelineUrl = "https://api.twitter.com/1.1/statuses/user_timeline.json";

            if(!sinceId.HasValue){
                sinceId = 1000;
            }

            // unique request details
            var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var oauth_timestamp = Convert.ToInt64(
                (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
                    .TotalSeconds).ToString();

            // create signature. 
            // https://developer.twitter.com/en/docs/authentication/oauth-1-0a
            var baseString = 
                $"oauth_consumer_key={config.OAuthConsumerKey}&oauth_nonce={oauth_nonce}&oauth_signature_method={oauth_signature_method}&" +
                $"oauth_timestamp={oauth_timestamp}&oauth_token={config.OAuthToken}&oauth_version={oauth_version}&screen_name={handle}&since_id={sinceId.Value}";

            baseString = string.Concat("GET&", Uri.EscapeDataString(UserTimelineUrl), "&", Uri.EscapeDataString(baseString));
            var compositeKey = string.Concat(Uri.EscapeDataString(config.OAuthConsumerSecret), "&", Uri.EscapeDataString(config.OAuthTokenSecret));
            
            string oauth_signature;
            using (var hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
            {
                oauth_signature = Convert.ToBase64String(
                hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }

            // create the request header
            var authHeader =
                $"OAuth oauth_nonce=\"{Uri.EscapeDataString(oauth_nonce)}\", oauth_signature_method=\"{Uri.EscapeDataString(oauth_signature_method)}\", " +
                $"oauth_timestamp=\"{Uri.EscapeDataString(oauth_timestamp)}\", oauth_consumer_key=\"{Uri.EscapeDataString(config.OAuthConsumerKey)}\", " +
                $"oauth_token=\"{Uri.EscapeDataString(config.OAuthToken)}\", oauth_signature=\"{Uri.EscapeDataString(oauth_signature)}\", " +
                $"oauth_version=\"{Uri.EscapeDataString(oauth_version)}\"";

            // send request
            string resource_url = $"{UserTimelineUrl}?screen_name={handle}&since_id={sinceId.Value}" ;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource_url);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.PreAuthenticate = true;

            var tresponse = await request.GetResponseAsync();
            using(var responseStream = tresponse.GetResponseStream())
            {
                using(var sr = new StreamReader(responseStream))
                {
                    var response = await sr.ReadToEndAsync();
                    return response;
                }
            }
        }
    }

    public class TwitterConfig{
        public string OAuthToken { get; set; }
        public string OAuthTokenSecret { get; set; }
        public string OAuthConsumerKey { get; set; }
        public string OAuthConsumerSecret { get; set; }
    }
}

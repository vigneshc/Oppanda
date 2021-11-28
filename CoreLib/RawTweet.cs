using System;

namespace OppandaCoreLib.TwitterIntegration
{
    internal class RawTweet {
        static readonly long Epoch = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        private long timestamp = -1;

        public long id { get; set; }
        
        public string text { get; set; }

        public ExtendedTweet extended_tweet { get; set;}
        public TweetEntities entities { get; set; }
        public string timestamp_ms { get; set; }

        public RawTweet retweeted_status { get; set; }

        public User user { get; set; }

        public long GetTimestamp () {
            if(timestamp == -1)
            {
                if(string.IsNullOrEmpty(timestamp_ms))
                {
                    this.timestamp = 0;
                }
                else
                {
                    this.timestamp = Epoch + long.Parse (timestamp_ms) * TimeSpan.TicksPerMillisecond;
                }
            }

            return this.timestamp;
        }

        public class TweetEntities {
            public HashTag[] hashtags { get; set; }

            public class HashTag {
                public string text { get; set; }
            }
        }

        public class User{
            public string screen_name { get; set;}
        }

        public class ExtendedTweet {
            public string full_text { get; set; }
        }
    }
}
using System;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;

namespace OppandaCoreLib.IPFS
{
    public class Web3Client
    {
        static Uri Web3BaseUrl = new Uri("https://api.web3.storage");
        static Uri  UploadFileRelativeUrl = new Uri("/upload", UriKind.Relative);
        static Uri UploadUrl = new Uri(Web3BaseUrl, UploadFileRelativeUrl);
        private readonly string bearerToken;

        public Web3Client(string bearerToken){
            this.bearerToken = $"bearer {bearerToken}";
        }

        public async Task<string> UploadContentAsync(Stream streamToUpload){
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UploadUrl);
            request.Headers.Add("Authorization", this.bearerToken) ;
            request.Method = "POST";
            try{
                using(var requestStream = await request.GetRequestStreamAsync()){
                    await streamToUpload.CopyToAsync(requestStream);
                }

                var response = await request.GetResponseAsync();
                using(var responseStream = response.GetResponseStream())
                using(var sr = new StreamReader(responseStream))
                {
                    var responseString = await sr.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<UploadFileResponse>(responseString).cid;
                }
            }
            catch(WebException e){
                string errorResponse = string.Empty;
                using(var errorStream = e.Response.GetResponseStream())
                {
                    using(var sr = new StreamReader(errorStream))
                    {
                        errorResponse = sr.ReadToEnd();
                    }
                }

                throw new OppandaException(
                    "error while send request to web3.storage", 
                    new OppandaException($"Error Response: {errorResponse}"));
            }
        }
        public async Task<string> UploadContentAsync(string stringToUpload){
            using(var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms)){
                sw.Write(stringToUpload);
                await sw.FlushAsync();
                ms.Seek(0, SeekOrigin.Begin);
                return await this.UploadContentAsync(ms);
            }
        }
    }

    class UploadFileResponse{
        public string cid { get; set; }
    }
}

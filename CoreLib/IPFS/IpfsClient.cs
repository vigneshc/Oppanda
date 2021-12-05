using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace OppandaCoreLib.IPFS
{
    public class IpfsClient
    {
        public static readonly Uri IPFSIOGateway = new Uri("https://ipfs.io/ipfs/");
        public async Task<Stream> GetContentStreamAsync(string cid){
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(IPFSIOGateway, new Uri(cid, UriKind.Relative)));
            request.Method = "GET";
            try{
                var response = await request.GetResponseAsync();
                return response.GetResponseStream();
            }
            catch(WebException e){
                throw new OppandaException("error while send request to web3.storage", e);
            }
        }

        public async Task<string> GetContentStringAsync(string cid){
            try{
                using(var responseStream = await this.GetContentStreamAsync(cid))
                using(var sr = new StreamReader(responseStream))
                {
                    return await sr.ReadToEndAsync();
                }
            }
            catch(OppandaException){
                return null;
            }
        }
    }
}
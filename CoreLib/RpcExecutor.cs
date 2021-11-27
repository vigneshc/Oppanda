using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace OppandaCoreLib
{
    // executes rpc calls. input is json. 
    public class RpcExecutor{
        const string MethodName = "method";
        const string Payload = "payload";

        private readonly ProposalManager proposalManager;
        public RpcExecutor(ProposalManager proposalManager){
            this.proposalManager = proposalManager;
        }

        // Executes rpcs and returns a response
        public async Task<(HttpStatusCode,string)> ExecuteAsync(string payload){
            JObject request = null;
            try{
                request = JsonConvert.DeserializeObject<JObject>(payload);
                switch(request[MethodName].Value<string>().ToLowerInvariant()){
                    case "createproposal":
                        var proposal = request[Payload].Value<JObject>().ToObject<Proposal>();
                        var createdTime = await this.proposalManager.CreateProposalAsync(proposal);
                        return new Response<object>(){
                            Payload =  new {
                                CreatedDate = createdTime
                            }
                        }.SetStatusAndGetResponse(HttpStatusCode.OK);

                    case "isapproved":
                        var requestDetails = request[Payload].Value<JObject>().ToObject<Dictionary<string, string>>();
                        requestDetails.TryGetValue("ProposalId", out string proposalId);
                        bool isApproved = await this.proposalManager.IsApprovedAsync(proposalId);
                        return new Response<object>(){
                            Payload =  new {
                                IsApproved = isApproved
                            }
                        }.SetStatusAndGetResponse(HttpStatusCode.OK);
                    default:
                            return new Response<ErrorResponse>(){
                            Payload =  new ErrorResponse(){
                            ErrorMessage = "Unknown Method"
                            }
                        }.SetStatusAndGetResponse(HttpStatusCode.BadRequest);
                }
            }
            catch(JsonException e){
                return new Response<ErrorResponse>(){
                    Payload =  new ErrorResponse(){
                    ErrorMessage = "Request format invalid",
                    Details = e.ToString()
                    }
                }.SetStatusAndGetResponse(HttpStatusCode.BadRequest);
            }
            catch(OppandaException e){
                return new Response<ErrorResponse>(){
                    Payload =  new ErrorResponse(){
                    ErrorMessage = "Error while executing request.",
                    Details = e.ToString()
                    }
                }.SetStatusAndGetResponse(HttpStatusCode.BadRequest);
            }
        }

        class Response<T>{
            public string HttpStatusCode { get; set; }
            public T Payload { get;set;}
            public string Serialize() => JsonConvert.SerializeObject(this);
            public (HttpStatusCode,string) SetStatusAndGetResponse(HttpStatusCode statusCode){
                this.HttpStatusCode = statusCode.ToString();
                return (statusCode, this.Serialize());
            }
        }

        class ErrorResponse{
            public string ErrorMessage { get; set; }
            public string Details { get; set; }
        }
    }
}
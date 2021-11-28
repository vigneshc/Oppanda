using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading;

namespace OppandaCoreLib
{
    // executes rpc calls. input is json. 
    public class RpcExecutor{
        const string MethodName = "method";
        const string Payload = "payload";

        private readonly ProposalManager proposalManager;
        private int maxRequestsPerMinute;
        private Stopwatch timer;
        private object lockObject = new Object();
        private int remainingRequests;

        public RpcExecutor(ProposalManager proposalManager, int maxRequestsPerMinute){
            this.proposalManager = proposalManager;
            this.maxRequestsPerMinute = maxRequestsPerMinute;
            if(this.maxRequestsPerMinute > 0){
                this.remainingRequests = this.maxRequestsPerMinute;
                timer = Stopwatch.StartNew();
            }
        }

        // Executes rpcs and returns a response. Entry point for azure functions.
        public async Task<(HttpStatusCode,string)> ExecuteAsync(IDictionary<string, string> queryParameters, string bodyString){
            if(this.maxRequestsPerMinute > 0){
                this.ResetLimit();
                if(Interlocked.Decrement(ref this.remainingRequests) <= 0){
                    return (HttpStatusCode.TooManyRequests, "Exceeded limits");
                }
            }

            if(queryParameters.TryGetValue("type", out string typeValue) && !string.IsNullOrEmpty(typeValue) && typeValue.Equals("jsonrpc", StringComparison.InvariantCultureIgnoreCase)){
                // https://<functionUrl>?type=jsonrpc , with body containing json parameters.
                return await ExecuteAsync(bodyString);
            }
            else
            {
                // isApproved method, https://<functionUrl>?proposalId=<proposalId>
                bool approved = false;
                if(queryParameters.TryGetValue("proposalId", out string proposalId) && !string.IsNullOrEmpty(proposalId)){
                    approved = (await this.proposalManager.IsApprovedAsync(proposalId, approvalMetadata: null)).IsApproved;
                }
                var responseObj = new {
                    Approved = approved
                };
                return (HttpStatusCode.OK, JsonConvert.SerializeObject(responseObj));
            }
        }

        // Executes rpcs and returns a response for payload.
        internal async Task<(HttpStatusCode,string)> ExecuteAsync(string payload){
            JObject request = null;
            try{
                request = JsonConvert.DeserializeObject<JObject>(payload);
                if(!request.TryGetValue(MethodName, System.StringComparison.InvariantCultureIgnoreCase, out JToken methodJToken)){
                    return new Response<ErrorResponse>(){
                            Payload =  new ErrorResponse(){
                            ErrorMessage = "Method not provided"
                            }
                        }.SetStatusAndGetResponse(HttpStatusCode.BadRequest);
                }

                if(!request.TryGetValue(Payload, System.StringComparison.InvariantCultureIgnoreCase, out JToken payloadJObject)){
                    return new Response<ErrorResponse>(){
                            Payload =  new ErrorResponse(){
                            ErrorMessage = "Payload not provided"
                            }
                        }.SetStatusAndGetResponse(HttpStatusCode.BadRequest);
                }

                switch(methodJToken.Value<string>().ToLowerInvariant()){
                    case "createproposal":
                        var proposal = payloadJObject.ToObject<Proposal>();
                        var createdTime = await this.proposalManager.CreateProposalAsync(proposal);
                        return new Response<object>(){
                            Payload =  new {
                                CreatedDate = createdTime
                            }
                        }.SetStatusAndGetResponse(HttpStatusCode.OK);

                    case "isapproved":
                        var requestDetails = payloadJObject.ToObject<Dictionary<string, string>>();
                        requestDetails.TryGetValue("ProposalId", out string proposalId);
                        requestDetails.TryGetValue("ApprovalMetadata", out string approvalMetadata);
                        (bool isApproved, string validationRecordCid) = await this.proposalManager.IsApprovedAsync(proposalId, approvalMetadata);
                        return new Response<object>(){
                            Payload =  new {
                                IsApproved = isApproved,
                                ValidationRecordCID = validationRecordCid
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
            catch(Exception){
                // TODO:- log
                return new Response<ErrorResponse>(){
                    Payload =  new ErrorResponse(){
                    ErrorMessage = "Unexpected error.",
                    }
                }.SetStatusAndGetResponse(HttpStatusCode.InternalServerError);
            }
        }

        private void ResetLimit(){
            if(this.timer.ElapsedMilliseconds > 60000){
                lock(this.lockObject){
                    Interlocked.Exchange(ref this.remainingRequests, this.maxRequestsPerMinute);
                    this.timer.Restart();
                }
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
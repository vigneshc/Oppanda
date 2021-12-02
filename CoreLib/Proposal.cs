using System;
using Newtonsoft.Json;
using System.Linq;

namespace OppandaCoreLib
{
    public enum ApprovalType
    {
        Twitter,
        OfflineSignatures
    }

    // Represents a proposal.
    public class Proposal
    {
        public string Id { get; set; }
        public ApprovalType ApprovalType { get; set; }
        public string ProposalDetailsLink { get; set; }
        public string OwnerHandle { get; set; }
        public string ProposalsDetailsIPFSCID { get; set;}
        public DateTime CreatedDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAmended { get;set; }
        public string PreviousProposalCID { get; set; }
        public string ProposalCID { get; set; }
        public uint AmendmentNumber { get; set;}

        public bool StoreInIPFS { get; set; }

        /* ApprovalType = Twitter ==> Require a tweet that includes #ProposalId and ( #Approve or #DisApprove) from each handle listed in ValidatorHandles.
        *  ApprovalType = OfflineSignatures ==> Each key listed should sign {"Id": "<Id>", "Approved": "<true/false>"}
        */
        public string[] ValidatorHandles { get; set; }
        public static Proposal Deserialize(string serializedProposal){ 
            try{
                return JsonConvert.DeserializeObject<Proposal>(serializedProposal);
            }
            catch(JsonException e){
                throw new OppandaException($"Invalid json: {e.Message}", e);
            }
        }
        public void Validate(){
            if(this.IsAmended && string.IsNullOrEmpty(this.PreviousProposalCID)){
                throw new OppandaException("Amended proposals need previous proposal cid");
            }
            
            if(this.EndDate < DateTime.UtcNow){
                throw new OppandaException("Proposal should have a valid end date");
            }

            if(string.IsNullOrEmpty(this.Id)){
                throw new OppandaException("Id cannot be empty");
            }

            if(string.IsNullOrEmpty(this.OwnerHandle)){
                throw new OppandaException("Owner handle is required");
            }

            if(this.ValidatorHandles == null || this.ValidatorHandles.Length == 0 || this.ValidatorHandles.Any(h => h.Equals(this.OwnerHandle, StringComparison.InvariantCultureIgnoreCase))){
                throw new OppandaException("Validation handle invalid");
            }
        }

        public string Serialize()=> JsonConvert.SerializeObject(this);
    }

    public class OppandaException: Exception{
        public OppandaException(string message): base(message) {

        }

        public OppandaException(string message, Exception e): base(message, e){

        }
    }
}

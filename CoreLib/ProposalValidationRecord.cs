using System;
using System.Linq;
using Newtonsoft.Json;

namespace OppandaCoreLib
{
    // current status of proposal validation.
    public class ProposalValidationRecord{
        public DateTime LastUpdated { get; set; }
        public string ProposalId { get; set; }
        public ValidatorRecord[] ValidationRecords { get; set; }

        public string ApprovalMetadata { get; set; }
        
        // TODO:- store in IPFS and set CID.
        public string ValidationRecordCID { get; set; }

        public bool IsApprovalComplete(Proposal proposal){
            // no un approved validators.
            return !proposal.ValidatorHandles
            .Except(this.ValidationRecords.Where(h => h.Approved).Select(h => h.ValidatorHandle))
            .Any();
        }

        public string Serialize() => JsonConvert.SerializeObject(this);
        public static ProposalValidationRecord Deserialize(string json)=> JsonConvert.DeserializeObject<ProposalValidationRecord>(json);
    }

    // represents a single validator's approval.
    public class ValidatorRecord{
        public string ValidatorHandle { get; set; }
        public DateTime ApprovalDate { get; set; }
        public bool Approved { get; set; }
        
        // TODO:- location of validation record. For signature based oracle, it will be IPFS CID of rpc request that recorded SignatureBasedApproval
        public string RecordCID { get; set; }
        
        // Id of validation record. For twitter, it will be tweet id.
        public string ValidationRecordId { get; set; }
    }
}

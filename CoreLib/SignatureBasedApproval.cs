using Newtonsoft.Json;

namespace OppandaCoreLib
{
    // format of the message validators would sign.
    public class SignatureBasedApprovalRecord{
        public string ProposalId { get; set; }
        public string ValidatorHandle { get; set; }
        public bool Approved { get; set; }

        public static SignatureBasedApprovalRecord Deserialize(string json) => JsonConvert.DeserializeObject<SignatureBasedApprovalRecord>(json);
    }

    // Represents one signature. Collect all such signatures, and store an array of SignatureRecords in IPFS
    // Use the IPFS CID containing SignatureRecord[] to record proposal approval.
    public class SignatureRecord{
        private SignatureBasedApprovalRecord signatureBasedApprovalRecord;
        private string signatureBasedApprovalRecordPayload;

        // This should be a json of SignatureBasedApprovalRecord
        public string SignatureBasedApprovalRecordPayload { 
            get{
                return this.signatureBasedApprovalRecordPayload;
            }
            set{
                this.signatureBasedApprovalRecord = SignatureBasedApprovalRecord.Deserialize(value);
                this.signatureBasedApprovalRecordPayload = value;
            }
        }
        
        public SignatureBasedApprovalRecord SignatureBasedApprovalRecord => this.signatureBasedApprovalRecord;
        
        // This should be the signature obtained from sign(this.SignatureBasedApprovalRecordPayload, <private key of SignatureBasedApprovalRecord.ValidatorHandle>)
        public string Signature { get; set; }
    }
}
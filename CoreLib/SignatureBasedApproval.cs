using Newtonsoft.Json;

namespace OppandaCoreLib
{
    public class SignatureBasedApproval{
        public string ProposalId { get; set; }
        public string ValidatorHandle { get; set; }
        public bool Approved { get; set; }

        public static SignatureBasedApproval Deserialize(string json) => JsonConvert.DeserializeObject<SignatureBasedApproval>(json);
    }
}
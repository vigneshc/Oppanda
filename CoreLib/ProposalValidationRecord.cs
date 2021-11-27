using System;
using System.Linq;

namespace OppandaCoreLib
{
    // current status of proposal validation.
    public class ProposalValidationRecord{
        public DateTime LastUpdated { get; set; }
        public string ProposalId { get; set; }
        public ValidatorRecord[] ValidationRecords { get; set; }
        public bool IsApprovalComplete(Proposal proposal){
            // no un approved validators.
            return !proposal.ValidatorHandles
            .Except(this.ValidationRecords.Where(h => h.Approved).Select(h => h.ValidatorHandle))
            .Any();
        }
    }

    // represents a single validator's approval.
    public class ValidatorRecord{
        public string ValidatorHandle { get; set; }
        public DateTime ApprovalDate { get; set; }
        public bool Approved { get; set; }
        public string RecordCID { get; set; }
    }
}

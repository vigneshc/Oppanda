using System;
using System.Threading.Tasks;

namespace OppandaCoreLib
{
    public class ProposalManager{
        private readonly IProposalStore proposalStore;
        private readonly ITwitterValidator twitterValidator;
        public ProposalManager(IProposalStore proposalStore, ITwitterValidator twitterValidator){
            this.proposalStore = proposalStore;
            this.twitterValidator = twitterValidator;
        }

        // creates a new proposal.
        public async Task<DateTime> CreateProposalAsync(string proposalJson){
            Proposal proposal = Proposal.Deserialize(proposalJson);
            proposal.Validate();
            proposal.AmendmentNumber = 0;
            proposal.CreatedDate = DateTime.UtcNow;
            await this.proposalStore.InsertProposalAsync(proposal);
            return proposal.CreatedDate;
        }

        public Task AmendProposalAsync(string newProposalJson){
            throw new NotImplementedException();
        }

        public async Task<bool> IsApprovedAsync(string proposalId){
            Proposal proposal = await this.proposalStore.GetProposalAsync(proposalId);
            ProposalValidationRecord validationRecord = await this.proposalStore.GetProposalValidationRecordAsync(proposalId);
            if(validationRecord != null && validationRecord.IsApprovalComplete(proposal)){
                return true;
            }

            // query twitter again.
            if(proposal.ApprovalType == ApprovalType.Twitter && validationRecord == null || validationRecord.LastUpdated < DateTime.UtcNow.AddMinutes(-30)){
                var newValidationRecord = await this.twitterValidator.GetProposalValidationRecordAsync(proposal);
                newValidationRecord.LastUpdated = DateTime.UtcNow;
                await this.proposalStore.UpdateProposalValidationRecordAsync(newValidationRecord);
                return newValidationRecord.IsApprovalComplete(proposal);
            }

            return false;
        }

        // Retrieve proposal, check if validator in payload is in the list, check signature and update proposal.
        public Task AddApprovalWithSignatureAsync(string payload, string signature){
            throw new NotImplementedException();
        }
    }
}
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
        public async Task<DateTime> CreateProposalAsync(Proposal proposal){
            proposal.Validate();
            proposal.AmendmentNumber = 0;
            proposal.CreatedDate = DateTime.UtcNow;
            await this.proposalStore.InsertProposalAsync(proposal);
            return proposal.CreatedDate;
        }

        public Task AmendProposalAsync(string newProposalJson){
            throw new NotImplementedException();
        }

        public async Task<(bool IsApproved, string ValidationRecordCID)> IsApprovedAsync(string proposalId, string approvalMetadata){
            Proposal proposal = await this.proposalStore.GetProposalAsync(proposalId);
            ProposalValidationRecord validationRecord = await this.proposalStore.GetProposalValidationRecordAsync(proposalId);
            if(validationRecord != null && validationRecord.IsApprovalComplete(proposal)){
                return (true, validationRecord.ValidationRecordCID);
            }

            // query twitter again.
            if(proposal.ApprovalType == ApprovalType.Twitter && (validationRecord == null || validationRecord.LastUpdated < DateTime.UtcNow.AddMilliseconds(-30))){
                ulong minTweetId = 1000;
                if(!string.IsNullOrEmpty(approvalMetadata) && ulong.TryParse(approvalMetadata, out ulong parsedMinTweetId) ){
                    minTweetId = parsedMinTweetId;
                }

                var newValidationRecord = await this.twitterValidator.GetProposalValidationRecordAsync(proposal, minTweetId);
                newValidationRecord.LastUpdated = DateTime.UtcNow;
                await this.proposalStore.UpdateProposalValidationRecordAsync(newValidationRecord);
                return (newValidationRecord.IsApprovalComplete(proposal), newValidationRecord.ValidationRecordCID);
            }

            return (false, validationRecord?.ValidationRecordCID);
        }

        // Retrieve proposal, check if validator in payload is in the list, check signature and update proposal.
        public Task AddApprovalWithSignatureAsync(string proposalId, string payload, string signature){
            throw new NotImplementedException();
        }
    }
}
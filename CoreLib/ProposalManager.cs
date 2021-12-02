using System;
using System.Threading.Tasks;
using OppandaCoreLib.IPFS;

namespace OppandaCoreLib
{
    public class ProposalManager{
        private readonly IProposalStore proposalStore;
        private readonly ITwitterValidator twitterValidator;
        private readonly Web3Client web3Client;

        public ProposalManager(IProposalStore proposalStore, ITwitterValidator twitterValidator, Web3Client web3Client){
            this.proposalStore = proposalStore;
            this.twitterValidator = twitterValidator;
            this.web3Client = web3Client;
        }

        // creates a new proposal.
        public async Task<(DateTime CreatedTime, string IPFSCID)> CreateProposalAsync(Proposal proposal){
            proposal.Validate();
            proposal.AmendmentNumber = 0;
            proposal.CreatedDate = DateTime.UtcNow;
            if(proposal.StoreInIPFS){
                proposal.ProposalCID = await this.web3Client.UploadContentAsync(proposal.Serialize());
            }

            await this.proposalStore.InsertProposalAsync(proposal);
            return (proposal.CreatedDate, proposal.ProposalCID);
        }

        public Task AmendProposalAsync(string newProposalJson){
            throw new NotImplementedException();
        }

        public async Task<(bool IsApproved, string ValidationRecordCID)> IsApprovedAsync(string proposalId, string approvalMetadata){
            Proposal proposal = await this.proposalStore.GetProposalAsync(proposalId);
            ProposalValidationRecord validationRecord = await this.proposalStore.GetProposalValidationRecordAsync(proposalId);
            if(proposal!= null && validationRecord != null && validationRecord.IsApprovalComplete(proposal)){
                return (true, validationRecord.ValidationRecordCID);
            }

            // query twitter again.
            if(proposal != null && proposal.ApprovalType == ApprovalType.Twitter && (validationRecord == null || validationRecord.LastUpdated < DateTime.UtcNow.AddSeconds(-30))){
                ulong minTweetId = 1000;
                if(approvalMetadata == null){
                    approvalMetadata = validationRecord?.ApprovalMetadata;
                }
                
                if(!string.IsNullOrEmpty(approvalMetadata) && ulong.TryParse(approvalMetadata, out ulong parsedMinTweetId) ){
                    minTweetId = parsedMinTweetId;
                }

                var newValidationRecord = await this.twitterValidator.GetProposalValidationRecordAsync(proposal, minTweetId);
                newValidationRecord.LastUpdated = DateTime.UtcNow;
                newValidationRecord.ApprovalMetadata = approvalMetadata;
                if(proposal.StoreInIPFS){
                    newValidationRecord.ValidationRecordCID = await this.web3Client.UploadContentAsync(newValidationRecord.Serialize());
                }
                
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
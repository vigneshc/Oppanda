using System.Threading.Tasks;

namespace OppandaCoreLib
{
    public class TwitterValidator : ITwitterValidator
    {
        private readonly TwitterConfig config;
        public TwitterValidator(TwitterConfig config){
            this.config = config;
        }

        public Task<ProposalValidationRecord> GetProposalValidationRecordAsync(Proposal proposal)
        {
            // TODO:- implement.
            var result = new ProposalValidationRecord(){
                ProposalId = proposal.Id,
                LastUpdated = System.DateTime.MinValue,
                ValidationRecords = new ValidatorRecord[0]
            };
            return Task.FromResult(result);
        }
    }

    public class TwitterConfig{
        // TODO:-

    }
}
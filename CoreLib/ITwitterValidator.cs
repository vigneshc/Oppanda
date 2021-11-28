using System.Threading.Tasks;

namespace OppandaCoreLib
{
    public interface ITwitterValidator{
        Task<ProposalValidationRecord> GetProposalValidationRecordAsync(Proposal proposal, ulong? minTweetId);
    }
}
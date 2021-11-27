using System.Threading.Tasks;

namespace OppandaCoreLib
{
    // Storage for proposals. Writes to single proposalId should be sequential.
    public interface IProposalStore{
        
        // inserts proposal if a proposal with Id does not exist.
        Task InsertProposalAsync(Proposal proposal);

        // throws OppandaException if proposalId is not found. 
        Task<Proposal> GetProposalAsync(string proposalId);

        // returns proposal.
        Task<ProposalValidationRecord> GetProposalValidationRecordAsync(string proposalId);

        // overwrites with new record.
        Task UpdateProposalValidationRecordAsync(ProposalValidationRecord newRecord);
    }
}
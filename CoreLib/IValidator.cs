using System.Threading.Tasks;

namespace OppandaCoreLib
{
    public interface IValidator{
        Task<bool> IsApprovedAsync(Proposal proposal);
    }
}

using OppandaCoreLib;
using OppandaCoreLib.TwitterIntegration;
using OppandaCoreLib.IPFS;
using Oppanda.AzureTableStore;
using System.Threading.Tasks;

namespace Oppanda.AzureFunctions
{
    public static class RpcExecutorFactory {
        public async static Task<RpcExecutor> GetRpcExecutorAsync(OppandaConfig config){
            AzureTableProposalStore proposalStore = new AzureTableProposalStore(config.StorageConnectionString);
            await proposalStore.InitializeAsync();
            TwitterValidator twitterValidator = new TwitterValidator(config.TwitterConfig);
            Web3Client web3Client = new Web3Client(config.Web3ApiKey);
            ProposalManager proposalManager = new ProposalManager(proposalStore, twitterValidator, web3Client);
            RpcExecutor executor = new RpcExecutor(proposalManager, config.MaxRequestsPerMinute);
            return executor;
        }
    }
}
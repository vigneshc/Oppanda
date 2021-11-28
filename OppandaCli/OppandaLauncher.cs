using OppandaCoreLib;
using OppandaCoreLib.TwitterIntegration;
using Oppanda.AzureTableStore;
using System.Threading.Tasks;

namespace OppandaCli
{
    public static class OppandaLauncher{
        public async static Task<RpcExecutor> GetRpcExecutorAsync(OppandaConfig config){
            AzureTableProposalStore proposalStore = new AzureTableProposalStore(config.StorageConnectionString);
            await proposalStore.InitializeAsync();
            TwitterValidator twitterValidator = new TwitterValidator(config.TwitterConfig);
            ProposalManager proposalManager = new ProposalManager(proposalStore, twitterValidator);
            RpcExecutor executor = new RpcExecutor(proposalManager);
            return executor;
        }
    }
}
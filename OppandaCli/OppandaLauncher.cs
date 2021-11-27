using OppandaCoreLib;
using Oppanda.AzureTableStore;

namespace OppandaCli
{
    public static class OppandaLauncher{
        public static RpcExecutor GetRpcExecutor(OppandaConfig config){
            IProposalStore proposalStore = new AzureTableProposalStore(
                config.StorageInfo.Uri, 
                config.StorageInfo.AccountName, 
                config.StorageInfo.AccountKey);
            TwitterValidator twitterValidator = new TwitterValidator(config.TwitterConfig);
            ProposalManager proposalManager = new ProposalManager(proposalStore, twitterValidator);
            RpcExecutor executor = new RpcExecutor(proposalManager);
            return executor;
        }
    }
}
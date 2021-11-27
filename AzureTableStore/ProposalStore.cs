using System.Threading.Tasks;
using OppandaCoreLib;
using Azure;
using Azure.Data.Tables;

namespace Oppanda.AzureTableStore
{
    public class AzureTableProposalStore : IProposalStore
    {
        const string ProposalTableName = "proposals";
        const string DataFieldName = "data";
        private TableClient proposalTable;

        public AzureTableProposalStore(string storageUri, string accountName, string accountKey)
        {
            this.proposalTable = new TableClient(new System.Uri(storageUri), ProposalTableName,new TableSharedKeyCredential(accountName, accountKey));
        }
        public async Task InitializeAsync(){
            await this.proposalTable.CreateIfNotExistsAsync();
        }

        public async Task<Proposal> GetProposalAsync(string proposalId)
        {
            var response = await this.proposalTable.GetEntityAsync<TableEntity>(proposalId, proposalId);
            var entity = response.Value;
            if(entity == null){
                throw new OppandaException("Invalid proposal");
            }

            return Proposal.Deserialize(entity.GetString(DataFieldName));
        }

        public Task<ProposalValidationRecord> GetProposalValidationRecordAsync(string proposalId)
        {
            // TODO:- implement.
            return null;
        }

        public async Task InsertProposalAsync(Proposal proposal)
        {
            var proposalEntity = new TableEntity()
            {
                PartitionKey = proposal.Id,
                RowKey = proposal.Id
            };

            proposalEntity.Add(DataFieldName, proposal.Serialize());
            try{
                await this.proposalTable.AddEntityAsync(proposalEntity);
            }
            catch(RequestFailedException e){
                throw new OppandaException("Error creating proposal", e);
            }
        }

        public Task UpdateProposalValidationRecordAsync(ProposalValidationRecord newRecord)
        {
            // TODO:- implement
            return Task.CompletedTask;
        }
    }
}
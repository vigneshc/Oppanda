using System.Threading.Tasks;
using OppandaCoreLib;
using Azure;
using Azure.Data.Tables;

namespace Oppanda.AzureTableStore
{
    public class AzureTableProposalStore : IProposalStore
    {
        const string ProposalTableName = "proposals";
        const string ValidationRecordsTableName = "validationrecords";
        const string DataFieldName = "data";
        private TableClient proposalTable;
        private TableClient validationRecordsTable;

        public AzureTableProposalStore(string storageConnectionString)
        {
            this.proposalTable = new TableClient(storageConnectionString, ProposalTableName);
            this.validationRecordsTable = new TableClient(storageConnectionString, ValidationRecordsTableName);
        }

        public async Task InitializeAsync(){
            await this.proposalTable.CreateIfNotExistsAsync();
            await this.validationRecordsTable.CreateIfNotExistsAsync();
        }

        public async Task<Proposal> GetProposalAsync(string proposalId)
        {
            try{
                var response = await this.proposalTable.GetEntityAsync<TableEntity>(proposalId, proposalId);
                var entity = response.Value;
                if(entity == null){
                    throw new OppandaException("Invalid proposal");
                }

                return Proposal.Deserialize(entity.GetString(DataFieldName));
            }
            catch(RequestFailedException){
                // TODO:- log.
                return null;
            }
        }

        public async Task<ProposalValidationRecord> GetProposalValidationRecordAsync(string proposalId)
        {
            var entity = await this.GetProposalValidationEntityAsync(proposalId);
            if(entity != null){
                return ProposalValidationRecord.Deserialize(entity.GetString(DataFieldName));
            }

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

        public async Task UpdateProposalValidationRecordAsync(ProposalValidationRecord newRecord)
        {
            var newValidationRecord = new TableEntity()
            {
                PartitionKey = newRecord.ProposalId,
                RowKey = newRecord.ProposalId
            };

            newValidationRecord.Add(DataFieldName, newRecord.Serialize());
            try
            {
                // TODO:- last write wins. Should fail on conflict instead.
                await this.validationRecordsTable.UpsertEntityAsync(newValidationRecord, TableUpdateMode.Replace);
            }
            catch(RequestFailedException e){
                // TODO:- log. Only ignore 409.
                throw new OppandaException("Error Updating validation record", e);
            }

            return;
        }
        
        private async Task<TableEntity> GetProposalValidationEntityAsync(string proposalId)
        {
            try
            {
                var response = await this.validationRecordsTable.GetEntityAsync<TableEntity>(proposalId, proposalId);
                var entity = response.Value;
                if(entity != null){
                    return entity;
                }
            }
            catch(RequestFailedException)
            {
                // ignore
            }

            return null;
        }

    }
}
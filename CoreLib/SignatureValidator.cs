using Newtonsoft.Json;
using Nethereum.Signer;
using System;
using System.Linq;
using System.Collections.Generic;

namespace OppandaCoreLib
{
    public class SignatureValidator{

        public ProposalValidationRecord GetProposalValidationRecord(Proposal proposal, string signatureRecordsPayload){
            var signatureRecords = new SignatureRecord[0];
            if(!string.IsNullOrEmpty(signatureRecordsPayload)){
                try{
                    signatureRecords = JsonConvert.DeserializeObject<SignatureRecord[]>(signatureRecordsPayload);
                }
                catch(JsonException){
                }
            }

            return GetProposalValidationRecord(proposal, signatureRecords);
        }

        public ProposalValidationRecord GetProposalValidationRecord(Proposal proposal, SignatureRecord[] signatureRecords){
            var signer = new EthereumMessageSigner();
            var now = DateTime.UtcNow;
            var result = new ProposalValidationRecord(){
                ProposalId = proposal.Id,
                LastUpdated = now
            };
            List<ValidatorRecord> validValidationRecords = new List<ValidatorRecord>();
            if(signatureRecords == null || signatureRecords.Length != proposal.ValidatorHandles.Length){
                return result;
            }

            // 1. for each address in validator handle
            foreach(var validator in proposal.ValidatorHandles){

                // 2. Get the corresponding signature.
                var validatorSignature = signatureRecords
                .FirstOrDefault(record => record.SignatureBasedApprovalRecord?.ValidatorHandle.Equals(validator) ?? false);

                // validate the signature and add record.
                if(validatorSignature != null){
                    var retrievedAddress = signer.EncodeUTF8AndEcRecover(validatorSignature.SignatureBasedApprovalRecordPayload, validatorSignature.Signature);
                    if(retrievedAddress.Equals(validator) && validatorSignature.SignatureBasedApprovalRecord != null &&  validatorSignature.SignatureBasedApprovalRecord.ProposalId.Equals(proposal.Id)){
                        var validationRecord = new ValidatorRecord(){
                            ValidatorHandle = validator,
                            ApprovalDate = now,
                            Approved = validatorSignature.SignatureBasedApprovalRecord?.Approved ?? false,
                            ValidationRecordId = validatorSignature.SignatureBasedApprovalRecordPayload
                        };
                        validValidationRecords.Add(validationRecord);
                    }
                }
            }

            result.ValidationRecords = validValidationRecords.ToArray();
            return result;
        }
    }
}
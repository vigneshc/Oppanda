**Oppanda** is an experimental project to add support for creating off chain proposals with defined validators that can be funded. Proposals are defined off-chain and `proposal=> external validator url` is recorded onchain. Funds are transfered when external oracle validates that proposal is approved by validators. Currently proposals can be validated through tweets from validators and Off-chain signatures stored in IPFS.
Proposals and approval records are optionally stored in IPFS using Web3.Storage api.

## Concepts
### On-Chain Contract
A solidity contract that records `proposalId => (Oracle Url, AvailableFund)`. 
* *Proposers* creates the proposal. 
* Anyone can fund the proposal.
* Proposers* requests funds. Contract requests external oracle url if the fund is approved, and if approved it transfers available fund.

### Proposal
* A *Json* document describing who the validators along with IPFS CID that provides human readable details.
* Defines oracle type. Currently *Twitter* and *signature proofs* are added.
    * Twitter - Validators is a set of twitter handles. To approve a proposal, all validators should tweet with both `#<proposalId>` and `#approve` hashtags in the tweet.
    * Signature Proof - Proposer should record a IPFS CID that contains signature from all validators. File format is in `SignatureBasedApproval.cs`.

### Off-Chain Oracle
* External service that is able to confirm if a proposal has been validated by the validators. Both *twitter* and *signature proof* can be validated by anyone. Function of oracle is to provide this service and it introduces a point of centralization.
* Has APIs for registering `Proposals` and checking if a proposal has been approved. Proposals and validation records are stored in IPFS in addition to oracle's own storage.
* `On chain contract` has a `proposalId => Oracle Url` map. The assumption is that the ones that fund trust the `Oracle Url`. On-Chain contract can be extended with multiple such urls to minimize trust on one url.
* Only requirement for this `Oracle Url` is to return `{"Approved": true/false}` for `https://oracleurl?proposalId=<proposalId`>` requests.

### Oppanda Oracle
* It is an implementation of `Off-Chain Oracle`. 

**Example Use case:** Fund Raising for social impact

Proposer can define a proposal with reputed twitter handles as validators and launch a campaign on twitter. Validators can show their support through tweets. Anyone can view the list of validators, oracle url and fund the proposal. When all the validators have approved, proposer can request fund and use it. Amount of fund disbursed  by a oracle url signals its reputation to some extent. Gas costs for each of the validator is avoided. 

### Other approaches
1. This can be done purely on-chain, with each validator recording approval on-chain. It would have additional gas costs.
2. *Snapshot* is an off-chain voting mechanism. It focus on DAOs. *Reality.eth* can be used to record the decisions on-chain, it has bond based incentives for recorders.


## Code structure
1. *EthContracts/*: Solidity contracts. There is only one now.
2. *CoreLib/*: Definition for contracts, twitter validation and IPFS interaction.
3. *AzureTableStore/*: Storage for proposal entries and validation records.
4. *AzureFunctionsLauncher/*: Helper methods for hosting it in azure function.
5. *OppandaCli/*: Launches it locally. Used for testing.


## Hosting

1. `RpcExecutor.ExecuteAsync(queryParameters, bodyString)` is the entry point. Call this with http query parameters and request body.
2. When `type=jsonrpc` is added to queryParameters, it executes the json rpc request. Format of requests is described later.
3. Without `type=jsonrpc` parameter, it executes `IsApprovedAsync` taking `proposalId` query parameter. It would return a json response of the form below.
```json
{
    "Approved": <true/false>
}
```
4. `OppandaFunctionsRunner.Initialize` has the initialization code that reads config and creates `RpcExecutor`. 

Above pieces should support hosting in a variety of environments. 

## Next Steps

1. Better documentation for creating and approving proposals.
2. Share hosted url for `Oppanda Oracle`
3. Solidity Contract updates
     - Implement withdrawal.
     - Require LINK tokens for request funds api calls.
     - Emit events.
     - Mainnet deployment.
4. UX for querying proposals, statuses.
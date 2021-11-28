// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;
import "@chainlink/contracts/src/v0.8/ChainlinkClient.sol";

contract Oppanda is ChainlinkClient{
    using Chainlink for Chainlink.Request;
    enum OracleTypeEnum{
        NoOracle,
        Url
    }

    struct Proposal{
        string Id;
        OracleTypeEnum OracleType;
        address Beneficiary;
        string OracleUrl;
        uint AvailableWei;
        bool Created;
    }

    struct PendingRequestDetails{
        string ProposalId;
        uint RequestedWei;
        bool IsValid;
        bool Fulfilled;
    }

    // proposals.
    mapping(string => Proposal) private Proposals;

    // requests that are initiated but not fulfilled.
    mapping(bytes32 => PendingRequestDetails) private PendingRequests;

    // transfers that are requested but not completed.
    mapping(string => uint) private PendingTransfers;

    address private oracleAddress;
    address private ownerAddress;
    bytes32 private jobId;
    uint256 private fee;

    constructor(){
        setPublicChainlinkToken();
        oracleAddress = 0xc57B33452b4F7BB189bB5AfaE9cc4aBa1f7a4FD8;
        ownerAddress = 0xBd7d128E0b7F634868cda39F26607a19b0E8c40F;
        jobId = "bc746611ebee40a3989bbe49e12a02b9";
        fee = 0.1 * 10 ** 18;
    }

    // transfers wei sent to contract and records.
    function fundProposal(string calldata proposalId) external payable{
        Proposals[proposalId].AvailableWei += msg.value;
        if(PendingTransfers[proposalId] > 0){
            handleTransfer(proposalId, PendingTransfers[proposalId] );
        }
    }

    modifier calledByOwner(){
        require(msg.sender == ownerAddress);
        _;
    }

    function updateContractParameters(address _oracleAddress, bytes32 _jobId, uint256 _fee) external calledByOwner{
        oracleAddress = _oracleAddress;
        jobId = _jobId;
        fee = _fee;
    }

    function updateOwner(address _newOwner) external calledByOwner{
        ownerAddress = _newOwner;
    }

    function getAvailableFund(string calldata proposalId) external view returns (uint){
        return Proposals[proposalId].AvailableWei;
    }

    function getOracleUrl(string calldata proposalId) external view returns (string memory){
        return Proposals[proposalId].OracleUrl;
    }

    // there shouldn't be another proposal.
    modifier newProposal(string calldata proposalId){
        require(Proposals[proposalId].Created == false);
        _;
    }

    modifier proposalExists(string calldata proposalId){
        require(Proposals[proposalId].Created == true);
        _;
    }

    // adds a new proposal.
    function createProposal(
        string calldata proposalId,
        OracleTypeEnum oracleType,
        address beneficiary,
        string calldata oracleUrl
    ) external newProposal(proposalId){
        Proposals[proposalId].Id = proposalId;
        Proposals[proposalId].OracleType = oracleType;
        Proposals[proposalId].Beneficiary = beneficiary;
        Proposals[proposalId].OracleUrl = oracleUrl;
        Proposals[proposalId].Created = true;

        // AvailableWei is not updated. Proposal can be funded before it is created.
    }

    error NotEnoughFunds();
    error OracleRejected();
    error CouldNotTransferFunds();

    function requestFund(string calldata proposalId, uint amountInWei) external payable proposalExists(proposalId){
        
        // check if enough funds exist for proposal.
        if(Proposals[proposalId].AvailableWei < amountInWei){
            revert NotEnoughFunds();
        }

        if(Proposals[proposalId].OracleType == OracleTypeEnum.NoOracle){
            // no additional checks needed.
            transferFund(proposalId, amountInWei);
        }
        else if (Proposals[proposalId].OracleType == OracleTypeEnum.Url){
            // TODO:- require LINK token.
            initiateOracleCheck(proposalId, amountInWei);
        }
    }

    function transferFund(string memory proposalId, uint amountInWei) private {
        Proposals[proposalId].AvailableWei -= amountInWei;
        // transfer funds

        bool moneySent = payable(Proposals[proposalId].Beneficiary).send(amountInWei);
        if(!moneySent){
            revert CouldNotTransferFunds();
        }
    }

    function initiateOracleCheck(string memory proposalId, uint amountInWei) private {
        Chainlink.Request memory request = buildChainlinkRequest(jobId, address(this), this.fulfillOracleCheck.selector);
        request.add(
            "get", 
            string(abi.encodePacked(Proposals[proposalId].OracleUrl, "?proposalId=", proposalId)));
        request.add("path", "Approved");
        bytes32 _requestId =  sendChainlinkRequestTo(oracleAddress, request, fee);

        PendingRequests[_requestId].IsValid = true;
        PendingRequests[_requestId].ProposalId = proposalId;
        PendingRequests[_requestId].RequestedWei = amountInWei;
    }

    modifier calledByOracle(){
        require(msg.sender == oracleAddress);
        _;
    }

    // complete request after oracle call.
    // Return values. 0 = Requested Funds transfered. 1 = Oracle rejected. 2 = Partial funds available.
    function fulfillOracleCheck(bytes32 _requestId, bool approved) external payable returns (uint8) {
        require(PendingRequests[_requestId].IsValid == true);
        require(PendingRequests[_requestId].Fulfilled == false);
        PendingRequests[_requestId].Fulfilled = true;
        
        if(!approved){
            // rejected by oracle.
            return 1;
        }
        
        return handleTransfer(PendingRequests[_requestId].ProposalId, PendingRequests[_requestId].RequestedWei);
    }

    // Return values. 0 = Requested Funds transfered. 1 = Oracle rejected. 2 = Partial funds available.
    function handleTransfer(string memory proposalId, uint weiRequestedToTransfer) private returns (uint8){
        weiRequestedToTransfer += PendingTransfers[proposalId];
        uint weiAvailableToTransfer = Proposals[proposalId].AvailableWei;
        uint finalWeiToTransfer = weiRequestedToTransfer;

        if (weiAvailableToTransfer < finalWeiToTransfer){
            finalWeiToTransfer = weiAvailableToTransfer;
        }
        PendingTransfers[proposalId] = weiRequestedToTransfer - finalWeiToTransfer;
        if (finalWeiToTransfer > 0){
            transferFund(proposalId, finalWeiToTransfer);
        }

        if(PendingTransfers[proposalId] > 0 ){
            return 2;
        }
        else{
            return 0;
        }
    }
}
using System.Collections.Generic;

namespace AElf.Kernel.SmartContract
{
    public class ContractOptions
    {
        public bool ContractDeploymentAuthorityRequired { get; set; } = true;
        public string GenesisContractDir { get; set; }

        public bool IsTxExecutionTimeoutEnabled { get; set; } = true;

        public int TransactionExecutionTimePeriodLimitInMilliSeconds { get; set; } =
            SmartContractConstants.TransactionExecutionTimePeriodLimitInMilliSeconds;
        
        //TODO: remove
        public List<string> ContractFeeStrategyAcsList { get; set; }
    }
}
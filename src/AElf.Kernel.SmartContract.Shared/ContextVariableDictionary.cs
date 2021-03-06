using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AElf.Kernel.SmartContract
{
    /// <summary>
    /// Convention: Use ',' as separator.
    /// </summary>
    public class ContextVariableDictionary : ReadOnlyDictionary<string, string>
    {
        public ContextVariableDictionary(IDictionary<string, string> dictionary) : base(dictionary)
        {
        }
    
        public string NativeSymbol => this[nameof(NativeSymbol)];
        
        //TODO:remove following fields, NativeSymbol is a common property for almost all blockchain system,
        //but SymbolListToPayTxFee, SymbolListToPayRental... are related to some specific blockchain systems
        //I have add a OnInitialize for smart contract in smart contract SDK, so we can override it to initialize
        //some fields
        public List<string> SymbolListToPayTxFee => this[nameof(SymbolListToPayTxFee)].Split(',').ToList();
        public List<string> SymbolListToPayRental => this[nameof(SymbolListToPayRental)].Split(',').ToList();
    
        public const string NativeSymbolName = nameof(NativeSymbol);
        public const string PayTxFeeSymbolList = nameof(SymbolListToPayTxFee);
        public const string PayRentalSymbolList = nameof(SymbolListToPayRental);
    }
}
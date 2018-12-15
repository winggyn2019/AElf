using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.SmartContract;
using AElf.Kernel;
using AElf.Types.CSharp;
using ByteString = Google.Protobuf.ByteString;
using AElf.Common;
using AElf.Cryptography.ECDSA;

namespace AElf.Contracts.Consensus.Tests
{
    public class ConsensusContractShim
    {
        private MockSetup _mock;
        public IExecutive Executive { get; set; }

        public TransactionContext TransactionContext { get; private set; }

        public Address Sender
        {
            get => Address.Zero;
        }

        public Address ConsensusContractAddress { get; set; }

        public ConsensusContractShim(MockSetup mock)
        {
            _mock = mock;
            Init();
        }

        private void Init()
        {
            DeployConsensusContractAsync().Wait();
            var task = _mock.GetExecutiveAsync(ConsensusContractAddress);
            task.Wait();
            Executive = task.Result;
        }

        private async Task<TransactionContext> PrepareTransactionContextAsync(Transaction tx)
        {
            var chainContext = await _mock.ChainContextService.GetChainContextAsync(_mock.ChainId);
            var tc = new TransactionContext
            {
                PreviousBlockHash = chainContext.BlockHash,
                BlockHeight = chainContext.BlockHeight,
                Transaction = tx,
                Trace = new TransactionTrace()
            };
            return tc;
        }

        private TransactionContext PrepareTransactionContext(Transaction tx)
        {
            var task = PrepareTransactionContextAsync(tx);
            task.Wait();
            return task.Result;
        }

        private async Task CommitChangesAsync(TransactionTrace trace)
        {
            await trace.CommitChangesAsync(_mock.StateStore);
        }

        private async Task DeployConsensusContractAsync()
        {
            var address0 = ContractHelpers.GetGenesisBasicContractAddress(_mock.ChainId);
            var executive0 = await _mock.GetExecutiveAsync(address0);

            var tx = new Transaction
            {
                From = Sender,
                To = address0,
                IncrementId = 0,
                MethodName = "DeploySmartContract",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(1, _mock.GetContractCode(_mock.ConsensusContractName)))
            };

            var tc = await PrepareTransactionContextAsync(tx);
            await executive0.SetTransactionContext(tc).Apply();
            await CommitChangesAsync(tc.Trace);
            ConsensusContractAddress = Address.FromBytes(tc.Trace.RetVal.ToFriendlyBytes());
        }

        #region Process

        #region View Only Methods
        
        public Round GetRoundInfo(ulong roundNumber)
        {
            var tx = new Transaction
            {
                From = Sender,
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "GetRoundInfo",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(roundNumber))
            };
            
            TransactionContext = new TransactionContext
            {
                Transaction = tx
            };
            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            TransactionContext.Trace.CommitChangesAsync(_mock.StateStore).Wait();
            return TransactionContext.Trace.RetVal?.Data.DeserializeToPbMessage<Round>();
        }

        #endregion View Only Methods

        #region Actions

        public void InitialTerm(ECKeyPair minerKeyPair, Term initialTerm)
        {
            var tx = new Transaction
            {
                From = GetAddress(minerKeyPair),
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "InitialTerm",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(initialTerm, 1))
            };
            var signer = new ECSigner();
            var signature = signer.Sign(minerKeyPair, tx.GetHash().DumpByteArray());
            tx.Sigs.Add(ByteString.CopyFrom(signature.SigBytes));

            TransactionContext = PrepareTransactionContext(tx);
            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            CommitChangesAsync(TransactionContext.Trace).Wait();
        }
        
        public void NextTerm(ECKeyPair minerKeyPair, Term nextTerm)
        {
            var tx = new Transaction
            {
                From = GetAddress(minerKeyPair),
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "NextTerm",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(nextTerm))
            };
            var signer = new ECSigner();
            var signature = signer.Sign(minerKeyPair, tx.GetHash().DumpByteArray());
            tx.Sigs.Add(ByteString.CopyFrom(signature.SigBytes));

            TransactionContext = PrepareTransactionContext(tx);
            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            CommitChangesAsync(TransactionContext.Trace).Wait();
        }
        
        public void PackageOutValue(ECKeyPair minerKeyPair, ToPackage toPackage)
        {
            var tx = new Transaction
            {
                From = GetAddress(minerKeyPair),
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "PackageOutValue",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(toPackage))
            };
            var signer = new ECSigner();
            var signature = signer.Sign(minerKeyPair, tx.GetHash().DumpByteArray());
            tx.Sigs.Add(ByteString.CopyFrom(signature.SigBytes));

            TransactionContext = PrepareTransactionContext(tx);
            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            CommitChangesAsync(TransactionContext.Trace).Wait();
        }
        
        public void BroadcastInValue(ECKeyPair minerKeyPair, ToBroadcast toBroadcast)
        {
            var tx = new Transaction
            {
                From = GetAddress(minerKeyPair),
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "BroadcastInValue",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(toBroadcast))
            };
            var signer = new ECSigner();
            var signature = signer.Sign(minerKeyPair, tx.GetHash().DumpByteArray());
            tx.Sigs.Add(ByteString.CopyFrom(signature.SigBytes));

            TransactionContext = PrepareTransactionContext(tx);
            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            CommitChangesAsync(TransactionContext.Trace).Wait();
        }

        #endregion Actions

        #endregion

        #region Election

        #region View Only Methods

        public bool? IsCandidate(string publicKey)
        {
            var tx = new Transaction
            {
                From = Sender,
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "IsCandidate",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(publicKey))
            };

            TransactionContext = new TransactionContext
            {
                Transaction = tx
            };
            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            TransactionContext.Trace.CommitChangesAsync(_mock.StateStore).Wait();
            return TransactionContext.Trace.RetVal?.Data.DeserializeToBool();
        }

        public Tickets GetTicketsInfo(ECKeyPair keyPair)
        {
            var tx = new Transaction
            {
                From = Sender,
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "GetTicketsInfo",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(keyPair.PublicKey.ToHex()))
            };
            var signer = new ECSigner();
            var signature = signer.Sign(keyPair, tx.GetHash().DumpByteArray());
            tx.Sigs.Add(ByteString.CopyFrom(signature.SigBytes));

            TransactionContext = new TransactionContext
            {
                Transaction = tx
            };

            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            TransactionContext.Trace.CommitChangesAsync(_mock.StateStore).Wait();
            return TransactionContext.Trace.RetVal?.Data.DeserializeToPbMessage<Tickets>();
        }

        public string GetCurrentVictories()
        {
            var tx = new Transaction
            {
                From = Sender,
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "GetCurrentVictories",
                Params = ByteString.CopyFrom(ParamsPacker.Pack())
            };

            TransactionContext = new TransactionContext
            {
                Transaction = tx
            };
            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            TransactionContext.Trace.CommitChangesAsync(_mock.StateStore).Wait();
            return TransactionContext.Trace.RetVal?.Data.DeserializeToString();
        }

        #endregion View Only Methods

        #region Actions

        public void AnnounceElection(ECKeyPair candidateKeyPair)
        {
            var tx = new Transaction
            {
                From = GetAddress(candidateKeyPair),
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "AnnounceElection",
                Params = ByteString.CopyFrom(ParamsPacker.Pack())
            };
            var signer = new ECSigner();
            var signature = signer.Sign(candidateKeyPair, tx.GetHash().DumpByteArray());
            tx.Sigs.Add(ByteString.CopyFrom(signature.SigBytes));

            TransactionContext = PrepareTransactionContext(tx);
            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            CommitChangesAsync(TransactionContext.Trace).Wait();
        }

        public void QuitElection(ECKeyPair candidateKeyPair)
        {
            var tx = new Transaction
            {
                From = GetAddress(candidateKeyPair),
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "QuitElection",
                Params = ByteString.CopyFrom(ParamsPacker.Pack())
            };
            var signer = new ECSigner();
            var signature = signer.Sign(candidateKeyPair, tx.GetHash().DumpByteArray());
            tx.Sigs.Add(ByteString.CopyFrom(signature.SigBytes));

            TransactionContext = PrepareTransactionContext(tx);
            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            CommitChangesAsync(TransactionContext.Trace).Wait();
        }

        public void Vote(ECKeyPair voterKeyPair, ECKeyPair candidateKeyPair, ulong amount, int lockDays)
        {
            var tx = new Transaction
            {
                From = GetAddress(voterKeyPair),
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "Vote",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(candidateKeyPair.PublicKey.ToHex(), amount, lockDays))
            };
            var signer = new ECSigner();
            var signature = signer.Sign(voterKeyPair, tx.GetHash().DumpByteArray());
            tx.Sigs.Add(ByteString.CopyFrom(signature.SigBytes));

            TransactionContext = PrepareTransactionContext(tx);
            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            CommitChangesAsync(TransactionContext.Trace).Wait();
        }

        public void GetAllDividends(ECKeyPair ownerKeyPair)
        {
            var tx = new Transaction
            {
                From = GetAddress(ownerKeyPair),
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "GetAllDividends",
                Params = ByteString.CopyFrom(ParamsPacker.Pack())
            };
            var signer = new ECSigner();
            var signature = signer.Sign(ownerKeyPair, tx.GetHash().DumpByteArray());
            tx.Sigs.Add(ByteString.CopyFrom(signature.SigBytes));
            
            TransactionContext = PrepareTransactionContext(tx);
            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            CommitChangesAsync(TransactionContext.Trace).Wait();
        }

        public void Approve(Address spender, ulong amount)
        {
            var tx = new Transaction
            {
                From = Sender,
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "Approve",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(spender, amount))
            };

            TransactionContext = PrepareTransactionContext(tx);
            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            CommitChangesAsync(TransactionContext.Trace).Wait();
        }

        public void UnApprove(Address spender, ulong amount)
        {
            var tx = new Transaction
            {
                From = Sender,
                To = ConsensusContractAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "UnApprove",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(spender, amount))
            };

            TransactionContext = PrepareTransactionContext(tx);
            Executive.SetTransactionContext(TransactionContext).Apply().Wait();
            CommitChangesAsync(TransactionContext.Trace).Wait();
        }

        public Address GetContractOwner(Address scZeroAddress)
        {
            var executive = _mock.GetExecutiveAsync(scZeroAddress).Result;

            var tx = new Transaction
            {
                From = Sender,
                To = scZeroAddress,
                IncrementId = MockSetup.NewIncrementId,
                MethodName = "GetContractOwner",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(ConsensusContractAddress))
            };

            TransactionContext = PrepareTransactionContext(tx);
            executive.SetTransactionContext(TransactionContext).Apply().Wait();
            CommitChangesAsync(TransactionContext.Trace).Wait();
            return TransactionContext.Trace.RetVal?.Data.DeserializeToPbMessage<Address>();
        }

        #endregion Actions

        #endregion Election

        private Address GetAddress(ECKeyPair keyPair)
        {
            return Address.FromPublicKey(_mock.ChainId.DumpByteArray(), keyPair.PublicKey);
        }
    }
}
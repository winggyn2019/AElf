﻿using System;
using System.Security.Cryptography;
using AElf.Cryptography.ECDSA;
using AElf.Common;
using Google.Protobuf;
using Org.BouncyCastle.Math;

// ReSharper disable once CheckNamespace
namespace AElf.Kernel
{
    public partial class BlockHeader: IBlockHeader
    {
        private Hash _blockHash;
        
        public BlockHeader(Hash preBlockHash)
        {
            PreviousBlockHash = preBlockHash;
        }

        public Hash GetHash()
        {
            if (_blockHash == null)
            {
                _blockHash = Hash.FromBytes(GetSignatureData());
            }

            return _blockHash;
        }

        public byte[] GetHashBytes()
        {
            if (_blockHash == null)
                _blockHash = Hash.FromBytes(GetSignatureData());

            return _blockHash.GetHashBytes();
        }
        
        public ECSignature GetSignature()
        {
            return new ECSignature(R.ToByteArray(), S.ToByteArray());
        }

        private byte[] GetSignatureData()
        {
            var rawBlock = new BlockHeader
            {
                ChainId = ChainId.Clone(),
                Index = Index,
                PreviousBlockHash = PreviousBlockHash.Clone(),
                MerkleTreeRootOfTransactions = MerkleTreeRootOfTransactions.Clone(),
                MerkleTreeRootOfWorldState = MerkleTreeRootOfWorldState.Clone(),
                Bloom = Bloom, 
                SideChainBlockHeadersRoot = SideChainBlockHeadersRoot?.Clone(),
                SideChainTransactionsRoot = MerkleTreeRootOfTransactions?.Clone()
            };
            if (Index != 0)
                rawBlock.Time = Time.Clone();
            
            return rawBlock.ToByteArray();
        }
    }
}
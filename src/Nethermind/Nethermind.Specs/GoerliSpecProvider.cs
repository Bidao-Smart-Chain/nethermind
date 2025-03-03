// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System.Linq;
using Nethermind.Core;
using Nethermind.Core.Specs;
using Nethermind.Int256;
using Nethermind.Specs.Forks;

namespace Nethermind.Specs
{
    public class GoerliSpecProvider : ISpecProvider
    {
        public static readonly GoerliSpecProvider Instance = new();
        private GoerliSpecProvider() { }

        private ForkActivation? _theMergeBlock = null;
        private UInt256? _terminalTotalDifficulty = 10790000;

        public void UpdateMergeTransitionInfo(long? blockNumber, UInt256? terminalTotalDifficulty = null)
        {
            if (blockNumber is not null)
                _theMergeBlock = (ForkActivation)blockNumber;
            if (terminalTotalDifficulty is not null)
                _terminalTotalDifficulty = terminalTotalDifficulty;
        }

        public ForkActivation? MergeBlockNumber => _theMergeBlock;
        public ulong TimestampFork => ShanghaiTimestamp;
        public UInt256? TerminalTotalDifficulty => _terminalTotalDifficulty;
        public IReleaseSpec GenesisSpec { get; } = ConstantinopleFix.Instance;

        public IReleaseSpec GetSpec(ForkActivation forkActivation)
        {
            return forkActivation.BlockNumber switch
            {
                < IstanbulBlockNumber => GenesisSpec,
                < BerlinBlockNumber => Istanbul.Instance,
                < LondonBlockNumber => Berlin.Instance,
                _ => forkActivation.Timestamp switch
                {
                    null or < ShanghaiTimestamp => London.Instance,
                    _ => Shanghai.Instance
                }
            };
        }

        public long? DaoBlockNumber => null;
        public const long IstanbulBlockNumber = 1_561_651;
        public const long BerlinBlockNumber = 4_460_644;
        public const long LondonBlockNumber = 5_062_605;
        public const ulong ShanghaiTimestamp = 1678832736;
        public ulong NetworkId => BlockchainIds.Goerli;
        public ulong ChainId => NetworkId;

        public ForkActivation[] TransitionActivations { get; } =
        {
            (ForkActivation)IstanbulBlockNumber,
            (ForkActivation)BerlinBlockNumber,
            (ForkActivation)LondonBlockNumber,
            (LondonBlockNumber, ShanghaiTimestamp)
        };
    }
}

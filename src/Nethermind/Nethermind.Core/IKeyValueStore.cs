// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;

namespace Nethermind.Core
{
    public interface IKeyValueStore : IReadOnlyKeyValueStore
    {
        new byte[]? this[ReadOnlySpan<byte> key]
        {
            get => Get(key, ReadFlags.None);
            set => Set(key, value, WriteFlags.None);
        }

        void Set(ReadOnlySpan<byte> key, byte[]? value, WriteFlags flags = WriteFlags.None);
    }

    public interface IReadOnlyKeyValueStore
    {
        byte[]? this[ReadOnlySpan<byte> key] => Get(key, ReadFlags.None);

        byte[]? Get(ReadOnlySpan<byte> key, ReadFlags flags = ReadFlags.None);
    }

    public enum ReadFlags
    {
        None,

        // Hint that the workload is likely to not going to benefit from caching and should skip any cache handling
        // to reduce CPU usage
        HintCacheMiss,
    }

    public enum WriteFlags
    {
        None,

        // Hint that this is a low priority write
        LowPriority,
    }
}

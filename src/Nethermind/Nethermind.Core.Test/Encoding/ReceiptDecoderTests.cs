// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System.Collections.Generic;
using FluentAssertions;
using Nethermind.Core.Crypto;
using Nethermind.Core.Test.Builders;
using Nethermind.Serialization.Rlp;
using NUnit.Framework;
#pragma warning disable 618

namespace Nethermind.Core.Test.Encoding
{
    [TestFixture]
    public class ReceiptDecoderTests
    {
        [Test]
        public void Can_do_roundtrip_storage([Values(true, false)] bool encodeWithTxHash, [Values(RlpBehaviors.Storage | RlpBehaviors.Eip658Receipts, RlpBehaviors.Storage)] RlpBehaviors encodeBehaviors, [Values(true, false)] bool withError, [Values(true, false)] bool valueDecoder)
        {
            TxReceipt GetExpected()
            {
                ReceiptBuilder receiptBuilder = Build.A.Receipt.WithAllFieldsFilled;

                if ((encodeBehaviors & RlpBehaviors.Eip658Receipts) != 0)
                {
                    receiptBuilder.WithState(null!);
                }
                else
                {
                    receiptBuilder.WithStatusCode(0);
                }

                if (!encodeWithTxHash)
                {
                    receiptBuilder.WithTransactionHash(null!);
                }

                if (!withError)
                {
                    receiptBuilder.WithError(string.Empty);
                }

                return receiptBuilder.TestObject;
            }

            TxReceipt BuildReceipt()
            {
                ReceiptBuilder receiptBuilder = Build.A.Receipt.WithAllFieldsFilled;
                if (!withError)
                {
                    receiptBuilder.WithError(string.Empty);
                }

                return receiptBuilder.TestObject;
            }

            TxReceipt txReceipt = BuildReceipt();

            ReceiptStorageDecoder encoder = new(encodeWithTxHash);
            Rlp rlp = encoder.Encode(txReceipt, encodeBehaviors);

            ReceiptStorageDecoder decoder = new();
            TxReceipt deserialized;
            if (valueDecoder)
            {
                Rlp.ValueDecoderContext valueContext = rlp.Bytes.AsRlpValueContext();
                deserialized = decoder.Decode(ref valueContext, RlpBehaviors.Storage);
            }
            else
            {
                deserialized = decoder.Decode(rlp.Bytes.AsRlpStream(), RlpBehaviors.Storage);
            }

            deserialized.Should().BeEquivalentTo(GetExpected());
        }

        [Test]
        public void Can_do_roundtrip_storage_eip()
        {
            TxReceipt txReceipt = Build.A.Receipt.TestObject;
            txReceipt.BlockNumber = 1;
            txReceipt.BlockHash = TestItem.KeccakA;
            txReceipt.Bloom = new Bloom();
            txReceipt.Bloom.Set(Keccak.EmptyTreeHash.Bytes);
            txReceipt.ContractAddress = TestItem.AddressA;
            txReceipt.Sender = TestItem.AddressB;
            txReceipt.Recipient = TestItem.AddressC;
            txReceipt.GasUsed = 100;
            txReceipt.GasUsedTotal = 1000;
            txReceipt.Index = 2;
            txReceipt.PostTransactionState = TestItem.KeccakH;
            txReceipt.StatusCode = 1;

            ReceiptStorageDecoder decoder = new();
            Rlp rlp = decoder.Encode(txReceipt, RlpBehaviors.Storage | RlpBehaviors.Eip658Receipts);
            TxReceipt deserialized = decoder.Decode(rlp.Bytes.AsRlpStream(), RlpBehaviors.Storage | RlpBehaviors.Eip658Receipts);

            AssertStorageReceipt(txReceipt, deserialized);
        }

        [Test]
        public void Can_do_roundtrip_root()
        {
            TxReceipt txReceipt = Build.A.Receipt.TestObject;
            txReceipt.BlockNumber = 1;
            txReceipt.BlockHash = TestItem.KeccakA;
            txReceipt.Bloom = new Bloom();
            txReceipt.Bloom.Set(Keccak.EmptyTreeHash.Bytes);
            txReceipt.ContractAddress = TestItem.AddressA;
            txReceipt.Sender = TestItem.AddressB;
            txReceipt.Recipient = TestItem.AddressC;
            txReceipt.GasUsed = 100;
            txReceipt.GasUsedTotal = 1000;
            txReceipt.Index = 2;
            txReceipt.PostTransactionState = TestItem.KeccakH;

            ReceiptStorageDecoder decoder = new();
            Rlp rlp = decoder.Encode(txReceipt);
            TxReceipt deserialized = decoder.Decode(rlp.Bytes.AsRlpStream());

            Assert.That(deserialized.BlockHash, Is.EqualTo(null), "block hash");
            Assert.That(deserialized.BlockNumber, Is.EqualTo(0L), "block number");
            Assert.That(deserialized.Index, Is.EqualTo(0), "index");
            Assert.That(deserialized.ContractAddress, Is.EqualTo(null), "contract");
            Assert.That(deserialized.Sender, Is.EqualTo(null), "sender");
            Assert.That(deserialized.GasUsed, Is.EqualTo(0L), "gas used");
            Assert.That(deserialized.GasUsedTotal, Is.EqualTo(1000L), "gas used total");
            Assert.That(deserialized.Bloom, Is.EqualTo(txReceipt.Bloom), "bloom");
            Assert.That(deserialized.PostTransactionState, Is.EqualTo(txReceipt.PostTransactionState), "post transaction state");
            Assert.That(deserialized.Recipient, Is.EqualTo(null), "recipient");
            Assert.That(deserialized.StatusCode, Is.EqualTo(txReceipt.StatusCode), "status");
        }

        [Test]
        public void Can_do_roundtrip_storage_rlp_stream()
        {
            TxReceipt txReceipt = Build.A.Receipt.TestObject;
            txReceipt.BlockNumber = 1;
            txReceipt.BlockHash = TestItem.KeccakA;
            txReceipt.Bloom = new Bloom();
            txReceipt.Bloom.Set(Keccak.EmptyTreeHash.Bytes);
            txReceipt.ContractAddress = TestItem.AddressA;
            txReceipt.Sender = TestItem.AddressB;
            txReceipt.Recipient = TestItem.AddressC;
            txReceipt.GasUsed = 100;
            txReceipt.GasUsedTotal = 1000;
            txReceipt.Index = 2;
            txReceipt.PostTransactionState = TestItem.KeccakH;

            ReceiptStorageDecoder decoder = new();

            byte[] rlpStreamResult = decoder.Encode(txReceipt, RlpBehaviors.Storage).Bytes;
            TxReceipt deserialized = decoder.Decode(new RlpStream(rlpStreamResult), RlpBehaviors.Storage);

            AssertStorageReceipt(txReceipt, deserialized);
        }

        [Test]
        public void Can_do_roundtrip_none_rlp_stream()
        {
            TxReceipt txReceipt = Build.A.Receipt.TestObject;
            txReceipt.Bloom = new Bloom();
            txReceipt.Bloom.Set(Keccak.EmptyTreeHash.Bytes);
            txReceipt.GasUsedTotal = 1000;
            txReceipt.PostTransactionState = TestItem.KeccakH;

            ReceiptMessageDecoder decoder = new();

            byte[] rlpStreamResult = decoder.EncodeNew(txReceipt, RlpBehaviors.None);
            TxReceipt deserialized = Rlp.Decode<TxReceipt>(rlpStreamResult, RlpBehaviors.None);

            AssertMessageReceipt(txReceipt, deserialized);
        }

        [Test]
        public void Can_do_roundtrip_with_receipt_message_and_tx_type_access_list()
        {
            TxReceipt txReceipt = Build.A.Receipt.TestObject;
            txReceipt.Bloom = new Bloom();
            txReceipt.Bloom.Set(Keccak.EmptyTreeHash.Bytes);
            txReceipt.GasUsedTotal = 1000;
            txReceipt.PostTransactionState = TestItem.KeccakH;
            txReceipt.TxType = TxType.AccessList;

            ReceiptMessageDecoder decoder = new();

            byte[] rlpStreamResult = decoder.EncodeNew(txReceipt, RlpBehaviors.None);
            TxReceipt deserialized = decoder.Decode(new RlpStream(rlpStreamResult));

            AssertMessageReceipt(txReceipt, deserialized);
        }

        [Test]
        public void Can_do_roundtrip_with_storage_receipt_and_tx_type_access_list()
        {
            TxReceipt txReceipt = Build.A.Receipt.TestObject;
            txReceipt.BlockNumber = 1;
            txReceipt.BlockHash = TestItem.KeccakA;
            txReceipt.Bloom = new Bloom();
            txReceipt.Bloom.Set(Keccak.EmptyTreeHash.Bytes);
            txReceipt.ContractAddress = TestItem.AddressA;
            txReceipt.Sender = TestItem.AddressB;
            txReceipt.Recipient = TestItem.AddressC;
            txReceipt.GasUsed = 100;
            txReceipt.GasUsedTotal = 1000;
            txReceipt.Index = 2;
            txReceipt.PostTransactionState = TestItem.KeccakH;
            txReceipt.StatusCode = 1;
            txReceipt.TxType = TxType.AccessList;

            ReceiptStorageDecoder decoder = new();
            Rlp rlp = decoder.Encode(txReceipt, RlpBehaviors.Storage | RlpBehaviors.Eip658Receipts);
            TxReceipt deserialized = decoder.Decode(rlp.Bytes.AsRlpStream(), RlpBehaviors.Storage | RlpBehaviors.Eip658Receipts);

            AssertStorageReceipt(txReceipt, deserialized);
        }

        [Test]
        public void Netty_and_rlp_array_encoding_should_be_the_same()
        {
            TxReceipt[] receipts = new[]
            {
                Build.A.Receipt.WithAllFieldsFilled.TestObject,
                Build.A.Receipt.WithAllFieldsFilled.TestObject
            };

            ReceiptStorageDecoder decoder = new();
            Rlp rlp = decoder.Encode(receipts);
            using (NettyRlpStream nettyRlpStream = decoder.EncodeToNewNettyStream(receipts))
            {
                byte[] nettyBytes = nettyRlpStream.AsSpan().ToArray();
                nettyBytes.Should().BeEquivalentTo(rlp.Bytes);
            }
        }

        public static IEnumerable<(TxReceipt, string)> TestCaseSource()
        {
            Bloom bloom = new();
            bloom.Set(Keccak.EmptyTreeHash.Bytes);
            yield return (Build.A.Receipt.TestObject, "basic with defaults");
            yield return (Build.A.Receipt.WithBloom(bloom).WithGasUsedTotal(1000).WithState(TestItem.KeccakH).TestObject, "basic");
            yield return (Build.A.Receipt.WithBloom(bloom).WithGasUsedTotal(500).WithState(TestItem.KeccakA).WithTxType(TxType.AccessList).TestObject, "access list");
            yield return (Build.A.Receipt.WithBloom(bloom).WithGasUsedTotal(100).WithState(TestItem.KeccakH).WithTxType(TxType.EIP1559).TestObject, "eip 1559");
        }

        [TestCaseSource(nameof(TestCaseSource))]
        public void Can_do_roundtrip_with_storage_receipt((TxReceipt TxReceipt, string Description) testCase)
        {
            TxReceipt txReceipt = testCase.TxReceipt;

            ReceiptStorageDecoder decoder = new();
            Rlp rlp = decoder.Encode(txReceipt, RlpBehaviors.Storage | RlpBehaviors.Eip658Receipts);
            TxReceipt deserialized = decoder.Decode(rlp.Bytes.AsRlpStream(), RlpBehaviors.Storage | RlpBehaviors.Eip658Receipts);

            AssertStorageReceipt(txReceipt, deserialized);
        }

        [TestCaseSource(nameof(TestCaseSource))]
        public void Can_do_roundtrip_with_receipt_message((TxReceipt TxReceipt, string Description) testCase)
        {
            TxReceipt txReceipt = testCase.TxReceipt;

            ReceiptMessageDecoder decoder = new();

            byte[] rlpStreamResult = decoder.EncodeNew(txReceipt);
            TxReceipt deserialized = decoder.Decode(new RlpStream(rlpStreamResult));

            AssertMessageReceipt(txReceipt, deserialized);
        }

        private void AssertMessageReceipt(TxReceipt txReceipt, TxReceipt deserialized)
        {
            Assert.That(deserialized.Bloom, Is.EqualTo(txReceipt.Bloom), "bloom");
            Assert.That(deserialized.GasUsedTotal, Is.EqualTo(txReceipt.GasUsedTotal), "gas used total");
            Assert.That(deserialized.PostTransactionState, Is.EqualTo(txReceipt.PostTransactionState), "post transaction state");
            Assert.That(deserialized.StatusCode, Is.EqualTo(txReceipt.StatusCode), "status");
            Assert.That(deserialized.TxType, Is.EqualTo(txReceipt.TxType), "type");
        }

        private void AssertStorageReceipt(TxReceipt txReceipt, TxReceipt deserialized)
        {
            Assert.That(deserialized.TxType, Is.EqualTo(txReceipt.TxType), "tx type");
            Assert.That(deserialized.BlockHash, Is.EqualTo(txReceipt.BlockHash), "block hash");
            Assert.That(deserialized.BlockNumber, Is.EqualTo(txReceipt.BlockNumber), "block number");
            Assert.That(deserialized.Index, Is.EqualTo(txReceipt.Index), "index");
            Assert.That(deserialized.ContractAddress, Is.EqualTo(txReceipt.ContractAddress), "contract");
            Assert.That(deserialized.Sender, Is.EqualTo(txReceipt.Sender), "sender");
            Assert.That(deserialized.GasUsed, Is.EqualTo(txReceipt.GasUsed), "gas used");
            Assert.That(deserialized.GasUsedTotal, Is.EqualTo(txReceipt.GasUsedTotal), "gas used total");
            Assert.That(deserialized.Bloom, Is.EqualTo(txReceipt.Bloom), "bloom");
            Assert.That(deserialized.Recipient, Is.EqualTo(txReceipt.Recipient), "recipient");
            Assert.That(deserialized.StatusCode, Is.EqualTo(txReceipt.StatusCode), "status");
        }
    }
}

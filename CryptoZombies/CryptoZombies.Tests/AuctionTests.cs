using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stratis.SmartContracts;
using System.Collections.Generic;
using System.Text;

namespace CryptoZombies.Tests
{
    [TestClass]
    public class AuctionTests
    {
        private static readonly Address TestAddress = (Address)"mipcBbFg9gMiCh81Kj8tqqdgoZub1ZJRfn";
        private TestSmartContractState smartContractState;
        private const ulong Balance = 0;
        private const ulong GasLimit = 10000;
        private const ulong Value = 0;

        [TestInitialize]
        public void Initialize()
        {
            var block = new TestBlock
            {
                Coinbase = TestAddress,
                Number = 1
            };
            var message = new TestMessage
            {
                ContractAddress = TestAddress,
                GasLimit = (Gas)GasLimit,
                Sender = TestAddress,
                Value = Value
            };
            var getBalance = new Func<ulong>(() => Balance);
            var persistentState = new TestPersistentState();
            var internalHashHelper = new TestInternalHashHelper();

            this.smartContractState = new TestSmartContractState(
                block,
                message,
                persistentState,
                null,
                null,
                getBalance,
                internalHashHelper
            );
        }

        [TestMethod]
        public void TestConstruction()
        {
            const ulong duration = 20;
            var contract = new Auction(smartContractState, duration);
            Assert.AreEqual(TestAddress, smartContractState.PersistentState.GetObject<Address>("Owner"));
            Assert.IsFalse(smartContractState.PersistentState.GetObject<bool>("HasEnded"));
            Assert.AreEqual(duration + smartContractState.Block.Number, smartContractState.PersistentState.GetObject<ulong>("EndBlock"));
        }

        [TestMethod]
        public void TestBidding()
        {
            const ulong duration = 20;
            var contract = new Auction(smartContractState, duration);

            Assert.IsNull(smartContractState.PersistentState.GetObject<Address>("HighestBidder").Value);
            Assert.AreEqual(0uL, smartContractState.PersistentState.GetObject<ulong>("HighestBid"));

            ((TestMessage)smartContractState.Message).Value = 100;
            contract.Bid();
            Assert.IsNotNull(smartContractState.PersistentState.GetObject<Address>("HighestBidder").Value);
            Assert.AreEqual(100uL, smartContractState.PersistentState.GetObject<ulong>("HighestBid"));

            ((TestMessage)smartContractState.Message).Value = 90;
            Assert.ThrowsException<Exception>(() => contract.Bid());
        }

        [TestMethod]
        public void TestZombieLists()
        {
            const string zombNameKey = "ZombieNames";
            const string zombDnaKey = "ZombieDNAs";

            const string zombNameValue = "ferdeen";

            var contract = new ZombieFactory(smartContractState);

            Assert.AreEqual(0uL, smartContractState.PersistentState.GetList<string>(zombNameKey).Count);
            Assert.AreEqual(0uL, smartContractState.PersistentState.GetList<uint>(zombDnaKey).Count);

            uint id = contract.CreateRandomZombie(zombNameValue);

            Assert.AreEqual(1uL, smartContractState.PersistentState.GetList<string>(zombNameKey).Count);
            Assert.AreEqual(zombNameValue, smartContractState.PersistentState.GetList<string>(zombNameKey).Get(0));

            Assert.AreEqual(1uL, smartContractState.PersistentState.GetList<uint>(zombDnaKey).Count);
            Assert.AreEqual(id, smartContractState.PersistentState.GetList<uint>(zombDnaKey).Get(0));
        }
    }

    public class TestSmartContractState : ISmartContractState
    {
        public TestSmartContractState(
            IBlock block,
            IMessage message,
            IPersistentState persistentState,
            IGasMeter gasMeter,
            IInternalTransactionExecutor transactionExecutor,
            Func<ulong> getBalance,
            IInternalHashHelper hashHelper)
        {
            this.Block = block;
            this.Message = message;
            this.PersistentState = persistentState;
            this.GasMeter = gasMeter;
            this.InternalTransactionExecutor = transactionExecutor;
            this.GetBalance = getBalance;
            this.InternalHashHelper = hashHelper;
        }

        public IBlock Block { get; }
        public IMessage Message { get; }
        public IPersistentState PersistentState { get; }
        public IGasMeter GasMeter { get; }
        public IInternalTransactionExecutor InternalTransactionExecutor { get; }
        public Func<ulong> GetBalance { get; }
        public IInternalHashHelper InternalHashHelper { get; }
    }

    public class TestBlock : IBlock
    {
        public Address Coinbase { get; set; }

        public ulong Number { get; set; }
    }

    public class TestMessage : IMessage
    {
        public Address ContractAddress { get; set; }

        public Address Sender { get; set; }

        public Gas GasLimit { get; set; }

        public ulong Value { get; set; }
    }

    public class TestInternalHashHelper : IInternalHashHelper
    {
        public byte[] Keccak256(byte[] toHash)
        {
            return Encoding.ASCII.GetBytes("707d7a2f11266609dac44fcded84b1d835d3439bd66c66e92b814a8e89bb7e3b");
        }
    }

    public class TestPersistentState : IPersistentState
    {
        private Dictionary<string, object> objects = new Dictionary<string, object>();

        private readonly ISmartContractList<string> zombieNames = new TestSmartContractList<string>();
        private readonly ISmartContractList<uint> zombieDNAs = new TestSmartContractList<uint>();

        public ISmartContractList<T> GetList<T>(string name)
        {
            if (name == "ZombieNames")
                return zombieNames as ISmartContractList<T>;

            return zombieDNAs as ISmartContractList<T>; 
        }

        public ISmartContractMapping<V> GetMapping<V>(string name)
        {
            throw new NotImplementedException();
        }

        public T GetObject<T>(string key)
        {
            if (objects.ContainsKey(key))
                return (T)objects[key];

            return default(T);
        }

        public void SetObject<T>(string key, T obj)
        {
            objects[key] = obj;
        }
    }

    public class TestSmartContractList<T> : ISmartContractList<T>
    {
        private readonly List<T> internalList;

        public TestSmartContractList()
        {
            this.internalList = new List<T>();
        }
        public void Add(T item)
        {
            this.internalList.Add(item);
        }

        public T Get(uint index)
        {
            return this.internalList[(int)index];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.internalList.GetEnumerator();
        }

        public uint Count => (uint)this.internalList.Count;
    }
}

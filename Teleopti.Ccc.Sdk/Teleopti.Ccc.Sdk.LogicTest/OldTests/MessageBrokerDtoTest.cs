using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class MessageBrokerDtoTest
    {
        private MessageBrokerDto _target;
        private string           _connetionStrig= "test connection sttring";

        [SetUp]
        public void Setup()
        {
            _target = new MessageBrokerDto();

            _target.Server =  "172.22.1.31";;
            _target.Port = 9080;
            _target.Threads = 1;
            _target.ConnectionString = @"Data Source=antonov\sql2005dev;Initial Catalog=Raptor_Messaging;Persist Security Info=True;User ID=sa;Password=cadadi"; 
            _target.GeneralThreadPoolThreads = 3;
            _target.DatabaseThreadPoolThreads = 3;
            _target.ReceiptThreadPoolThreads = 3;
            _target.HeartbeatThreadPoolThreads = 1;
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual( "172.22.1.31",_target.Server);
            Assert.AreEqual(9080, _target.Port);
            Assert.AreEqual(1, _target.Threads);
            Assert.AreEqual(_connetionStrig, _connetionStrig);
            Assert.AreEqual(3, _target.GeneralThreadPoolThreads);
            Assert.AreEqual(3, _target.DatabaseThreadPoolThreads);
            Assert.AreEqual(3, _target.ReceiptThreadPoolThreads);
            Assert.AreEqual(1, _target.HeartbeatThreadPoolThreads);
        }

        [Test]
        public void VerifyCanSetProperties()
        {
            _target.Server = "172.22.1.31";
            Assert.AreEqual("172.22.1.31", _target.Server);
            _target.Port = 9080;
            Assert.AreEqual(9080, _target.Port);
            _target.Threads = 1;
            Assert.AreEqual(1, _target.Threads);
            _target.ConnectionString = _connetionStrig;
            Assert.AreEqual(_connetionStrig, _target.ConnectionString);
            _target.GeneralThreadPoolThreads = 3;
            Assert.AreEqual(3, _target.GeneralThreadPoolThreads);
            _target.DatabaseThreadPoolThreads = 3;
            Assert.AreEqual(3, _target.DatabaseThreadPoolThreads);
            _target.ReceiptThreadPoolThreads = 3;
            Assert.AreEqual(3, _target.ReceiptThreadPoolThreads);
            _target.HeartbeatThreadPoolThreads = 1;
            Assert.AreEqual(1, _target.HeartbeatThreadPoolThreads);
        }
    }
}
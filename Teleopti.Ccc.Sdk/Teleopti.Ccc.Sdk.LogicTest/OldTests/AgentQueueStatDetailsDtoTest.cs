using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class AgentQueueStatDetailsDtoTest
    {
        private AgentQueueStatDetailsDto _target;
        private long _afterContractWorkTime;
        private int _answeredContacts;
        private long _averageTalkTime;
        private string _queueName;
        private long _totalHandlingTime;

        [SetUp]
        public void Setup()
        {
            _target = new AgentQueueStatDetailsDto();
            _afterContractWorkTime = TimeSpan.FromMinutes(5).Ticks;
            _answeredContacts = 76;
            _averageTalkTime = TimeSpan.FromMinutes(3).Ticks;
            _queueName = "QueueName";
            _totalHandlingTime = TimeSpan.FromMinutes(35).Ticks;
        }

        [Test]
        public void VerifyCanCreate()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifySetAndGetProperties()
        {
            _target.AfterContactWorkTime = _afterContractWorkTime;
            _target.AnsweredContacts = _answeredContacts;
            _target.AverageTalkTime = _averageTalkTime;
            _target.QueueName = _queueName;
            _target.AverageHandlingTime = _totalHandlingTime;

            Assert.AreEqual(_afterContractWorkTime, _target.AfterContactWorkTime);
            Assert.AreEqual(_answeredContacts, _target.AnsweredContacts);
            Assert.AreEqual(_averageTalkTime, _target.AverageTalkTime);
            Assert.AreEqual(_queueName, _target.QueueName);
            Assert.AreEqual(_totalHandlingTime, _target.AverageHandlingTime);
        }
    }
}

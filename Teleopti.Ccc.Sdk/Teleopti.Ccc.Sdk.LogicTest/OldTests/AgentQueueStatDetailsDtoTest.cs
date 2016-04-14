using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class AgentQueueStatDetailsDtoTest
    {
        [Test]
        public void VerifyCanCreate()
        {
			var _target = new AgentQueueStatDetailsDto();
			Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifySetAndGetProperties()
        {
			var afterContractWorkTime = TimeSpan.FromMinutes(5).Ticks;
			const int answeredContacts = 76;
			var averageTalkTime = TimeSpan.FromMinutes(3).Ticks;
			const string queueName = "QueueName";
			var totalHandlingTime = TimeSpan.FromMinutes(35).Ticks;

			var target = new AgentQueueStatDetailsDto();
			target.AfterContactWorkTime = afterContractWorkTime;
            target.AnsweredContacts = answeredContacts;
            target.AverageTalkTime = averageTalkTime;
            target.QueueName = queueName;
            target.AverageHandlingTime = totalHandlingTime;

            Assert.AreEqual(afterContractWorkTime, target.AfterContactWorkTime);
            Assert.AreEqual(answeredContacts, target.AnsweredContacts);
            Assert.AreEqual(averageTalkTime, target.AverageTalkTime);
            Assert.AreEqual(queueName, target.QueueName);
            Assert.AreEqual(totalHandlingTime, target.AverageHandlingTime);
        }
    }
}

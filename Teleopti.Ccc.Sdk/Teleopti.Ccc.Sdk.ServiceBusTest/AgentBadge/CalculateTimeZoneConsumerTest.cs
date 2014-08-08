using System;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	class CalculateTimeZoneConsumerTest
	{
		private MockRepository mocks;
		private IServiceBus serviceBus;
		private CalculateTimeZoneConsumer target;
		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			serviceBus = mocks.DynamicMock<IServiceBus>();
			target = new CalculateTimeZoneConsumer(serviceBus);
		}

		[Test]
		public void IsConsumerCalled()
		{
			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var message = new CalculateTimeZoneMessage();
			message.Timestamp = DateTime.Now;
			message.BusinessUnitId = bussinessUnit.Id.GetValueOrDefault();
			message.TimeZone = TimeZoneInfo.Utc;

			mocks.ReplayAll();
			target.Consume(message);
			mocks.VerifyAll();

		}
	}
}

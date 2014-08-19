using System;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	[TestFixture]
	public class CalculateTimeZoneConsumerTest
	{
		private IServiceBus serviceBus;
		private CalculateTimeZoneConsumer target;
		private INow now;

		[SetUp]
		public void Setup()
		{
			serviceBus = MockRepository.GenerateMock<IServiceBus>();
			now = MockRepository.GenerateMock<INow>();
			target = new CalculateTimeZoneConsumer(serviceBus, now);
		}

		[Test]
		public void ShouldSendCalculateBadgeMessageAtRightTime()
		{
			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var message = new CalculateTimeZoneMessage
			{
				Timestamp = DateTime.Now,
				BusinessUnitId = bussinessUnit.Id.GetValueOrDefault(),
				TimeZoneCode = TimeZoneInfo.Utc.Id
			};
			var timezone = TimeZoneInfo.FindSystemTimeZoneById(message.TimeZoneCode);
			var uTCToday = new DateTime(2014, 8, 8);
			var today = TimeZoneInfo.ConvertTime(uTCToday, TimeZoneInfo.Utc, TimeZoneInfo.Local);
			var todayForGivenTimeZone = TimeZoneInfo.ConvertTime(today, TimeZoneInfo.Local, timezone);
			now.Stub(x => x.UtcDateTime()).Return(today);
			var expectedCalculationDate = todayForGivenTimeZone.AddDays(-1);
			target.Consume(message);

			serviceBus.AssertWasCalled(x => x.Send(new object()),
				o =>
					o.Constraints(
						Rhino.Mocks.Constraints.Is.Matching(new Predicate<object[]>(m =>
						{
							var msg = ((CalculateBadgeMessage) m[0]);
							return msg.TimeZoneCode == TimeZoneInfo.Utc.Id && msg.CalculationDate == expectedCalculationDate.Date;
						}))));

		}
	}
}

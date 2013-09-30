using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Rta
{
	[TestFixture]
	public class NotifyRtaDeciderTest
	{
		private DateTimePeriod period;
		private DateTime nearestLayerStartDateTime;

		[SetUp]
		public void Setup()
		{
			nearestLayerStartDateTime = DateTime.UtcNow;
		}
		
		[Test]
		public void ChangeIs_FromYesterday_ShouldNotSend()
		{
			period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1));
			var result = NotifyRtaDecider.ShouldSendMessage(period, nearestLayerStartDateTime);
			result.Should().Be.False();
		}

		[Test]
		public void ChangeIs_RightNow_ShouldSend()
		{
			period = new DateTimePeriod(DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddMinutes(10));
			var result = NotifyRtaDecider.ShouldSendMessage(period, nearestLayerStartDateTime);
			result.Should().Be.True();
		}

		[Test]
		public void ChangeIs_ThreeDaysOneHourAfterNextActivityStartTime_ShouldNotSend()
		{
			nearestLayerStartDateTime = DateTime.UtcNow.AddDays(14);
			period = new DateTimePeriod(nearestLayerStartDateTime.AddDays(3).AddHours(1), nearestLayerStartDateTime.AddDays(4));
			var result = NotifyRtaDecider.ShouldSendMessage(period, nearestLayerStartDateTime);
			result.Should().Be.False();
		}

		[Test]
		public void ChangeIs_TwoDaysOneHourBeforeNextActivityStartTime_ShouldNotSend()
		{
			nearestLayerStartDateTime = DateTime.UtcNow.AddDays(14);
			period = new DateTimePeriod(nearestLayerStartDateTime.AddDays(-3), nearestLayerStartDateTime.AddDays(-2).AddHours(-1));
			var result = NotifyRtaDecider.ShouldSendMessage(period, nearestLayerStartDateTime);
			result.Should().Be.False();
		}
	}
}
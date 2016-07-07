using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.PerformanceTool
{
	[TestFixture]
	public class PerformanceCounterTest
	{
		[Test]
		public void ShouldEnableWhenLimitIsGreaterThanZero()
		{
			var target = new PerformanceCounter(null, null, new NewtonsoftJsonSerializer()) { Limit = 1 };
			target.IsEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldSendMessageWhenCounterReachesLimit()
		{
			var messageSender = MockRepository.GenerateStub<IMessageSender>(); 
			var now = MockRepository.GenerateMock<INow>();
			now.Stub(x => x.UtcDateTime()).Return(new DateTime());
			var target = new PerformanceCounter(messageSender, now, new NewtonsoftJsonSerializer()) { Limit = 1 };
			target.Count();
			messageSender.AssertWasCalled(x => x.Send(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldRecordTimestampForFirstCount()
		{
			var timestamp = new DateTime(2015, 1, 1, 1, 1, 1, 1);
			var now = MockRepository.GenerateMock<INow>();
			now.Stub(x => x.UtcDateTime()).Return(timestamp);
			var target = new PerformanceCounter(null, now, new NewtonsoftJsonSerializer()) { Limit = 2 };
			target.Count();
			target.FirstTimestamp.Should().Be(timestamp);
		}

		[Test]
		public void ShouldRecordTimestampForLastCount()
		{
			var timestamp = new DateTime(2015, 1, 1, 1, 1, 1, 1);
			var now = MockRepository.GenerateMock<INow>();
			var messageSender = MockRepository.GenerateStub<IMessageSender>(); 
			now.Stub(x => x.UtcDateTime()).Return(timestamp);
			var target = new PerformanceCounter(messageSender, now, new NewtonsoftJsonSerializer()) { Limit = 1 };
			target.Count();
			target.LastTimestamp.Should().Be(timestamp);
		}
	}
}

using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ETL;
using Teleopti.Ccc.Domain.Status;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Status
{
	[DomainTest]
	public class EtlCheckerTest
	{
		public EtlChecker Target;
		public FakeTimeSinceLastEtlPing TimeSinceLastEtlPing;

		[TestCase(-5, ExpectedResult = true)]
		[TestCase(0, ExpectedResult = true)]
		[TestCase(15, ExpectedResult = true)]
		[TestCase(29, ExpectedResult = true)] //we allow a 30s delay (plus "EtlTickFrequency")
		[TestCase(31, ExpectedResult = false)]
		[TestCase(100, ExpectedResult = false)]
		public bool ShouldSetSuccessCorrectly(int secondsPassedSinceAPingShouldHaveHappened)
		{
			TimeSinceLastEtlPing.SetTime(TimeSpan.FromSeconds(EtlTickFrequency.Value.TotalSeconds + secondsPassedSinceAPingShouldHaveHappened));

			return Target.Execute().Success;
		}

		[Test]
		public void ShouldSetMessageWhenSuccess()
		{
			var lastPingInSeconds = EtlTickFrequency.Value.TotalSeconds - 1;
			TimeSinceLastEtlPing.SetTime(TimeSpan.FromSeconds(lastPingInSeconds));

			Target.Execute().Output
				.Should().Be.EqualTo(string.Format(EtlChecker.Message, lastPingInSeconds));
		}
		
		[Test]
		public void ShouldSetMessageWhenFailure()
		{
			var lastPingInSeconds = EtlTickFrequency.Value.TotalSeconds + 100;
			TimeSinceLastEtlPing.SetTime(TimeSpan.FromSeconds(lastPingInSeconds));

			Target.Execute().Output
				.Should().Be.EqualTo(string.Format(EtlChecker.Message, lastPingInSeconds));
		}

		[Test]
		public void ShouldSetMessageWhenDeadForLongTime()
		{
			var lastPing = TimeSpan.FromDays(10);
			TimeSinceLastEtlPing.SetTime(lastPing);

			Target.Execute().Output
				.Should().Be.EqualTo(EtlChecker.MessageWhenDeadForLongTime);
		}
	}
}
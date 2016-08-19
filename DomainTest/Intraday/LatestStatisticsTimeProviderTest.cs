using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class LatestStatisticsTimeProviderTest
	{
		public LatestStatisticsTimeProvider Target;
		public FakeLatestStatisticsIntervalIdLoader LatestStatisticsIntervalIdLoader;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;

		[Test]
		public void ShouldReturnIntervalTime()
		{
			IntervalLengthFetcher.Has(15);
			LatestStatisticsIntervalIdLoader.Has(95);

			var expectedTime = DateTime.MinValue.AddMinutes(95*15);
			Target.Get(new [] {Guid.NewGuid()}).Should().Be.EqualTo(expectedTime);
		}

		[Test]
		public void ShouldReturnNullWhenNoStatistics()
		{
			IntervalLengthFetcher.Has(15);
			LatestStatisticsIntervalIdLoader.Has(null);

			Target.Get(new[] { Guid.NewGuid() }).Should().Be.EqualTo(null);
		}
	}
}
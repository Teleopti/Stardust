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

			var expectedTime = new LatestStatitsticsTimeModel
			{
				StartTime = DateTime.MinValue.AddMinutes(95 * 15),
				EndTime = DateTime.MinValue.AddMinutes(96 * 15)
			};
			var result = Target.Get(new [] {Guid.NewGuid()});
			
			result.StartTime.Should().Be.EqualTo(expectedTime.StartTime);
			result.EndTime.Should().Be.EqualTo(expectedTime.EndTime);
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
using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Intraday.UnitTests
{
	[DomainTest]
	public class LatestStatisticsTimeProviderTest
	{
		public ILatestStatisticsTimeProvider Target;
		public FakeLatestStatisticsIntervalIdLoader LatestStatisticsIntervalIdLoader;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public MutableNow Now;

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

		[Test]
		public void ShouldReturnIntervalTimeForSpecifiedDate()
		{
			IntervalLengthFetcher.Has(15);
			LatestStatisticsIntervalIdLoader.Has(95);
			LatestStatisticsIntervalIdLoader.Has(new DateOnly(2017, 1, 1));

			var expectedTime = new LatestStatitsticsTimeModel
			{
				StartTime = DateTime.MinValue.AddMinutes(95 * 15),
				EndTime = DateTime.MinValue.AddMinutes(96 * 15)
			};

			var result = Target.Get(new[] {Guid.NewGuid()}, new DateTime(2017, 1, 1));

			result.StartTime.Should().Be.EqualTo(expectedTime.StartTime);
			result.EndTime.Should().Be.EqualTo(expectedTime.EndTime);
		}

		[Test]
		public void ShouldReturnNullForInvalidSpecifiedDate()
		{
			IntervalLengthFetcher.Has(15);
			LatestStatisticsIntervalIdLoader.Has(95);
			LatestStatisticsIntervalIdLoader.Has(new DateOnly(2016, 1, 1));

			var result = Target.Get(new[] { Guid.NewGuid() }, new DateTime(2017, 1, 1));

			result.Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnIntervalTimeForSpecifiedDayOffset()
		{
			IntervalLengthFetcher.Has(15);
			LatestStatisticsIntervalIdLoader.Has(95);
			LatestStatisticsIntervalIdLoader.Has(new DateOnly(2017, 1, 1));
			Now.Is(new DateTime(2017, 1, 2));

			var expectedTime = new LatestStatitsticsTimeModel
			{
				StartTime = DateTime.MinValue.AddMinutes(95 * 15),
				EndTime = DateTime.MinValue.AddMinutes(96 * 15)
			};

			var result = Target.Get(new[] { Guid.NewGuid() }, -1);

			result.StartTime.Should().Be.EqualTo(expectedTime.StartTime);
			result.EndTime.Should().Be.EqualTo(expectedTime.EndTime);
		}
	}
}
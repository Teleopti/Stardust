using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[Category("LongRunning")]
	[TestFixture]
	[AnalyticsDatabaseTest]
	public class AnalyticsIntervalRepositoryTest
	{
		public IAnalyticsIntervalRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private AnalyticsDataFactory analyticsDataFactory;

		[SetUp]
		public void Setup()
		{
			analyticsDataFactory = new AnalyticsDataFactory();
			var intervals = new QuarterOfAnHourInterval();
			analyticsDataFactory.Setup(intervals);
			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldGetIntervalsPerDay()
		{
			var intervalsPerDay = WithAnalyticsUnitOfWork.Get(() => Target.IntervalsPerDay());
			intervalsPerDay.Should().Be.EqualTo(96);
		}

		[Test]
		public void ShouldGetMaxInterval()
		{
			var maxInterval = WithAnalyticsUnitOfWork.Get(() => Target.MaxInterval());
			maxInterval.IntervalId.Should().Be.EqualTo(95);
		}

		[Test]
		public void ShouldGetAllInterval()
		{
			var maxInterval = WithAnalyticsUnitOfWork.Get(() => Target.GetAll());
			maxInterval.Count.Should().Be.EqualTo(96);
		}
	}
}
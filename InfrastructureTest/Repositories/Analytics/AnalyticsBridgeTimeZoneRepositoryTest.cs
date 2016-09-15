using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[Category("LongRunning")]
	[TestFixture]
	[AnalyticsDatabaseTest]
	public class AnalyticsBridgeTimeZoneRepositoryTest
	{
		public IAnalyticsBridgeTimeZoneRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private AnalyticsDataFactory analyticsDataFactory;

		[SetUp]
		public void Setup()
		{
			analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new CurrentWeekDates());
			analyticsDataFactory.Setup(new QuarterOfAnHourInterval());
			analyticsDataFactory.Setup(new UtcAndCetTimeZones());
			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldAddBridgesAndGetByTimeZone()
		{
			var expected1 = new AnalyticsBridgeTimeZone(1, 1, 0)
				{
					LocalIntervalId = 1,
					LocalDateId = 1
				};
			var expected2 = new AnalyticsBridgeTimeZone(2, 1, 0)
			{
				LocalIntervalId = 1,
				LocalDateId = 2
			};

			var maxDateId = WithAnalyticsUnitOfWork.Get(() => Target.GetMaxDateForTimeZone(0));
			maxDateId.Should().Be.EqualTo(0);
			WithAnalyticsUnitOfWork.Do(() => Target.Save(new [] { expected1, expected2 }));

			var utcBridges = WithAnalyticsUnitOfWork.Get(() => Target.GetBridgesPartial(0, 0));
			utcBridges.Count.Should().Be.EqualTo(2);
			utcBridges.First().DateId.Should().Be.EqualTo(expected1.DateId);
			utcBridges.First().IntervalId.Should().Be.EqualTo(expected1.IntervalId);
			utcBridges.First().TimeZoneId.Should().Be.EqualTo(expected1.TimeZoneId);

			maxDateId = WithAnalyticsUnitOfWork.Get(() => Target.GetMaxDateForTimeZone(0));
			maxDateId.Should().Be.EqualTo(expected2.DateId);
		}

	}
}
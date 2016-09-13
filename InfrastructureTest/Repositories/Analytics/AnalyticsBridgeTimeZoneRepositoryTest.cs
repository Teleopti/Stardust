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
			var expected = new AnalyticsBridgeTimeZone(1, 1, 0)
				{
					LocalIntervalId = 1,
					LocalDateId = 1
				};
			WithAnalyticsUnitOfWork.Do(() => Target.Save(new [] { expected }));

			var utcBridges = WithAnalyticsUnitOfWork.Get(() => Target.GetBridgesPartial(0));
			utcBridges.Count.Should().Be.EqualTo(1);
			utcBridges.First().DateId.Should().Be.EqualTo(expected.DateId);
			utcBridges.First().IntervalId.Should().Be.EqualTo(expected.IntervalId);
			utcBridges.First().TimeZoneId.Should().Be.EqualTo(expected.TimeZoneId);
		}

	}
}
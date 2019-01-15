using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	public class IntervalLengthFetcherTest
	{
		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();
		}

		[Test]
		public void ShouldGetAnalyticsIntervalLength()
		{
			var sysConfig = new SysConfiguration("IntervalLengthMinutes", "30");

			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(sysConfig);
			analyticsDataFactory.Persist();


			var target = new IntervalLengthFetcher(CurrentDataSource.Make());
			target.GetIntervalLength().Should().Be.EqualTo(30);
		}

		[TearDown]
		public void TearDown()
		{
			SetupFixtureForAssembly.EndTest();
		}
	}
}
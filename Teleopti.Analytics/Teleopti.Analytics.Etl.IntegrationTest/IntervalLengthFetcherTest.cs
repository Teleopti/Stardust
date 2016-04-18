using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	[Category("LongRunning")]
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
			target.IntervalLength.Should().Be.EqualTo(30);
		}

		[TearDown]
		public void TearDown()
		{
			SetupFixtureForAssembly.EndTest();
		}
	}
}
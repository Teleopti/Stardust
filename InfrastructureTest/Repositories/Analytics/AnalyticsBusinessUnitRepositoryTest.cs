using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsBusinessUnitRepositoryTest
	{
		ICurrentDataSource currentDataSource;
		private BusinessUnit businessUnitInAnalytics;
		private ExistingDatasources datasourceInAnalytics;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			datasourceInAnalytics = new ExistingDatasources(timeZones);
			businessUnitInAnalytics = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasourceInAnalytics);

			analyticsDataFactory.Setup(businessUnitInAnalytics);
			analyticsDataFactory.Persist();
			currentDataSource = CurrentDataSource.Make();
		}

		[Test]
		public void ShouldGetBusinessUnitByCode()
		{
			var target = new AnalyticsBusinessUnitRepository(currentDataSource);
			var result = target.Get(BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault());

			result.BusinessUnitId.Should().Be.EqualTo(businessUnitInAnalytics.BusinessUnitId);
			result.DatasourceId.Should().Be.EqualTo(datasourceInAnalytics.RaptorDefaultDatasourceId);
		}

		[Test]
		public void ShouldReturnNullForNotExistingBusinessUnit()
		{
			var target = new AnalyticsBusinessUnitRepository(currentDataSource);
			var result = target.Get(Guid.NewGuid());

			result.Should().Be.Null();
		}
	}
}
using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{

	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsUnitOfWorkTest]
	public class AnalyticsBusinessUnitRepositoryTest
	{
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public IAnalyticsBusinessUnitRepository Target;
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
		}

		[Test]
		public void ShouldGetBusinessUnitByCode()
		{
			var result = Target.Get(BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault());

			result.BusinessUnitId.Should().Be.EqualTo(businessUnitInAnalytics.BusinessUnitId);
			result.DatasourceId.Should().Be.EqualTo(datasourceInAnalytics.RaptorDefaultDatasourceId);
		}

		[Test]
		public void ShouldReturnNullForNotExistingBusinessUnit()
		{
			var result = Target.Get(Guid.NewGuid());

			result.Should().Be.Null();
		}
	}
}
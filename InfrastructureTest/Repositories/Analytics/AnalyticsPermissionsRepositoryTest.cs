using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
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
	public class AnalyticsPermissionsRepositoryTest
	{
		ICurrentDataSource currentDataSource;
		private ExistingDatasources datasource;
		private BusinessUnit businessUnit;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			datasource = new ExistingDatasources(timeZones);
			businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource);

			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Persist();
			currentDataSource = CurrentDataSource.Make();
		}

		[Test]
		public void GetPermissionsForPerson_ShouldReturnPermissionsForPerson()
		{
			var personId = Guid.NewGuid();

			var target = new AnalyticsPermissionRepository(currentDataSource);
			var permissionForPerson = new AnalyticsPermission
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceId = datasource.RaptorDefaultDatasourceId,
				PersonCode = personId,
				MyOwn = false,
				TeamId = 10,
				ReportId = Guid.NewGuid(),
				DatasourceUpdateDate = DateTime.Now
			};

			var permissionNotForPerson = new AnalyticsPermission
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceId = datasource.RaptorDefaultDatasourceId,
				PersonCode = Guid.NewGuid(),
				MyOwn = false,
				TeamId = 10,
				ReportId = Guid.NewGuid(),
				DatasourceUpdateDate = DateTime.Now
			};
			target.InsertPermissions(new [] { permissionForPerson, permissionNotForPerson });

			var permissions = target.GetPermissionsForPerson(personId);

			permissions.Should().Not.Be.Empty();
			permissions.Where(x => x.Equals(permissionForPerson)).Should().Not.Be.Empty();
			permissions.Where(x => x.Equals(permissionNotForPerson)).Should().Be.Empty();
		}

		[Test]
		public void DeletePermissions_ShouldNotBeReturned()
		{
			var personId = Guid.NewGuid();

			var target = new AnalyticsPermissionRepository(currentDataSource);
			var permissionForPerson = new AnalyticsPermission
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceId = datasource.RaptorDefaultDatasourceId,
				PersonCode = personId,
				MyOwn = false,
				TeamId = 10,
				ReportId = Guid.NewGuid(),
				DatasourceUpdateDate = DateTime.Now
			};

			var permissionNotForPerson = new AnalyticsPermission
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceId = datasource.RaptorDefaultDatasourceId,
				PersonCode = Guid.NewGuid(),
				MyOwn = false,
				TeamId = 10,
				ReportId = Guid.NewGuid(),
				DatasourceUpdateDate = DateTime.Now
			};
			target.InsertPermissions(new[] { permissionForPerson, permissionNotForPerson });
			permissionForPerson.DatasourceUpdateDate = DateTime.Today;
			target.DeletePermissions(new[] { permissionForPerson });
			var permissions = target.GetPermissionsForPerson(personId);

			permissions.Should().Be.Empty();
		}
	}
}
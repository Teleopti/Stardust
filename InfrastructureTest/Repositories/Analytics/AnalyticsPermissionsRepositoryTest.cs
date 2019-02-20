using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[AnalyticsDatabaseTest]
	public class AnalyticsPermissionsRepositoryTest
	{
		public IAnalyticsPermissionRepository Target;
		private ExistingDatasources datasource;
		private BusinessUnit businessUnit;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			datasource = new ExistingDatasources(timeZones);
			businessUnit = new BusinessUnit(BusinessUnitUsedInTests.BusinessUnit, datasource);
			
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Persist();
		}

		[Test]
		public void GetPermissionsForPerson_ShouldReturnPermissionsForPerson()
		{
			var personId = Guid.NewGuid();
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

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.InsertPermissions(new[] { permissionForPerson, permissionNotForPerson });
			});

			var permissions = WithAnalyticsUnitOfWork.Get(() => Target.GetPermissionsForPerson(personId, businessUnit.BusinessUnitId));
			
			permissions.Should().Not.Be.Empty();
			permissions.Where(x => x.Equals(permissionForPerson)).Should().Not.Be.Empty();
			permissions.Where(x => x.Equals(permissionNotForPerson)).Should().Be.Empty();
		}

		[Test]
		public void GetPermissionsForPerson_ShouldNotReturnPermissionsForPersonInDifferentBusinessUnit()
		{
			var personId = Guid.NewGuid();
			var permissionForPersonInOtherBusinessUnit = new AnalyticsPermission
			{
				BusinessUnitId = businessUnit.BusinessUnitId+1,
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

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.InsertPermissions(new[] { permissionForPersonInOtherBusinessUnit, permissionNotForPerson });
			});

			var permissions = WithAnalyticsUnitOfWork.Get(() => Target.GetPermissionsForPerson(personId, businessUnit.BusinessUnitId));

			permissions.Should().Be.Empty();
		}

		[Test]
		public void DeletePermissions_ShouldNotBeReturned()
		{
			var personId = Guid.NewGuid();

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

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.InsertPermissions(new[] { permissionForPerson, permissionNotForPerson });
				permissionForPerson.DatasourceUpdateDate = DateTime.Today;
			});

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.DeletePermissions(new[] { permissionForPerson });
			});

			var permissions = WithAnalyticsUnitOfWork.Get(() => Target.GetPermissionsForPerson(personId, businessUnit.BusinessUnitId));
			permissions.Should().Be.Empty();
		}
	}
}
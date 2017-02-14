using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Reporting.Core
{
	[TestFixture]
	public class AnalyticsPermissionsUpdaterTest
	{
		private IAnalyticsPermissionsUpdater target;
		private IAnalyticsPermissionRepository _analyticsPermissionRepository;
		private IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private IAnalyticsPermissionExecutionRepository _analyticsPermissionExecutionRepository;
		private IPermissionsConverter _permissionsConverter;
		private IDistributedLockAcquirer _distributedLockAcquirer;
		private ICurrentAnalyticsUnitOfWork _currentAnalyticsUnitOfWork;
		private MutableNow _now;
		private Guid businessUnitId;
		private Guid personId;

		[SetUp]
		public void Setup()
		{
			_now = new MutableNow();
			_analyticsPermissionRepository = new FakeAnalyticsPermissionRepository();
			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository {UseList = true};
			_analyticsPermissionExecutionRepository = new FakeAnalyticsPermissionExecutionRepository(_now);
			_permissionsConverter = MockRepository.GenerateMock<IPermissionsConverter>();
			_distributedLockAcquirer = new FakeDistributedLockAcquirer();
			_currentAnalyticsUnitOfWork = new ThisAnalyticsUnitOfWork(new FakeUnitOfWork());

			businessUnitId = Guid.NewGuid();
			personId = Guid.NewGuid();

			target = new AnalyticsPermissionsUpdater(_analyticsPermissionRepository, 
				_analyticsBusinessUnitRepository, 
				_analyticsPermissionExecutionRepository, 
				_permissionsConverter);
		}

		[Test]
		public void ShouldNotUpdatePermissionsIfRecentlyUpdated()
		{
			const int analyticsBusinessUnitId = 1;
			_analyticsBusinessUnitRepository.AddOrUpdate(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId,BusinessUnitCode = businessUnitId});
			_now.Is(DateTime.UtcNow);
			_analyticsPermissionExecutionRepository.Set(personId, analyticsBusinessUnitId);

			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.GetPermissionsForPerson(personId, analyticsBusinessUnitId).Should().Be.Empty();
		}

		[Test]
		public void ShouldUpdateWithEmptySetWhenNoPermissions()
		{
			const int analyticsBusinessUnitId = 1;
			_now.Is(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			_analyticsPermissionExecutionRepository.Set(personId, analyticsBusinessUnitId);
			_analyticsBusinessUnitRepository.AddOrUpdate(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId, BusinessUnitCode = businessUnitId });
			_permissionsConverter.Stub(r => r.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId))
				.Return(new AnalyticsPermission[] {});

			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.GetPermissionsForPerson(personId, analyticsBusinessUnitId).Should().Be.Empty();
		}

		[Test]
		public void ShouldRemovePermissionsThatAreNotInApplicationDatabase()
		{
			const int analyticsBusinessUnitId = 1;
			_now.Is(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			_analyticsPermissionExecutionRepository.Set(personId, analyticsBusinessUnitId);
			_analyticsBusinessUnitRepository.AddOrUpdate(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId, BusinessUnitCode = businessUnitId });
			_permissionsConverter.Stub(r => r.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId))
				.Return(new AnalyticsPermission[] { });
			var toBeRemoved = new AnalyticsPermission {BusinessUnitId = analyticsBusinessUnitId, DatasourceId = 1, DatasourceUpdateDate = DateTime.UtcNow, MyOwn = false, PersonCode = personId, ReportId = Guid.NewGuid(), TeamId = 123};
			_analyticsPermissionRepository.InsertPermissions(new[] { toBeRemoved });

			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.GetPermissionsForPerson(personId, analyticsBusinessUnitId).Should().Be.Empty();
		}

		[Test]
		public void ShouldAddPermissionsThatAreInApplicationDatabase()
		{
			const int analyticsBusinessUnitId = 1;
			_now.Is(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			_analyticsPermissionExecutionRepository.Set(personId, analyticsBusinessUnitId);
			_analyticsBusinessUnitRepository.AddOrUpdate(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId, BusinessUnitCode = businessUnitId });
			var toBeAdded = new AnalyticsPermission { BusinessUnitId = analyticsBusinessUnitId, DatasourceId = 1, DatasourceUpdateDate = DateTime.UtcNow, MyOwn = false, PersonCode = personId, ReportId = Guid.NewGuid(), TeamId = 123 };

			_permissionsConverter.Stub(r => r.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId))
				.Return(new [] { toBeAdded });
			
			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.GetPermissionsForPerson(personId, analyticsBusinessUnitId).First().Should().Be.SameInstanceAs(toBeAdded);
		}

		[Test]
		public void ShouldUpdateWithEmptySetWhenExistsInBoth()
		{
			const int analyticsBusinessUnitId = 1;
			_now.Is(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			_analyticsPermissionExecutionRepository.Set(personId, analyticsBusinessUnitId);
			_analyticsBusinessUnitRepository.AddOrUpdate(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId, BusinessUnitCode = businessUnitId });
			var existsInBoth = new AnalyticsPermission { BusinessUnitId = analyticsBusinessUnitId, DatasourceId = 1, DatasourceUpdateDate = DateTime.UtcNow, MyOwn = false, PersonCode = personId, ReportId = Guid.NewGuid(), TeamId = 123 };

			_permissionsConverter.Stub(r => r.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId))
				.Return(new[] { existsInBoth });
			_analyticsPermissionRepository.InsertPermissions(new[] { existsInBoth });

			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.GetPermissionsForPerson(personId, analyticsBusinessUnitId).First().Should().Be.SameInstanceAs(existsInBoth);
		}
	}
}
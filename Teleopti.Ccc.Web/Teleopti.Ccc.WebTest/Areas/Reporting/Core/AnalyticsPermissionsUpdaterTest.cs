using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
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
		private Guid businessUnitId;
		private Guid personId;

		[SetUp]
		public void Setup()
		{
			_analyticsPermissionRepository = MockRepository.GenerateMock<IAnalyticsPermissionRepository>();
			_analyticsBusinessUnitRepository = MockRepository.GenerateMock<IAnalyticsBusinessUnitRepository>();
			_analyticsPermissionExecutionRepository = MockRepository.GenerateMock<IAnalyticsPermissionExecutionRepository>();
			_permissionsConverter = MockRepository.GenerateMock<IPermissionsConverter>();
			_distributedLockAcquirer = new FakeDistributedLockAcquirer();
			_currentAnalyticsUnitOfWork = MockRepository.GenerateMock<ICurrentAnalyticsUnitOfWork>();
			_currentAnalyticsUnitOfWork.Stub(x => x.Current()).Return(MockRepository.GenerateMock<IUnitOfWork>());

			businessUnitId = Guid.NewGuid();
			personId = Guid.NewGuid();

			target = new AnalyticsPermissionsUpdater(_analyticsPermissionRepository, 
				_analyticsBusinessUnitRepository, 
				_analyticsPermissionExecutionRepository, 
				_permissionsConverter,
				_distributedLockAcquirer,
				_currentAnalyticsUnitOfWork);
		}

		[Test]
		public void ShouldNotUpdatePermissionsIfRecentlyUpdated()
		{
			const int analyticsBusinessUnitId = 1;
			_analyticsBusinessUnitRepository.Stub(r => r.Get(businessUnitId)).Return(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId });
			_analyticsPermissionExecutionRepository.Stub(x => x.Get(personId, analyticsBusinessUnitId)).Return(DateTime.UtcNow);

			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.AssertWasNotCalled(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionRepository.AssertWasNotCalled(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionExecutionRepository.AssertWasNotCalled(r => r.Set(personId, analyticsBusinessUnitId));
		}

		[Test]
		public void ShouldUpdateWithEmptySetWhenNoPermissions()
		{
			const int analyticsBusinessUnitId = 1;
			_analyticsPermissionExecutionRepository.Stub(x => x.Get(personId, analyticsBusinessUnitId)).Return(DateTime.UtcNow-TimeSpan.FromMinutes(16));
			_analyticsBusinessUnitRepository.Stub(r => r.Get(businessUnitId)).Return(new AnalyticBusinessUnit {BusinessUnitId = analyticsBusinessUnitId });
			_permissionsConverter.Stub(r => r.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId))
				.Return(new AnalyticsPermission[] {});
			_analyticsPermissionRepository.Stub(r => r.GetPermissionsForPerson(personId, analyticsBusinessUnitId))
				.Return(new AnalyticsPermission[] {});
			_analyticsPermissionRepository.Stub(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionRepository.Stub(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionExecutionRepository.Stub(r => r.Set(personId, analyticsBusinessUnitId));

			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.AssertWasCalled(r => r.InsertPermissions((Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.IsEmpty()))));
			_analyticsPermissionRepository.AssertWasCalled(r => r.DeletePermissions((Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.IsEmpty()))));
			_analyticsPermissionExecutionRepository.AssertWasCalled(r => r.Set(personId, analyticsBusinessUnitId));
		}

		[Test]
		public void ShouldRemovePermissionsThatAreNotInApplicationDatabase()
		{
			const int analyticsBusinessUnitId = 1;
			_analyticsPermissionExecutionRepository.Stub(x => x.Get(personId, analyticsBusinessUnitId)).Return(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			_analyticsBusinessUnitRepository.Stub(r => r.Get(businessUnitId)).Return(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId });
			_permissionsConverter.Stub(r => r.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId))
				.Return(new AnalyticsPermission[] { });
			var toBeRemoved = new AnalyticsPermission {BusinessUnitId = analyticsBusinessUnitId, DatasourceId = 1, DatasourceUpdateDate = DateTime.UtcNow, MyOwn = false, PersonCode = personId, ReportId = Guid.NewGuid(), TeamId = 123};
			_analyticsPermissionRepository.Stub(r => r.GetPermissionsForPerson(personId, analyticsBusinessUnitId))
				.Return(new[] { toBeRemoved });
			_analyticsPermissionRepository.Stub(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionRepository.Stub(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionExecutionRepository.Stub(r => r.Set(personId, analyticsBusinessUnitId));

			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.AssertWasCalled(r => r.InsertPermissions((Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.IsEmpty()))));
			_analyticsPermissionRepository.AssertWasCalled(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.Contains(toBeRemoved))));
			_analyticsPermissionExecutionRepository.AssertWasCalled(r => r.Set(personId, analyticsBusinessUnitId));
		}

		[Test]
		public void ShouldAddPermissionsThatAreInApplicationDatabase()
		{
			const int analyticsBusinessUnitId = 1;
			_analyticsPermissionExecutionRepository.Stub(x => x.Get(personId, analyticsBusinessUnitId)).Return(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			_analyticsBusinessUnitRepository.Stub(r => r.Get(businessUnitId)).Return(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId });
			var toBeAdded = new AnalyticsPermission { BusinessUnitId = analyticsBusinessUnitId, DatasourceId = 1, DatasourceUpdateDate = DateTime.UtcNow, MyOwn = false, PersonCode = personId, ReportId = Guid.NewGuid(), TeamId = 123 };

			_permissionsConverter.Stub(r => r.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId))
				.Return(new [] { toBeAdded });
			
			_analyticsPermissionRepository.Stub(r => r.GetPermissionsForPerson(personId, analyticsBusinessUnitId))
				.Return(new AnalyticsPermission[] { });
			_analyticsPermissionRepository.Stub(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionRepository.Stub(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionExecutionRepository.Stub(r => r.Set(personId, analyticsBusinessUnitId));

			target.Handle(personId, businessUnitId);
			
			_analyticsPermissionRepository.AssertWasCalled(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.Contains(toBeAdded))));
			_analyticsPermissionRepository.AssertWasCalled(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.IsEmpty())));
			_analyticsPermissionExecutionRepository.AssertWasCalled(r => r.Set(personId, analyticsBusinessUnitId));
		}

		[Test]
		public void ShouldUpdateWithEmptySetWhenExistsInBoth()
		{
			const int analyticsBusinessUnitId = 1;
			_analyticsPermissionExecutionRepository.Stub(x => x.Get(personId, analyticsBusinessUnitId)).Return(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			_analyticsBusinessUnitRepository.Stub(r => r.Get(businessUnitId)).Return(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId });
			var existsInBoth = new AnalyticsPermission { BusinessUnitId = analyticsBusinessUnitId, DatasourceId = 1, DatasourceUpdateDate = DateTime.UtcNow, MyOwn = false, PersonCode = personId, ReportId = Guid.NewGuid(), TeamId = 123 };

			_permissionsConverter.Stub(r => r.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId))
				.Return(new[] { existsInBoth });
			_analyticsPermissionRepository.Stub(r => r.GetPermissionsForPerson(personId, analyticsBusinessUnitId))
				.Return(new[] { existsInBoth });
			_analyticsPermissionRepository.Stub(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionRepository.Stub(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionExecutionRepository.Stub(r => r.Set(personId, analyticsBusinessUnitId));

			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.AssertWasCalled(r => r.InsertPermissions((Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.IsEmpty()))));
			_analyticsPermissionRepository.AssertWasCalled(r => r.DeletePermissions((Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.IsEmpty()))));
			_analyticsPermissionExecutionRepository.AssertWasCalled(r => r.Set(personId, analyticsBusinessUnitId));
		}
	}
}
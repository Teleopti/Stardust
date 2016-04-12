using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Reporting.Core;

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
		private Guid businessUnitId;
		private Guid personId;

		[SetUp]
		public void Setup()
		{
			_analyticsPermissionRepository = MockRepository.GenerateMock<IAnalyticsPermissionRepository>();
			_analyticsBusinessUnitRepository = MockRepository.GenerateMock<IAnalyticsBusinessUnitRepository>();
			_analyticsPermissionExecutionRepository = MockRepository.GenerateMock<IAnalyticsPermissionExecutionRepository>();
			_permissionsConverter = MockRepository.GenerateMock<IPermissionsConverter>();

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
			_analyticsPermissionExecutionRepository.Stub(x => x.Get(personId)).Return(DateTime.UtcNow);

			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.AssertWasNotCalled(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionRepository.AssertWasNotCalled(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionExecutionRepository.AssertWasNotCalled(r => r.Set(personId));
		}

		[Test]
		public void ShouldUpdateWithEmptySetWhenNoPermissions()
		{
			const int analyticsBusinessUnitId = 1;
			_analyticsPermissionExecutionRepository.Stub(x => x.Get(personId)).Return(DateTime.UtcNow-TimeSpan.FromMinutes(16));
			_analyticsBusinessUnitRepository.Stub(r => r.Get(businessUnitId)).Return(new AnalyticBusinessUnit {BusinessUnitId = analyticsBusinessUnitId });
			_permissionsConverter.Stub(r => r.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId))
				.Return(new AnalyticsPermission[] {});
			_analyticsPermissionRepository.Stub(r => r.GetPermissionsForPerson(personId))
				.Return(new AnalyticsPermission[] {});
			_analyticsPermissionRepository.Stub(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionRepository.Stub(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionExecutionRepository.Stub(r => r.Set(personId));

			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.AssertWasCalled(r => r.InsertPermissions((Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.IsEmpty()))));
			_analyticsPermissionRepository.AssertWasCalled(r => r.DeletePermissions((Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.IsEmpty()))));
			_analyticsPermissionExecutionRepository.AssertWasCalled(r => r.Set(personId));
		}

		[Test]
		public void ShouldRemovePermissionsThatAreNotInApplicationDatabase()
		{
			const int analyticsBusinessUnitId = 1;
			_analyticsPermissionExecutionRepository.Stub(x => x.Get(personId)).Return(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			_analyticsBusinessUnitRepository.Stub(r => r.Get(businessUnitId)).Return(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId });
			_permissionsConverter.Stub(r => r.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId))
				.Return(new AnalyticsPermission[] { });
			var toBeRemoved = new AnalyticsPermission {BusinessUnitId = analyticsBusinessUnitId, DatasourceId = 1, DatasourceUpdateDate = DateTime.UtcNow, MyOwn = false, PersonCode = personId, ReportId = Guid.NewGuid(), TeamId = 123};
			_analyticsPermissionRepository.Stub(r => r.GetPermissionsForPerson(personId))
				.Return(new[] { toBeRemoved });
			_analyticsPermissionRepository.Stub(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionRepository.Stub(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionExecutionRepository.Stub(r => r.Set(personId));

			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.AssertWasCalled(r => r.InsertPermissions((Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.IsEmpty()))));
			_analyticsPermissionRepository.AssertWasCalled(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.Contains(toBeRemoved))));
			_analyticsPermissionExecutionRepository.AssertWasCalled(r => r.Set(personId));
		}

		[Test]
		public void ShouldAddPermissionsThatAreInApplicationDatabase()
		{
			const int analyticsBusinessUnitId = 1;
			_analyticsPermissionExecutionRepository.Stub(x => x.Get(personId)).Return(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			_analyticsBusinessUnitRepository.Stub(r => r.Get(businessUnitId)).Return(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId });
			var toBeAdded = new AnalyticsPermission { BusinessUnitId = analyticsBusinessUnitId, DatasourceId = 1, DatasourceUpdateDate = DateTime.UtcNow, MyOwn = false, PersonCode = personId, ReportId = Guid.NewGuid(), TeamId = 123 };

			_permissionsConverter.Stub(r => r.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId))
				.Return(new [] { toBeAdded });
			
			_analyticsPermissionRepository.Stub(r => r.GetPermissionsForPerson(personId))
				.Return(new AnalyticsPermission[] { });
			_analyticsPermissionRepository.Stub(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionRepository.Stub(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionExecutionRepository.Stub(r => r.Set(personId));

			target.Handle(personId, businessUnitId);
			
			_analyticsPermissionRepository.AssertWasCalled(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.Contains(toBeAdded))));
			_analyticsPermissionRepository.AssertWasCalled(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.IsEmpty())));
			_analyticsPermissionExecutionRepository.AssertWasCalled(r => r.Set(personId));
		}

		[Test]
		public void ShouldUpdateWithEmptySetWhenExistsInBoth()
		{
			const int analyticsBusinessUnitId = 1;
			_analyticsPermissionExecutionRepository.Stub(x => x.Get(personId)).Return(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			_analyticsBusinessUnitRepository.Stub(r => r.Get(businessUnitId)).Return(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId });
			var existsInBoth = new AnalyticsPermission { BusinessUnitId = analyticsBusinessUnitId, DatasourceId = 1, DatasourceUpdateDate = DateTime.UtcNow, MyOwn = false, PersonCode = personId, ReportId = Guid.NewGuid(), TeamId = 123 };

			_permissionsConverter.Stub(r => r.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId))
				.Return(new[] { existsInBoth });
			_analyticsPermissionRepository.Stub(r => r.GetPermissionsForPerson(personId))
				.Return(new[] { existsInBoth });
			_analyticsPermissionRepository.Stub(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionRepository.Stub(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionExecutionRepository.Stub(r => r.Set(personId));

			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.AssertWasCalled(r => r.InsertPermissions((Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.IsEmpty()))));
			_analyticsPermissionRepository.AssertWasCalled(r => r.DeletePermissions((Arg<IEnumerable<AnalyticsPermission>>.Matches(x => x.IsEmpty()))));
			_analyticsPermissionExecutionRepository.AssertWasCalled(r => r.Set(personId));
		}
	}
}
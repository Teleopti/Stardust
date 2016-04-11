using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Reporting.Core
{
	[TestFixture]
	public class AnalyticsPermissionsUpdaterTest
	{
		private IAnalyticsPermissionsUpdater target;
		private IPersonRepository _personRepository;
		private ISiteRepository _siteRepository;
		private IApplicationFunctionRepository _applicationFunctionRepository;
		private INow _now;
		private ICurrentDataSource _currentDataSource;
		private IAnalyticsPermissionRepository _analyticsPermissionRepository;
		private IAnalyticsTeamRepository _analyticsTeamRepository;
		private IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private IAnalyticsPermissionExecutionRepository _analyticsPermissionExecutionRepository;
		private Guid businessUnitId;
		private Guid personId;

		[SetUp]
		public void Setup()
		{
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			_applicationFunctionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();
			_now = MockRepository.GenerateMock<INow>();
			_currentDataSource = MockRepository.GenerateMock<ICurrentDataSource>();
			_analyticsPermissionRepository = MockRepository.GenerateMock<IAnalyticsPermissionRepository>();
			_analyticsTeamRepository = MockRepository.GenerateMock<IAnalyticsTeamRepository>();
			_analyticsBusinessUnitRepository = MockRepository.GenerateMock<IAnalyticsBusinessUnitRepository>();
			_analyticsPermissionExecutionRepository = MockRepository.GenerateMock<IAnalyticsPermissionExecutionRepository>();

			businessUnitId = Guid.NewGuid();
			personId = Guid.NewGuid();

			target = new AnalyticsPermissionsUpdater(_personRepository, _siteRepository, _applicationFunctionRepository, _now, _currentDataSource, _analyticsPermissionRepository, _analyticsTeamRepository, _analyticsBusinessUnitRepository, _analyticsPermissionExecutionRepository);
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
		public void ShouldUpdatePermissionsIfNotRecentlyUpdated()
		{
			_analyticsPermissionExecutionRepository.Stub(x => x.Get(personId)).Return(DateTime.UtcNow-TimeSpan.FromMinutes(16));
			_siteRepository.Stub(r => r.LoadAll()).Return(new ISite[] {});
			_analyticsTeamRepository.Stub(r => r.GetTeams()).Return(new AnalyticTeam[] {});
			_analyticsBusinessUnitRepository.Stub(r => r.Get(businessUnitId)).Return(new AnalyticBusinessUnit());
			_personRepository.Stub(r => r.Get(personId)).Return(new Person());
			_applicationFunctionRepository.Stub(r => r.ExternalApplicationFunctions()).Return(new IApplicationFunction[] {});
			_now.Stub(r => r.UtcDateTime()).Return(DateTime.UtcNow);
			_currentDataSource.Stub(r => r.Current()).Return(new FakeDataSource());
			_analyticsPermissionRepository.Stub(r => r.GetPermissionsForPerson(personId)).Return(new AnalyticsPermission[] {});
			_analyticsPermissionRepository.Stub(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionRepository.Stub(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionExecutionRepository.Stub(r => r.Set(personId));

			target.Handle(personId, businessUnitId);

			_analyticsPermissionRepository.AssertWasCalled(r => r.InsertPermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionRepository.AssertWasCalled(r => r.DeletePermissions(Arg<IEnumerable<AnalyticsPermission>>.Is.Anything));
			_analyticsPermissionExecutionRepository.AssertWasCalled(r => r.Set(personId));
		}
	}
}
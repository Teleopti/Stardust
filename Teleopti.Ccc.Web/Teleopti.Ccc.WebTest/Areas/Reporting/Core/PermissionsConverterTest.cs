using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Reporting.Core
{
	[TestFixture]
	public class PermissionsConverterTest
	{
		private IPermissionsConverter target;
		private IApplicationPermissionProvider _applicationPermissionProvider;
		private INow _now;
		private IAnalyticsTeamRepository _analyticsTeamRepository;
		private Guid personId;
		private int analyticsBusinessUnitId;

		[SetUp]
		public void Setup()
		{
			_now = MockRepository.GenerateMock<INow>();
			_analyticsTeamRepository = MockRepository.GenerateMock<IAnalyticsTeamRepository>();
			_applicationPermissionProvider = MockRepository.GenerateMock<IApplicationPermissionProvider>();
			personId = Guid.NewGuid();
			analyticsBusinessUnitId = 1;
			target = new PermissionsConverter(_analyticsTeamRepository, _now, _applicationPermissionProvider);
		}

		[Test]
		public void EmptyPermissionsReturnsEmptyAnalyticsPermissions()
		{
			var now = DateTime.UtcNow;
			_applicationPermissionProvider.Stub(r => r.GetPermissions(personId)).Return(new MatrixPermissionHolder[] {});
			_analyticsTeamRepository.Stub(r => r.GetTeams()).Return(new AnalyticTeam[] {});
			_now.Stub(r => r.UtcDateTime()).Return(now);

			var result = target.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId);

			result.Should().Be.Empty();
		}

		[Test]
		public void PermissionsShouldBeMappedAnalyticsPermissions()
		{
			var now = DateTime.UtcNow;
			var reportId = Guid.NewGuid();
			var analyticTeam = new AnalyticTeam { TeamCode = Guid.NewGuid(), TeamId = 123 };
			var team = new Team();
			team.SetId(analyticTeam.TeamCode);

			var person = new Person();
			person.SetId(personId);
			
			var applicationPermission = new MatrixPermissionHolder(person, team, true, new ApplicationFunction { ForeignId = reportId.ToString() });
			
			_applicationPermissionProvider.Stub(r => r.GetPermissions(personId)).Return(new[] { applicationPermission });
			_analyticsTeamRepository.Stub(r => r.GetTeams()).Return(new[] { analyticTeam  });
			_now.Stub(r => r.UtcDateTime()).Return(now);

			var result = target.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId);

			result.Should().Not.Be.Empty();
			var analyticsPermission = result.First();
			analyticsPermission.BusinessUnitId.Should().Be.EqualTo(analyticsBusinessUnitId);
			analyticsPermission.DatasourceId.Should().Be.EqualTo(1);
			analyticsPermission.DatasourceUpdateDate.Should().Be.EqualTo(now);
			analyticsPermission.MyOwn.Should().Be.EqualTo(applicationPermission.IsMy);
			analyticsPermission.PersonCode.Should().Be.EqualTo(personId);
			analyticsPermission.ReportId.Should().Be.EqualTo(reportId);
			analyticsPermission.TeamId.Should().Be.EqualTo(analyticTeam.TeamId);
		}
	}
}
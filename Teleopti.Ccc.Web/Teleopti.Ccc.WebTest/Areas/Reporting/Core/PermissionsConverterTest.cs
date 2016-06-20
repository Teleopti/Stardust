using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Reporting.Core
{
	[TestFixture]
	public class PermissionsConverterTest
	{
		private IPermissionsConverter target;
		private INow _now;
		private IAnalyticsTeamRepository _analyticsTeamRepository;
		private IPersonRepository _personRepository;
		private ISiteRepository _siteRepository;
		private IApplicationFunctionRepository _applicationFunctionRepository;
		private Guid personId;
		private int analyticsBusinessUnitId;

		[SetUp]
		public void Setup()
		{
			_now = MockRepository.GenerateMock<INow>();
			_analyticsTeamRepository = MockRepository.GenerateMock<IAnalyticsTeamRepository>();
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			_applicationFunctionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();
			personId = Guid.NewGuid();
			
			analyticsBusinessUnitId = 1;
			target = new PermissionsConverter(_analyticsTeamRepository, _now, _personRepository, _siteRepository, _applicationFunctionRepository);
		}

		[Test]
		public void EmptyPermissionsReturnsEmptyAnalyticsPermissions()
		{
			var now = DateTime.UtcNow;
			var person = PersonFactory.CreatePerson("Test");
			person.SetId(personId);
			_personRepository.Stub(r => r.Get(personId)).Return(person);

			_analyticsTeamRepository.Stub(r => r.GetTeams()).Return(new AnalyticTeam[] {});
			_now.Stub(r => r.UtcDateTime()).Return(now);

			var result = target.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId);

			result.Should().Be.Empty();
		}

		[Test]
		public void PermissionsShouldBeMappedAnalyticsPermissions()
		{
			var fakeCurrentBusinessUnit = new FakeCurrentBusinessUnit();
			var businessUnit = new BusinessUnit("Test");
			businessUnit.SetId(Guid.NewGuid());
			fakeCurrentBusinessUnit.FakeBusinessUnit(businessUnit);
			ServiceLocatorForEntity.CurrentBusinessUnit = fakeCurrentBusinessUnit;
			var now = DateTime.UtcNow;
			var reportId = Guid.NewGuid();
			var analyticTeam = new AnalyticTeam { TeamCode = Guid.NewGuid(), TeamId = 123 };
			var team = new Team();
			team.SetId(analyticTeam.TeamCode);

			var person = PersonFactory.CreatePerson("Test");
			var applicationRole = new ApplicationRole();
			applicationRole.AddApplicationFunction(new ApplicationFunction {ForeignId = reportId.ToString(), ForeignSource = DefinedForeignSourceNames.SourceMatrix });
			applicationRole.AvailableData = new AvailableData
			{
				AvailableDataRange = AvailableDataRangeOption.None
			};
			applicationRole.AvailableData.AddAvailableTeam(team);
			person.PermissionInformation.AddApplicationRole(applicationRole);
			person.SetId(personId);
			_personRepository.Stub(r => r.Get(personId)).Return(person);

			_siteRepository.Stub(r => r.LoadAll()).Return(new List<ISite>());
			_analyticsTeamRepository.Stub(r => r.GetTeams()).Return(new[] { analyticTeam  });
			_now.Stub(r => r.UtcDateTime()).Return(now);

			var result = target.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId).ToList();

			result.Should().Not.Be.Empty();
			var analyticsPermission = result.First();
			analyticsPermission.BusinessUnitId.Should().Be.EqualTo(analyticsBusinessUnitId);
			analyticsPermission.DatasourceId.Should().Be.EqualTo(1);
			analyticsPermission.DatasourceUpdateDate.Should().Be.EqualTo(now);
			analyticsPermission.MyOwn.Should().Be.EqualTo(false);
			analyticsPermission.PersonCode.Should().Be.EqualTo(personId);
			analyticsPermission.ReportId.Should().Be.EqualTo(reportId);
			analyticsPermission.TeamId.Should().Be.EqualTo(analyticTeam.TeamId);
		}

		[Test]
		public void SuperRoleShouldHaveAccessToAllReports()
		{
			var reportId = Guid.NewGuid();
			var fakeCurrentBusinessUnit = new FakeCurrentBusinessUnit();
			var businessUnit = new BusinessUnit("Test");
			businessUnit.SetId(Guid.NewGuid());
			fakeCurrentBusinessUnit.FakeBusinessUnit(businessUnit);
			ServiceLocatorForEntity.CurrentBusinessUnit = fakeCurrentBusinessUnit;
			var now = DateTime.UtcNow;
			var analyticTeam = new AnalyticTeam { TeamCode = Guid.NewGuid(), TeamId = 123 };
			var team = new Team();
			team.SetId(analyticTeam.TeamCode);

			var person = PersonFactory.CreatePerson("Test");
			var applicationRole = new ApplicationRole
			{
				BuiltIn = true,
				AvailableData = new AvailableData
				{
					AvailableDataRange = AvailableDataRangeOption.Everyone
				}
			};
			applicationRole.SetId(SystemUser.SuperRoleId);
			applicationRole.AddApplicationFunction(new ApplicationFunction { ForeignId = "0000", ForeignSource = DefinedForeignSourceNames.SourceRaptor });
			person.PermissionInformation.AddApplicationRole(applicationRole);
			person.SetId(personId);
			_personRepository.Stub(r => r.Get(personId)).Return(person);

			var site = new Site("Test");
			site.AddTeam(team);
			_siteRepository.Stub(r => r.LoadAll()).Return(new List<ISite> {site});
			_analyticsTeamRepository.Stub(r => r.GetTeams()).Return(new[] { analyticTeam });
			_now.Stub(r => r.UtcDateTime()).Return(now);
			_applicationFunctionRepository.Stub(x => x.ExternalApplicationFunctions()).Return(new List<IApplicationFunction>
			{
				new ApplicationFunction {ForeignId = reportId.ToString(), ForeignSource = DefinedForeignSourceNames.SourceMatrix}
			});

			var result = target.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId).ToList();

			result.Should().Not.Be.Empty();

			var analyticsPermission = result.First();
			analyticsPermission.BusinessUnitId.Should().Be.EqualTo(analyticsBusinessUnitId);
			analyticsPermission.DatasourceId.Should().Be.EqualTo(1);
			analyticsPermission.DatasourceUpdateDate.Should().Be.EqualTo(now);
			analyticsPermission.MyOwn.Should().Be.EqualTo(false);
			analyticsPermission.PersonCode.Should().Be.EqualTo(personId);
			analyticsPermission.ReportId.Should().Be.EqualTo(reportId);
			analyticsPermission.TeamId.Should().Be.EqualTo(analyticTeam.TeamId);
		}
	}
}
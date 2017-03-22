using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Reporting.Core;

namespace Teleopti.Ccc.WebTest.Areas.Reporting.Core
{
	[TestFixture]
	[DomainTest]
	public class PermissionsConverterTest : ISetup
	{
		public IPermissionsConverter Target;
		public MutableNow Now;
		public FakeAnalyticsTeamRepository AnalyticsTeamRepository;
		public FakePersonRepository PersonRepository;
		public FakeSiteRepository SiteRepository;
		public FakeApplicationFunctionRepository ApplicationFunctionRepository;
		private readonly Guid personId = Guid.NewGuid();
		private readonly Guid reportId = Guid.NewGuid();
		private readonly Guid teamId = Guid.NewGuid();
		private const int analyticsBusinessUnitId = 1;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<PermissionsConverter>();
			system.AddService<FakeAnalyticsTeamRepository>();
		}

		[Test]
		public void EmptyPermissionsReturnsEmptyAnalyticsPermissions()
		{
			var now = DateTime.UtcNow;
			var person = PersonFactory.CreatePerson("Test").WithId(personId);
			PersonRepository.Has(person);

			Now.Is(now);

			var result = Target.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId);

			result.Should().Be.Empty();
		}

		[Test]
		public void PermissionsShouldBeMappedAnalyticsPermissions()
		{
			var now = DateTime.UtcNow;
			
			var analyticTeam = new AnalyticTeam { TeamCode = teamId, TeamId = 123 };
			var team = new Team().WithId(teamId);

			var person = PersonFactory.CreatePerson("Test").WithId(personId);
			var applicationRole = new ApplicationRole();
			applicationRole.AddApplicationFunction(new ApplicationFunction {ForeignId = reportId.ToString(), ForeignSource = DefinedForeignSourceNames.SourceMatrix });
			applicationRole.AvailableData = new AvailableData
			{
				AvailableDataRange = AvailableDataRangeOption.None
			};
			applicationRole.AvailableData.AddAvailableTeam(team);
			person.PermissionInformation.AddApplicationRole(applicationRole);
			PersonRepository.Has(person);

			AnalyticsTeamRepository.Has(analyticTeam);
			Now.Is(now);

			var result = Target.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId).ToList();

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
			var now = DateTime.UtcNow;
			var analyticTeam = new AnalyticTeam { TeamCode = teamId, TeamId = 123 };
			var team = new Team().WithId(teamId);

			var person = PersonFactory.CreatePerson("Test").WithId(personId);
			var applicationRole = new ApplicationRole
			{
				BuiltIn = true,
				AvailableData = new AvailableData
				{
					AvailableDataRange = AvailableDataRangeOption.Everyone
				}
			}.WithId(SystemUser.SuperRoleId);
			applicationRole.AddApplicationFunction(new ApplicationFunction { ForeignId = "0000", ForeignSource = DefinedForeignSourceNames.SourceRaptor });
			person.PermissionInformation.AddApplicationRole(applicationRole);
			PersonRepository.Has(person);

			SiteRepository.Has(SiteFactory.CreateSiteWithTeam(Guid.NewGuid(), "_", team));
			AnalyticsTeamRepository.Has(analyticTeam);
			Now.Is(now);
			ApplicationFunctionRepository.Add(new ApplicationFunction { ForeignId = reportId.ToString(), ForeignSource = DefinedForeignSourceNames.SourceMatrix });

			var result = Target.GetApplicationPermissionsAndConvert(personId, analyticsBusinessUnitId).ToList();

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
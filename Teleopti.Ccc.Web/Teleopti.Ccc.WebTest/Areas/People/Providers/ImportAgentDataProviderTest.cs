using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.WebTest.Areas.People.Providers
{
	[TestFixture]
	[DomainTest]
	public class ImportAgentDataProviderTest: IIsolateSystem
	{
		public Global.FakePermissionProvider PermissionProvider;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakeApplicationRoleRepository RoleRepository;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		public FakeLoggedOnUser CurrentLoggedOnUser;
		public FakeSkillRepository SkillRepository;
		public ImportAgentDataProvider Target;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			isolate.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			isolate.UseTestDouble(new FakeCurrentBusinessUnit()).For<ICurrentBusinessUnit>();
			isolate.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<ImportAgentDataProvider>().For<IImportAgentDataProvider>();
			isolate.UseTestDouble<FakeApplicationRoleRepository>().For<IApplicationRoleRepository>();
			isolate.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
		}

		[Test]
		public void ShouldReturnPermittedTeamsForSettingsData()
		{
			var team = TeamFactory.CreateTeamWithId("team");
			var anotherTeam = TeamFactory.CreateTeamWithId("anotherTeam").WithId();
			var site = SiteFactory.CreateSimpleSite("site").WithId();
			var site2 = SiteFactory.CreateSimpleSite("site2").WithId();

			var bu = BusinessUnitFactory.CreateWithId("bu");
			var bu2 = BusinessUnitFactory.CreateWithId("bu2");

			site.AddTeam(team);
			site.SetBusinessUnit(bu);
			site2.AddTeam(anotherTeam);
			site2.SetBusinessUnit(bu2);

			CurrentBusinessUnit.FakeBusinessUnit(bu);

			SiteRepository.Add(site);
			SiteRepository.Add(site2);
			TeamRepository.Add(team);
			TeamRepository.Add(anotherTeam);

			PermissionProvider.Enable();
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.WebPeople, team, DateOnly.Today);

			var permittedTeams = Target.GetPermittedTeams();

			permittedTeams.Count.Should().Be.EqualTo(1);
			permittedTeams.Single().SiteAndTeam.Should().Be.EqualTo(team.SiteAndTeam);
		}

		[Test]
		public void ShouldUseDescriptionTextForFindingRoleWithoutCaseSensitivity()
		{
			var role = ApplicationRoleFactory.CreateRole("name", "Description");
			RoleRepository.Has(role);

			var foundRole = Target.FindRole("description");
			foundRole.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMatchFirstForFindingRoleWithoutCaseSensitivityAndWithSimilarRoles()
		{
			var role = ApplicationRoleFactory.CreateRole("name", "description");
			var roleBig = ApplicationRoleFactory.CreateRole("name", "Description");
			RoleRepository.Has(roleBig);
			RoleRepository.Has(role);
			

			var foundRole = Target.FindRole("description");
			foundRole.Should().Not.Be.Null();
		    foundRole.DescriptionText.Should().Be.EqualTo(roleBig.DescriptionText);
		}


		[Test]
		public void ShouldMatchSkillWithoutCaseSensitivity()
		{
			var skill = SkillFactory.CreateSkill("testSkill");
			SkillRepository.Has(skill);

			var foundSkill = Target.FindSkill("testskill");
			foundSkill.Should().Not.Be.Null();
		}
	}
}

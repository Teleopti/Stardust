using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.ImportAgent;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.ImportAgent
{
	[TestFixture]
	[DomainTest]
	public class ImportAgentDataProviderTest: ISetup
	{
		public FakePermissionProvider PermissionProvider;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		public FakeLoggedOnUser CurrentLoggedOnUser;
		public ImportAgentDataProvider Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble(new FakeCurrentBusinessUnit()).For<ICurrentBusinessUnit>();
			system.UseTestDouble<FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<ImportAgentDataProvider>().For<IImportAgentDataProvider>();
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
	}
}

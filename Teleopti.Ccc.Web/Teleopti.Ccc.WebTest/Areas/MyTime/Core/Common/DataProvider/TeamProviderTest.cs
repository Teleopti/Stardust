using System.IdentityModel.Claims;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[MyTimeWebTest]
	[TestFixture]
	public class TeamProviderTest : IIsolateSystem
	{
		public ITeamProvider Target;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakePersonRepository PersonRepository;
		public FakeThreadPrincipalContext ThreadPrincipalContext;
		public FakePersonFinderReadOnlyRepository PersonFinderReadOnlyRepository;
		public ILoggedOnUser LoggedOnUser;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			isolate.UseTestDouble<PermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<PrincipalAuthorization>().For<IAuthorization>();
			isolate.UseTestDouble<FakeThreadPrincipalContext>().For<IThreadPrincipalContext>();
			isolate.UseTestDouble<FakePersonFinderReadOnlyRepository>().For<IPersonFinderReadOnlyRepository>();
		}

		[Test]
		public void ShouldQueryAllTeams()
		{
			var site1 = new Site("site1").WithId();
			SiteRepository.Add(site1);
			var team1 = new Team { Site = site1 }.WithDescription(new Description("team1")).WithId();
			TeamRepository.Add(team1);
			var person1 = LoggedOnUser.CurrentUser();
			PersonRepository.Add(person1);
			addTeamAndSiteToPerson(person1, team1);

			((OrganisationMembership)ThreadPrincipalContext.Current().Organisation).InitializeFromPerson(person1);

			setPermissions(DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			var teams = Target.GetPermittedTeams(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			teams.Count().Should().Be(1);
			teams.ElementAt(0).Description.Name.Should().Be("team1");
		}

		[Test]
		public void ShouldFilterPermittedTeamsWhenQueryingAll()
		{
			var site1 = new Site("site1").WithId();
			SiteRepository.Add(site1);
			var team1 = new Team { Site = site1 }.WithDescription(new Description("team1")).WithId();
			TeamRepository.Add(team1);

			var person1 = LoggedOnUser.CurrentUser();
			PersonRepository.Add(person1);
			addTeamAndSiteToPerson(person1, team1);
			((OrganisationMembership)ThreadPrincipalContext.Current().Organisation).InitializeFromPerson(person1);

			var site2 = new Site("site2").WithId();
			SiteRepository.Add(site2);
			var team2 = new Team { Site = site2 }.WithDescription(new Description("team2")).WithId();
			TeamRepository.Add(team2);

			var person2 = PersonFactory.CreatePerson("person2").WithId();
			PersonRepository.Add(person2);
			addTeamAndSiteToPerson(person2, team2);

			setPermissions(DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			var teams = Target.GetPermittedTeams(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			teams.Count().Should().Be(1);
			teams.ElementAt(0).Description.Name.Should().Be("team1");
		}

		[Test]
		public void ShouldFilterEmptyTeamsWhenQueryingAll()
		{
			var site1 = new Site("site1").WithId();
			SiteRepository.Add(site1);
			var team1 = new Team { Site = site1 }.WithDescription(new Description("team1")).WithId();
			TeamRepository.Add(team1);

			var person1 = LoggedOnUser.CurrentUser();
			PersonRepository.Add(person1);
			PersonFinderReadOnlyRepository.Has(person1);
			addTeamAndSiteToPerson(person1, team1);
			((OrganisationMembership)ThreadPrincipalContext.Current().Organisation).InitializeFromPerson(person1);
			
			var team2 = new Team { Site = site1 }.WithDescription(new Description("team2")).WithId();
			TeamRepository.Add(team2);

			setPermissions(DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			var teams = Target.GetPermittedNotEmptyTeams(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			teams.Count().Should().Be(1);
			teams.ElementAt(0).Description.Name.Should().Be("team1");
		}

		private void addTeamAndSiteToPerson(IPerson person,ITeam team)
		{
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, team));
		}

		private void setPermissions(params string[] functionPaths)
		{
			var teleoptiPrincipal = ThreadPrincipalContext.Current();
			var claims = functionPaths.Select(functionPath => new Claim(string.Concat(
					TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
					, "/", functionPath)
				, new AuthorizeMySite(), Rights.PossessProperty)).ToList();
			claims.Add(new Claim(
				string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
					"/AvailableData"), new AuthorizeMySite(), Rights.PossessProperty));
			teleoptiPrincipal.AddClaimSet(new DefaultClaimSet(ClaimSet.System, claims));
		}
	}
}
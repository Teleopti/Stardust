using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core
{
	[TestFixture, DomainTest]
	public class TeamsProviderTest:ISetup
	{
		public Areas.Global.FakePermissionProvider PermissionProvider;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public TeamsProvider Target;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		private FakePersonRepository personRepository;
		public FakeLoggedOnUser CurrentLoggedOnUser;
		
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble(new FakeCurrentBusinessUnit()).For<ICurrentBusinessUnit>();
			system.UseTestDouble<TeamsProvider>().For<ITeamsProvider>();
			system.UseTestDouble<Areas.Global.FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			personRepository = new FakePersonRepositoryLegacy();
		}
		[Test]
		public void ShouldReturnPermittedTeamsHierachyWhenUserHasMyTeamSchedulesPermission()
		{
			var date = new DateOnly(2016, 11, 29);
			var bu = BusinessUnitFactory.CreateWithId("_");
			CurrentBusinessUnit.FakeBusinessUnit(bu);
			var team = TeamFactory.CreateTeamWithId("myteam");
			var anotherTeam = TeamFactory.CreateTeamWithId("another").WithId();
			var site = SiteFactory.CreateSimpleSite("mysite").WithId();
			site.AddTeam(team);
			SiteRepository.Add(site);
			TeamRepository.Add(team);
			TeamRepository.Add(anotherTeam);
			PermissionProvider.Enable();
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team, date);

			var permittedHierachy = Target.GetPermittedTeamHierachy(date, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			permittedHierachy.Children.Count.Should().Be.EqualTo(1);
			permittedHierachy.Name.Should().Be.EqualTo("_");
			permittedHierachy.Children.First().Name.Should().Be.EqualTo("mysite");
			permittedHierachy.Children.First().Children.Single().Name.Should().Be.EqualTo("myteam");
		}

		[Test]
		public void ShouldReturnPermittedTeamsHierachyUnderCurrentBusinessUnit()
		{
			var date = new DateOnly(2016, 11, 29);
			
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
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team, date);
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, anotherTeam, date);

			var permittedHierachy = Target.GetPermittedTeamHierachy(date, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			permittedHierachy.Children.Count.Should().Be.EqualTo(1);
			permittedHierachy.Name.Should().Be.EqualTo("bu");
			permittedHierachy.Children.First().Name.Should().Be.EqualTo("site");
			permittedHierachy.Children.First().Children.Single().Name.Should().Be.EqualTo("team");
		}

		[Test]
		public void ShouldNotReturnLogonUserTeamIdUnderOtherNotCurrentBusinessUnit()
		{
			var date = new DateOnly(2016, 11, 29);

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
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team, date);
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, anotherTeam, date);

			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, anotherTeam);

			var meOfAnotherTeam = PersonFactory.CreatePerson().WithId();
			meOfAnotherTeam.AddPersonPeriod(personPeriod);

			personRepository.Add(meOfAnotherTeam);
			CurrentLoggedOnUser.SetFakeLoggedOnUser(meOfAnotherTeam);

			var permittedHierachy = Target.GetPermittedTeamHierachy(date, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			permittedHierachy.Children.Count.Should().Be.EqualTo(1);
			permittedHierachy.Name.Should().Be.EqualTo("bu");
			permittedHierachy.Children.First().Name.Should().Be.EqualTo("site");
			permittedHierachy.Children.First().Children.Single().Name.Should().Be.EqualTo("team");
			permittedHierachy.LogonUserTeamId.Should().Be(null);
		}

		[Test]
		public void ShouldReturnLogonUserTeamIdWhenUserHasMyTeamSchedulesPermission()
		{

			var date = new DateOnly(2016, 11, 29);
			var bu = BusinessUnitFactory.CreateWithId("_");
			CurrentBusinessUnit.FakeBusinessUnit(bu);
			var team = TeamFactory.CreateTeamWithId("myteam");
			var site = SiteFactory.CreateSimpleSite("mysite").WithId();

			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);

			var me = PersonFactory.CreatePerson().WithId();
			me.AddPersonPeriod(personPeriod);

			personRepository.Add(me);

			CurrentLoggedOnUser.SetFakeLoggedOnUser(me);

			site.AddTeam(team);
			SiteRepository.Add(site);
			TeamRepository.Add(team);
			PermissionProvider.Enable();
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team, date);

			var permittedHierachy = Target.GetPermittedTeamHierachy(date, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			permittedHierachy.Children.Count.Should().Be.EqualTo(1);
			permittedHierachy.Name.Should().Be.EqualTo("_");
			permittedHierachy.Children.First().Name.Should().Be.EqualTo("mysite");
			permittedHierachy.Children.First().Children.Single().Name.Should().Be.EqualTo("myteam");
			permittedHierachy.LogonUserTeamId.Should().Be(null);
		}

		[Test]
		public void ShouldReturnLogonUserTeamIdWhenUserHasMyOwnDataPermissionOnly()
		{

			var date = new DateOnly(2016, 11, 29);
			var bu = BusinessUnitFactory.CreateWithId("_");
			CurrentBusinessUnit.FakeBusinessUnit(bu);
			var team = TeamFactory.CreateTeamWithId("myteam");
			var site = SiteFactory.CreateSimpleSite("mysite").WithId();

			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);

			var me = PersonFactory.CreatePerson().WithId();
			me.AddPersonPeriod(personPeriod);

			personRepository.Add(me);

			CurrentLoggedOnUser.SetFakeLoggedOnUser(me);

			site.AddTeam(team);
			SiteRepository.Add(site);
			TeamRepository.Add(team);
			PermissionProvider.Enable();
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, me, date);

			var permittedHierachy = Target.GetPermittedTeamHierachy(date, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			permittedHierachy.Children.Count.Should().Be.EqualTo(1);
			permittedHierachy.Name.Should().Be.EqualTo("_");
			permittedHierachy.Children.First().Name.Should().Be.EqualTo("mysite");
			permittedHierachy.Children.First().Children.Single().Name.Should().Be.EqualTo("myteam");
			permittedHierachy.LogonUserTeamId.Should().Be(team.Id);
		}
	}
}

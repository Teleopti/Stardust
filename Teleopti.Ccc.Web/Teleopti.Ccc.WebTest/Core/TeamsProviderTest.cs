using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Core;


namespace Teleopti.Ccc.WebTest.Core
{
	[TestFixture, DomainTest]
	public class TeamsProviderTest : IIsolateSystem
	{
		public Areas.Global.FakePermissionProvider PermissionProvider;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public TeamsProvider Target;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		private FakePersonRepository personRepository;
		public FakeLoggedOnUser CurrentLoggedOnUser;
		public FakeGroupingReadOnlyRepository GroupingReadOnlyRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			isolate.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			isolate.UseTestDouble(new FakeCurrentBusinessUnit()).For<ICurrentBusinessUnit>();
			isolate.UseTestDouble<TeamsProvider>().For<ITeamsProvider>();
			isolate.UseTestDouble<Areas.Global.FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
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


		[Test]
		public void ShouldOrderNameByCultureWhenGetPermittedTeamHierachy()
		{
			var date = new DateOnly(2017, 6, 5);
			var bu = BusinessUnitFactory.CreateWithId("_");
			CurrentBusinessUnit.FakeBusinessUnit(bu);
			var site1 = SiteFactory.CreateSimpleSite("D").WithId();
			var site2 = SiteFactory.CreateSimpleSite("Ä").WithId();
			var site3 = SiteFactory.CreateSimpleSite("A").WithId();

			var team1 = TeamFactory.CreateTeamWithId("teamD");
			var team2 = TeamFactory.CreateTeamWithId("teamÄ");
			var team3 = TeamFactory.CreateTeamWithId("teamA");
			var team4 = TeamFactory.CreateTeamWithId("teamC");

			site1.AddTeam(team1);
			site2.AddTeam(team2);
			site2.AddTeam(team4);
			site3.AddTeam(team3);

			SiteRepository.Add(site1);
			SiteRepository.Add(site2);
			SiteRepository.Add(site3);

			TeamRepository.Add(team1);
			TeamRepository.Add(team2);
			TeamRepository.Add(team3);
			TeamRepository.Add(team4);

			CurrentLoggedOnUser.CurrentUser().PermissionInformation.SetUICulture(new CultureInfo("sv"));
			PermissionProvider.Enable();

			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team1, date);
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team2, date);
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team3, date);
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team4, date);

			var permittedHierachy = Target.GetPermittedTeamHierachy(date, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
			permittedHierachy.Children.Last().Name.Should().Be("Ä");
			permittedHierachy.Children.Last().Children.Last().Name.Should().Be("teamÄ");

			CurrentLoggedOnUser.CurrentUser().PermissionInformation.SetUICulture(new CultureInfo("en-US"));
			permittedHierachy = Target.GetPermittedTeamHierachy(date, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
			permittedHierachy.Children.Last().Name.Should().Be("D");
		}


		[Test]
		public void ShouldReturnPermittedTeamsHierachyWhenUserHasMyTeamSchedulesPermission_WithPeriod()
		{
			var date = new DateOnly(2016, 11, 29);
			var bu = BusinessUnitFactory.CreateWithId("_");
			CurrentBusinessUnit.FakeBusinessUnit(bu);
			var team = TeamFactory.CreateTeamWithId("myteam");
			TeamRepository.Add(team);
			var anotherTeam = TeamFactory.CreateTeamWithId("another").WithId();
			var site = SiteFactory.CreateSimpleSite("mysite").WithId();
			site.AddTeam(team);
			site.AddTeam(anotherTeam);
			GroupingReadOnlyRepository.Has(new[]
			{
				new ReadOnlyGroupPage
				{
					PageId = Group.PageMainId
				}
			}, new[]
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					SiteId = site.Id.Value,
					TeamId = team.Id.Value,
					GroupName = team.SiteAndTeam
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					SiteId = site.Id.Value,
					TeamId = anotherTeam.Id.Value,
					GroupName = anotherTeam.SiteAndTeam
				}
			});
			
			PermissionProvider.Enable();
			//PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, date, new PersonAuthorization
			{
				SiteId = site.Id.Value,
				TeamId = team.Id.Value
			});

			var permittedHierachy = Target.GetOrganizationWithPeriod(new DateOnlyPeriod(date, date), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			permittedHierachy.Children.Count.Should().Be.EqualTo(1);
			permittedHierachy.Name.Should().Be.EqualTo("_");
			permittedHierachy.Children.First().Name.Should().Be.EqualTo("mysite");
			permittedHierachy.Children.First().Children.Single().Name.Should().Be.EqualTo("myteam");
		}

	

		[Test]
		public void ShouldReturnLogonUserTeamIdWhenUserHasMyTeamSchedulesPermission_WithPeriod()
		{

			var date = new DateOnly(2016, 11, 29);
			var bu = BusinessUnitFactory.CreateWithId("_");
			CurrentBusinessUnit.FakeBusinessUnit(bu);
			var team = TeamFactory.CreateTeamWithId("myteam");
			TeamRepository.Add(team);
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

			GroupingReadOnlyRepository.Has(new[]
			{
				new ReadOnlyGroupPage
				{
					PageId = Group.PageMainId
				}
			}, new[]
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					SiteId = site.Id.Value,
					TeamId = team.Id.Value,
					GroupName = team.SiteAndTeam
				}

			});
			
			PermissionProvider.Enable();
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules,  date,new PersonAuthorization
			{
				SiteId = site.Id.Value,
				TeamId = team.Id.Value
			});

			var permittedHierachy = Target.GetOrganizationWithPeriod(new DateOnlyPeriod(date, date), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			permittedHierachy.Children.Count.Should().Be.EqualTo(1);
			permittedHierachy.Name.Should().Be.EqualTo("_");
			permittedHierachy.Children.Single().Name.Should().Be.EqualTo("mysite");
			permittedHierachy.Children.Single().Children.Single().Name.Should().Be.EqualTo("myteam");
			permittedHierachy.LogonUserTeamId.Should().Be(null);
		}

		[Test]
		public void ShouldReturnLogonUserTeamIdWhenUserHasMyOwnDataPermissionOnly_WithPeriod()
		{

			var date = new DateOnly(2016, 11, 29);
			var bu = BusinessUnitFactory.CreateWithId("_");
			CurrentBusinessUnit.FakeBusinessUnit(bu);
			var team = TeamFactory.CreateTeamWithId("myteam");
			TeamRepository.Add(team);
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
			GroupingReadOnlyRepository.Has(new[]
			{
				new ReadOnlyGroupPage
				{
					PageId = Group.PageMainId
				}
			}, new[]
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					SiteId = site.Id.Value,
					TeamId = team.Id.Value,
					GroupName = team.SiteAndTeam
				}
				
			});
			PermissionProvider.Enable();
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, me, date);

			var permittedHierachy = Target.GetOrganizationWithPeriod(new DateOnlyPeriod(date, date), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			permittedHierachy.Children.Count.Should().Be.EqualTo(1);
			permittedHierachy.Name.Should().Be.EqualTo("_");
			permittedHierachy.Children.First().Name.Should().Be.EqualTo("mysite");
			permittedHierachy.Children.First().Children.Single().Name.Should().Be.EqualTo("myteam");
			permittedHierachy.LogonUserTeamId.Should().Be(team.Id);
		}


		[Test]
		public void ShouldOrderNameByCultureWhenGetPermittedTeamHierachy_WithPeriod()
		{
			var date = new DateOnly(2017, 6, 5);
			var bu = BusinessUnitFactory.CreateWithId("_");
			CurrentBusinessUnit.FakeBusinessUnit(bu);
			var site1 = SiteFactory.CreateSimpleSite("D").WithId();
			var site2 = SiteFactory.CreateSimpleSite("Ä").WithId();
			var site3 = SiteFactory.CreateSimpleSite("A").WithId();

			var team1 = TeamFactory.CreateTeamWithId("teamD");
			TeamRepository.Add(team1);
			var team2 = TeamFactory.CreateTeamWithId("teamÄ");
			TeamRepository.Add(team2);
			var team3 = TeamFactory.CreateTeamWithId("teamA");
			TeamRepository.Add(team3);
			var team4 = TeamFactory.CreateTeamWithId("teamC");
			TeamRepository.Add(team4);

			site1.AddTeam(team1);
			site2.AddTeam(team2);
			site2.AddTeam(team4);
			site3.AddTeam(team3);

			GroupingReadOnlyRepository.Has(new[]
			{
				new ReadOnlyGroupPage
				{
					PageId = Group.PageMainId
				}
			}, new[]
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					SiteId = site1.Id.Value,
					TeamId = team1.Id.Value,
					GroupName = team1.SiteAndTeam
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					SiteId = site2.Id.Value,
					TeamId = team2.Id.Value,
					GroupName = team2.SiteAndTeam
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					SiteId = site3.Id.Value,
					TeamId = team3.Id.Value,
					GroupName = team3.SiteAndTeam
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					SiteId = site2.Id.Value,
					TeamId = team4.Id.Value,
					GroupName = team4.SiteAndTeam
				}
			});

			CurrentLoggedOnUser.CurrentUser().PermissionInformation.SetUICulture(new CultureInfo("sv"));
			PermissionProvider.Enable();

			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules,date, new PersonAuthorization
			{
				SiteId = site1.Id.Value,
				TeamId = team1.Id.Value
			});
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, date, new PersonAuthorization
			{
				SiteId = site2.Id.Value,
				TeamId = team2.Id.Value
			});
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, date, new PersonAuthorization
			{
				SiteId = site3.Id.Value,
				TeamId = team3.Id.Value
			});
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, date, new PersonAuthorization
			{
				SiteId = site2.Id.Value,
				TeamId = team4.Id.Value
			});

			var permittedHierachy = Target.GetOrganizationWithPeriod(new DateOnlyPeriod(date, date), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
			permittedHierachy.Children.Last().Name.Should().Be("Ä");
			permittedHierachy.Children.Last().Children.Last().Name.Should().Be("teamÄ");

			CurrentLoggedOnUser.CurrentUser().PermissionInformation.SetUICulture(new CultureInfo("en-US"));
			permittedHierachy = Target.GetOrganizationWithPeriod(new DateOnlyPeriod(date, date), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
			permittedHierachy.Children.Last().Name.Should().Be("D");
		}
	}
}

﻿using System.Globalization;
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
	public class TeamsProviderTest : ISetup
	{
		public Areas.Global.FakePermissionProvider PermissionProvider;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public TeamsProvider Target;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		private FakePersonRepository personRepository;
		public FakeLoggedOnUser CurrentLoggedOnUser;
		public FakePersonSelectorReadOnlyRepository PersonSelectorReadOnlyRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble(new FakeCurrentBusinessUnit()).For<ICurrentBusinessUnit>();
			system.UseTestDouble<TeamsProvider>().For<ITeamsProvider>();
			system.UseTestDouble<Areas.Global.FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			personRepository = new FakePersonRepositoryLegacy();
			system.UseTestDouble<FakePersonSelectorReadOnlyRepository>().For<IPersonSelectorReadOnlyRepository>();
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
			var anotherTeam = TeamFactory.CreateTeamWithId("another").WithId();
			var site = SiteFactory.CreateSimpleSite("mysite").WithId();
			site.AddTeam(team);
			site.AddTeam(anotherTeam);
			PersonSelectorReadOnlyRepository.Has(team);
			PersonSelectorReadOnlyRepository.Has(anotherTeam);
			PermissionProvider.Enable();
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team, date);

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
			PersonSelectorReadOnlyRepository.Has(team);
			PermissionProvider.Enable();
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team, date);

			var permittedHierachy = Target.GetOrganizationWithPeriod(new DateOnlyPeriod(date, date), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			permittedHierachy.Children.Count.Should().Be.EqualTo(1);
			permittedHierachy.Name.Should().Be.EqualTo("_");
			permittedHierachy.Children.First().Name.Should().Be.EqualTo("mysite");
			permittedHierachy.Children.First().Children.Single().Name.Should().Be.EqualTo("myteam");
			permittedHierachy.LogonUserTeamId.Should().Be(null);
		}

		[Test]
		public void ShouldReturnLogonUserTeamIdWhenUserHasMyOwnDataPermissionOnly_WithPeriod()
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

			PersonSelectorReadOnlyRepository.Has(team);
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
			var team2 = TeamFactory.CreateTeamWithId("teamÄ");
			var team3 = TeamFactory.CreateTeamWithId("teamA");
			var team4 = TeamFactory.CreateTeamWithId("teamC");

			site1.AddTeam(team1);
			site2.AddTeam(team2);
			site2.AddTeam(team4);
			site3.AddTeam(team3);

		
			PersonSelectorReadOnlyRepository.Has(team1);
			PersonSelectorReadOnlyRepository.Has(team2);
			PersonSelectorReadOnlyRepository.Has(team3);
			PersonSelectorReadOnlyRepository.Has(team4);

			CurrentLoggedOnUser.CurrentUser().PermissionInformation.SetUICulture(new CultureInfo("sv"));
			PermissionProvider.Enable();

			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team1, date);
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team2, date);
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team3, date);
			PermissionProvider.PermitTeam(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, team4, date);

			var permittedHierachy = Target.GetOrganizationWithPeriod(new DateOnlyPeriod(date, date), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
			permittedHierachy.Children.Last().Name.Should().Be("Ä");
			permittedHierachy.Children.Last().Children.Last().Name.Should().Be("teamÄ");

			CurrentLoggedOnUser.CurrentUser().PermissionInformation.SetUICulture(new CultureInfo("en-US"));
			permittedHierachy = Target.GetOrganizationWithPeriod(new DateOnlyPeriod(date, date), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
			permittedHierachy.Children.Last().Name.Should().Be("D");
		}
	}
}

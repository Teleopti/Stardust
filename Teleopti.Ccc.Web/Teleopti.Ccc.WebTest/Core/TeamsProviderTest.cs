using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
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
			personRepository = new FakePersonRepository();
		}
		[Test]
		public void ShouldReturnPermittedTeamsHierachy()
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

			var permittedHierachy = Target.GetPermittedTeamHierachy(date);

			permittedHierachy.Children.Count.Should().Be.EqualTo(1);
			permittedHierachy.Name.Should().Be.EqualTo("_");
			permittedHierachy.Children.First().Name.Should().Be.EqualTo("mysite");
			permittedHierachy.Children.First().Children.Single().Name.Should().Be.EqualTo("myteam");

		}

		[Test]
		public void ShouldReturnLogonUserTeamId()
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

			var permittedHierachy = Target.GetPermittedTeamHierachy(date);

			permittedHierachy.Children.Count.Should().Be.EqualTo(1);
			permittedHierachy.Name.Should().Be.EqualTo("_");
			permittedHierachy.Children.First().Name.Should().Be.EqualTo("mysite");
			permittedHierachy.Children.First().Children.Single().Name.Should().Be.EqualTo("myteam");
			permittedHierachy.LogonUserTeamId.Should().Be(team.Id);
		}
	}
}

using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class AgentBadgeProviderTest
	{
		private IAgentBadgeRepository agentBadgeRepository;
		private IPersonRepository personRepository;
		private IBadgeSettingProvider settingProvider;
		private AgentBadgeProvider target;
		private IPermissionProvider permissionProvider;
		private IPersonNameProvider personNameProvider;
		private ISiteRepository siteRepository;
		private ITeamRepository teamRepository;

		[SetUp]
		public void SetUp()
		{
			agentBadgeRepository = MockRepository.GenerateMock<IAgentBadgeRepository>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			settingProvider = MockRepository.GenerateMock<IBadgeSettingProvider>();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			personNameProvider = MockRepository.GenerateMock<IPersonNameProvider>();
			siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			target = new AgentBadgeProvider(agentBadgeRepository, permissionProvider, personRepository, settingProvider,
				personNameProvider, siteRepository, teamRepository);
		}



		[Test]
		public void ShouldQueryForEveryone()
		{
			personRepository.Stub(x => x.FindPeople(new Guid[] {Guid.NewGuid()}))
				.IgnoreArguments()
				.Return(new IPerson[] {new Person()});
			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};
			target.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option);

			agentBadgeRepository.AssertWasCalled(x => x.GetAllAgentBadges());
		}

		[Test]
		public void ShouldReturnBadgesForEveryoneAndMyOwn()
		{
			
			var agents = new IAgentBadge[] {new AgentBadge(), new AgentBadge()};
			var person0 = new Person();
			person0.Name = new Name("first1", "last1");
			var person1 = new Person();
			person1.Name = new Name("first2", "last2");
			var persons = new IPerson[] { person0, person1 };
			var personName0 = "first1 last1";
			var personName1 = "first2 last2";
			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};
			agentBadgeRepository.Stub(x => x.GetAllAgentBadges()).Return(agents);
			personRepository.Stub(x => x.FindPeople(agents.Select(item => item.Person).ToList())).IgnoreArguments().Return(persons);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, DateOnly.Today, person0)).Return(false);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, DateOnly.Today, person1)).Return(true);
			personNameProvider.Stub(x => x.BuildNameFromSetting(person0.Name)).Return(personName0);
			personNameProvider.Stub(x => x.BuildNameFromSetting(person1.Name)).Return(personName1);
			
			var result = target.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option);

			result.Single().AgentName.Should().Be(personName1);
		}

		[Test]
		public void ShouldReturnBadgesForSite()
		{

			var date = DateOnly.Today;
			var agents = new IAgentBadge[] { new AgentBadge(), new AgentBadge() };
			var siteId = Guid.NewGuid();
			var team0 = TeamFactory.CreateSimpleTeam("team0");
			var team1 = TeamFactory.CreateSimpleTeam("team1");
			var site = SiteFactory.CreateSiteWithTeams(new[] {team0, team1});
			var person0 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(date, team0);
			var person1 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(date, team1);
			person0.Name = new Name("first1", "last1");
			person1.Name = new Name("first2", "last2");
			const string personName0 = "first1 last1";
			const string personName1 = "first2 last2";
			siteRepository.Stub(x => x.Get(siteId)).Return(site);
			var option = new LeaderboardQuery
			{
				Date = date,
				SelectedId = siteId,
				Type = LeadboardQueryType.Site
			};
			agentBadgeRepository.Stub(x => x.Find(new []{person0.Id.Value, person1.Id.Value})).IgnoreArguments().Return(agents);
			personRepository.Stub(x => x.FindPeopleBelongTeam(team0,new DateOnlyPeriod(date.AddDays(-1),date))).Return(new []{person0});
			personRepository.Stub(x => x.FindPeopleBelongTeam(team1,new DateOnlyPeriod(date.AddDays(-1),date))).Return(new []{person1});
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, person0)).Return(true);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, person1)).Return(true);
			personNameProvider.Stub(x => x.BuildNameFromSetting(person0.Name)).Return(personName0);
			personNameProvider.Stub(x => x.BuildNameFromSetting(person1.Name)).Return(personName1);

			var result = target.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option);

			result.First().AgentName.Should().Be(personName0);
			result.Last().AgentName.Should().Be(personName1);
		}

		[Test]
		public void ShouldReturnBadgesForTeam()
		{
			var date = DateOnly.Today;
			var agents = new IAgentBadge[] { new AgentBadge(), new AgentBadge() };
			var teamId = Guid.NewGuid();
			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(teamId);
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(date, team);
			person.Name = new Name("first1", "last1");
			const string personName = "first1 last1";
			var option = new LeaderboardQuery
			{
				Date = date,
				SelectedId = team.Id.Value,
				Type = LeadboardQueryType.Team
			};
			agentBadgeRepository.Stub(x => x.Find(new[] { person.Id.Value })).IgnoreArguments().Return(agents);
			teamRepository.Stub(x => x.Get(team.Id.Value)).Return(team);
			personRepository.Stub(x => x.FindPeopleBelongTeam(team, new DateOnlyPeriod(date.AddDays(-1), date))).Return(new[] { person });
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, person)).Return(true);
			personNameProvider.Stub(x => x.BuildNameFromSetting(person.Name)).Return(personName);

			var result = target.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option);

			result.Single().AgentName.Should().Be(personName);
		}

		[Test]
		public void ShouldReturnCorrectTotalBadgeNumber()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var persons= new IPerson[]{person};
			var agents = new IAgentBadge[]
			{
				new AgentBadge
				{
					BadgeType = BadgeType.Adherence,
					TotalAmount = 16,
					Person = (Guid) person.Id
				}, 

				new AgentBadge
				{
					BadgeType = BadgeType.AnsweredCalls,
					TotalAmount = 25,
					Person = (Guid) person.Id
				},

				new AgentBadge
				{
					BadgeType = BadgeType.AverageHandlingTime,
					TotalAmount = 32,
					Person = (Guid) person.Id
				}
			};
			var setting = new AgentBadgeSettings()
			{
				BadgeEnabled = true,
				GoldToSilverBadgeRate = 2,
				SilverToBronzeBadgeRate = 5
			};
			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};
			agentBadgeRepository.Stub(x=>x.GetAllAgentBadges()).Return(agents);
			personRepository.Stub(x => x.FindPeople(agents.Select(item => item.Person).ToList())).IgnoreArguments().Return(persons);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, DateOnly.Today, persons.ElementAt(0))).Return(true);
			settingProvider.Stub(x => x.GetBadgeSettings()).Return(setting);
			var result = target.GetPermittedAgents( DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option).ToArray();
			result.Single().Gold.Should().Be(6);
			result.Single().Silver.Should().Be(2);
			result.Single().Bronze.Should().Be(3);

		}

	}
}
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class LeaderboardAgentBadgeProviderTest
	{
		private IAgentBadgeRepository agentBadgeRepository;
		private IAgentBadgeWithRankRepository agentBadgeWithRankRepository;
		private IPersonRepository personRepository;
		private IBadgeSettingProvider settingProvider;
		private LeaderboardAgentBadgeProvider target;
		private IPermissionProvider permissionProvider;
		private IPersonNameProvider personNameProvider;
		private ISiteRepository siteRepository;
		private ITeamRepository teamRepository;
		private IPerson person1;
		private IPerson person2;
		private IAgentBadgeSettings setting;
		private IList<IAgentBadge> agentBadges;
		private IList<IAgentBadgeWithRank> agentsWithRankedBadge;
		private const string personName1 = "first1 last1";
		private const string personName2 = "first2 last2";
		private DateOnly date;
		private Team team0;
		private Team team1;
		private ISite site;
		private  IToggleManager _toggleManager;

		[SetUp]
		public void SetUp()
		{
			date = DateOnly.Today;
			team0 = TeamFactory.CreateSimpleTeam("team0");
			team1 = TeamFactory.CreateSimpleTeam("team1");
			site = SiteFactory.CreateSiteWithTeams(new[] { team0, team1 });
			person1 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(date, team0);
			person2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(date, team1);
			person1.Name = new Name("first1", "last1");
			person2.Name = new Name("first2", "last2");
			_toggleManager = MockRepository.GenerateMock<IToggleManager>();
			_toggleManager.Stub(x => x.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185)).Return(true);
			setting = new AgentBadgeSettings
			{
				BadgeEnabled = true,
				CalculateBadgeWithRank = false,
				GoldToSilverBadgeRate = 2,
				SilverToBronzeBadgeRate = 5
			};
			agentBadges = new IAgentBadge[]
			{
				new AgentBadge
				{
					BadgeType = BadgeType.Adherence,
					TotalAmount = 16,
					Person = (Guid) person1.Id
				}, 

				new AgentBadge
				{
					BadgeType = BadgeType.AnsweredCalls,
					TotalAmount = 25,
					Person = (Guid) person2.Id
				},

				new AgentBadge
				{
					BadgeType = BadgeType.AverageHandlingTime,
					TotalAmount = 32,
					Person = (Guid) person1.Id
				}
			};
			agentBadgeRepository = MockRepository.GenerateMock<IAgentBadgeRepository>();


			agentsWithRankedBadge = new IAgentBadgeWithRank[]
			{
				new AgentBadgeWithRank
				{
					Person = (Guid) person1.Id,
					BadgeType = BadgeType.Adherence,
					GoldBadgeAmount = 0,
					SilverBadgeAmount = 1,
					BronzeBadgeAmount = 3
				}, 
				new AgentBadgeWithRank
				{
					Person = (Guid) person1.Id,
					BadgeType = BadgeType.AnsweredCalls,
					GoldBadgeAmount = 3,
					SilverBadgeAmount = 2,
					BronzeBadgeAmount = 1
				}, 
				new AgentBadgeWithRank
				{
					Person = (Guid) person1.Id,
					BadgeType = BadgeType.AverageHandlingTime,
					GoldBadgeAmount = 4,
					SilverBadgeAmount = 0,
					BronzeBadgeAmount = 9
				} 
			};
			agentBadgeWithRankRepository = MockRepository.GenerateMock<IAgentBadgeWithRankRepository>();


			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			settingProvider = MockRepository.GenerateMock<IBadgeSettingProvider>();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			personNameProvider = MockRepository.GenerateMock<IPersonNameProvider>();
			siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			teamRepository = MockRepository.GenerateMock<ITeamRepository>();

			personNameProvider.Stub(x => x.BuildNameFromSetting(person1.Name)).Return(personName1);
			personNameProvider.Stub(x => x.BuildNameFromSetting(person2.Name)).Return(personName2);
			settingProvider.Stub(x => x.GetBadgeSettings()).Return(setting);

			target = new LeaderboardAgentBadgeProvider(agentBadgeRepository, agentBadgeWithRankRepository, permissionProvider,
				personRepository, settingProvider, personNameProvider, siteRepository, teamRepository, _toggleManager);
		}

		[Test]
		public void ShouldQueryForEveryone()
		{
			personRepository.Stub(x => x.FindPeople(new[] {Guid.NewGuid()}))
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
			var persons = new[] { person1, person2 };
			
			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};
			agentBadgeRepository.Stub(x => x.GetAllAgentBadges()).Return(agentBadges);
			personRepository.Stub(x => x.FindPeople(agentBadges.Select(item => item.Person).ToList())).IgnoreArguments().Return(persons);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, DateOnly.Today, person1)).Return(false);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, DateOnly.Today, person2)).Return(true);
			
			var result = target.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option);

			result.Single().AgentName.Should().Be(personName2);
		}

		[Test]
		public void ShouldReturnBadgesForSite()
		{
			var siteId = Guid.NewGuid();
			siteRepository.Stub(x => x.Get(siteId)).Return(site);
			var option = new LeaderboardQuery
			{
				Date = date,
				SelectedId = siteId,
				Type = LeadboardQueryType.Site
			};
			agentBadgeRepository.Stub(x => x.Find(new[] { person2.Id.Value, person1.Id.Value })).IgnoreArguments().Return(agentBadges);
			agentBadgeWithRankRepository.Stub(x => x.Find(new[] { person2.Id.Value, person1.Id.Value })).IgnoreArguments().Return(agentsWithRankedBadge);
			personRepository.Stub(x => x.FindPeopleBelongTeam(team0,new DateOnlyPeriod(date.AddDays(-1),date))).Return(new []{person1});
			personRepository.Stub(x => x.FindPeopleBelongTeam(team1,new DateOnlyPeriod(date.AddDays(-1),date))).Return(new []{person2});
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, person1)).Return(true);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, person2)).Return(true);
			
			var result = target.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option).ToList();

			result.First().AgentName.Should().Be(personName1);
			result.Last().AgentName.Should().Be(personName2);
		}

		[Test]
		public void ShouldReturnBadgesForTeam()
		{
			var teamId = Guid.NewGuid();
			var option = new LeaderboardQuery
			{
				Date = date,
				SelectedId = teamId,
				Type = LeadboardQueryType.Team
			};
			agentBadgeRepository.Stub(x => x.Find(new[] { person2.Id.Value, person1.Id.Value })).IgnoreArguments().Return(agentBadges);
			agentBadgeWithRankRepository.Stub(x => x.Find(new[] { person2.Id.Value, person1.Id.Value })).IgnoreArguments().Return(agentsWithRankedBadge);
			teamRepository.Stub(x => x.Get(teamId)).Return(team0);
			personRepository.Stub(x => x.FindPeopleBelongTeam(team0, new DateOnlyPeriod(date.AddDays(-1), date))).Return(new[] { person1, person2 });
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, person1)).Return(true);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, person2)).Return(true);

			var result = target.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option).ToList();

			result.First().AgentName.Should().Be(personName1);
			result.Last().AgentName.Should().Be(personName2);
		}		

		[Test]
		public void ShouldReturnTotalBadgeCount()
		{
			var personId = Guid.NewGuid();
			var person = new Person();
			person.SetId(personId);
			var persons = new IPerson[] { person };

			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};

			var agentsWithBadge = new IAgentBadge[]
			{
				new AgentBadge
				{
					BadgeType = BadgeType.Adherence,
					TotalAmount = 16, // 1 Gold, 1 Silver, 1 Bronze
					Person = personId
				}, 

				new AgentBadge
				{
					BadgeType = BadgeType.AnsweredCalls,
					TotalAmount = 25, // 2 Gold, 1 Silver, 0 Bronze
					Person = personId
				},

				new AgentBadge
				{
					BadgeType = BadgeType.AverageHandlingTime,
					TotalAmount = 32, // 3 Gold, 0 Silver, 2 Bronze
					Person = personId
				}
			};
			agentBadgeRepository.Stub(x => x.GetAllAgentBadges()).Return(agentsWithBadge);

			var agentsWithRankedBadge = new IAgentBadgeWithRank[]
			{
				new AgentBadgeWithRank
				{
					Person = personId,
					BadgeType = BadgeType.Adherence,
					GoldBadgeAmount = 0,
					SilverBadgeAmount = 1,
					BronzeBadgeAmount = 3
				}, 
				new AgentBadgeWithRank
				{
					Person = personId,
					BadgeType = BadgeType.AnsweredCalls,
					GoldBadgeAmount = 3,
					SilverBadgeAmount = 2,
					BronzeBadgeAmount = 1
				}, 
				new AgentBadgeWithRank
				{
					Person = personId,
					BadgeType = BadgeType.AverageHandlingTime,
					GoldBadgeAmount = 4,
					SilverBadgeAmount = 0,
					BronzeBadgeAmount = 9
				} 
			};
			agentBadgeWithRankRepository.Stub(x => x.GetAllAgentBadges()).Return(agentsWithRankedBadge);

			personRepository.Stub(x => x.FindPeople(new List<Guid>{personId})).IgnoreArguments().Return(persons);
			permissionProvider.Stub(
				x =>
					x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, DateOnly.Today,
						persons.ElementAt(0))).Return(true);

			
			var result = target.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option).ToArray();

			result.Single().Gold.Should().Be(13);
			result.Single().Silver.Should().Be(5);
			result.Single().Bronze.Should().Be(16);
		}
	}
}
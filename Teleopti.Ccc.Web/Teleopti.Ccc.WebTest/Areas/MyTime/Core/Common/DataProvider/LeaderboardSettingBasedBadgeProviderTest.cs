using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class LeaderboardSettingBasedBadgeProviderTest
	{
		private FakeAgentBadgeRepository agentBadgeRepository;
		private FakeAgentBadgeWithRankRepository agentBadgeWithRankRepository;
		private LeaderboardSettingBasedBadgeProvider target;
		private IPermissionProvider permissionProvider;
		private ISiteRepository siteRepository;
		private ITeamRepository teamRepository;
		private FakeAgentBadgeTransactionRepository agentBadgeTransactionRepository;
		private FakeAgentBadgeWithRankTransactionRepository agentBadgeWithRankTransactionRepository;

		private readonly ReadOnlyGroupDetail personDetail1 = new ReadOnlyGroupDetail
		{
			PersonId = Guid.NewGuid(),
			FirstName = "first1",
			LastName = "last1"
		};

		private IPerson person1;

		private readonly ReadOnlyGroupDetail personDetail2 = new ReadOnlyGroupDetail
		{
			PersonId = Guid.NewGuid(),
			FirstName = "first2",
			LastName = "last2"
		};

		private IPerson person2;

		private readonly IGamificationSetting setting1 = new GamificationSetting("setting1")
		{
			GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithRatioConvertor,
			GoldToSilverBadgeRate = 5,
			SilverToBronzeBadgeRate = 5
		};

		private readonly IGamificationSetting setting2 = new GamificationSetting("setting2")
		{
			GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold,
		};

		private IList<AgentBadge> agentBadges;
		private IList<IAgentBadgeWithRank> agentsWithRankedBadge;
		private const string personName1 = "first1 last1";
		private const string personName2 = "first2 last2";
		private DateOnly date;
		private Team team0;
		private Team team1;
		private IGroupingReadOnlyRepository groupingRepository;
		private ITeamGamificationSettingRepository teamSettingRepository;
		private FakePersonRepository personRepository;
		private FakeGlobalSettingDataRepository settingDataRepository;

		[SetUp]
		public void SetUp()
		{
			date = DateOnly.Today;
			team0 = TeamFactory.CreateSimpleTeam("team0");
			team0.SetId(Guid.NewGuid());
			team1 = TeamFactory.CreateSimpleTeam("team1");
			team1.SetId(Guid.NewGuid());

			agentBadges = new[]
			{
				new AgentBadge
				{
					BadgeType = BadgeType.Adherence,
					TotalAmount = 16,
					Person = personDetail1.PersonId
				},

				new AgentBadge
				{
					BadgeType = BadgeType.AnsweredCalls,
					TotalAmount = 25,
					Person = personDetail2.PersonId
				},

				new AgentBadge
				{
					BadgeType = BadgeType.AverageHandlingTime,
					TotalAmount = 32,
					Person = personDetail1.PersonId
				}
			};

			teamSettingRepository = MockRepository.GenerateMock<ITeamGamificationSettingRepository>();
			teamSettingRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new List<ITeamGamificationSetting>
				{
					new TeamGamificationSetting {Team = team0, GamificationSetting = setting1},
					new TeamGamificationSetting {Team = team1, GamificationSetting = setting2}
				});

			person1 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(date, team0);
			person1.WithId(personDetail1.PersonId);
			person1.WithName(new Name("first1", "last1"));
			person1.SetEmploymentNumber("1");
			person2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(date, team1);
			person2.WithId(personDetail2.PersonId);
			person2.WithName(new Name("first2", "last2"));
			person2.SetEmploymentNumber("2");
			personRepository = new FakePersonRepository(null);
			personRepository.Has(new List<IPerson> { person1, person2 });

			agentBadgeRepository = new FakeAgentBadgeRepository();
			agentBadgeRepository.Add(agentBadges);

			agentsWithRankedBadge = new IAgentBadgeWithRank[]
			{
				new AgentBadgeWithRank
				{
					Person = personDetail1.PersonId,
					BadgeType = BadgeType.Adherence,
					GoldBadgeAmount = 0,
					SilverBadgeAmount = 1,
					BronzeBadgeAmount = 3
				},
				new AgentBadgeWithRank
				{
					Person = personDetail1.PersonId,
					BadgeType = BadgeType.AnsweredCalls,
					GoldBadgeAmount = 3,
					SilverBadgeAmount = 2,
					BronzeBadgeAmount = 1
				},
				new AgentBadgeWithRank
				{
					Person = personDetail2.PersonId,
					BadgeType = BadgeType.AverageHandlingTime,
					GoldBadgeAmount = 4,
					SilverBadgeAmount = 0,
					BronzeBadgeAmount = 9
				}
			};
			agentBadgeWithRankRepository = new FakeAgentBadgeWithRankRepository();
			agentBadgeWithRankRepository.Add(agentsWithRankedBadge);

			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			groupingRepository = MockRepository.GenerateMock<IGroupingReadOnlyRepository>();
			agentBadgeTransactionRepository = new FakeAgentBadgeTransactionRepository();
			agentBadgeWithRankTransactionRepository = new FakeAgentBadgeWithRankTransactionRepository();

			var personalSettingDataRepository = new FakePersonalSettingDataRepository();
			personalSettingDataRepository.PersistSettingValue(NameFormatSettings.Key, new NameFormatSettings {NameFormatId = 0});
			settingDataRepository = new FakeGlobalSettingDataRepository();
			settingDataRepository.PersistSettingValue("CommonNameDescription", new CommonNameDescriptionSetting());


			target = new LeaderboardSettingBasedBadgeProvider(agentBadgeRepository, agentBadgeWithRankRepository,
				permissionProvider, siteRepository, teamRepository, groupingRepository,
				teamSettingRepository, personRepository, agentBadgeTransactionRepository,agentBadgeWithRankTransactionRepository,
				settingDataRepository);
		}

		[Test]
		public void ShouldGenerateAgentNameBySetting()
		{
			var commonNameDescriptionSetting = settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());
			commonNameDescriptionSetting.AliasFormat = "{EmployeeNumber} {FirstName} {LastName}";

			var result = target.GetAgentBadgeOverviewsForPeople(new[] { person1.Id.Value, person2.Id.Value }, DateOnly.Today);

			result.First().AgentName.Should().Be("1 "+personName1);
		}

		[Test]
		public void ShouldQueryForEveryone()
		{
			groupingRepository.Stub(x => x.AvailableGroups((ReadOnlyGroupPage)null, date))
				.IgnoreArguments()
				.Return(new[] {new ReadOnlyGroupDetail()});
			groupingRepository.Stub(x => x.DetailsForGroup(Guid.Empty, date)).Return(new[] {personDetail1});
			
			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail1))
				.Return(true);
			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};

			target.PermittedAgentBadgeOverviewsForEveryoneOrMyOwn(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard,
				option);

			Assert.IsTrue(agentBadgeRepository.FindByPersonListCalledTimes() > 0);
		}

		[Test]
		public void ShouldGetBadgesForPeople()
		{
			var result = target.GetAgentBadgeOverviewsForPeople(new[] {person1.Id.Value, person2.Id.Value},DateOnly.Today);

			result.First().AgentName.Should().Be(personName1);
		}

		[Test]
		public void ShouldReturnBadgesForEveryoneAndMyOwn()
		{
			var persons = new[]
			{
				personDetail1, personDetail2
			};

			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};

			groupingRepository.Stub(x => x.AvailableGroups(null, date))
				.IgnoreArguments()
				.Return(new[] {new ReadOnlyGroupDetail()});
			groupingRepository.Stub(x => x.DetailsForGroup(Guid.Empty, date)).Return(persons);

			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail1))
				.Return(false);
			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail2))
				.Return(true);

			var result =
				target.PermittedAgentBadgeOverviewsForEveryoneOrMyOwn(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard,
					option);

			result.Single().AgentName.Should().Be(personName2);
		}

		[Test]
		public void ShouldReturnBadgesForSite()
		{
			var site = SiteFactory.CreateSiteWithTeams(new[] {team0, team1});
			site.SetId(Guid.NewGuid());
			siteRepository.Stub(x => x.Get(site.Id.GetValueOrDefault())).Return(site);
			var option = new LeaderboardQuery
			{
				Date = date,
				SelectedId = site.Id.GetValueOrDefault(),
				Type = LeadboardQueryType.Site
			};
			groupingRepository.Stub(x => x.DetailsForGroup(team0.Id.GetValueOrDefault(), date)).Return(new[] {personDetail1});
			groupingRepository.Stub(x => x.DetailsForGroup(team1.Id.GetValueOrDefault(), date)).Return(new[] {personDetail2});

			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail1))
				.Return(true);
			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail2))
				.Return(true);

			var result =
				target.PermittedAgentBadgeOverviewsForSite(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option)
					.ToList();

			result.First().AgentName.Should().Be(personName1);
			result.Last().AgentName.Should().Be(personName2);
		}

		[Test]
		public void ShouldReturnBadgesForTeam()
		{
			var option = new LeaderboardQuery
			{
				Date = date,
				SelectedId = team0.Id.GetValueOrDefault(),
				Type = LeadboardQueryType.Team
			};
			agentBadgeRepository.Remove(agentBadges[1]);
			agentBadgeWithRankRepository.Remove(agentsWithRankedBadge[2]);
			teamRepository.Stub(x => x.Get(team0.Id.GetValueOrDefault())).Return(team0);
			groupingRepository.Stub(x => x.DetailsForGroup(team0.Id.GetValueOrDefault(), date))
				.Return(new[] {personDetail1, personDetail2});
			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail1))
				.Return(true);
			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail2))
				.Return(true);

			var result =
				target.PermittedAgentBadgeOverviewsForTeam(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option)
					.ToList();

			result.Single().AgentName.Should().Be(personName1);
		}

		[Test]
		public void ShouldReturnTotalBadgeCount()
		{
			var persons = new[] {personDetail1};

			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};

			agentBadges = new[]
			{
				new AgentBadge
				{
					BadgeType = BadgeType.Adherence,
					TotalAmount = 31, // 1 Gold, 1 Silver, 1 Bronze
					Person = personDetail1.PersonId
				},

				new AgentBadge
				{
					BadgeType = BadgeType.AnsweredCalls,
					TotalAmount = 55, // 2 Gold, 1 Silver, 0 Bronze
					Person = personDetail1.PersonId
				},

				new AgentBadge
				{
					BadgeType = BadgeType.AverageHandlingTime,
					TotalAmount = 77, // 3 Gold, 0 Silver, 2 Bronze
					Person = personDetail1.PersonId
				}
			};
			agentBadgeRepository.ClearAll();
			agentBadgeRepository.Add(agentBadges);

			agentsWithRankedBadge = new IAgentBadgeWithRank[]
			{
				new AgentBadgeWithRank
				{
					Person = personDetail1.PersonId,
					BadgeType = BadgeType.Adherence,
					GoldBadgeAmount = 0,
					SilverBadgeAmount = 1,
					BronzeBadgeAmount = 3
				},
				new AgentBadgeWithRank
				{
					Person = personDetail1.PersonId,
					BadgeType = BadgeType.AnsweredCalls,
					GoldBadgeAmount = 3,
					SilverBadgeAmount = 2,
					BronzeBadgeAmount = 1
				},
				new AgentBadgeWithRank
				{
					Person = personDetail1.PersonId,
					BadgeType = BadgeType.AverageHandlingTime,
					GoldBadgeAmount = 4,
					SilverBadgeAmount = 0,
					BronzeBadgeAmount = 9
				}
			};
			agentBadgeWithRankRepository.ClearAll();
			agentBadgeWithRankRepository.Add(agentsWithRankedBadge);

			var teamGroupDetail = new ReadOnlyGroupDetail {GroupId = Guid.NewGuid()};
			groupingRepository.Stub(x => x.AvailableGroups(new ReadOnlyGroupPage(), date))
				.IgnoreArguments()
				.Return(new[] {teamGroupDetail});
			groupingRepository.Stub(x => x.DetailsForGroup(teamGroupDetail.GroupId, date)).Return(persons);
			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, DateOnly.Today,
						personDetail1)).Return(true);

			var result =
				target.PermittedAgentBadgeOverviewsForEveryoneOrMyOwn(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard,
					option).ToArray();

			result.Single().Gold.Should().Be(13);
			result.Single().Silver.Should().Be(5);
			result.Single().Bronze.Should().Be(16);
		}

		[Test]
		public void ShouldReturnTotalBadgeCountWithinPeriod()
		{
			var person1 = PersonFactory.CreatePerson("a");
			person1.WithId(personDetail1.PersonId);
		
			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};

			var agentBadgeTransactions = new[]
			{
				new AgentBadgeTransaction
				{
					BadgeType = BadgeType.Adherence,
					Amount = 7,
					Person = person1,
					CalculatedDate = new DateOnly(2014, 10, 1)
				},
				new AgentBadgeTransaction
				{
					BadgeType = BadgeType.Adherence,
					Amount = 7,
					Person = person1,
					CalculatedDate = new DateOnly(2014, 10, 15)
				},
				new AgentBadgeTransaction
				{
					BadgeType = BadgeType.Adherence,
					Amount = 7,
					Person = person1,
					CalculatedDate = new DateOnly(2014, 10, 20)
				}
			};

			foreach (var agentBadgeTransaction in agentBadgeTransactions)
			{
				agentBadgeTransactionRepository.Add(agentBadgeTransaction);
			}

			var agentBadgeWithRankTransactions = new[]
			{
				new AgentBadgeWithRankTransaction
				{
					BadgeType = BadgeType.Adherence,					
					Person = person1,
					CalculatedDate = new DateOnly(2014, 10, 1),
					GoldBadgeAmount = 1,
					SilverBadgeAmount = 1,
					BronzeBadgeAmount = 1
				},
				new AgentBadgeWithRankTransaction
				{
					BadgeType = BadgeType.Adherence,					
					Person = person1,
					CalculatedDate = new DateOnly(2014, 10, 15),
					GoldBadgeAmount = 1,
					SilverBadgeAmount = 1,
					BronzeBadgeAmount = 1
				},
				new AgentBadgeWithRankTransaction
				{
					BadgeType = BadgeType.Adherence,
					Person = person1,
					CalculatedDate = new DateOnly(2014, 10, 20),
					GoldBadgeAmount = 1,
					SilverBadgeAmount = 1,
					BronzeBadgeAmount = 1
				}
			};

			foreach (var agentBadgeWithRankTransaction in agentBadgeWithRankTransactions)
			{
				agentBadgeWithRankTransactionRepository.Add(agentBadgeWithRankTransaction);
			}

			var teamGroupDetail = new ReadOnlyGroupDetail { GroupId = Guid.NewGuid() };
			groupingRepository.Stub(x => x.AvailableGroups(new ReadOnlyGroupPage(),date))
				.IgnoreArguments()
				.Return(new[] { teamGroupDetail });
			groupingRepository.Stub(x => x.DetailsForGroup(teamGroupDetail.GroupId,date)).Return(new[] { personDetail1 });
			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard,DateOnly.Today,
						personDetail1)).Return(true);

			var result =
				target.PermittedAgentBadgeOverviewsForEveryoneOrMyOwn(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard,
					option, new DateOnlyPeriod(2014, 10, 1, 2014, 10, 15)).ToArray();

			result.Single().Gold.Should().Be(2);
			result.Single().Silver.Should().Be(4);
			result.Single().Bronze.Should().Be(6);
		}
	}
}
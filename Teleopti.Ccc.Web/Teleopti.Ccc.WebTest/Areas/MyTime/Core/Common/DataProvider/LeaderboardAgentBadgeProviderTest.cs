using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
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
		private IBadgeSettingProvider settingProvider;
		private LeaderboardAgentBadgeProvider target;
		private IPermissionProvider permissionProvider;
		private ISiteRepository siteRepository;
		private ITeamRepository teamRepository;
		private readonly ReadOnlyGroupDetail personDetail1 = new ReadOnlyGroupDetail { PersonId = new Guid("65228D77-67BB-4474-93FC-C58CC49CD5E8"), FirstName = "first1", LastName = "last1" };
		private readonly ReadOnlyGroupDetail personDetail2 = new ReadOnlyGroupDetail { PersonId = new Guid("99228D77-67BB-4474-93FC-C58CC49CD5E8"), FirstName = "first2", LastName = "last2" };
		private readonly IAgentBadgeSettings setting = new AgentBadgeSettings
			{
				BadgeEnabled = true,
				CalculateBadgeWithRank = false,
				GoldToSilverBadgeRate = 2,
				SilverToBronzeBadgeRate = 5
			};
		private IList<IAgentBadge> agentBadges;
		private IList<IAgentBadgeWithRank> agentsWithRankedBadge;
		private const string personName1 = "first1 last1";
		private const string personName2 = "first2 last2";
		private DateOnly date;
		private Team team0;
		private Team team1;
		private  IToggleManager _toggleManager;
		private IGroupingReadOnlyRepository groupingRepository;

		[SetUp]
		public void SetUp()
		{
			date = DateOnly.Today;
			team0 = TeamFactory.CreateSimpleTeam("team0");
			team0.SetId(Guid.NewGuid());
			team1 = TeamFactory.CreateSimpleTeam("team1");
			team1.SetId(Guid.NewGuid());
			
			_toggleManager = MockRepository.GenerateMock<IToggleManager>();
			_toggleManager.Stub(x => x.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185)).Return(true);
			agentBadges = new IAgentBadge[]
			{
				new AgentBadge
				{
					BadgeType = BadgeType.Adherence,
					TotalAmount = 16,
					Person = (Guid) personDetail1.PersonId
				}, 

				new AgentBadge
				{
					BadgeType = BadgeType.AnsweredCalls,
					TotalAmount = 25,
					Person = (Guid) personDetail2.PersonId
				},

				new AgentBadge
				{
					BadgeType = BadgeType.AverageHandlingTime,
					TotalAmount = 32,
					Person = (Guid) personDetail1.PersonId
				}
			};
			agentBadgeRepository = MockRepository.GenerateMock<IAgentBadgeRepository>();


			agentsWithRankedBadge = new IAgentBadgeWithRank[]
			{
				new AgentBadgeWithRank
				{
					Person = (Guid) personDetail1.PersonId,
					BadgeType = BadgeType.Adherence,
					GoldBadgeAmount = 0,
					SilverBadgeAmount = 1,
					BronzeBadgeAmount = 3
				}, 
				new AgentBadgeWithRank
				{
					Person = (Guid) personDetail1.PersonId,
					BadgeType = BadgeType.AnsweredCalls,
					GoldBadgeAmount = 3,
					SilverBadgeAmount = 2,
					BronzeBadgeAmount = 1
				}, 
				new AgentBadgeWithRank
				{
					Person = (Guid) personDetail1.PersonId,
					BadgeType = BadgeType.AverageHandlingTime,
					GoldBadgeAmount = 4,
					SilverBadgeAmount = 0,
					BronzeBadgeAmount = 9
				} 
			};
			agentBadgeWithRankRepository = MockRepository.GenerateMock<IAgentBadgeWithRankRepository>();

			settingProvider = MockRepository.GenerateMock<IBadgeSettingProvider>();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			groupingRepository = MockRepository.GenerateMock<IGroupingReadOnlyRepository>();

			settingProvider.Stub(x => x.GetBadgeSettings()).Return(setting);

			var nameProvider = new PersonNameProvider(new FakeNameFormatSettingsPersisterAndProvider(new NameFormatSettings{NameFormatId = 0}));
			target = new LeaderboardAgentBadgeProvider(agentBadgeRepository, agentBadgeWithRankRepository, permissionProvider,
				settingProvider, nameProvider, siteRepository, teamRepository, _toggleManager, groupingRepository);
		}

		[Test]
		public void ShouldQueryForEveryone()
		{
			var groupDetail = new ReadOnlyGroupDetail(){GroupId = new Guid("54228D77-67BB-4474-93FC-C58CC49CD5E9")};
			var personDetail = new ReadOnlyGroupDetail() { PersonId = new Guid("65228D77-67BB-4474-93FC-C58CC49CD5E8") };
			groupingRepository.Stub(x => x.AvailableGroups(null, date))
				.IgnoreArguments()
				.Return(new [] { groupDetail });
			groupingRepository.Stub(x => x.DetailsForGroup(groupDetail.GroupId, date)).Return(new[] {personDetail});
			agentBadgeRepository.Stub(x => x.Find(new[] {personDetail.PersonId})).Return(new Collection<IAgentBadge>());
			agentBadgeWithRankRepository.Stub(x => x.Find(new[] { personDetail.PersonId })).Return(new Collection<IAgentBadgeWithRank>());
			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail))
				.Return(true);
			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};

			target.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option);

			agentBadgeRepository.AssertWasCalled(x => x.Find(new[]{personDetail.PersonId}));
		}

		[Test]
		public void ShouldReturnBadgesForEveryoneAndMyOwn()
		{
			
			var persons = new[]
			{
				personDetail1,personDetail2
			};
			
			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};

			groupingRepository.Stub(x => x.AvailableGroups(null, date))
				.IgnoreArguments()
				.Return(new[] { new ReadOnlyGroupDetail() });
			groupingRepository.Stub(x => x.DetailsForGroup(Guid.Empty, date)).Return(persons);
			agentBadgeRepository.Stub(x => x.Find(new[] { personDetail2.PersonId })).Return(agentBadges.Where(b => b.Person == personDetail2.PersonId).ToArray());
			agentBadgeWithRankRepository.Stub(x => x.Find(new[] { personDetail2.PersonId })).Return(new List<IAgentBadgeWithRank>());
			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail1))
				.Return(false);
			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail2))
				.Return(true);
			
			var result = target.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option);

			result.Single().AgentName.Should().Be(personName2);
		}
		
		[Test]
		public void ShouldReturnBadgesForSite()
		{
			var site = SiteFactory.CreateSiteWithTeams(new[] { team0, team1 });
			site.SetId(Guid.NewGuid());
			siteRepository.Stub(x => x.Get(site.Id.GetValueOrDefault())).Return(site);
			var option = new LeaderboardQuery
			{
				Date = date,
				SelectedId = site.Id.GetValueOrDefault(),
				Type = LeadboardQueryType.Site
			};
			agentBadgeRepository.Stub(x => x.Find(new[] {personDetail1.PersonId, personDetail2.PersonId })).Return(agentBadges);
			agentBadgeWithRankRepository.Stub(x => x.Find(new[] { personDetail1.PersonId, personDetail2.PersonId })).Return(agentsWithRankedBadge);
			groupingRepository.Stub(x => x.DetailsForGroup(team0.Id.GetValueOrDefault(), date)).Return(new []{personDetail1});
			groupingRepository.Stub(x => x.DetailsForGroup(team1.Id.GetValueOrDefault(), date)).Return(new []{personDetail2});


			permissionProvider.Stub(x => x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail1)).Return(true);
			permissionProvider.Stub(x => x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail2)).Return(true);
			
			var result = target.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option).ToList();

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
			agentBadgeRepository.Stub(x => x.Find(new[] { personDetail1.PersonId, personDetail2.PersonId })).Return(agentBadges);
			agentBadgeWithRankRepository.Stub(x => x.Find(new[] { personDetail1.PersonId, personDetail2.PersonId })).Return(agentsWithRankedBadge);
			teamRepository.Stub(x => x.Get(team0.Id.GetValueOrDefault())).Return(team0);
			groupingRepository.Stub(x => x.DetailsForGroup(team0.Id.GetValueOrDefault(), date)).Return(new[] { personDetail1, personDetail2 });
			permissionProvider.Stub(x => x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail1)).Return(true);
			permissionProvider.Stub(x => x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, date, personDetail2)).Return(true);

			var result = target.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option).ToList();

			result.First().AgentName.Should().Be(personName1);
			result.Last().AgentName.Should().Be(personName2);
		}

		[Test]
		public void ShouldReturnTotalBadgeCount()
		{
			var persons = new [] {personDetail1};

			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};

			agentBadges = new IAgentBadge[]
			{
				new AgentBadge
				{
					BadgeType = BadgeType.Adherence,
					TotalAmount = 16, // 1 Gold, 1 Silver, 1 Bronze
					Person = personDetail1.PersonId
				},

				new AgentBadge
				{
					BadgeType = BadgeType.AnsweredCalls,
					TotalAmount = 25, // 2 Gold, 1 Silver, 0 Bronze
					Person = personDetail1.PersonId
				},

				new AgentBadge
				{
					BadgeType = BadgeType.AverageHandlingTime,
					TotalAmount = 32, // 3 Gold, 0 Silver, 2 Bronze
					Person = personDetail1.PersonId
				}
			};
			agentBadgeRepository.Stub(x => x.Find(new[] { personDetail1.PersonId })).Return(agentBadges);

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
			agentBadgeWithRankRepository.Stub(x => x.Find(new []{personDetail1.PersonId})).Return(agentsWithRankedBadge);

			var teamGroupDetail = new ReadOnlyGroupDetail{GroupId = Guid.NewGuid()};
			groupingRepository.Stub(x => x.AvailableGroups(new ReadOnlyGroupPage(), date)).IgnoreArguments().Return(new[] {teamGroupDetail});
			groupingRepository.Stub(x => x.DetailsForGroup(teamGroupDetail.GroupId,date)).Return(persons);
			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, DateOnly.Today,
						personDetail1)).Return(true);

			var result = target.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option).ToArray();

			result.Single().Gold.Should().Be(13);
			result.Single().Silver.Should().Be(5);
			result.Single().Bronze.Should().Be(16);
		}
	}

	public class FakeNameFormatSettingsPersisterAndProvider : ISettingsPersisterAndProvider<NameFormatSettings>
	{
		private NameFormatSettings _setting;

		public FakeNameFormatSettingsPersisterAndProvider(NameFormatSettings setting)
		{
			_setting = setting;
		}

		public NameFormatSettings Persist(NameFormatSettings isActive)
		{
			_setting = isActive;
			return isActive;
		}

		public NameFormatSettings Get()
		{
			return _setting;
		}

		public NameFormatSettings GetByOwner(IPerson person)
		{
			return _setting;
		}
	}
}
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Authentication.DataProvider
{
	[TestFixture]
	public class BadgeProviderTest
	{
		private IBadgeProvider target;
		private ILoggedOnUser loggedOnUser;
		private IAgentBadgeRepository badgeRepository;
		private IAgentBadgeWithRankRepository badgeWithRankRepository;
		private IToggleManager toggleManager;

		private readonly Guid currentUserId = Guid.NewGuid();
		private ITeamGamificationSettingRepository teamSettingRepository;

		[SetUp]
		public void Setup()
		{
			loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var currentUser = new Person();
			currentUser.SetId(currentUserId);
			loggedOnUser.Stub(x => x.CurrentUser()).Return(currentUser);

			badgeRepository = MockRepository.GenerateMock<IAgentBadgeRepository>();
			mockAgentBadgeRepository(currentUser);

			badgeWithRankRepository = MockRepository.GenerateMock<IAgentBadgeWithRankRepository>();
			mockAgentBadgeWithRankRepository(currentUser);

			teamSettingRepository = MockRepository.GenerateMock<ITeamGamificationSettingRepository>();
			toggleManager = MockRepository.GenerateMock<IToggleManager>();
		}

		#region Mock badge repositories

		private void mockAgentBadgeRepository(Person currentUser)
		{
			badgeRepository.Stub(x => x.Find(currentUser, BadgeType.Adherence)).Return(
				new AgentBadge
				{
					Person = currentUserId,
					BadgeType = BadgeType.Adherence,
					TotalAmount = 12 // 1 gold, 0 silver, 2 bronze
				});
			badgeRepository.Stub(x => x.Find(currentUser, BadgeType.AnsweredCalls)).Return(
				new AgentBadge
				{
					Person = currentUserId,
					BadgeType = BadgeType.AnsweredCalls,
					TotalAmount = 6 // 0 gold, 1 silver, 1 bronze
				});
			badgeRepository.Stub(x => x.Find(currentUser, BadgeType.AverageHandlingTime)).Return(
				new AgentBadge
				{
					Person = currentUserId,
					BadgeType = BadgeType.AverageHandlingTime,
					TotalAmount = 7 // 0 gold, 1 silver, 2 bronze
				});
		}

		private void mockAgentBadgeWithRankRepository(Person currentUser)
		{
			badgeWithRankRepository.Stub(x => x.Find(currentUser, BadgeType.Adherence)).Return(
				new AgentBadgeWithRank
				{
					Person = currentUserId,
					BadgeType = BadgeType.Adherence,
					GoldBadgeAmount = 3,
					SilverBadgeAmount = 1,
					BronzeBadgeAmount = 7
				});
			badgeWithRankRepository.Stub(x => x.Find(currentUser, BadgeType.AnsweredCalls)).Return(
				new AgentBadgeWithRank
				{
					Person = currentUserId,
					BadgeType = BadgeType.AnsweredCalls,
					GoldBadgeAmount = 1,
					SilverBadgeAmount = 0,
					BronzeBadgeAmount = 3
				});
			badgeWithRankRepository.Stub(x => x.Find(currentUser, BadgeType.AverageHandlingTime)).Return(
				new AgentBadgeWithRank
				{
					Person = currentUserId,
					BadgeType = BadgeType.AverageHandlingTime,
					GoldBadgeAmount = 2,
					SilverBadgeAmount = 1,
					BronzeBadgeAmount = 3
				});
		}

		#endregion

		[Test]
		public void ShouldGetTotalBadgesIfCalculateBadgeWithRankToggleIsEnableWhenUsingTeamGamification()
		{
			var myTeam = loggedOnUser.CurrentUser().MyTeam(DateOnly.Today);
			teamSettingRepository.Stub(x => x.FindTeamGamificationSettingsByTeam(myTeam)).Return(new TeamGamificationSetting
			{
				Team = myTeam,
				GamificationSetting = new GamificationSetting("setting")
				{
					SilverToBronzeBadgeRate = 5,
					GoldToSilverBadgeRate = 5,
				}
			});

			toggleManager.Stub(x => x.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318)).Return(true);

			target = new BadgeProvider(loggedOnUser, badgeRepository, badgeWithRankRepository, teamSettingRepository, toggleManager);

			var result = target.GetBadges().ToList();

			var adherenceBadge = result.Single(x => (x.BadgeType == BadgeType.Adherence));
			Assert.AreEqual(adherenceBadge.GoldBadge, 3);
			Assert.AreEqual(adherenceBadge.SilverBadge, 3);
			Assert.AreEqual(adherenceBadge.BronzeBadge, 9);

			var answeredCallBadge = result.Single(x => (x.BadgeType == BadgeType.AnsweredCalls));
			Assert.AreEqual(answeredCallBadge.GoldBadge, 1);
			Assert.AreEqual(answeredCallBadge.SilverBadge, 1);
			Assert.AreEqual(answeredCallBadge.BronzeBadge, 4);

			var ahtBadge = result.Single(x => (x.BadgeType == BadgeType.AverageHandlingTime));
			Assert.AreEqual(ahtBadge.GoldBadge, 2);
			Assert.AreEqual(ahtBadge.SilverBadge, 2);
			Assert.AreEqual(ahtBadge.BronzeBadge, 5);
		}
	}
}

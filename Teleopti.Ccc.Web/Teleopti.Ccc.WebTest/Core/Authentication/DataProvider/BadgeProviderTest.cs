using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
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
		}

		#region Mock badge repositories

		private void mockAgentBadgeRepository(Person currentUser)
		{
			badgeRepository.Stub(x => x.Find(currentUser, BadgeType.Adherence, false)).Return(
				new AgentBadge
				{
					Person = currentUserId,
					BadgeType = BadgeType.Adherence,
					IsExternal = false,
					TotalAmount = 12 // 1 gold, 0 silver, 2 bronze
				});
			badgeRepository.Stub(x => x.Find(currentUser, BadgeType.AnsweredCalls, false)).Return(
				new AgentBadge
				{
					Person = currentUserId,
					BadgeType = BadgeType.AnsweredCalls,
					IsExternal = false,
					TotalAmount = 6 // 0 gold, 1 silver, 1 bronze
				});
			badgeRepository.Stub(x => x.Find(currentUser, BadgeType.AverageHandlingTime, false)).Return(
				new AgentBadge
				{
					Person = currentUserId,
					BadgeType = BadgeType.AverageHandlingTime,
					IsExternal = false,
					TotalAmount = 7 // 0 gold, 1 silver, 2 bronze
				});
			
			// Returns an external-measure Badge
			badgeRepository.Stub(x => x.Find(currentUser, 0, true)).Return(
				new AgentBadge
				{
					Person = currentUserId,
					BadgeType = 0,
					IsExternal = true,
					TotalAmount = 7 // 0 gold, 1 silver, 2 bronze
				});
		}

		private void mockAgentBadgeWithRankRepository(Person currentUser)
		{
			badgeWithRankRepository.Stub(x => x.Find(currentUser, BadgeType.Adherence, false)).Return(
				new AgentBadgeWithRank
				{
					Person = currentUserId,
					BadgeType = BadgeType.Adherence,
					IsExternal = false,
					GoldBadgeAmount = 3,
					SilverBadgeAmount = 1,
					BronzeBadgeAmount = 7
				});
			badgeWithRankRepository.Stub(x => x.Find(currentUser, BadgeType.AnsweredCalls, false)).Return(
				new AgentBadgeWithRank
				{
					Person = currentUserId,
					BadgeType = BadgeType.AnsweredCalls,
					IsExternal = false,
					GoldBadgeAmount = 1,
					SilverBadgeAmount = 0,
					BronzeBadgeAmount = 3
				});
			badgeWithRankRepository.Stub(x => x.Find(currentUser, BadgeType.AverageHandlingTime, false)).Return(
				new AgentBadgeWithRank
				{
					Person = currentUserId,
					BadgeType = BadgeType.AverageHandlingTime,
					IsExternal = false,
					GoldBadgeAmount = 2,
					SilverBadgeAmount = 1,
					BronzeBadgeAmount = 3
				});

			// Returns an external-measure Badge
			badgeWithRankRepository.Stub(x => x.Find(currentUser, 0, true)).Return(
				new AgentBadgeWithRank
				{
					Person = currentUserId,
					BadgeType = 0,
					IsExternal = true,
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
					AnsweredCallsBadgeEnabled = true,
					AdherenceBadgeEnabled = true,
					AHTBadgeEnabled = true
				}
			});

			target = new BadgeProvider(loggedOnUser, badgeRepository, badgeWithRankRepository, teamSettingRepository);

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

		[Test]
		public void ShouldGetTotalBadgesWhenExternalBadgeSettingIsEnabled()
		{
			var myTeam = loggedOnUser.CurrentUser().MyTeam(DateOnly.Today);
			var external = new BadgeSetting { QualityId = 0, Name = "external", Enabled = true };
			teamSettingRepository.Stub(x => x.FindTeamGamificationSettingsByTeam(myTeam)).Return(new TeamGamificationSetting
			{
				Team = myTeam,
				GamificationSetting = new GamificationSetting("setting")
				{
					SilverToBronzeBadgeRate = 5,
					GoldToSilverBadgeRate = 5,
					AnsweredCallsBadgeEnabled = true,
					AdherenceBadgeEnabled = true,
					AHTBadgeEnabled = false,
					BadgeSettings = new List<IBadgeSetting> { external }
				}
			});

			target = new BadgeProvider(loggedOnUser, badgeRepository, badgeWithRankRepository, teamSettingRepository);

			var result = target.GetBadges().ToList();

			Assert.AreEqual(result.Count(), 3);

			var adherenceBadge = result.Single(x => (x.BadgeType == BadgeType.Adherence && x.IsExternal == false));
			Assert.AreEqual(adherenceBadge.GoldBadge, 3);
			Assert.AreEqual(adherenceBadge.SilverBadge, 3);
			Assert.AreEqual(adherenceBadge.BronzeBadge, 9);

			var answeredCallBadge = result.Single(x => (x.BadgeType == BadgeType.AnsweredCalls && x.IsExternal == false));
			Assert.AreEqual(answeredCallBadge.GoldBadge, 1);
			Assert.AreEqual(answeredCallBadge.SilverBadge, 1);
			Assert.AreEqual(answeredCallBadge.BronzeBadge, 4);

			var externalBadge = result.Single(x => (x.BadgeType == external.QualityId && x.IsExternal == true));
			Assert.AreEqual(2, externalBadge.GoldBadge);
			Assert.AreEqual(2, externalBadge.SilverBadge);
			Assert.AreEqual(5, externalBadge.BronzeBadge);
		}
	}
}
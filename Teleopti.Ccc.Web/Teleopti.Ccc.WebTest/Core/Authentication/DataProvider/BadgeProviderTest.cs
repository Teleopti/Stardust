using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
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
		private IAgentBadgeSettingsRepository settingRepository;
		private IToggleManager toggleManager;

		private readonly Guid currentUserId = Guid.NewGuid();

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

			settingRepository = MockRepository.GenerateMock<IAgentBadgeSettingsRepository>();
			toggleManager = MockRepository.GenerateMock<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185)).Return(true);
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
		public void ShouldReturnBadgesWithoutRankIfNotCalculateBadgeWithRank()
		{
			settingRepository.Stub(x => x.GetSettings()).Return(new AgentBadgeSettings
			{
				BadgeEnabled = true,
				AdherenceBadgeEnabled = true,
				AnsweredCallsBadgeEnabled = true,
				AHTBadgeEnabled = true,
				CalculateBadgeWithRank = false,
				SilverToBronzeBadgeRate = 5,
				GoldToSilverBadgeRate = 2
			});

			target = new BadgeProvider(loggedOnUser, badgeRepository, badgeWithRankRepository, settingRepository, toggleManager);
			var result = target.GetBadges().ToList();

			var adherenceBadge = result.Single(x => (x.BadgeType == BadgeType.Adherence));
			Assert.AreEqual(adherenceBadge.GoldBadge, 1);
			Assert.AreEqual(adherenceBadge.SilverBadge, 0);
			Assert.AreEqual(adherenceBadge.BronzeBadge, 2);

			var answeredCallBadge = result.Single(x => (x.BadgeType == BadgeType.AnsweredCalls));
			Assert.AreEqual(answeredCallBadge.GoldBadge, 0);
			Assert.AreEqual(answeredCallBadge.SilverBadge, 1);
			Assert.AreEqual(answeredCallBadge.BronzeBadge, 1);

			var ahtBadge = result.Single(x => (x.BadgeType == BadgeType.AverageHandlingTime));
			Assert.AreEqual(ahtBadge.GoldBadge, 0);
			Assert.AreEqual(ahtBadge.SilverBadge, 1);
			Assert.AreEqual(ahtBadge.BronzeBadge, 2);
		}

		[Test]
		public void ShouldGetTotalBadgesIfCalculateBadgeWithRank()
		{
			settingRepository.Stub(x => x.GetSettings()).Return(new AgentBadgeSettings
			{
				BadgeEnabled = true,
				AdherenceBadgeEnabled = true,
				AnsweredCallsBadgeEnabled = true,
				AHTBadgeEnabled = true,
				CalculateBadgeWithRank = true,
				SilverToBronzeBadgeRate = 5,
				GoldToSilverBadgeRate = 2
			});

			target = new BadgeProvider(loggedOnUser, badgeRepository, badgeWithRankRepository, settingRepository, toggleManager);

			var result = target.GetBadges().ToList();

			var adherenceBadge = result.Single(x => (x.BadgeType == BadgeType.Adherence));
			Assert.AreEqual(adherenceBadge.GoldBadge, 4);
			Assert.AreEqual(adherenceBadge.SilverBadge, 1);
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

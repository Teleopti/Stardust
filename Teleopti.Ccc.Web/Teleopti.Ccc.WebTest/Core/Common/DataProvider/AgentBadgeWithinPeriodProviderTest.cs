using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[MyTimeWebTest, TestFixture]
	public class AgentBadgeWithinPeriodProviderTest : IIsolateSystem
	{
		public IAgentBadgeWithinPeriodProvider Target;
		public FakeGamificationSettingRepository GamificationSettingRepository;
		public FakeTeamGamificationSettingRepository TeamGamificationSettingRepository;
		public FakeAgentBadgeTransactionRepository AgentBadgeTransactionRepository;
		public FakeAgentBadgeWithRankTransactionRepository AgentBadgeWithRankTransactionRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeCurrentTeleoptiPrincipal CurrentTeleoptiPrincipal;
		public FakeCurrentDatasource CurrentDataSource;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<FakeCurrentTeleoptiPrincipal>().For<ICurrentTeleoptiPrincipal>();
			isolate.UseTestDouble<FakeCurrentDatasource>().For<ICurrentDataSource>();
			isolate.UseTestDouble<FakePermissions>().For<IAuthorization>();
		}

		[Test]
		public void ShouldGetBadgesWithinPeriod()
		{
			var calculatedDate = new DateOnly(2018, 4, 9);
			var gamificationSetting = createGamificationSetting();
			createTeamGamificationSetting(gamificationSetting);

			setAgentBadge(gamificationSetting, calculatedDate);
			setAgentBadgeWithRank(gamificationSetting, calculatedDate);

			var period = new DateOnlyPeriod(DateOnly.MinValue, calculatedDate);
			var result = Target.GetBadges(period);

			result.ToList()[0].BronzeBadge.Should().Be.EqualTo(1);
			result.ToList()[0].SilverBadge.Should().Be.EqualTo(0);
			result.ToList()[0].GoldBadge.Should().Be.EqualTo(0);
			result.ToList()[1].BronzeBadge.Should().Be.EqualTo(5);
			result.ToList()[1].SilverBadge.Should().Be.EqualTo(0);
			result.ToList()[1].GoldBadge.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotGetBadgesWithoutPeriod()
		{
			var calculatedDate = new DateOnly(2018, 4, 9);
			var gamificationSetting = createGamificationSetting();
			createTeamGamificationSetting(gamificationSetting);
			setAgentBadge(gamificationSetting, calculatedDate);
			setAgentBadgeWithRank(gamificationSetting, calculatedDate);

			var period = new DateOnlyPeriod(DateOnly.MinValue, calculatedDate.AddDays(-1));
			var result = Target.GetBadges(period);

			result.ToList()[0].BronzeBadge.Should().Be.EqualTo(0);
			result.ToList()[0].SilverBadge.Should().Be.EqualTo(0);
			result.ToList()[0].GoldBadge.Should().Be.EqualTo(0);
			result.ToList()[1].BronzeBadge.Should().Be.EqualTo(0);
			result.ToList()[1].SilverBadge.Should().Be.EqualTo(0);
			result.ToList()[1].GoldBadge.Should().Be.EqualTo(0);
		}

		private void createTeamGamificationSetting(IGamificationSetting gamificationSetting = null)
		{
			setupLoggedOnUser();
			if (gamificationSetting == null) return;

			var teamGamificationSetting = new TeamGamificationSetting
			{
				Team = LoggedOnUser.CurrentUser().MyTeam(DateOnly.Today),
				GamificationSetting = gamificationSetting
			};
			TeamGamificationSettingRepository.Add(teamGamificationSetting);
		}

		private void setAgentBadge(IGamificationSetting gamificationSetting, DateOnly calculatedDate)
		{
			var agentBadge = new AgentBadgeTransaction
			{
				BadgeType = gamificationSetting.BadgeSettings[0].QualityId,
				IsExternal = true,
				Person = LoggedOnUser.CurrentUser(),
				Amount = 1,
				CalculatedDate = calculatedDate
			};
			AgentBadgeTransactionRepository.Add(agentBadge);
		}

		private void setAgentBadgeWithRank(IGamificationSetting gamificationSetting, DateOnly calculatedDate)
		{
			var agentBadgeWithRank = new AgentBadgeWithRankTransaction
			{
				BadgeType = gamificationSetting.BadgeSettings[1].QualityId,
				BronzeBadgeAmount = 5,
				SilverBadgeAmount = 0,
				GoldBadgeAmount = 1,
				IsExternal = true,
				CalculatedDate = calculatedDate,
				Person = LoggedOnUser.CurrentUser()
			};

			AgentBadgeWithRankTransactionRepository.Add(agentBadgeWithRank);
		}

		private void setupLoggedOnUser()
		{
			CurrentDataSource.FakeName("default");
			var team = TeamFactory.CreateTeamWithId("myTeam");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.MinValue, team);
			person.SetName(new Name("arne", "arne"));
			var culture = CultureInfoFactory.CreateChineseCulture();
			person.PermissionInformation.SetCulture(culture);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("Pelle", null, null, null, null), person);
			CurrentTeleoptiPrincipal.Fake(principal);
		}

		private IGamificationSetting createGamificationSetting()
		{
			var gSetting = new GamificationSetting("bla")
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold,
				AnsweredCallsBadgeEnabled = false,
				AHTBadgeEnabled = false,
				AdherenceBadgeEnabled = false,

				AnsweredCallsThreshold = 30,
				AnsweredCallsBronzeThreshold = 30,
				AnsweredCallsSilverThreshold = 40,
				AnsweredCallsGoldThreshold = 50,

				AHTThreshold = TimeSpan.FromSeconds(60),
				AHTBronzeThreshold = TimeSpan.FromSeconds(60),
				AHTSilverThreshold = TimeSpan.FromSeconds(50),
				AHTGoldThreshold = TimeSpan.FromSeconds(40),

				AdherenceThreshold = new Percent(0.70),
				AdherenceBronzeThreshold = new Percent(0.70),
				AdherenceSilverThreshold = new Percent(0.85),
				AdherenceGoldThreshold = new Percent(0.95),

				SilverToBronzeBadgeRate = 5,
				GoldToSilverBadgeRate = 2,
				RollingPeriodSet = GamificationRollingPeriodSet.Monthly
			};

			gSetting.BadgeSettings.Add(new BadgeSetting { QualityId = 2, DataType = ExternalPerformanceDataType.Numeric, Enabled = true });
			gSetting.BadgeSettings.Add(new BadgeSetting { QualityId = 1, DataType = ExternalPerformanceDataType.Percent, Enabled = true });

			gSetting.WithId(Guid.NewGuid());

			GamificationSettingRepository.Add(gSetting);
			return gSetting;
		}
	}
}

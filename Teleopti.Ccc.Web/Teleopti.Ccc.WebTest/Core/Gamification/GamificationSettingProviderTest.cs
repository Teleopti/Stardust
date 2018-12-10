using NUnit.Framework;
using SharpTestsEx;
using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.Gamification;


namespace Teleopti.Ccc.WebTest.Core.Gamification
{
	[TestFixture, DomainTest]
	public class GamificationSettingProviderTest : IIsolateSystem, IExtendSystem
	{
		public IGamificationSettingProvider Target;
		public IGamificationSettingRepository GamificationSettingRepository;
		public FakeTeamGamificationSettingRepository TeamGamificationSettingRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeTeamRepository TeamRepository;

		[Test]
		public void ShouldGetGamificationSettingByIdAndSortExternalBadgeSettingsById()
		{
			var gamificationSetting = createGamificationSettingWithId();

			var setting = Target.GetGamificationSetting(gamificationSetting.Id.GetValueOrDefault());
			setting.Should().Not.Be.Null();
			setting.ExternalBadgeSettings[0].QualityId.Should().Be.EqualTo(1);
			setting.ExternalBadgeSettings[1].QualityId.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldGetSettingDescriptionList()
		{
			var expectedDescription1 = "1";
			var expectedDescription2 = "2";
			createGamificationSettingWithId(expectedDescription1);
			createGamificationSettingWithId(expectedDescription2);

			var list = Target.GetGamificationList();

			list.Count.Should().Be.EqualTo(2);
			list[0].Value.Name.Should().Be.EqualTo(expectedDescription1);
			list[1].Value.Name.Should().Be.EqualTo(expectedDescription2);
		}

		[Test]
		public void ShouldGetLoggedOnUserGamificationSetting()
		{
			var expectedGamificationSetting = createGamificationSettingWithId();
			createTeamGamificationSetting(expectedGamificationSetting);

			var result = Target.GetGamificationSetting();
			result.Should().Be.EqualTo(expectedGamificationSetting);
		}

		[Test]
		public void ShouldNotGetGamificationSettingWhenThereWasNoGamificationTeamSetting()
		{
			createTeamGamificationSetting();

			var result = Target.GetGamificationSetting();
			result.Should().Be.Null();
		}
				
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));
		}

		public void Isolate(IIsolate isolate)
		{
			
			var team = TeamFactory.CreateTeamWithId("myTeam");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.MinValue, team);
			
			isolate.UseTestDouble(new FakeLoggedOnUser(person)).For<ILoggedOnUser>();
		}

		private void createTeamGamificationSetting(IGamificationSetting gamificationSetting = null)
		{
			
			if (gamificationSetting == null) return;

			var teamGamificationSetting = new TeamGamificationSetting
			{
				Team = LoggedOnUser.CurrentUser().MyTeam(DateOnly.Today),
				GamificationSetting = gamificationSetting
			};
			TeamGamificationSettingRepository.Add(teamGamificationSetting);
		}

		private IGamificationSetting createGamificationSettingWithId(string description = "Default Gamification Setting")
		{
			var id = Guid.NewGuid();
			var aSetting = new GamificationSetting(description)
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold,
				AnsweredCallsBadgeEnabled = true,
				AHTBadgeEnabled = false,
				AdherenceBadgeEnabled = true,

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

			aSetting.BadgeSettings.Add(new BadgeSetting { QualityId = 2, DataType = ExternalPerformanceDataType.Numeric });
			aSetting.BadgeSettings.Add(new BadgeSetting { QualityId = 1, DataType = ExternalPerformanceDataType.Percent });

			aSetting.WithId(id);

			GamificationSettingRepository.Add(aSetting);

			return aSetting;
		}

	}
}

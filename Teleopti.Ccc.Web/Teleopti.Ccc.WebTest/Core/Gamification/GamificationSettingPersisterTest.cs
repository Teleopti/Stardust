using NUnit.Framework;
using SharpTestsEx;
using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.Gamification;


namespace Teleopti.Ccc.WebTest.Core.Gamification
{	
	[TestFixture, DomainTest]
	public class GamificationSettingPersisterTest : IExtendSystem
	{
		public IGamificationSettingPersister Target;
		public IGamificationSettingRepository GamificationSettingRepository;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));
		}
		
		[Test]
		public void ShouldPersistDescription()
		{
			var name = "default";
			var id = createGamificationSettingWithId();
			
			GamificationDescriptionForm settingDescription = new GamificationDescriptionForm { GamificationSettingId = id, Name = name };
			var result = Target.PersistDescription(settingDescription);
			result.GamificationSettingId.Should().Be.EqualTo(id);
			result.Value.Name.Should().Be.EqualTo(name);
		}

		[Test]
		public void ShouldPersistAnsweredCallsEnabled()
		{
			var id = createGamificationSettingWithId();
			Target.PersistAnsweredCallsEnabled(new GamificationThresholdEnabledViewModel() { GamificationSettingId = id, Value = true });
			var setting = GamificationSettingRepository.Get(id);
			setting.AnsweredCallsBadgeEnabled.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldPersistAnsweredCallsGoldThreshold()
		{
			var id = createGamificationSettingWithId();
			Target.PersistAnsweredCallsGoldThreshold(new GamificationAnsweredCallsThresholdViewModel {  GamificationSettingId = id, Value = 100 });
			var setting = GamificationSettingRepository.Get(id);
			setting.AnsweredCallsGoldThreshold.Should().Be.EqualTo(100);
		}

		[Test]
		public void ShouldPersistAnsweredCallsSilverThreshold()
		{
			var id = createGamificationSettingWithId();
			Target.PersistAnsweredCallsSilverThreshold(new GamificationAnsweredCallsThresholdViewModel { GamificationSettingId = id, Value = 100 });
			var setting = GamificationSettingRepository.Get(id);
			setting.AnsweredCallsSilverThreshold.Should().Be.EqualTo(100);
		}

		[Test]
		public void ShouldPersistAnsweredCallsBronzeThreshold()
		{
			var id = createGamificationSettingWithId();
			Target.PersistAnsweredCallsBronzeThreshold(new GamificationAnsweredCallsThresholdViewModel { GamificationSettingId = id, Value = 100 });
			var setting = GamificationSettingRepository.Get(id);
			setting.AnsweredCallsBronzeThreshold.Should().Be.EqualTo(100);
		}

		[Test]
		public void ShouldPersistAHTEnabled()
		{
			var id = createGamificationSettingWithId();
			Target.PersistAHTEnabled(new GamificationThresholdEnabledViewModel { GamificationSettingId = id, Value = true });
			var setting = GamificationSettingRepository.Get(id);
			setting.AHTBadgeEnabled.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldPersistAHTGoldThreshold()
		{
			var id = createGamificationSettingWithId();
			var ts = new TimeSpan(0, 30, 30);
			Target.PersistAHTGoldThreshold(new GamificationAHTThresholdViewModel { GamificationSettingId = id, Value = ts });
			var setting = GamificationSettingRepository.Get(id);
			setting.AHTGoldThreshold.Should().Be.EqualTo(ts);
		}

		[Test]
		public void ShouldPersistAHTSilverThreshold()
		{
			var id = createGamificationSettingWithId();
			var ts = new TimeSpan(0, 30, 30);
			Target.PersistAHTSilverThreshold(new GamificationAHTThresholdViewModel { GamificationSettingId = id, Value = ts });
			var setting = GamificationSettingRepository.Get(id);
			setting.AHTSilverThreshold.Should().Be.EqualTo(ts);
		}

		[Test]
		public void ShouldPersistAHTBronzeThreshold()
		{
			var id = createGamificationSettingWithId();
			var ts = new TimeSpan(0, 30, 30);
			Target.PersistAHTBronzeThreshold(new GamificationAHTThresholdViewModel { GamificationSettingId = id, Value = ts });
			var setting = GamificationSettingRepository.Get(id);
			setting.AHTBronzeThreshold.Should().Be.EqualTo(ts);
		}

		[Test]
		public void ShouldPersistAdherenceEnabled()
		{
			var id = createGamificationSettingWithId();
			Target.PersistAdherenceEnabled(new GamificationThresholdEnabledViewModel { GamificationSettingId = id, Value = false });
			var setting = GamificationSettingRepository.Get(id);
			setting.AdherenceBadgeEnabled.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldPersistAdherenceGoldThreshold()
		{
			var id = createGamificationSettingWithId();
			Target.PersistAdherenceGoldThreshold(new GamificationAdherenceThresholdViewModel { GamificationSettingId = id, Value = 0.5 });
			var setting = GamificationSettingRepository.Get(id);
			setting.AdherenceGoldThreshold.Should().Be.EqualTo(new Percent(0.5));
		}

		[Test]
		public void ShouldPersistAdherenceSilverThreshold()
		{
			var id = createGamificationSettingWithId();
			Target.PersistAdherenceSilverThreshold(new GamificationAdherenceThresholdViewModel { GamificationSettingId = id, Value = 0.5 });
			var setting = GamificationSettingRepository.Get(id);
			setting.AdherenceSilverThreshold.Should().Be.EqualTo(new Percent(0.5));
		}

		[Test]
		public void ShouldPersistAdherenceBronzeThreshold()
		{
			var id = createGamificationSettingWithId();
			Target.PersistAdherenceBronzeThreshold(new GamificationAdherenceThresholdViewModel { GamificationSettingId = id, Value = 0.5 });
			var setting = GamificationSettingRepository.Get(id);
			setting.AdherenceBronzeThreshold.Should().Be.EqualTo(new Percent(0.5));
		}

		[Test]
		public void ShouldPersistRuleChange()
		{
			var id = createGamificationSettingWithId();
			Target.PersistRuleChange(new GamificationChangeRuleForm { GamificationSettingId = id, Rule = GamificationSettingRuleSet.RuleWithDifferentThreshold });
			var setting = GamificationSettingRepository.Get(id);
			setting.GamificationSettingRuleSet.Should().Be.EqualTo(GamificationSettingRuleSet.RuleWithDifferentThreshold);
		}

		[Test]
		public void ShouldPersistRollingPeriodChange()
		{
			var id = createGamificationSettingWithId();
			Target.PersistRollingPeriodChange(new GamificationModifyRollingPeriodForm { GamificationSettingId = id, RollingPeriodSet = GamificationRollingPeriodSet.Monthly });
			var setting = GamificationSettingRepository.Get(id);
			setting.RollingPeriodSet.Should().Be.EqualTo(GamificationRollingPeriodSet.Monthly);
		}

		[Test]
		public void ShouldPersistAnsweredCallsThreshold()
		{
			var id = createGamificationSettingWithId();
			Target.PersistAnsweredCallsThreshold(new GamificationAnsweredCallsThresholdViewModel { GamificationSettingId = id, Value = 100 });
			var setting = GamificationSettingRepository.Get(id);
			setting.AnsweredCallsThreshold.Should().Be.EqualTo(100);
		}

		[Test]
		public void ShouldPersistAHTThreshold()
		{
			var id = createGamificationSettingWithId();
			var timespan = new TimeSpan(0, 3, 0);
			Target.PersistAHTThreshold(new GamificationAHTThresholdViewModel { GamificationSettingId = id, Value = timespan });
			var setting = GamificationSettingRepository.Get(id);
			setting.AHTThreshold.Should().Be.EqualTo(timespan);
		}

		[Test]
		public void ShouldPersistAdherenceThreshold()
		{
			var id = createGamificationSettingWithId();
			Target.PersistAdherenceThreshold(new GamificationAdherenceThresholdViewModel { GamificationSettingId = id, Value = 0.5 });
			var setting = GamificationSettingRepository.Get(id);
			setting.AdherenceThreshold.Should().Be.EqualTo(new Percent(0.5));
		}

		[Test]
		public void ShouldPersistGoldToSilverRate()
		{
			var id = createGamificationSettingWithId();
			Target.PersistGoldToSilverRate(new GamificationBadgeConversRateViewModel { GamificationSettingId = id, Rate = 55 });
			var setting = GamificationSettingRepository.Get(id);
			setting.GoldToSilverBadgeRate.Should().Be.EqualTo(55);
		}

		[Test]
		public void ShouldPersistSilverToBronzeRate()
		{
			var id = createGamificationSettingWithId();
			Target.PersistSilverToBronzeRate(new GamificationBadgeConversRateViewModel { GamificationSettingId = id, Rate = 55 });
			var setting = GamificationSettingRepository.Get(id);
			setting.SilverToBronzeBadgeRate.Should().Be.EqualTo(55);
		}

		[Test]
		public void ShouldPersistExternalBadgeDescription()
		{
			var id = createGamificationSettingWithId();
			Target.PersistExternalBadgeDescription(new ExternalBadgeSettingDescriptionViewModel { GamificationSettingId = id, Name = "new name", QualityId = 1 });
			var setting = GamificationSettingRepository.Get(id);
			setting.BadgeSettings[1].Name.Should().Be.EqualTo("new name");
		}

		[Test]
		public void ShouldPersistExternalBadgeThreshold()
		{
			var id = createGamificationSettingWithId();
			Target.PersistExternalBadgeThreshold(new ExternalBadgeSettingThresholdViewModel { GamificationSettingId = id, QualityId = 1, ThresholdValue = 0.25 });
			var setting = GamificationSettingRepository.Get(id);
			setting.BadgeSettings[1].Threshold.Should().Be.EqualTo(0.25);
		}

		[Test]
		public void ShouldPersistExternalBadgeGoldThreshold()
		{
			var id = createGamificationSettingWithId();
			Target.PersistExternalBadgeGoldThreshold(new ExternalBadgeSettingThresholdViewModel { GamificationSettingId = id, QualityId = 1, ThresholdValue = 0.36});
			var setting = GamificationSettingRepository.Get(id);
			setting.BadgeSettings[1].GoldThreshold.Should().Be.EqualTo(0.36);
		}

		[Test]
		public void ShouldPersistExternalBadgeSilverThreshold()
		{
			var id = createGamificationSettingWithId();
			Target.PersistExternalBadgeSilverThreshold(new ExternalBadgeSettingThresholdViewModel { GamificationSettingId = id, QualityId = 1, ThresholdValue = 0.25 });
			var setting = GamificationSettingRepository.Get(id);
			setting.BadgeSettings[1].SilverThreshold.Should().Be.EqualTo(0.25);
		}

		[Test]
		public void ShouldPersistExternalBadgeBronzeThreshold()
		{
			var id = createGamificationSettingWithId();
			Target.PersistExternalBadgeBronzeThreshold(new ExternalBadgeSettingThresholdViewModel { GamificationSettingId = id, QualityId = 1, ThresholdValue = 0.7 });
			var setting = GamificationSettingRepository.Get(id);
			setting.BadgeSettings[1].BronzeThreshold.Should().Be.EqualTo(0.7);
		}

		[Test]
		public void ShouldPersistExternalBadgeEnabled()
		{
			var id = createGamificationSettingWithId();
			Target.PersistExternalBadgeEnabled(new ExternalBadgeSettingBooleanViewModel { GamificationSettingId = id, QualityId = 1, Value = true });
			var setting = GamificationSettingRepository.Get(id);
			setting.BadgeSettings[1].Enabled.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldPersistExternalBadgeLargerIsBetter()
		{
			var id = createGamificationSettingWithId();
			Target.PersistExternalBadgeLargerIsBetter(new ExternalBadgeSettingBooleanViewModel { GamificationSettingId = id, QualityId = 1, Value = true });
			var setting = GamificationSettingRepository.Get(id);
			setting.BadgeSettings[1].LargerIsBetter.Should().Be.EqualTo(true);
		}


		private Guid createGamificationSettingWithId(string description = "Default Gamification Setting")
		{
			var id = Guid.NewGuid();
			var aSetting = new GamificationSetting(description)
			{
				RollingPeriodSet = GamificationRollingPeriodSet.Weekly,
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold,
				AnsweredCallsBadgeEnabled = false,
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

			};

			aSetting.BadgeSettings.Add(new BadgeSetting { QualityId = 2, DataType = ExternalPerformanceDataType.Numeric, Name ="name1" , BronzeThreshold =5, Enabled = false, GoldThreshold = 10, LargerIsBetter = false, SilverThreshold = 12, Threshold = 12});
			aSetting.BadgeSettings.Add(new BadgeSetting
			{
				QualityId = 1,
				DataType = ExternalPerformanceDataType.Percent,
				Name = "name1" ,
				BronzeThreshold = 0.2,
				Enabled = false,
				GoldThreshold = 0.3,
				LargerIsBetter = false,
				SilverThreshold = 0.8,
				Threshold = 0.6
			});

			aSetting.WithId(id);

			GamificationSettingRepository.Add(aSetting);

			return id;
		}
	}
}

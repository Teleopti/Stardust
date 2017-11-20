using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Gamification.Mapping;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Gamification.Mapping
{
	[TestFixture]
	public class GamificationSettingMapperTest
	{
		private IStatisticRepository statisticRepository;
		private GamificationSettingMapper mapper;

		[SetUp]
		public void SetUp()
		{
			statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			statisticRepository.Stub(x => x.LoadAllQualityInfo()).Return(new List<QualityInfo>());
			mapper = new GamificationSettingMapper(statisticRepository);
		}

		[Test]
		public void ShouldMapGamificationSettingPropertiesCorrectly()
		{
			var rawSetting = createRawGamificationSetting();

			var vm = mapper.Map(rawSetting);
			checkRawSettingAndVm(rawSetting, vm);
			Assert.IsNotNull(vm.ExternalBadgeSettings);
			Assert.IsFalse(vm.ExternalBadgeSettings.Any());
		}

		[Test]
		public void ShouldNotMapEmptyExternalBadgeSettings()
		{
			var rawSetting = createRawGamificationSetting();
			rawSetting.BadgeSettings = new List<IBadgeSetting>(); 

			var vm = mapper.Map(rawSetting);
			Assert.IsNotNull(vm.ExternalBadgeSettings);
			Assert.IsFalse(vm.ExternalBadgeSettings.Any());
		}

		[Test]
		public void ShouldMapExternalBadgeSettings()
		{
			var rawSetting = createRawGamificationSetting();
			var externalBadgeSetting1 = new BadgeSetting()
			{
				Name = "ExternalBadge 1",
				QualityId = 5,
				LargerIsBetter = true,
				Enabled = true,
				Threshold = 100,
				BronzeThreshold = 100,
				SilverThreshold = 125,
				GoldThreshold = 150,
				UnitType = BadgeUnitType.Timespan
			};
			var externalBadgeSetting2 = new BadgeSetting()
			{
				Name = "ExternalBadge 2",
				QualityId = 8,
				LargerIsBetter = false,
				Enabled = false,
				Threshold = 9000,
				BronzeThreshold = 9000,
				SilverThreshold = 7500,
				GoldThreshold = 5000,
				UnitType = BadgeUnitType.Percentage
			};
			var externalBadgeSettings = new List<IBadgeSetting> {externalBadgeSetting1, externalBadgeSetting2};
			rawSetting.BadgeSettings = externalBadgeSettings;

			var vm = mapper.Map(rawSetting);

			checkRawSettingAndVm(rawSetting, vm);
			Assert.AreEqual(vm.ExternalBadgeSettings.Count, externalBadgeSettings.Count);

			checkRawBadgeSettingAndVm(externalBadgeSetting1, vm.ExternalBadgeSettings.First());
			checkRawBadgeSettingAndVm(externalBadgeSetting2, vm.ExternalBadgeSettings.Second());
		}

		[Test]
		public void ShouldMapNewQualityInfoIntoExternalBadgeSettings()
		{
			var rawSetting = createRawGamificationSetting();
			var externalBadgeSetting1 = new BadgeSetting()
			{
				Name = "ExternalBadge 1",
				QualityId = 5,
				LargerIsBetter = true,
				Enabled = true,
				Threshold = 100,
				BronzeThreshold = 100,
				SilverThreshold = 125,
				GoldThreshold = 150,
				UnitType = BadgeUnitType.Timespan
			};
			var externalBadgeSetting2 = new BadgeSetting()
			{
				Name = "ExternalBadge 2",
				QualityId = 8,
				LargerIsBetter = false,
				Enabled = false,
				Threshold = 9000,
				BronzeThreshold = 9000,
				SilverThreshold = 7500,
				GoldThreshold = 5000,
				UnitType = BadgeUnitType.Percentage
			};
			var externalBadgeSettings = new List<IBadgeSetting> {externalBadgeSetting1, externalBadgeSetting2};
			rawSetting.BadgeSettings = externalBadgeSettings;

			var alreadySetQualityInfo = new QualityInfo
			{
				QualityId = 8,
				QualityName = "Exist Quality Info",
				QualityType = "PERCENT",
				ScoreWeight = 1
			};
			var newQualityInfo = new QualityInfo
			{
				QualityId = 9,
				QualityName = "New Quality Info",
				QualityType = "GRADE",
				ScoreWeight = 1
			};

			var statisticRepositoryWithQualityInfo = MockRepository.GenerateMock<IStatisticRepository>();
			statisticRepositoryWithQualityInfo.Stub(x => x.LoadAllQualityInfo()).Return(new List<QualityInfo>
			{
				alreadySetQualityInfo,
				newQualityInfo
			});
			var targetMapper = new GamificationSettingMapper(statisticRepositoryWithQualityInfo);
			var vm = targetMapper.Map(rawSetting);

			checkRawSettingAndVm(rawSetting, vm);
			Assert.AreEqual(externalBadgeSettings.Count + 1, vm.ExternalBadgeSettings.Count);

			checkRawBadgeSettingAndVm(externalBadgeSetting1, vm.ExternalBadgeSettings.First());
			checkRawBadgeSettingAndVm(externalBadgeSetting2, vm.ExternalBadgeSettings.Second());
			checkRawBadgeSettingAndVm(new BadgeSetting
			{
				Name = "New Quality Info",
				QualityId = 9,
				LargerIsBetter = true,
				Enabled = false,
				Threshold = 0,
				BronzeThreshold = 0,
				SilverThreshold = 0,
				GoldThreshold = 0,
				UnitType = BadgeUnitType.Count
			}, vm.ExternalBadgeSettings.ElementAt(2));
		}

		private GamificationSetting createRawGamificationSetting()
		{
			return new GamificationSetting("Default Gamification Setting")
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
				GoldToSilverBadgeRate = 2
			};
		}

		private void checkRawSettingAndVm(IGamificationSetting rawSetting, GamificationSettingViewModel vm)
		{
			Assert.AreEqual(rawSetting.Description.Name, vm.Name);
			Assert.AreEqual(rawSetting.GamificationSettingRuleSet, vm.GamificationSettingRuleSet);
			Assert.AreEqual(rawSetting.AnsweredCallsBadgeEnabled, vm.AnsweredCallsBadgeEnabled);
			Assert.AreEqual(rawSetting.AHTBadgeEnabled, vm.AHTBadgeEnabled);
			Assert.AreEqual(rawSetting.AdherenceBadgeEnabled, vm.AdherenceBadgeEnabled);
			Assert.AreEqual(rawSetting.AnsweredCallsThreshold, vm.AnsweredCallsThreshold);
			Assert.AreEqual(rawSetting.AnsweredCallsBronzeThreshold, vm.AnsweredCallsBronzeThreshold);
			Assert.AreEqual(rawSetting.AnsweredCallsSilverThreshold, vm.AnsweredCallsSilverThreshold);
			Assert.AreEqual(rawSetting.AnsweredCallsGoldThreshold, vm.AnsweredCallsGoldThreshold);
			Assert.AreEqual(rawSetting.AHTThreshold, vm.AHTThreshold);
			Assert.AreEqual(rawSetting.AHTBronzeThreshold, vm.AHTBronzeThreshold);
			Assert.AreEqual(rawSetting.AHTSilverThreshold, vm.AHTSilverThreshold);
			Assert.AreEqual(rawSetting.AHTGoldThreshold, vm.AHTGoldThreshold);
			Assert.AreEqual(rawSetting.AdherenceThreshold, vm.AdherenceThreshold);
			Assert.AreEqual(rawSetting.AdherenceBronzeThreshold, vm.AdherenceBronzeThreshold);
			Assert.AreEqual(rawSetting.AdherenceSilverThreshold, vm.AdherenceSilverThreshold);
			Assert.AreEqual(rawSetting.AdherenceGoldThreshold, vm.AdherenceGoldThreshold);
			Assert.AreEqual(rawSetting.SilverToBronzeBadgeRate, vm.SilverToBronzeBadgeRate);
			Assert.AreEqual(rawSetting.GoldToSilverBadgeRate, vm.GoldToSilverBadgeRate);
		}

		private void checkRawBadgeSettingAndVm(IBadgeSetting rawBadgeSetting, ExternalBadgeSettingViewModel vm)
		{
			Assert.AreEqual(rawBadgeSetting.Name, vm.Name);
			Assert.AreEqual(rawBadgeSetting.QualityId, vm.QualityId);
			Assert.AreEqual(rawBadgeSetting.LargerIsBetter, vm.LargerIsBetter);
			Assert.AreEqual(rawBadgeSetting.Enabled, vm.Enabled);
			Assert.AreEqual(rawBadgeSetting.Threshold, vm.Threshold);
			Assert.AreEqual(rawBadgeSetting.BronzeThreshold, vm.BronzeThreshold);
			Assert.AreEqual(rawBadgeSetting.SilverThreshold, vm.SilverThreshold);
			Assert.AreEqual(rawBadgeSetting.GoldThreshold, vm.GoldThreshold);
			Assert.AreEqual(rawBadgeSetting.UnitType, vm.UnitType);
		}
	}
}

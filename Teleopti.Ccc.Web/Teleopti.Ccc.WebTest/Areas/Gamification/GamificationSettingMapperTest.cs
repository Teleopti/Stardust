using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Gamification.Mapping;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using Teleopti.Ccc.Web.Core.IoC;


namespace Teleopti.Ccc.WebTest.Areas.Gamification
{
	[TestFixture]
	[DomainTest]
	public class GamificationSettingMapperTest : IExtendSystem
	{
		public FakeExternalPerformanceRepository ExternalPerformanceRepository;
		public IGamificationSettingMapper Target;
				
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));
		}
		
		[Test]
		public void ShouldMapGamificationSettingPropertiesCorrectly()
		{
			var rawSetting = createRawGamificationSetting();

			var vm = Target.Map(rawSetting);
			checkRawSettingAndVm(rawSetting, vm);
			Assert.IsNotNull(vm.ExternalBadgeSettings);
			Assert.IsFalse(vm.ExternalBadgeSettings.Any());
		}

		[Test]
		public void ShouldNotMapEmptyExternalBadgeSettings()
		{
			var rawSetting = createRawGamificationSetting();
			rawSetting.BadgeSettings = new List<IBadgeSetting>(); 

			var vm = Target.Map(rawSetting);
			Assert.IsNotNull(vm.ExternalBadgeSettings);
			Assert.IsFalse(vm.ExternalBadgeSettings.Any());
		}

		[Test]
		public void ShouldMapExternalBadgeSettings()
		{
			var rawSetting = createRawGamificationSetting();
			var externalBadgeSetting1 = new BadgeSetting
			{
				Name = "ExternalBadge 1",
				QualityId = 5,
				LargerIsBetter = true,
				Enabled = true,
				Threshold = 100,
				BronzeThreshold = 100,
				SilverThreshold = 125,
				GoldThreshold = 150,
				DataType = ExternalPerformanceDataType.Numeric
			};
			var externalBadgeSetting2 = new BadgeSetting
			{
				Name = "ExternalBadge 2",
				QualityId = 8,
				LargerIsBetter = true,
				Enabled = false,
				Threshold = 9000,
				BronzeThreshold = 9000,
				SilverThreshold = 7500,
				GoldThreshold = 5000,
				DataType = ExternalPerformanceDataType.Percent
			};
			var externalBadgeSettings = new List<IBadgeSetting> {externalBadgeSetting1, externalBadgeSetting2};
			rawSetting.BadgeSettings = externalBadgeSettings;

			var vm = Target.Map(rawSetting);

			checkRawSettingAndVm(rawSetting, vm);
			Assert.AreEqual(vm.ExternalBadgeSettings.Count, externalBadgeSettings.Count);

			checkRawBadgeSettingAndVm(externalBadgeSetting1, vm.ExternalBadgeSettings.First());
			checkRawBadgeSettingAndVm(externalBadgeSetting2, vm.ExternalBadgeSettings.Second());
		}

		[Test]
		public void ShouldMapNewExternalPerformanceIntoExternalBadgeSettings()
		{
			var rawSetting = createRawGamificationSetting();
			var externalBadgeSetting1 = new BadgeSetting
			{
				Name = "ExternalBadge 1",
				QualityId = 5,
				LargerIsBetter = true,
				Enabled = true,
				Threshold = 100,
				BronzeThreshold = 100,
				SilverThreshold = 125,
				GoldThreshold = 150,
				DataType = ExternalPerformanceDataType.Numeric
			};
			var externalBadgeSetting2 = new BadgeSetting
			{
				Name = "ExternalBadge 2",
				QualityId = 8,
				LargerIsBetter = true,
				Enabled = false,
				Threshold = 9000,
				BronzeThreshold = 9000,
				SilverThreshold = 7500,
				GoldThreshold = 5000,
				DataType = ExternalPerformanceDataType.Percent
			};
			var externalBadgeSettings = new List<IBadgeSetting> {externalBadgeSetting1, externalBadgeSetting2};
			rawSetting.BadgeSettings = externalBadgeSettings;

			var alreadySetExternalPerformances = new ExternalPerformance
			{
				ExternalId = 8,
				Name = "Exist External Performance Info",
				DataType = ExternalPerformanceDataType.Percent
			};
			var newExternalPerformance = new ExternalPerformance
			{
				ExternalId = 9,
				Name = "New External Performance Info",
				DataType = ExternalPerformanceDataType.Numeric
			};

			var externalPerformanceRepositoryWithExternalPerformance = new FakeExternalPerformanceRepository();
			externalPerformanceRepositoryWithExternalPerformance.Add(alreadySetExternalPerformances);
			externalPerformanceRepositoryWithExternalPerformance.Add(newExternalPerformance);
			var targetMapper = new GamificationSettingMapper(externalPerformanceRepositoryWithExternalPerformance);
			var vm = targetMapper.Map(rawSetting);

			checkRawSettingAndVm(rawSetting, vm);
			Assert.AreEqual(externalBadgeSettings.Count + 1, vm.ExternalBadgeSettings.Count);

			checkRawBadgeSettingAndVm(externalBadgeSetting1, vm.ExternalBadgeSettings.First());
			checkRawBadgeSettingAndVm(externalBadgeSetting2, vm.ExternalBadgeSettings.Second());
			checkRawBadgeSettingAndVm(new BadgeSetting
			{
				Name = "New External Performance Info",
				QualityId = 9,
				LargerIsBetter = true,
				Enabled = false,
				Threshold = 0,
				BronzeThreshold = 0,
				SilverThreshold = 0,
				GoldThreshold = 0,
				DataType = ExternalPerformanceDataType.Numeric
			}, vm.ExternalBadgeSettings.ElementAt(2));
		}

		private GamificationSetting createRawGamificationSetting()
		{
			return new GamificationSetting("Default Gamification Setting")
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold,
				RollingPeriodSet = GamificationRollingPeriodSet.OnGoing,
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
			Assert.AreEqual(rawSetting.RollingPeriodSet, vm.RollingPeriodSet);
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
			Assert.AreEqual(rawBadgeSetting.DataType, vm.DataType);
		}
	}
}

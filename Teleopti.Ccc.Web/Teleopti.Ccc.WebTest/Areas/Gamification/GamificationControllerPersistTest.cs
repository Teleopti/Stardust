using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Gamification.Controller;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Gamification
{
	[DomainTest]
	[TestFixture]
	public class GamificationControllerPersistTest : IExtendSystem
	{
		public GamificationController Target;
		public FakeGamificationSettingRepository GamificationSettingRepository;
		public FakeExternalPerformanceRepository ExternalPerformanceRepository;
		public FakeTeamGamificationSettingRepository TeamGamificationSettingRepository;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));
		}
		
		[Test]
		public void ShouldPersistNewGamification()
		{
			var vm = Target.CreateGamification();
			
			var result = GamificationSettingRepository.LoadAll();
			result.Count().Should().Be.EqualTo(1);
			vm.Id.HasValue.Should().Be.True();
		}
		
		[Test]
		public void ShouldGiveDifferentDefaultNameWhenPersistNewGamification()
		{
			Target.CreateGamification();
			Target.CreateGamification();

			var result = GamificationSettingRepository.LoadAll().ToList();
			result.Count.Should().Be.EqualTo(2);
			result[0].Description.Name.Should().Be.EqualTo(Resources.NewGamificationSetting);
			result[1].Description.Name.Should().Be.EqualTo(Resources.NewGamificationSetting + "1");
		}

		[Test]
		public void ShouldDeleteGamificationSuccessfully()
		{
			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.RemoveGamification(gamificationSetting.Id.Value);
			result.Should().Be.InstanceOf<OkResult>();
			GamificationSettingRepository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnFalseWhenCannotFindGamificationForDelete()
		{
			var result = Target.RemoveGamification(Guid.NewGuid());
			result.Should().Be.InstanceOf<NotFoundResult>();
		}

		[Test]
		public void ShuldRemoveTeamGamificationSettingWhenRemove()
		{
			var gamificationId = Guid.NewGuid();
			var gamificationSetting = new GamificationSetting("newGamification").WithId(gamificationId);
			GamificationSettingRepository.Add(gamificationSetting);
			var team = TeamFactory.CreateTeam("teamBla", "siteBla");
			var teamGamificationSetting = new TeamGamificationSetting{Team = team, GamificationSetting = gamificationSetting};
			TeamGamificationSettingRepository.Add(teamGamificationSetting);

			Target.RemoveGamification(gamificationId);

			TeamGamificationSettingRepository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldPersistGamificationDescription()
		{
			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationDescription(new GamificationDescriptionForm
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Name = "modifiedDescription"
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Name.Should().Be.EqualTo("modifiedDescription");
		}

		[Test]
		public void ShouldReturnNUllWhenCannotFindGamificationSettingForPersistDescription()
		{
			var result = Target.GamificationDescription(new GamificationDescriptionForm
			{
				GamificationSettingId = Guid.NewGuid(),
				Name = "bla"
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsEnabled()
		{
			const bool expectedResult = true;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAnsweredCallsEnabled(new GamificationThresholdEnabledViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAnsweredCallsEnabled()
		{
			var result = Target.GamificationAnsweredCallsEnabled(new GamificationThresholdEnabledViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = true
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsThreshold()
		{
			var expectedResult = 150;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAnsweredCalls(new GamificationAnsweredCallsThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAnsweredCallsThreshold()
		{
			var result = Target.GamificationAnsweredCalls(new GamificationAnsweredCallsThresholdViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = 100
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsGoldThreshold()
		{
			var expectedResult = 150;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAnsweredCallsForGold(new GamificationAnsweredCallsThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAnsweredCallsGoldThreshold()
		{
			var result = Target.GamificationAnsweredCallsForGold(new GamificationAnsweredCallsThresholdViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = 100
			});

			result.Should().Be.Null();
		}
		
		[Test]
		public void ShouldPersistGamificationAnsweredCallsSilverThreshold()
		{
			var expectedResult = 110;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAnsweredCallsForSilver(new GamificationAnsweredCallsThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		private GamificationSetting createDefaultGamificationSetting()
		{
			var gamificationSetting = new GamificationSetting("newGamification").WithId();
			var badgeSetting = new BadgeSetting
			{
				QualityId = 1,
				DataType = ExternalPerformanceDataType.Numeric
			};
			gamificationSetting.BadgeSettings = new List<IBadgeSetting> {badgeSetting};

			return gamificationSetting;
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAnsweredCallsSilverThreshold()
		{
			var result = Target.GamificationAnsweredCallsForSilver(new GamificationAnsweredCallsThresholdViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = 100
			});

			result.Should().Be.Null();
		}
		
		[Test]
		public void ShouldPersistGamificationAnsweredCallsBronzeThreshold()
		{
			var expectedResult = 90;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAnsweredCallsForBronze(new GamificationAnsweredCallsThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAnsweredCallsBronzeThreshold()
		{
			var result = Target.GamificationAnsweredCallsForBronze(new GamificationAnsweredCallsThresholdViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = 100
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAhtEnabled()
		{
			const bool expectedResult = true;
			
			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAHTEnabled(new GamificationThresholdEnabledViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAhtEnabled()
		{
			var result = Target.GamificationAHTEnabled(new GamificationThresholdEnabledViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = false
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAhtThreshold()
		{
			var expectedResult = new TimeSpan(0, 2, 30);

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAHTThreshold(new GamificationAHTThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAhtThreshold()
		{
			var result = Target.GamificationAHTThreshold(new GamificationAHTThresholdViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = new TimeSpan(0, 2, 30)
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAhtGoldThreshold()
		{
			var expectedResult = new TimeSpan(0, 2, 30);

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAHTForGold(new GamificationAHTThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAhtGoldThreshold()
		{
			var result = Target.GamificationAHTForGold(new GamificationAHTThresholdViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = new TimeSpan(0, 2, 30)
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAhtSilverThreshold()
		{
			var expectedResult = new TimeSpan(0, 3, 30);

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAHTForSilver(new GamificationAHTThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAhtSilverThreshold()
		{
			var result = Target.GamificationAHTForSilver(new GamificationAHTThresholdViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = new TimeSpan(0, 2, 30)
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAhtBronzeThreshold()
		{
			var expectedResult = new TimeSpan(0, 4, 30);

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAHTForBronze(new GamificationAHTThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAhtBronzeThreshold()
		{
			var result = Target.GamificationAHTForBronze(new GamificationAHTThresholdViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = new TimeSpan(0, 2, 30)
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAdherenceEnabled()
		{
			const bool expectedResult = true;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAdherenceEnabled(new GamificationThresholdEnabledViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAdherenceEnabled()
		{
			var result = Target.GamificationAdherenceEnabled(new GamificationThresholdEnabledViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = false
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAdherenceThreshold()
		{
			var expectedResult = 0.9;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAdherence(new GamificationAdherenceThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAdherenceThreshold()
		{
			var result = Target.GamificationAdherence(new GamificationAdherenceThresholdViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = 0.8
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAdherenceGoldThreshold()
		{
			var expectedResult = 0.9876;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAdherenceForGold(new GamificationAdherenceThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAdherenceGoldThreshold()
		{
			var result = Target.GamificationAdherenceForGold(new GamificationAdherenceThresholdViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = 0.8123
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAdherenceSilverThreshold()
		{
			var expectedResult = 0.8567;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAdherenceForSilver(new GamificationAdherenceThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAdherenceSilverThreshold()
		{
			var result = Target.GamificationAdherenceForSilver(new GamificationAdherenceThresholdViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = 0.8765
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAdherenceBronzeThreshold()
		{
			var expectedResult = 0.7654;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationAdherenceForBronze(new GamificationAdherenceThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAdherenceBronzeThreshold()
		{
			var result = Target.GamificationAdherence(new GamificationAdherenceThresholdViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Value = 0.8765
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationRuleChange()
		{
			var expectedResult = GamificationSettingRuleSet.RuleWithRatioConvertor;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationChangeRule(new GamificationChangeRuleForm
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Rule = expectedResult
			});

			result.Id.Should().Be.EqualTo(gamificationSetting.Id);
			result.GamificationSettingRuleSet.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldPersistGamificationRollingPeriodChange()
		{
			var expectedResult = GamificationRollingPeriodSet.Monthly;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationModifyRollingPeriod(new GamificationModifyRollingPeriodForm
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				RollingPeriodSet = expectedResult
			});

			result.Id.Should().Be.EqualTo(gamificationSetting.Id);
			result.RollingPeriodSet.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistRuleChange()
		{
			var result = Target.GamificationChangeRule(new GamificationChangeRuleForm
			{
				GamificationSettingId = Guid.NewGuid(),
				Rule = GamificationSettingRuleSet.RuleWithDifferentThreshold
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationGoldToSilverRateChange()
		{
			var expectedResult = 6;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationGoldToSilverRate(new GamificationBadgeConversRateViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Rate = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Rate.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistGoldToSilverRateChange()
		{
			var result = Target.GamificationGoldToSilverRate(new GamificationBadgeConversRateViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Rate = 5
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationSilverToBronzeRateChange()
		{
			const int expectedResult = 6;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GamificationSilverToBronzeRate(new GamificationBadgeConversRateViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Rate = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.Rate.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistSilverToBronzeRateChange()
		{
			var result = Target.GamificationSilverToBronzeRate(new GamificationBadgeConversRateViewModel
			{
				GamificationSettingId = Guid.NewGuid(),
				Rate = 5
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistExternalBadgeSettingDescription()
		{
			var expected = "newName";

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);
			
			var result = Target.UpdateExternalBadgeSettingDescription(new ExternalBadgeSettingDescriptionViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Name = expected,
				QualityId = 1
			});

			result.QualityId.Should().Be.EqualTo(1);
			result.Name.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldPersistExternalBadgeSettingThreshold()
		{
			var expectedThreshold = 60;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);
			
			var result = Target.UpdateExternalBadgeSettingThreshold(new ExternalBadgeSettingThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				ThresholdValue = expectedThreshold,
				QualityId = gamificationSetting.BadgeSettings[0].QualityId,
				DataType = ExternalPerformanceDataType.Numeric
			});

			result.QualityId.Should().Be.EqualTo(1);
			result.ThresholdValue.Should().Be.EqualTo(expectedThreshold);
		}

		[Test]
		public void ShouldPersistExternalBadgeSettingGoldThreshold()
		{
			var expactedThreshold = 20;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.UpdateExternalBadgeSettingGoldThreshold(new ExternalBadgeSettingThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				ThresholdValue = expactedThreshold,
				QualityId = gamificationSetting.BadgeSettings[0].QualityId,
				DataType = ExternalPerformanceDataType.Numeric
			});

			result.QualityId.Should().Be.EqualTo(1);
			result.ThresholdValue.Should().Be.EqualTo(expactedThreshold);
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldPersistExternalBadgeSettingSilverThreshold()
		{
			var expectedThreshold = 0.3846;

			var gamificationSetting = createDefaultGamificationSetting();
			gamificationSetting.BadgeSettings[0].DataType = ExternalPerformanceDataType.Percent;
			GamificationSettingRepository.Add(gamificationSetting);
			
			var result = Target.UpdateExternalBadgeSettingSilverThreshold(new ExternalBadgeSettingThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				ThresholdValue = expectedThreshold,
				QualityId = gamificationSetting.BadgeSettings[0].QualityId,
				DataType = ExternalPerformanceDataType.Percent
			});

			result.QualityId.Should().Be.EqualTo(1);
			result.ThresholdValue.Should().Be.EqualTo(expectedThreshold);
		}

		[Test]
		public void ShouldPersistExternalBadgeSettingBronzeThreshold()
		{
			var expactedThreshold = 20;

			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);
			
			var result = Target.UpdateExternalBadgeSettingBronzeThreshold(new ExternalBadgeSettingThresholdViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				ThresholdValue = expactedThreshold,
				QualityId = gamificationSetting.BadgeSettings[0].QualityId,
				DataType = ExternalPerformanceDataType.Numeric
			});

			result.QualityId.Should().Be.EqualTo(1);
			result.ThresholdValue.Should().Be.EqualTo(expactedThreshold);
		}

		[Test]
		public void ShouldPersistExternalBadgeSettingLargerIsBetterWhenThereIsNoBadgeData()
		{
			var gamificationSetting = createDefaultGamificationSetting();
			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.UpdateExternalBadgeSettingLargerIsBetter(new ExternalBadgeSettingBooleanViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				QualityId = gamificationSetting.BadgeSettings[0].QualityId,
				Value = true
			});

			result.QualityId.Should().Be.EqualTo(1);
			result.Value.Should().Be.True();
		}

		[Test]
		public void ShouldCreateNewBadgeSettingForExternalexternalPerformanceNotSet()
		{
			var externalPerformance1 = new ExternalPerformance { ExternalId = 1, Name = "qi1", }.WithId();
			var externalPerformance2 = new ExternalPerformance { ExternalId = 2, Name = "qi2", DataType = ExternalPerformanceDataType.Percent }.WithId();
			ExternalPerformanceRepository.Add(externalPerformance1);
			ExternalPerformanceRepository.Add(externalPerformance2);

			var gamificationSetting = createDefaultGamificationSetting();
			gamificationSetting.BadgeSettings.Clear();
			GamificationSettingRepository.Add(gamificationSetting);

			var input = new ExternalBadgeSettingBooleanViewModel
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = true,
				QualityId = 2
			};
			var result = Target.UpdateExternalBadgeSettingEnabled(input);

			result.Value.Should().Be.True();
			Assert.AreEqual(1, gamificationSetting.BadgeSettings.Count);

			var newBadgeSetting = gamificationSetting.BadgeSettings.First();
			// Check new created badge settting
			Assert.AreEqual(externalPerformance2.Name, newBadgeSetting.Name);
			Assert.AreEqual(externalPerformance2.ExternalId, newBadgeSetting.QualityId);
			Assert.AreEqual(true, newBadgeSetting.Enabled);
			Assert.AreEqual(0, newBadgeSetting.Threshold);
			Assert.AreEqual(0, newBadgeSetting.BronzeThreshold);
			Assert.AreEqual(0, newBadgeSetting.SilverThreshold);
			Assert.AreEqual(0, newBadgeSetting.GoldThreshold);
			Assert.AreEqual(true, newBadgeSetting.LargerIsBetter);
			Assert.AreEqual(externalPerformance2.DataType, newBadgeSetting.DataType);
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldUpateBadgeSettingThreshold()
		{
			var expectedThreshold = 75.46;
			var gamificationSetting = new GamificationSetting("bla").WithId();
			var existingBadgeSetting = new BadgeSetting() { Threshold = 80.45, QualityId = 1 };
			gamificationSetting.AddBadgeSetting(existingBadgeSetting);
			GamificationSettingRepository.Add(gamificationSetting);

			var viewModel = new ExternalBadgeSettingThresholdViewModel()
			{
				DataType = ExternalPerformanceDataType.Numeric,
				GamificationSettingId = gamificationSetting.Id.Value,
				QualityId = existingBadgeSetting.QualityId,
				ThresholdValue = expectedThreshold
			};

			var result = Target.UpdateExternalBadgeSettingThreshold(viewModel);

			result.ThresholdValue.Should().Be.EqualTo(expectedThreshold);
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldUpateBadgeSettingGoldThreshold()
		{
			var expectedThreshold = 0.5678;
			var gamificationSetting = new GamificationSetting("bla").WithId();
			var existingBadgeSetting = new BadgeSetting() { Threshold = 0.4567, QualityId = 1 };
			gamificationSetting.AddBadgeSetting(existingBadgeSetting);
			GamificationSettingRepository.Add(gamificationSetting);

			var viewModel = new ExternalBadgeSettingThresholdViewModel()
			{
				DataType = ExternalPerformanceDataType.Percent,
				GamificationSettingId = gamificationSetting.Id.Value,
				QualityId = existingBadgeSetting.QualityId,
				ThresholdValue = expectedThreshold
			};

			var result = Target.UpdateExternalBadgeSettingGoldThreshold(viewModel);

			result.ThresholdValue.Should().Be.EqualTo(expectedThreshold);
		}

		[Test]
		public void ShouldReturnNullWhenUpdatedBadgeSettingDoNotExist()
		{
			var gamificationSetting = new GamificationSetting("bla").WithId();
			GamificationSettingRepository.Add(gamificationSetting);

			var viewModel = new ExternalBadgeSettingThresholdViewModel()
			{
				DataType = ExternalPerformanceDataType.Percent,
				GamificationSettingId = gamificationSetting.Id.Value,
				QualityId = 2,
				ThresholdValue = 45
			};

			var result = Target.UpdateExternalBadgeSettingSilverThreshold(viewModel);
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldAllowZeroForPercentValue()
		{
			var expectedThreshold = 0;
			var gamificationSetting = new GamificationSetting("bla").WithId();
			var existingBadgeSetting = new BadgeSetting() { Threshold = 0.4567, QualityId = 1 };
			gamificationSetting.AddBadgeSetting(existingBadgeSetting);
			GamificationSettingRepository.Add(gamificationSetting);

			var viewModel = new ExternalBadgeSettingThresholdViewModel()
			{
				DataType = ExternalPerformanceDataType.Percent,
				GamificationSettingId = gamificationSetting.Id.Value,
				QualityId = existingBadgeSetting.QualityId,
				ThresholdValue = expectedThreshold
			};

			var result = Target.UpdateExternalBadgeSettingGoldThreshold(viewModel);

			result.ThresholdValue.Should().Be.EqualTo(expectedThreshold);
		}

		[Test]
		public void ShouldAllowZeroForNumericValue()
		{
			var expectedThreshold = 0;
			var gamificationSetting = new GamificationSetting("bla").WithId();
			var existingBadgeSetting = new BadgeSetting() { Threshold = 0.45, QualityId = 1 };
			gamificationSetting.AddBadgeSetting(existingBadgeSetting);
			GamificationSettingRepository.Add(gamificationSetting);

			var viewModel = new ExternalBadgeSettingThresholdViewModel()
			{
				DataType = ExternalPerformanceDataType.Numeric,
				GamificationSettingId = gamificationSetting.Id.Value,
				QualityId = existingBadgeSetting.QualityId,
				ThresholdValue = expectedThreshold
			};

			var result = Target.UpdateExternalBadgeSettingGoldThreshold(viewModel);

			result.ThresholdValue.Should().Be.EqualTo(expectedThreshold);
		}
	}
}

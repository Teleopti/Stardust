using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Mapping;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Gamification.Core.DataProvider
{
	[TestFixture]
	public class GamificationSettingPersisterTest
	{
		private Guid _settingId;
		private IGamificationSettingRepository _gamificationSettingRepository;
		private IGamificationSettingRepository _fakegamificationSettingRepository;
		private IGamificationSetting _gamificationSetting;
		private IGamificationSettingMapper _mapper;

		[SetUp]
		public void Setup()
		{
			_settingId = new Guid();
			_gamificationSetting = new GamificationSetting("newGamification");
			_gamificationSetting.SetId(_settingId);
			_gamificationSettingRepository = MockRepository.GenerateMock<IGamificationSettingRepository>();
			_gamificationSettingRepository.Stub(x => x.Get(_settingId)).Return(_gamificationSetting);

			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			statisticRepository.Stub(x => x.LoadAllQualityInfo()).Return(new List<QualityInfo>());
			_mapper = new GamificationSettingMapper(statisticRepository);

			_fakegamificationSettingRepository = new FakeGamificationSettingRepository();
		}

		[Test]
		public void ShouldPersistNewGamification()
		{
			var gamificationSettingRepository = new FakeGamificationSettingRepository();
			var target = new GamificationSettingPersister(gamificationSettingRepository, _mapper);

			var vm = target.Persist();

			var result = gamificationSettingRepository.LoadAll();
			result.Count.Should().Be.EqualTo(1);
			vm.Id.HasValue.Should().Be.True();
		}

		[Test]
		public void ShouldGiveDifferentDefaultNameWhenPersistNewGamification()
		{
			var gamificationSettingRepository = new FakeGamificationSettingRepository();
			var target = new GamificationSettingPersister(gamificationSettingRepository, _mapper);

			target.Persist();
			target.Persist();

			var result = gamificationSettingRepository.LoadAll();
			result.Count.Should().Be.EqualTo(2);
			result[0].Description.Name.Should().Be.EqualTo(Resources.NewGamificationSetting);
			result[1].Description.Name.Should().Be.EqualTo(Resources.NewGamificationSetting+"1");
		}

		[Test]
		public void ShouldDeleteGamificationSuccessfully()
		{
			var gamificationSettingRepository = new FakeGamificationSettingRepository();
			gamificationSettingRepository.Add(_gamificationSetting);
			var target = new GamificationSettingPersister(gamificationSettingRepository, _mapper);

			var result = target.RemoveGamificationSetting(_settingId);
			result.Should().Be.True();
			gamificationSettingRepository.LoadAll().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnFalseWhenCannotFindGamificationForDelete()
		{
			var gamificationSettingRepository = new FakeGamificationSettingRepository();
			var target = new GamificationSettingPersister(gamificationSettingRepository, _mapper);

			var result = target.RemoveGamificationSetting(_settingId);
			result.Should().Be.False();
		}

		[Test]
		public void ShouldPersistGamificationDescription()
		{
			var expactedDescription = new Description("modifiedDescription");
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.Description.Should().Not.Be.EqualTo(expactedDescription);

			var result = target.PersistDescription(new GamificationDescriptionViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expactedDescription
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Name.Should().Be.EqualTo(expactedDescription.Name);
		}

		[Test]
		public void ShouldReturnNUllWhenCannotFindGamificationSettingForPersistDescription()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistDescription(new GamificationDescriptionViewModel()
			{
				GamificationSettingId = _settingId,
				Value = new Description("bla")
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsEnabled()
		{
			const bool expectedResult = true;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AnsweredCallsBadgeEnabled.Should().Be.False();

			var result = target.PersistAnsweredCallsEnabled(new GamificationThresholdEnabledViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAnsweredCallsEnabled()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAnsweredCallsEnabled(new GamificationThresholdEnabledViewModel()
			{
				GamificationSettingId = _settingId,
				Value = true
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsThreshold()
		{
			var expectedResult = 150;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AnsweredCallsThreshold.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistAnsweredCallsThreshold(new GamificationAnsweredCallsThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAnsweredCallsThreshold()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAnsweredCallsThreshold(new GamificationAnsweredCallsThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = 100
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsGoldThreshold()
		{
			var expectedResult = 150;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AnsweredCallsGoldThreshold.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistAnsweredCallsGoldThreshold(new GamificationAnsweredCallsThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAnsweredCallsGoldThreshold()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAnsweredCallsGoldThreshold(new GamificationAnsweredCallsThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = 100
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsSilverThreshold()
		{
			var expectedResult = 110;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AnsweredCallsSilverThreshold.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistAnsweredCallsSilverThreshold(new GamificationAnsweredCallsThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAnsweredCallsSilverThreshold()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAnsweredCallsSilverThreshold(new GamificationAnsweredCallsThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = 100
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsBronzeThreshold()
		{
			var expectedResult = 90;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AnsweredCallsBronzeThreshold.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistAnsweredCallsBronzeThreshold(new GamificationAnsweredCallsThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAnsweredCallsBronzeThreshold()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAnsweredCallsBronzeThreshold(new GamificationAnsweredCallsThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = 100
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAhtEnabled()
		{
			const bool expectedResult = true;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AHTBadgeEnabled.Should().Be.False();

			var result = target.PersistAHTEnabled(new GamificationThresholdEnabledViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAhtEnabled()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAHTEnabled(new GamificationThresholdEnabledViewModel()
			{
				GamificationSettingId = _settingId,
				Value = false
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAhtThreshold()
		{
			var expectedResult = new TimeSpan(0,2,30);
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AHTThreshold.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistAHTThreshold(new GamificationAHTThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAhtThreshold()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAHTThreshold(new GamificationAHTThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = new TimeSpan(0,2,30)
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAhtGoldThreshold()
		{
			var expectedResult = new TimeSpan(0,2,30);
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AHTGoldThreshold.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistAHTGoldThreshold(new GamificationAHTThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAhtGoldThreshold()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAHTGoldThreshold(new GamificationAHTThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = new TimeSpan(0, 2, 30)
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAhtSilverThreshold()
		{
			var expectedResult = new TimeSpan(0,3,30);
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AHTSilverThreshold.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistAHTSilverThreshold(new GamificationAHTThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAhtSilverThreshold()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAHTSilverThreshold(new GamificationAHTThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = new TimeSpan(0, 2, 30)
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAhtBronzeThreshold()
		{
			var expectedResult = new TimeSpan(0,4,30);
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AHTBronzeThreshold.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistAHTBronzeThreshold(new GamificationAHTThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAhtBronzeThreshold()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAHTBronzeThreshold(new GamificationAHTThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = new TimeSpan(0, 2, 30)
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAdherenceEnabled()
		{
			const bool expectedResult = true;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AdherenceBadgeEnabled.Should().Be.False();

			var result = target.PersistAdherenceEnabled(new GamificationThresholdEnabledViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAdherenceEnabled()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAdherenceEnabled(new GamificationThresholdEnabledViewModel()
			{
				GamificationSettingId = _settingId,
				Value = false
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAdherenceThreshold()
		{
			var expectedResult = new Percent(90);
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AdherenceThreshold.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistAdherenceThreshold(new GamificationAdherenceThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAdherenceThreshold()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAdherenceThreshold(new GamificationAdherenceThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = new Percent(80)
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAdherenceGoldThreshold()
		{
			var expectedResult = new Percent(90);
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AdherenceGoldThreshold.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistAdherenceGoldThreshold(new GamificationAdherenceThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAdherenceGoldThreshold()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAdherenceGoldThreshold(new GamificationAdherenceThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = new Percent(80)
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAdherenceSilverThreshold()
		{
			var expectedResult = new Percent(85);
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AdherenceSilverThreshold.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistAdherenceSilverThreshold(new GamificationAdherenceThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAdherenceSilverThreshold()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAdherenceSilverThreshold(new GamificationAdherenceThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = new Percent(80)
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAdherenceBronzeThreshold()
		{
			var expectedResult = new Percent(70);
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.AdherenceBronzeThreshold.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistAdherenceBronzeThreshold(new GamificationAdherenceThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistAdherenceBronzeThreshold()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistAdherenceBronzeThreshold(new GamificationAdherenceThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				Value = new Percent(80)
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationRuleChange()
		{
			var expectedResult = GamificationSettingRuleSet.RuleWithRatioConvertor;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.GamificationSettingRuleSet.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistRuleChange(new GamificationChangeRuleForm()
			{
				GamificationSettingId = _settingId,
				Rule = expectedResult
			});

			result.Id.Should().Be.EqualTo(_gamificationSetting.Id);
			result.GamificationSettingRuleSet.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistRuleChange()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistRuleChange(new GamificationChangeRuleForm()
			{
				GamificationSettingId = _settingId,
				Rule = GamificationSettingRuleSet.RuleWithDifferentThreshold
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationGoldToSilverRateChange()
		{
			var expectedResult = 6;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.GoldToSilverBadgeRate.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistGoldToSilverRate(new GamificationBadgeConversRateViewModel()
			{
				GamificationSettingId = _settingId,
				Rate = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Rate.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistGoldToSilverRateChange()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistGoldToSilverRate(new GamificationBadgeConversRateViewModel()
			{
				GamificationSettingId = _settingId,
				Rate = 5
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationSilverToBronzeRateChange()
		{
			const int expectedResult = 6;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper);
			_gamificationSetting.SilverToBronzeBadgeRate.Should().Not.Be.EqualTo(expectedResult);

			var result = target.PersistSilverToBronzeRate(new GamificationBadgeConversRateViewModel()
			{
				GamificationSettingId = _settingId,
				Rate = expectedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Rate.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindGamificationSettingForPersistSilverToBronzeRateChange()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);

			var result = target.PersistSilverToBronzeRate(new GamificationBadgeConversRateViewModel()
			{
				GamificationSettingId = _settingId,
				Rate = 5
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldCreateNewBadgeSettingForExternalQualityInfoNotSet()
		{
			_fakegamificationSettingRepository.Add(_gamificationSetting);
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);
			var input = new UpdateExternalBadgeSettingViewModel
			{
				Id = _settingId,
				ExternalBadgeSettingId = null,
				Name = "New External Badge",
				Enabled = true,
				QualityId = 5,
				LargerIsBetter = false,
				Threshold = 100,
				BronzeThreshold = 100,
				SilverThreshold = 120,
				GoldThreshold = 140,
				UnitType = BadgeUnitType.Count
			};
			var result = target.PersistExternalBadgeSetting(input);
			
			Assert.AreEqual(1, _gamificationSetting.ExternalBadgeSettings.Count);

			var newBadgeSetting = _gamificationSetting.ExternalBadgeSettings.First();
			// Check new created badge settting
			Assert.AreEqual(input.Name, newBadgeSetting.Name);
			Assert.AreEqual(input.QualityId, newBadgeSetting.QualityId);
			Assert.AreEqual(input.Enabled, newBadgeSetting.Enabled);
			Assert.AreEqual(input.Threshold, newBadgeSetting.Threshold);
			Assert.AreEqual(input.BronzeThreshold, newBadgeSetting.BronzeThreshold);
			Assert.AreEqual(input.SilverThreshold, newBadgeSetting.SilverThreshold);
			Assert.AreEqual(input.GoldThreshold, newBadgeSetting.GoldThreshold);
			Assert.AreEqual(input.LargerIsBetter, newBadgeSetting.LargerIsBetter);
			Assert.AreEqual(input.UnitType, newBadgeSetting.UnitType);
			
			// Check returned viewmodel
			Assert.AreEqual(input.Name, result.Name);
			Assert.AreEqual(input.QualityId, result.QualityId);
			Assert.AreEqual(input.Enabled, result.Enabled);
			Assert.AreEqual(input.Threshold, result.Threshold);
			Assert.AreEqual(input.BronzeThreshold, result.BronzeThreshold);
			Assert.AreEqual(input.SilverThreshold, result.SilverThreshold);
			Assert.AreEqual(input.GoldThreshold, result.GoldThreshold);
			Assert.AreEqual(input.LargerIsBetter, result.LargerIsBetter);
			Assert.AreEqual(input.UnitType, result.UnitType);
		}

		[Test]
		public void ShouldUpdateExistingBadgeSetting()
		{
			var badgeSetting = new ExternalBadgeSetting
			{
				Name = "Existing External Badge",
				Enabled = false,
				QualityId = 5,
				LargerIsBetter = false,
				Threshold = 110,
				BronzeThreshold = 110,
				SilverThreshold = 130,
				GoldThreshold = 150,
				UnitType = BadgeUnitType.Count
			};
			badgeSetting.SetId(new Guid());
			_gamificationSetting.AddExternalBadgeSetting(badgeSetting);
			
			_fakegamificationSettingRepository.Add(_gamificationSetting);
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper);
			var input = new UpdateExternalBadgeSettingViewModel
			{
				Id = _settingId,
				ExternalBadgeSettingId = null,
				Name = "New External Badge",
				Enabled = true,
				QualityId = 5,
				LargerIsBetter = true,
				Threshold = 100,
				BronzeThreshold = 100,
				SilverThreshold = 120,
				GoldThreshold = 140,
				UnitType = BadgeUnitType.Percentage
			};
			var result = target.PersistExternalBadgeSetting(input);
			
			Assert.AreEqual(1, _gamificationSetting.ExternalBadgeSettings.Count);

			var newBadgeSetting = _gamificationSetting.ExternalBadgeSettings.First();
			// Check updated badge setting
			Assert.AreEqual(badgeSetting.QualityId, newBadgeSetting.QualityId);
			Assert.AreEqual(badgeSetting.LargerIsBetter, newBadgeSetting.LargerIsBetter);
			Assert.AreEqual(badgeSetting.UnitType, newBadgeSetting.UnitType);

			Assert.AreEqual(input.Name, newBadgeSetting.Name);
			Assert.AreEqual(input.Enabled, newBadgeSetting.Enabled);
			Assert.AreEqual(input.Threshold, newBadgeSetting.Threshold);
			Assert.AreEqual(input.BronzeThreshold, newBadgeSetting.BronzeThreshold);
			Assert.AreEqual(input.SilverThreshold, newBadgeSetting.SilverThreshold);
			Assert.AreEqual(input.GoldThreshold, newBadgeSetting.GoldThreshold);
			
			// Check returned viewmodel
			Assert.AreEqual(badgeSetting.QualityId, result.QualityId);
			Assert.AreEqual(badgeSetting.LargerIsBetter, result.LargerIsBetter);
			Assert.AreEqual(badgeSetting.UnitType, result.UnitType);
			
			Assert.AreEqual(input.Name, result.Name);
			Assert.AreEqual(input.Enabled, result.Enabled);
			Assert.AreEqual(input.Threshold, result.Threshold);
			Assert.AreEqual(input.BronzeThreshold, result.BronzeThreshold);
			Assert.AreEqual(input.SilverThreshold, result.SilverThreshold);
			Assert.AreEqual(input.GoldThreshold, result.GoldThreshold);
		}
	}
}

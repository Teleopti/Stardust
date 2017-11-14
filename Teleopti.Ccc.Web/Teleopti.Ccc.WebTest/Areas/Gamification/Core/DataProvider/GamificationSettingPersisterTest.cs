using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
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
		private IExternalBadgeSetting _externalBadgeSetting;
		private IStatisticRepository _statisticRepository;
		private QualityInfo _qualityInfo;

		[SetUp]
		public void Setup()
		{
			_settingId = new Guid();
			_gamificationSetting = new GamificationSetting("newGamification");
			_gamificationSetting.SetId(_settingId);
			_externalBadgeSetting = new ExternalBadgeSetting()
			{
				QualityId = 1,
				UnitType = BadgeUnitType.Count
			};
			_gamificationSetting.ExternalBadgeSettings = new List<IExternalBadgeSetting>(){ _externalBadgeSetting };
			_gamificationSettingRepository = MockRepository.GenerateMock<IGamificationSettingRepository>();
			_gamificationSettingRepository.Stub(x => x.Get(_settingId)).Return(_gamificationSetting);

			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			statisticRepository.Stub(x => x.LoadAllQualityInfo()).Return(new List<QualityInfo>());
			_mapper = new GamificationSettingMapper(statisticRepository);

			_fakegamificationSettingRepository = new FakeGamificationSettingRepository();

			var qualityInfo1 = new QualityInfo() { QualityId = 1, QualityName = "qi1", QualityType = "PERCENTAGE", ScoreWeight = 1 };
			_qualityInfo = new QualityInfo() { QualityId = 2, QualityName = "qi2", QualityType = "PERCENTAGE", ScoreWeight = 1 };
			_statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			_statisticRepository.Stub(x => x.LoadAllQualityInfo()).Return(new List<QualityInfo>() { qualityInfo1, _qualityInfo });
		}

		[Test]
		public void ShouldPersistNewGamification()
		{
			var gamificationSettingRepository = new FakeGamificationSettingRepository();
			var target = new GamificationSettingPersister(gamificationSettingRepository, _mapper, null);

			var vm = target.Persist();

			var result = gamificationSettingRepository.LoadAll();
			result.Count.Should().Be.EqualTo(1);
			vm.Id.HasValue.Should().Be.True();
		}

		[Test]
		public void ShouldGiveDifferentDefaultNameWhenPersistNewGamification()
		{
			var gamificationSettingRepository = new FakeGamificationSettingRepository();
			var target = new GamificationSettingPersister(gamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(gamificationSettingRepository, _mapper, null);

			var result = target.RemoveGamificationSetting(_settingId);
			result.Should().Be.True();
			gamificationSettingRepository.LoadAll().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnFalseWhenCannotFindGamificationForDelete()
		{
			var gamificationSettingRepository = new FakeGamificationSettingRepository();
			var target = new GamificationSettingPersister(gamificationSettingRepository, _mapper, null);

			var result = target.RemoveGamificationSetting(_settingId);
			result.Should().Be.False();
		}

		[Test]
		public void ShouldPersistGamificationDescription()
		{
			var expactedDescription = new Description("modifiedDescription");
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
			_gamificationSetting.Description.Should().Not.Be.EqualTo(expactedDescription);

			var result = target.PersistDescription(new GamificationDescriptionForm()
			{
				GamificationSettingId = _settingId,
				Name = expactedDescription.Name
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Name.Should().Be.EqualTo(expactedDescription.Name);
		}

		[Test]
		public void ShouldReturnNUllWhenCannotFindGamificationSettingForPersistDescription()
		{
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

			var result = target.PersistDescription(new GamificationDescriptionForm()
			{
				GamificationSettingId = _settingId,
				Name = "bla"
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsEnabled()
		{
			const bool expectedResult = true;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

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
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, null);
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
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, null);

			var result = target.PersistSilverToBronzeRate(new GamificationBadgeConversRateViewModel()
			{
				GamificationSettingId = _settingId,
				Rate = 5
			});

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistExternalBadgeSettingDescription()
		{
			var expactedName = "newName";
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, _statisticRepository);

			var result = target.PersistExternalBadgeDescription(new ExternalBadgeSettingDescriptionViewModel()
			{
				GamificationSettingId = _settingId,
				Name = expactedName,
				QualityId = 1
			});

			result.QualityId.Should().Be.EqualTo(1);
			result.Name.Should().Be.EqualTo(expactedName);
		}

		[Test]
		public void ShouldPersistExternalBadgeSettingThreshold()
		{
			var expactedThreshold = 20;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, _statisticRepository);

			var result = target.PersistExternalBadgeThreshold(new ExternalBadgeSettingThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				ThresholdValue = expactedThreshold,
				QualityId = _externalBadgeSetting.QualityId,
				UnitType = BadgeUnitType.Count
			});

			result.QualityId.Should().Be.EqualTo(1);
			result.ThresholdValue.Should().Be.EqualTo(expactedThreshold);
		}

		[Test]
		public void ShouldPersistExternalBadgeSettingGoldThreshold()
		{
			var expactedThreshold = 20;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, _statisticRepository);

			var result = target.PersistExternalBadgeGoldThreshold(new ExternalBadgeSettingThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				ThresholdValue = expactedThreshold,
				QualityId = _externalBadgeSetting.QualityId,
				UnitType = BadgeUnitType.Count
			});

			result.QualityId.Should().Be.EqualTo(1);
			result.ThresholdValue.Should().Be.EqualTo(expactedThreshold);
		}

		[Test]
		public void ShouldPersistExternalBadgeSettingSilverThreshold()
		{
			var expactedThreshold = 20;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, _statisticRepository);

			var result = target.PersistExternalBadgeSilverThreshold(new ExternalBadgeSettingThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				ThresholdValue = expactedThreshold,
				QualityId = _externalBadgeSetting.QualityId,
				UnitType = BadgeUnitType.Count
			});

			result.QualityId.Should().Be.EqualTo(1);
			result.ThresholdValue.Should().Be.EqualTo(expactedThreshold);
		}

		[Test]
		public void ShouldPersistExternalBadgeSettingBronzeThreshold()
		{
			var expactedThreshold = 20;
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, _statisticRepository);

			var result = target.PersistExternalBadgeBronzeThreshold(new ExternalBadgeSettingThresholdViewModel()
			{
				GamificationSettingId = _settingId,
				ThresholdValue = expactedThreshold,
				QualityId = _externalBadgeSetting.QualityId,
				UnitType = BadgeUnitType.Count
			});

			result.QualityId.Should().Be.EqualTo(1);
			result.ThresholdValue.Should().Be.EqualTo(expactedThreshold);
		}

		[Test]
		public void ShouldPersistExternalBadgeSettingLargerIsBetterWhenThereIsNoBadgeData()
		{
			var target = new GamificationSettingPersister(_gamificationSettingRepository, _mapper, _statisticRepository);

			var result = target.PersistExternalBadgeLargerIsBetter(new ExternalBadgeSettingBooleanViewModel()
			{
				GamificationSettingId = _settingId,
				QualityId = _externalBadgeSetting.QualityId,
				Value = true
			});

			result.QualityId.Should().Be.EqualTo(1);
			result.Value.Should().Be.True();
		}

		[Test]
		public void ShouldCreateNewBadgeSettingForExternalQualityInfoNotSet()
		{
			var settingId = Guid.NewGuid();
			var gamificationSetting = new GamificationSetting("gamificationSetting");
			gamificationSetting.WithId(settingId);
			_fakegamificationSettingRepository.Add(gamificationSetting);
			var target = new GamificationSettingPersister(_fakegamificationSettingRepository, _mapper, _statisticRepository);
			var input = new ExternalBadgeSettingBooleanViewModel
			{
				GamificationSettingId = settingId,
				Value = true,
				QualityId = 2
			};
			var result = target.PersistExternalBadgeEnabled(input);

			result.Value.Should().Be.True();
			Assert.AreEqual(1, gamificationSetting.ExternalBadgeSettings.Count);

			var newBadgeSetting = gamificationSetting.ExternalBadgeSettings.First();
			// Check new created badge settting
			Assert.AreEqual(_qualityInfo.QualityName, newBadgeSetting.Name);
			Assert.AreEqual(_qualityInfo.QualityId, newBadgeSetting.QualityId);
			Assert.AreEqual(true, newBadgeSetting.Enabled);
			Assert.AreEqual(0, newBadgeSetting.Threshold);
			Assert.AreEqual(0, newBadgeSetting.BronzeThreshold);
			Assert.AreEqual(0, newBadgeSetting.SilverThreshold);
			Assert.AreEqual(0, newBadgeSetting.GoldThreshold);
			Assert.AreEqual(true, newBadgeSetting.LargerIsBetter);
			Assert.AreEqual(_mapper.ConvertRawQualityType(_qualityInfo.QualityType), newBadgeSetting.UnitType);
		}

	}
}

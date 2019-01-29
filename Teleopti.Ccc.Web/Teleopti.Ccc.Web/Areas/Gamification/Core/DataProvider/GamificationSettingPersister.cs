using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Gamification.Mapping;
using Teleopti.Ccc.Web.Areas.Gamification.Models;


namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public class GamificationSettingPersister : IGamificationSettingPersister
	{
		private readonly IGamificationSettingRepository _gamificationSettingRepository;
		private readonly IGamificationSettingMapper _mapper;
		private readonly IExternalPerformanceRepository _externalPerformanceRepository;		
		private readonly BadgeSettingDataConverter _converter = new BadgeSettingDataConverter();
		private readonly ITeamGamificationSettingRepository _teamGamificationSettingRepository;

		public GamificationSettingPersister(IGamificationSettingRepository gamificationSettingRepository, IGamificationSettingMapper mapper, 
			IExternalPerformanceRepository externalPerformanceRepository, ITeamGamificationSettingRepository teamGamificationSettingRepository)
		{
			_gamificationSettingRepository = gamificationSettingRepository;
			_mapper = mapper;
			_externalPerformanceRepository = externalPerformanceRepository;
			_teamGamificationSettingRepository = teamGamificationSettingRepository;
		}

		public GamificationSettingViewModel Persist()
		{
			var gamificationSetting = getGamificationSetting(null);
			if (!gamificationSetting.Id.HasValue)
			{
				_gamificationSettingRepository.Add(gamificationSetting);
			}

			var result = _mapper.Map(gamificationSetting);
			result.ExternalBadgeSettings = result.ExternalBadgeSettings.OrderBy(ebs => ebs.QualityId).ToList();
			return result;
		}

		public bool RemoveGamificationSetting(Guid id)
		{
			var gamificationSetting = getGamificationSetting(id);
			if (gamificationSetting == null) return false;

			_gamificationSettingRepository.Remove(gamificationSetting);

			var teamGamificationSettings = _teamGamificationSettingRepository.FetchTeamGamificationSettings(id);
			foreach (var setting in teamGamificationSettings)
			{
				_teamGamificationSettingRepository.Remove(setting);
			}
			return true;
		}

		public GamificationDescriptionViewModel PersistDescription(GamificationDescriptionForm input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.Description.Name != input.Name) gamificationSetting.Description = new Description(input.Name);

			return new GamificationDescriptionViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.Description};
		}

		public GamificationThresholdEnabledViewModel PersistAnsweredCallsEnabled(GamificationThresholdEnabledViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AnsweredCallsBadgeEnabled != input.Value) gamificationSetting.AnsweredCallsBadgeEnabled = input.Value;

			return new GamificationThresholdEnabledViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsBadgeEnabled};
		}

		public GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsGoldThreshold(GamificationAnsweredCallsThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AnsweredCallsGoldThreshold != input.Value) gamificationSetting.AnsweredCallsGoldThreshold = input.Value;

			return new GamificationAnsweredCallsThresholdViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsGoldThreshold};
		}

		public GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsSilverThreshold(GamificationAnsweredCallsThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AnsweredCallsSilverThreshold != input.Value) gamificationSetting.AnsweredCallsSilverThreshold = input.Value;

			return new GamificationAnsweredCallsThresholdViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsSilverThreshold};
		}

		public GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsBronzeThreshold(GamificationAnsweredCallsThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AnsweredCallsBronzeThreshold != input.Value) gamificationSetting.AnsweredCallsBronzeThreshold = input.Value;

			return new GamificationAnsweredCallsThresholdViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsBronzeThreshold};
		}

		public GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsThreshold(GamificationAnsweredCallsThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AnsweredCallsThreshold != input.Value) gamificationSetting.AnsweredCallsThreshold = input.Value;

			return new GamificationAnsweredCallsThresholdViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsThreshold};
		}

		public GamificationThresholdEnabledViewModel PersistAHTEnabled(GamificationThresholdEnabledViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AHTBadgeEnabled != input.Value) gamificationSetting.AHTBadgeEnabled = input.Value;

			return new GamificationThresholdEnabledViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AHTBadgeEnabled };
		}

		public GamificationAHTThresholdViewModel PersistAHTGoldThreshold(GamificationAHTThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AHTGoldThreshold != input.Value) gamificationSetting.AHTGoldThreshold = input.Value;

			return new GamificationAHTThresholdViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AHTGoldThreshold };
		}

		public GamificationAHTThresholdViewModel PersistAHTSilverThreshold(GamificationAHTThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AHTSilverThreshold != input.Value) gamificationSetting.AHTSilverThreshold = input.Value;

			return new GamificationAHTThresholdViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AHTSilverThreshold };
		}

		public GamificationAHTThresholdViewModel PersistAHTBronzeThreshold(GamificationAHTThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AHTBronzeThreshold != input.Value) gamificationSetting.AHTBronzeThreshold = input.Value;

			return new GamificationAHTThresholdViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AHTBronzeThreshold };
		}

		public GamificationAHTThresholdViewModel PersistAHTThreshold(GamificationAHTThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AHTThreshold != input.Value) gamificationSetting.AHTThreshold = input.Value;

			return new GamificationAHTThresholdViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AHTThreshold };
		}

		public GamificationThresholdEnabledViewModel PersistAdherenceEnabled(GamificationThresholdEnabledViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AdherenceBadgeEnabled != input.Value) gamificationSetting.AdherenceBadgeEnabled = input.Value;

			return new GamificationThresholdEnabledViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AdherenceBadgeEnabled };
		}

		public GamificationAdherenceThresholdViewModel PersistAdherenceThreshold(GamificationAdherenceThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (!gamificationSetting.AdherenceThreshold.Value.Equals(input.Value)) gamificationSetting.AdherenceThreshold = new Percent(input.Value);

			return new GamificationAdherenceThresholdViewModel()
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = gamificationSetting.AdherenceThreshold.Value
			};
		}

		public GamificationAdherenceThresholdViewModel PersistAdherenceGoldThreshold(GamificationAdherenceThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (!gamificationSetting.AdherenceGoldThreshold.Value.Equals(input.Value)) gamificationSetting.AdherenceGoldThreshold = new Percent(input.Value);

			return new GamificationAdherenceThresholdViewModel()
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = gamificationSetting.AdherenceGoldThreshold.Value
			};
		}

		public GamificationAdherenceThresholdViewModel PersistAdherenceSilverThreshold(GamificationAdherenceThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (!gamificationSetting.AdherenceSilverThreshold.Value.Equals(input.Value)) gamificationSetting.AdherenceSilverThreshold = new Percent(input.Value);

			return new GamificationAdherenceThresholdViewModel()
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = gamificationSetting.AdherenceSilverThreshold.Value
			};
		}

		public GamificationAdherenceThresholdViewModel PersistAdherenceBronzeThreshold(GamificationAdherenceThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (!gamificationSetting.AdherenceBronzeThreshold.Value.Equals(input.Value)) gamificationSetting.AdherenceBronzeThreshold = new Percent(input.Value);

			return new GamificationAdherenceThresholdViewModel()
			{
				GamificationSettingId = gamificationSetting.Id.Value,
				Value = gamificationSetting.AdherenceBronzeThreshold.Value
			};
		}

		public GamificationSettingViewModel PersistRuleChange(GamificationChangeRuleForm input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			gamificationSetting.GamificationSettingRuleSet = input.Rule;
			return _mapper.Map(gamificationSetting);
		}

		public GamificationSettingViewModel PersistRollingPeriodChange(GamificationModifyRollingPeriodForm input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			gamificationSetting.RollingPeriodSet = input.RollingPeriodSet;
			return _mapper.Map(gamificationSetting);
		}

		public GamificationBadgeConversRateViewModel PersistGoldToSilverRate(GamificationBadgeConversRateViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.GoldToSilverBadgeRate != input.Rate) gamificationSetting.GoldToSilverBadgeRate = input.Rate;

			return new GamificationBadgeConversRateViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Rate = gamificationSetting.GoldToSilverBadgeRate };
		}

		public GamificationBadgeConversRateViewModel PersistSilverToBronzeRate(GamificationBadgeConversRateViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.SilverToBronzeBadgeRate != input.Rate) gamificationSetting.SilverToBronzeBadgeRate = input.Rate;

			return new GamificationBadgeConversRateViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Rate = gamificationSetting.SilverToBronzeBadgeRate };
		}

		public ExternalBadgeSettingDescriptionViewModel PersistExternalBadgeDescription(ExternalBadgeSettingDescriptionViewModel input)
		{
			var setting = getGamificationSetting(input.GamificationSettingId);
			if (setting == null) return null;

			var externalBadgeSetting = getExternalBadgeSetting(setting, input.QualityId);
			if (externalBadgeSetting == null) return null;
			externalBadgeSetting.Name = input.Name;

			return new ExternalBadgeSettingDescriptionViewModel()
			{
				GamificationSettingId = setting.Id.Value,
				Name = externalBadgeSetting.Name,
				QualityId = externalBadgeSetting.QualityId
			};
		}

		public ExternalBadgeSettingThresholdViewModel PersistExternalBadgeThreshold(ExternalBadgeSettingThresholdViewModel input)
		{
			var setting = getGamificationSetting(input.GamificationSettingId);
			if (setting == null) return null;

			var externalBadgeSetting = getExternalBadgeSetting(setting, input.QualityId);
			if (externalBadgeSetting == null) return null;
			externalBadgeSetting.Threshold = _converter.GetBadgeSettingValue(input.DataType, input.ThresholdValue);

			return new ExternalBadgeSettingThresholdViewModel
			{
				GamificationSettingId = setting.Id.Value,
				QualityId = externalBadgeSetting.QualityId,
				ThresholdValue = _converter.GetBadgeSettingValueForViewModel(externalBadgeSetting.DataType, externalBadgeSetting.Threshold),
				DataType = externalBadgeSetting.DataType
			};
		}

		public ExternalBadgeSettingThresholdViewModel PersistExternalBadgeGoldThreshold(ExternalBadgeSettingThresholdViewModel input)
		{
			var setting = getGamificationSetting(input.GamificationSettingId);
			if (setting == null) return null;

			var externalBadgeSetting = getExternalBadgeSetting(setting, input.QualityId);
			if (externalBadgeSetting == null) return null;
			externalBadgeSetting.GoldThreshold = _converter.GetBadgeSettingValue(input.DataType, input.ThresholdValue);

			return new ExternalBadgeSettingThresholdViewModel
			{
				GamificationSettingId = setting.Id.Value,
				QualityId = externalBadgeSetting.QualityId,
				ThresholdValue = _converter.GetBadgeSettingValueForViewModel(externalBadgeSetting.DataType, externalBadgeSetting.GoldThreshold),
				DataType = externalBadgeSetting.DataType
			};
		}

		public ExternalBadgeSettingThresholdViewModel PersistExternalBadgeSilverThreshold(ExternalBadgeSettingThresholdViewModel input)
		{
			var setting = getGamificationSetting(input.GamificationSettingId);
			if (setting == null) return null;

			var externalBadgeSetting = getExternalBadgeSetting(setting, input.QualityId);
			if (externalBadgeSetting == null) return null;
			externalBadgeSetting.SilverThreshold = _converter.GetBadgeSettingValue(input.DataType, input.ThresholdValue);

			return new ExternalBadgeSettingThresholdViewModel
			{
				GamificationSettingId = setting.Id.Value,
				QualityId = externalBadgeSetting.QualityId,
				ThresholdValue = _converter.GetBadgeSettingValueForViewModel(externalBadgeSetting.DataType, externalBadgeSetting.SilverThreshold),
				DataType = externalBadgeSetting.DataType
			};
		}

		public ExternalBadgeSettingThresholdViewModel PersistExternalBadgeBronzeThreshold(ExternalBadgeSettingThresholdViewModel input)
		{
			var setting = getGamificationSetting(input.GamificationSettingId);
			if (setting == null) return null;

			var externalBadgeSetting = getExternalBadgeSetting(setting, input.QualityId);
			if (externalBadgeSetting == null) return null;
			externalBadgeSetting.BronzeThreshold = _converter.GetBadgeSettingValue(input.DataType, input.ThresholdValue);

			return new ExternalBadgeSettingThresholdViewModel
			{
				GamificationSettingId = setting.Id.Value,
				QualityId = externalBadgeSetting.QualityId,
				ThresholdValue = _converter.GetBadgeSettingValueForViewModel(externalBadgeSetting.DataType, externalBadgeSetting.BronzeThreshold),
				DataType = externalBadgeSetting.DataType
			};
		}

		public ExternalBadgeSettingBooleanViewModel PersistExternalBadgeEnabled(ExternalBadgeSettingBooleanViewModel input)
		{
			var setting = getGamificationSetting(input.GamificationSettingId);
			if (setting == null) return null;

			var externalBadgeSetting = getExternalBadgeSetting(setting, input.QualityId);
			if (externalBadgeSetting == null) return null;
			externalBadgeSetting.Enabled = input.Value;

			return new ExternalBadgeSettingBooleanViewModel()
			{
				GamificationSettingId = setting.Id.Value,
				QualityId = externalBadgeSetting.QualityId,
				Value = externalBadgeSetting.Enabled
			};
		}

		public ExternalBadgeSettingBooleanViewModel PersistExternalBadgeLargerIsBetter(ExternalBadgeSettingBooleanViewModel input)
		{
			var setting = getGamificationSetting(input.GamificationSettingId);
			if (setting == null) return null;

			var externalBadgeSetting = getExternalBadgeSetting(setting, input.QualityId);
			if (externalBadgeSetting == null) return null;
			externalBadgeSetting.LargerIsBetter = input.Value;

			return new ExternalBadgeSettingBooleanViewModel()
			{
				GamificationSettingId = setting.Id.Value,
				QualityId = externalBadgeSetting.QualityId,
				Value = externalBadgeSetting.LargerIsBetter
			};
		}

		private IBadgeSetting getExternalBadgeSetting(IGamificationSetting setting, int qualityId)
		{
			var externalBadgeSetting = setting.BadgeSettings.FirstOrDefault(x => x.QualityId == qualityId);
			if (externalBadgeSetting != null) return externalBadgeSetting;

			var externalPerformance = getExternalPerformance(qualityId);
			if (externalPerformance == null) return null;

			externalBadgeSetting = new BadgeSetting
			{
				Name = externalPerformance.Name,
				QualityId = qualityId,
				LargerIsBetter = true,
				Enabled = false,
				Threshold = 0,
				BronzeThreshold = 0,
				SilverThreshold = 0,
				GoldThreshold = 0,
				DataType = externalPerformance.DataType
			};

			setting.AddBadgeSetting(externalBadgeSetting);

			return externalBadgeSetting;
		}

		private IExternalPerformance getExternalPerformance(int inputExternalPerformanceId)
		{
			return _externalPerformanceRepository.FindExternalPerformanceByExternalId(inputExternalPerformanceId);
		}

		private IGamificationSetting getGamificationSetting(Guid? id)
		{
			return id == null ? new GamificationSetting(getNewName()) : _gamificationSettingRepository.Get((Guid)id);
		}

		private string getNewName()
		{
			var currentCount = _gamificationSettingRepository.LoadAll().Count();
			return currentCount > 0 ? string.Format(Resources.NewGamificationSetting + "{0}", currentCount) : Resources.NewGamificationSetting;
		}
	}
}
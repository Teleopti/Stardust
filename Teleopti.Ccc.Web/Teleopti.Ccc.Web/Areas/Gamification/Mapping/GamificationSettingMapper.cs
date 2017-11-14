using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Mapping
{
	public class GamificationSettingMapper : IGamificationSettingMapper
	{
		private readonly IStatisticRepository _statisticRepository;
		private static readonly ILog logger = LogManager.GetLogger(typeof(GamificationSettingMapper));

		public GamificationSettingMapper(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		public GamificationSettingViewModel Map(IGamificationSetting setting)
		{
			var vm = new GamificationSettingViewModel
			{
				Id = setting.Id,
				Name = setting.Description.Name,
				UpdatedBy = setting.UpdatedBy?.Name.ToString() ?? string.Empty,
				UpdatedOn = setting.UpdatedOn,
				GamificationSettingRuleSet = setting.GamificationSettingRuleSet,

				AnsweredCallsBadgeEnabled = setting.AnsweredCallsBadgeEnabled,
				AHTBadgeEnabled = setting.AHTBadgeEnabled,
				AdherenceBadgeEnabled = setting.AdherenceBadgeEnabled,

				AnsweredCallsThreshold = setting.AnsweredCallsThreshold,
				AnsweredCallsBronzeThreshold = setting.AnsweredCallsBronzeThreshold,
				AnsweredCallsSilverThreshold = setting.AnsweredCallsSilverThreshold,
				AnsweredCallsGoldThreshold = setting.AnsweredCallsGoldThreshold,

				AHTThreshold = setting.AHTThreshold,
				AHTBronzeThreshold = setting.AHTBronzeThreshold,
				AHTSilverThreshold = setting.AHTSilverThreshold,
				AHTGoldThreshold = setting.AHTGoldThreshold,

				AdherenceThreshold = setting.AdherenceThreshold,
				AdherenceBronzeThreshold = setting.AdherenceBronzeThreshold,
				AdherenceSilverThreshold = setting.AdherenceSilverThreshold,
				AdherenceGoldThreshold = setting.AdherenceGoldThreshold,

				SilverToBronzeBadgeRate = setting.SilverToBronzeBadgeRate,
				GoldToSilverBadgeRate = setting.GoldToSilverBadgeRate
			};

			vm.ExternalBadgeSettings = setting.ExternalBadgeSettings == null || !setting.ExternalBadgeSettings.Any()
				? new List<ExternalBadgeSettingViewModel>()
				: setting.ExternalBadgeSettings.Select(x => new ExternalBadgeSettingViewModel
				{
					Id = x.Id ?? Guid.Empty,
					Name = x.Name,
					QualityId = x.QualityId,
					LargerIsBetter = x.LargerIsBetter,

					Enabled = x.Enabled,

					Threshold = x.Threshold,
					BronzeThreshold = x.BronzeThreshold,
					SilverThreshold = x.SilverThreshold,
					GoldThreshold = x.GoldThreshold,

					UnitType = x.UnitType
				}).ToList();

			var qualityInfoList = _statisticRepository.LoadAllQualityInfo().ToList();
			if (!qualityInfoList.Any()) return vm;


			foreach (var qualityInfo in qualityInfoList)
			{
				if (vm.ExternalBadgeSettings.Any(x => x.QualityId == qualityInfo.QualityId))
				{
					continue;
				}

				BadgeUnitType unitType;
				try
				{
					unitType = ConvertRawQualityType(qualityInfo.QualityType);
				}
				catch (Exception ex)
				{
					logger.Error("Failed to convert unit type of quality.", ex);
					continue;
				}

				vm.ExternalBadgeSettings.Add(new ExternalBadgeSettingViewModel
				{
					Id = Guid.Empty,
					Name = qualityInfo.QualityName,
					QualityId = qualityInfo.QualityId,
					LargerIsBetter = true,
					Enabled = false,
					UnitType = unitType
				});
			}

			return vm;
		}

		// TODO: Check quality type conversion in product environment
		public BadgeUnitType ConvertRawQualityType(string qualityType)
		{
			BadgeUnitType result;
			switch (qualityType)
			{
				case "PERCENTAGE":
					result = BadgeUnitType.Percentage;
					break;
				case "GRADE":
					result = BadgeUnitType.Count;
					break;
				case "POINT":
					result = BadgeUnitType.Count;
					break;
				case "TIMESPAN":
					result = BadgeUnitType.Timespan;
					break;
				default:
					throw new ArgumentException($@"Unsupported quality type '{qualityType}'", nameof(qualityType));
			}

			return result;
		}
	}
}
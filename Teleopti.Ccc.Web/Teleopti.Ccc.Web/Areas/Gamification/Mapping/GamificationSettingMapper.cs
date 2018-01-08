using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Mapping
{
	public class GamificationSettingMapper : IGamificationSettingMapper
	{
		private readonly IExternalPerformanceRepository _externalPerformanceRepository;

		public GamificationSettingMapper(IExternalPerformanceRepository externalPerformanceRepository)
		{
			_externalPerformanceRepository = externalPerformanceRepository;
		}

		public GamificationSettingViewModel Map(IGamificationSetting setting)
		{
			var externalBadgeSetting = setting.BadgeSettings == null || !setting.BadgeSettings.Any()
				? new List<ExternalBadgeSettingViewModel>()
				: setting.BadgeSettings.Select(x => new ExternalBadgeSettingViewModel
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

					DataType = x.DataType
				}).ToList();

			var vm = new GamificationSettingViewModel
			{
				Id = setting.Id,
				Name = setting.Description.Name,
				UpdatedBy = setting.UpdatedBy?.Name.ToString() ?? string.Empty,
				UpdatedOn = setting.UpdatedOn,
				GamificationSettingRuleSet = setting.GamificationSettingRuleSet,
				ExternalBadgeSettings = externalBadgeSetting,

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

			var externalPerformances = _externalPerformanceRepository.FindAllExternalPerformances().ToList();
			if (!externalPerformances.Any()) return vm;

			foreach (var performanceInfo in externalPerformances)
			{
				if (vm.ExternalBadgeSettings.Any(x => x.QualityId == performanceInfo.ExternalId))
				{
					continue;
				}

				vm.ExternalBadgeSettings.Add(new ExternalBadgeSettingViewModel
				{
					Id = Guid.Empty,
					Name = performanceInfo.Name,
					QualityId = performanceInfo.ExternalId,
					LargerIsBetter = true,
					Enabled = false,
					DataType = performanceInfo.DataType
				});
			}

			return vm;
		}
	}
}
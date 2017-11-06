using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Mapping
{
	public class GamificationSettingMapper : IGamificationSettingMapper
	{
		public GamificationSettingViewModel Map(IGamificationSetting setting)
		{
			var vm = new GamificationSettingViewModel
			{
				Id = setting.Id,
				Description = setting.Description,
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
			return vm;
		}
	}
}
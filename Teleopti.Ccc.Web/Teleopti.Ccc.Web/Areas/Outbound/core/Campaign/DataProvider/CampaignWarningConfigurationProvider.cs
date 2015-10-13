using System;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Rules;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class CampaignWarningConfigurationProvider : ICampaignWarningConfigurationProvider
	{
		private readonly ISettingsPersisterAndProvider<OutboundThresholdSettings> _settingsPersisterAndProvider;

		public CampaignWarningConfigurationProvider(ISettingsPersisterAndProvider<OutboundThresholdSettings> settingsPersisterAndProvider)
		{
			_settingsPersisterAndProvider = settingsPersisterAndProvider;
		}


		public CampaignWarningConfiguration GetConfiguration(Type rule)
		{
			var setting = _settingsPersisterAndProvider.Get();

			if (rule == typeof (CampaignUnderServiceLevelRule) || rule == typeof (CampaignOverstaffRule))
			{
				return new CampaignWarningConfiguration
				{
					ThresholdType = setting.WarningThresholdType,
					Threshold = setting.RelativeWarningThreshold.Value
				};
			}

			return null;			
		}
	}
}
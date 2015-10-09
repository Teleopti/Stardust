using System;
using Teleopti.Ccc.Domain.Outbound.Rules;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class OutboundRuleConfigurationProvider : IOutboundRuleConfigurationProvider
	{
		private readonly ISettingsPersisterAndProvider<OutboundThresholdSettings> _settingsPersisterAndProvider;

		public OutboundRuleConfigurationProvider(ISettingsPersisterAndProvider<OutboundThresholdSettings> settingsPersisterAndProvider)
		{
			_settingsPersisterAndProvider = settingsPersisterAndProvider;
		}

		public OutboundRuleConfiguration GetConfiguration(Type rule)
		{
			var setting = _settingsPersisterAndProvider.Get();

			if (rule == typeof(OutboundOverstaffRule))
			{
				return new OutboundOverstaffRuleConfiguration
				{
					Threshold = setting.RelativeWarningThreshold.Value,
					ThresholdType = ThresholdType.Relative
				};
			}

			if (rule == typeof(OutboundUnderSLARule))
			{
				return new OutboundUnderSLARuleConfiguration
				{
					Threshold = setting.RelativeWarningThreshold.Value,
					ThresholdType = ThresholdType.Relative
				};
			}

			return null;
		}
	}
}
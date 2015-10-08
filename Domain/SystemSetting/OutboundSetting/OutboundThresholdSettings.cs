using System;

namespace Teleopti.Ccc.Domain.SystemSetting.OutboundSetting
{
	[Serializable]
	public class OutboundThresholdSettings : SettingValue
	{
		public double RelativeWarningThreshold { get; set; }
	}
}

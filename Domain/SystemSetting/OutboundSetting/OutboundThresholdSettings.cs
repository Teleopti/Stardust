using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.OutboundSetting
{
	[Serializable]
	public class OutboundThresholdSettings : SettingValue
	{
		public Percent RelativeWarningThreshold { get; set; }
		public ThresholdType ThresholdType { get; set; }
	}
}

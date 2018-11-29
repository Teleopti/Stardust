using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.OutboundSetting
{
	public enum WarningThresholdType
	{
		Absolute,
		Relative
	}

	[Serializable]
	public class OutboundThresholdSettings : SettingValue
	{
		public Percent RelativeWarningThreshold { get; set; }
		public WarningThresholdType WarningThresholdType { get; set; }
	}
}

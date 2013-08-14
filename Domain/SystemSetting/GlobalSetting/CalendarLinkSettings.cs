using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{

	[Serializable]
	public class CalendarLinkSettings : SettingValue
	{
		public bool IsActive { get; set; }
	}
}
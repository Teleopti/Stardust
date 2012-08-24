using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[Serializable]
	public class SmsSettings : SettingValue
	{
		public Guid OptionalColumnId { get; set; }
	}
}
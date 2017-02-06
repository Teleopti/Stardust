using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[Serializable]
	public class DataProtectionResponse : SettingValue
	{
		public const string Key = "DataProtectionResponse";
		
		public DataProtectionEnum Response { get; set; }
		public DateTime ResponseDate { get; set; }
	}

	public enum DataProtectionEnum
	{
		None,
		No,
		Yes
	}
}
namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	public class DataProtectionResponse : SettingValue
	{
		public const string Key = "DataProtectionResponse";
		
		public DataProtectionEnum Response { get; set; }
	}

	public enum DataProtectionEnum
	{
		None,
		No,
		Yes
	}
}
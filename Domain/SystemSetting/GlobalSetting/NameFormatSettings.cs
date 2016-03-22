using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[Serializable]
	public class NameFormatSettings : SettingValue
	{
		public int NameFormatId { get; set; }

	    public NameFormatSetting ToNameFormatSetting()
	    {
	        return (NameFormatSetting) NameFormatId;
	    }
    }

    public enum NameFormatSetting
    {
        FirstNameThenLastName,
        LastNameThenFirstName
    }
}
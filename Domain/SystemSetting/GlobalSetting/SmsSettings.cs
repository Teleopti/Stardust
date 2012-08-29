using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms"), Serializable]
	public class SmsSettings : SettingValue
	{
		public Guid OptionalColumnId { get; set; }
	}
}
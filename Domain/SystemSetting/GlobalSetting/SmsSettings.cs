using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms"), Serializable]
	public class SmsSettings : SettingValue
	{
		public SmsSettings()
		{
			NotificationSelection = NotificationType.Sms;
		}

		public NotificationType NotificationSelection { get; set; }

		public Guid OptionalColumnId { get; set; }

		public string EmailFrom { get; set; }
	}
}
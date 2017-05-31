using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[Serializable]
	public class SmsSettings : SettingValue
	{
		public SmsSettings()
		{
			NotificationSelection = NotificationType.Sms;
			EmailFrom = "no-reply@teleopti.com";
		}

		public NotificationType NotificationSelection { get; set; }

		public Guid OptionalColumnId { get; set; }

		public string EmailFrom { get; set; }
		public bool IsMobileNotificationEnabled { get; set; }
	}
}
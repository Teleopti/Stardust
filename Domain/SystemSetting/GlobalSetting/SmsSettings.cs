using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
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
	}
}
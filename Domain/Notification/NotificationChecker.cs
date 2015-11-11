using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class NotificationChecker : INotificationChecker
	{
		private readonly IGlobalSettingDataRepository _settingDataRepository;

		public NotificationChecker(IGlobalSettingDataRepository settingDataRepository)
		{
			_settingDataRepository = settingDataRepository;
		}

		private SmsSettings notificationSetting()
		{
			return _settingDataRepository
				.FindValueByKey("SmsSettings", new SmsSettings());
		}

		public NotificationType NotificationType()
		{
			return notificationSetting().NotificationSelection;
		}

		public NotificationLookup Lookup()
		{
			return new NotificationLookup(notificationSetting());
		}
	}

	public class NotificationLookup
	{
		private readonly SmsSettings _settings;

		public NotificationLookup(SmsSettings settings)
		{
			_settings = settings;
		}

		public string EmailSender
		{
			get { return _settings.EmailFrom; }
		}

		public string SmsMobileNumber(IPerson person)
		{
			if (_settings.OptionalColumnId.Equals(Guid.Empty)) return string.Empty;

			var result =
				person.OptionalColumnValueCollection.FirstOrDefault(
					optionalColumnValue => optionalColumnValue.Parent.Id.Equals(_settings.OptionalColumnId)) ?? new OptionalColumnValue(string.Empty);
			
			return result.Description;
		}
	}
}
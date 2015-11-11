using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture]
	public class NotificationCheckerTest
	{
		private IGlobalSettingDataRepository _settingRepository;
		private NotificationChecker _target;
		private IPerson _person;
		private IOptionalColumn _optionalColumn;

		[SetUp]
		public void Setup()
		{
			_settingRepository = new FakeGlobalSettingDataRepository();
			_target = new NotificationChecker(_settingRepository);
			_person = PersonFactory.CreatePerson();
			_optionalColumn = new OptionalColumn("Phone Number");
			_optionalColumn.SetId(Guid.NewGuid());
		}

		[Test]
		public void ShouldReturnEmptyIfNoColumnDefinedForSms()
		{
			var no = _target.Lookup().SmsMobileNumber(_person);
			Assert.That(no,Is.Empty);
		}

		[Test]
		public void ShouldReturnEmptyIfNoOptionalValuesForSms()
		{
			var settings = new SmsSettings{OptionalColumnId = Guid.NewGuid()};
			_settingRepository.PersistSettingValue("SmsSettings", settings);

			var no = _target.Lookup().SmsMobileNumber(_person);
			Assert.That(no, Is.Empty);
		}

		[Test]
		public void ShouldReturnEmptyIfNoOptionalValueMatchesForSms()
		{
			var wrongOptionalColumn = new OptionalColumn("Shoe size");
			wrongOptionalColumn.SetId(Guid.NewGuid());

			_person.AddOptionalColumnValue(new OptionalColumnValue("46"), wrongOptionalColumn);

			var settings = new SmsSettings { OptionalColumnId = _optionalColumn.Id.GetValueOrDefault() };
			_settingRepository.PersistSettingValue("SmsSettings", settings);

			var no = _target.Lookup().SmsMobileNumber(_person);
			Assert.That(no, Is.Empty);
		}

		[Test]
		public void ShouldReturnValueWhenColumnMatchesForSms()
		{
			_person.AddOptionalColumnValue(new OptionalColumnValue("+461234657"), _optionalColumn);
			var settings = new SmsSettings { OptionalColumnId = _optionalColumn.Id.GetValueOrDefault() };
			_settingRepository.PersistSettingValue("SmsSettings", settings);

			var no = _target.Lookup().SmsMobileNumber(_person);
			Assert.That(no, Is.EqualTo("+461234657"));
		}

		[Test]
		public void ShouldHaveSmsAsDefaultNotificationType()
		{
			Assert.That(_target.NotificationType(), Is.EqualTo(NotificationType.Sms));
		}

		[Test]
		public void ShouldHaveEmailNotificationType()
		{
			var emailSetting = new SmsSettings { NotificationSelection = NotificationType.Email };
			_settingRepository.PersistSettingValue("SmsSettings", emailSetting);
			
			var type = _target.NotificationType();
			Assert.That(type, Is.EqualTo(emailSetting.NotificationSelection));
		}

		[Test]
		public void ShouldHaveDefaultEmailFromAddress()
		{
			Assert.That(_target.Lookup().EmailSender, Is.EqualTo("no-reply@teleopti.com"));
		}

		[Test]
		public void ShouldGetEmailSender()
		{
			var emailSetting = new SmsSettings {NotificationSelection = NotificationType.Email, EmailFrom = "ashley@andeen.com"};
			_settingRepository.PersistSettingValue("SmsSettings", emailSetting);

			var sender = _target.Lookup().EmailSender;
			Assert.That(sender, Is.EqualTo(emailSetting.EmailFrom));
		}

		[Test]
		public void ShouldReturnValueWhenUpdatedColumnMatchesForSms()
		{
			var newOptionalColumn = new OptionalColumn("Mobile Phone");
			newOptionalColumn.SetId(Guid.NewGuid());

			_person.AddOptionalColumnValue(new OptionalColumnValue("+461234657"), _optionalColumn);
			_person.AddOptionalColumnValue(new OptionalColumnValue("+461234658"), newOptionalColumn);

			var settings = new SmsSettings { OptionalColumnId = _optionalColumn.Id.GetValueOrDefault() };
			_settingRepository.PersistSettingValue("SmsSettings", settings);

			var no = _target.Lookup().SmsMobileNumber(_person);

			settings = new SmsSettings { OptionalColumnId = newOptionalColumn.Id.GetValueOrDefault() };
			_settingRepository.PersistSettingValue("SmsSettings", settings);

			Assert.That(_target.Lookup().SmsMobileNumber(_person), Is.Not.EqualTo(no));
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[DomainTest]
	public class NotifierTest : ISetup
	{
		public FakeNotificationSender Sender;
		public Notifier Target;
		public IGlobalSettingDataRepository GlobalSettingDataRepository;
		public IPersonalSettingDataRepository PersonalSettingDataRepository;
		public FakeHttpServer Server;
		public FakeConfigReader ConfigReader;

		[Test]
		public void ShouldSendNotification()
		{
			GlobalSettingDataRepository.PersistSettingValue("SmsSettings",
				new SmsSettings {EmailFrom = "sender@teleopti.com", NotificationSelection = NotificationType.Email});
			
			var messages = new NotificationMessage();
			var person = PersonFactory.CreatePersonWithGuid("a", "a");
			person.Email = "aa@teleopti.com";
			var notificationHeader = new NotificationHeader
			{
				EmailReceiver = person.Email,
				EmailSender = "sender@teleopti.com",
				MobileNumber = string.Empty,
				PersonName = person.Name.ToString()
			};
			
			Target.Notify(messages, person);

			Sender.SentNotifications.First().Should().Be.EqualTo(new Tuple<INotificationMessage, NotificationHeader>(messages, notificationHeader));
		}

		[Test]
		public void ShouldSendAppNotification()
		{
			GlobalSettingDataRepository.PersistSettingValue("SmsSettings",
				new SmsSettings { EmailFrom = "sender@teleopti.com", NotificationSelection = NotificationType.Email });

			var userDevices = new UserDevices();
			userDevices.AddToken("device-id-token");
			PersonalSettingDataRepository.PersistSettingValue(UserDevices.Key, userDevices);
			
			ConfigReader.FakeSetting("FCM","asdf");
			
			var messages = new NotificationMessage();
			var person = PersonFactory.CreatePersonWithGuid("a", "a");
			person.Email = "aa@teleopti.com";
			var notificationHeader = new NotificationHeader
			{
				EmailReceiver = person.Email,
				EmailSender = "sender@teleopti.com",
				MobileNumber = string.Empty,
				PersonName = person.Name.ToString()
			};

			Target.Notify(messages, person);

			Server.Requests.Count.Should().Be.EqualTo(1);
			Sender.SentNotifications.First().Should().Be.EqualTo(new Tuple<INotificationMessage, NotificationHeader>(messages, notificationHeader));
		}

		[Test]
		public void ShouldSendNotificationForPersons()
		{
			GlobalSettingDataRepository.PersistSettingValue("SmsSettings",
				new SmsSettings { EmailFrom = "sender@teleopti.com", NotificationSelection = NotificationType.Email });

			var messages = new NotificationMessage();
			var person1 = PersonFactory.CreatePersonWithGuid("a", "a");
			person1.Email = "aa@teleopti.com";
			
			var person2 = PersonFactory.CreatePersonWithGuid("b", "b");
			person2.Email = "bb@teleopti.com";
			
			var notificationHeader1 = new NotificationHeader
			{
				EmailReceiver = person1.Email,
				EmailSender = "sender@teleopti.com",
				MobileNumber = string.Empty,
				PersonName = person1.Name.ToString()
			};

			var notificationHeader2 = new NotificationHeader
			{
				EmailReceiver = person2.Email,
				EmailSender = "sender@teleopti.com",
				MobileNumber = string.Empty,
				PersonName = person2.Name.ToString()
			};
			
			Target.Notify(messages, person1, person2);

			Sender.SentNotifications.First().Should().Be.EqualTo(new Tuple<INotificationMessage,NotificationHeader>(messages, notificationHeader1));
			Sender.SentNotifications.Last().Should().Be.EqualTo(new Tuple<INotificationMessage, NotificationHeader>(messages, notificationHeader2));
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<Notifier>().For<INotifier>();
			system.UseTestDouble<MultipleNotificationSenderFactory>().For<INotificationSenderFactory>();
			system.UseTestDouble<NotificationChecker>().For<INotificationChecker>();
			system.UseTestDouble<FakeNotificationSender>().For<INotificationSender>();
			system.UseTestDouble<FakeNotificationConfigReader>().For<INotificationConfigReader>();
			system.UseTestDouble<FakeGlobalSettingDataRepository>().For<IGlobalSettingDataRepository>();
			system.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
			system.UseTestDouble<FakeHttpServer>().For<IHttpServer>();
		}
	}

	public class FakeNotificationConfigReader : INotificationConfigReader
	{
		public bool HasLoadedConfig { get; }
		public IXPathNavigable XmlDocument { get; }
		public Uri Url { get; }
		public string User { get; }
		public string Password { get; }
		public string From { get; }
		public string ClassName { get; }
		public string Assembly { get; }
		public string Api { get; }
		public string Data { get; }
		public string FindSuccessOrError { get; }
		public string ErrorCode { get; }
		public string SuccessCode { get; }
		public bool SkipSearch { get; }
		public string SmtpHost { get; }
		public int SmtpPort { get; }
		public bool SmtpUseSsl { get; }
		public string SmtpUser { get; }
		public string SmtpPassword { get; }
		public bool SmtpUseRelay { get; }

		public INotificationClient CreateClient()
		{
			return null;
		}
	}

	public class FakeNotificationSender : INotificationSender
	{
		public List<Tuple<INotificationMessage,NotificationHeader>> SentNotifications = new List<Tuple<INotificationMessage, NotificationHeader>>();

		public void SendNotification(INotificationMessage message, NotificationHeader notificationHeader)
		{
			SentNotifications.Add(new Tuple<INotificationMessage, NotificationHeader>(message,notificationHeader));
		}

		public void SetConfigReader(INotificationConfigReader notificationConfigReader)
		{
		}
	}
}

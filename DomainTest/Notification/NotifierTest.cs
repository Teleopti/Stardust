using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[DomainTest]
	public class NotifierTest : ISetup
	{
		public FakeNotificationSender Sender;
		public Notifier Target;
		public IGlobalSettingDataRepository GlobalSettingDataRepository;
		public FakeUserDeviceRepository UserDeviceRepository;
		public FakeHttpServer Server;
		public FakeConfigReader ConfigReader;
		public FakeCurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public FakeLoggedOnUser LoggedOnUser;


		[Test]
		public async Task ShouldNotNotifyWhenNoLicense()
		{
			setCurrentDataSource();

			var messages = new NotificationMessage();
			var person = PersonFactory.CreatePersonWithGuid("a", "a");
			person.Email = "aa@teleopti.com";

			await Target.Notify(messages, person);

			Sender.SentNotifications.Count.Should().Be.EqualTo(0);
		}


		[Test]
		public async Task ShouldNotifyAndSetCustomerName()
		{
			setValidLicense();
			GlobalSettingDataRepository.PersistSettingValue("SmsSettings",
				new SmsSettings { EmailFrom = "sender@teleopti.com", NotificationSelection = NotificationType.Email });

			var messages = new NotificationMessage();
			var person = PersonFactory.CreatePersonWithGuid("a", "a");
			person.Email = "aa@teleopti.com";

			await Target.Notify(messages, person);

			Sender.SentNotifications.Should().Not.Be.Empty();
			messages.CustomerName.Should().Be.EqualTo("test");
		}

		[Test]
		public async Task ShouldSendNotification()
		{
			setValidLicense();
			GlobalSettingDataRepository.PersistSettingValue("SmsSettings",
				new SmsSettings { EmailFrom = "sender@teleopti.com", NotificationSelection = NotificationType.Email });

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

			await Target.Notify(messages, person);

			Sender.SentNotifications.First().Should().Be.EqualTo(new Tuple<INotificationMessage, NotificationHeader>(messages, notificationHeader));
		}

		[Test]
		public async Task ShouldSendAppNotification()
		{
			setValidLicense();
			GlobalSettingDataRepository.PersistSettingValue("SmsSettings",
				new SmsSettings { EmailFrom = "sender@teleopti.com", NotificationSelection = NotificationType.Email, IsMobileNotificationEnabled = true });

			var person = PersonFactory.CreatePersonWithGuid("a", "a");

			UserDeviceRepository.Add(new UserDevice
			{
				Owner = person,
				Token = "device-id-token"
			});

			ConfigReader.FakeSetting("FCM", "asdf");

			var messages = new NotificationMessage();
			
			person.Email = "aa@teleopti.com";
			var notificationHeader = new NotificationHeader
			{
				EmailReceiver = person.Email,
				EmailSender = "sender@teleopti.com",
				MobileNumber = string.Empty,
				PersonName = person.Name.ToString()
			};

			await Target.Notify(messages, person);

			Server.Requests.Count.Should().Be.EqualTo(1);
			Sender.SentNotifications.First().Should().Be.EqualTo(new Tuple<INotificationMessage, NotificationHeader>(messages, notificationHeader));
		}

		[Test]
		public async Task ShouldSendNotificationForPersons()
		{
			setValidLicense();
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

			await Target.Notify(messages, person1, person2);

			Sender.SentNotifications.First().Should().Be.EqualTo(new Tuple<INotificationMessage, NotificationHeader>(messages, notificationHeader1));
			Sender.SentNotifications.Last().Should().Be.EqualTo(new Tuple<INotificationMessage, NotificationHeader>(messages, notificationHeader2));
		}

		[Test]
		public async Task ShouldRemoveUserDevicesInvalidTokensAfterFCMResponseResultWithError()
		{
			
			setValidLicense();
			GlobalSettingDataRepository.PersistSettingValue("SmsSettings",
				new SmsSettings { EmailFrom = "sender@teleopti.com", NotificationSelection = NotificationType.Email, IsMobileNotificationEnabled = true });

			var person = PersonFactory.CreatePersonWithGuid("a", "a");
			UserDeviceRepository.Add(new UserDevice{Token= "valid-id-token",Owner = person});
			UserDeviceRepository.Add(new UserDevice { Token = "invalid-id-token", Owner = person });

			ConfigReader.FakeSetting("FCM", "asdf");

			var messages = new NotificationMessage();
			
			person.Email = "aa@teleopti.com";

			var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
			var response = new FCMSendMessageResponse
			{
				failure = 1,
				results = new[]
				{
					new FCMSendMessageResult
					{
						message_id = "1:232432"
					},
					new FCMSendMessageResult
					{
						error = "InvalidRegistration"
					}
				}
			};
			responseMessage.Content = new StringContent(JsonConvert.SerializeObject(response));
			responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			Server.FakeResponseMessage(responseMessage);

			var nofifySuccess = await Target.Notify(messages, person);

			if (nofifySuccess)
			{
				UserDeviceRepository.Find(person).Single().Token.Should().Be("valid-id-token");
			}

		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<Notifier>().For<INotifier>();
			system.UseTestDouble<MultipleNotificationSenderFactory>().For<INotificationSenderFactory>();
			system.UseTestDouble<NotifyAppSubscriptions>().For<NotifyAppSubscriptions>();
			system.UseTestDouble<NotificationChecker>().For<INotificationChecker>();
			system.UseTestDouble<FakeNotificationSender>().For<INotificationSender>();
			system.UseTestDouble<FakeNotificationConfigReader>().For<INotificationConfigReader>();
			system.UseTestDouble<FakeGlobalSettingDataRepository>().For<IGlobalSettingDataRepository>();
			system.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
			system.UseTestDouble<FakeHttpServer>().For<IHttpServer>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		private void setValidLicense()
		{
			setCurrentDataSource();
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentUnitOfWorkFactory.Current().Name, new FakeLicenseActivator("test"));
			DefinedLicenseDataFactory.GetLicenseActivator(CurrentUnitOfWorkFactory.Current().Name).EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
		}

		private void setCurrentDataSource()
		{
			var currentUow = new FakeUnitOfWorkFactory();
			currentUow.Name = "test";
			CurrentUnitOfWorkFactory.WithCurrent(currentUow);
		}

	}

	

	public class FakeNotificationSender : INotificationSender
	{
		public List<Tuple<INotificationMessage, NotificationHeader>> SentNotifications = new List<Tuple<INotificationMessage, NotificationHeader>>();

		public void SendNotification(INotificationMessage message, NotificationHeader notificationHeader)
		{
			SentNotifications.Add(new Tuple<INotificationMessage, NotificationHeader>(message, notificationHeader));
		}

		public void SetConfigReader(INotificationConfigReader notificationConfigReader)
		{
		}
	}
}

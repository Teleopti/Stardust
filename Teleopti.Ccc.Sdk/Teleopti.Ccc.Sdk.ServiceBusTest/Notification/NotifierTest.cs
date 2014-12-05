using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Notification
{
	[TestFixture]
	public class NotifierTest
	{
		[Test]
		public void ShouldSendNotification()
		{
			var notificationSenderFactory = MockRepository.GenerateMock<INotificationSenderFactory>();
			var notificationSender = MockRepository.GenerateMock<INotificationSender>();
			var notificationChecker = MockRepository.GenerateMock<INotificationChecker>();
			notificationChecker.Stub(x => x.EmailSender).Return("sender@teleopti.com");
			var messages = new NotificationMessage();
			var person = PersonFactory.CreatePersonWithGuid("a", "a");
			person.Email = "aa@teleopti.com";
			notificationChecker.Stub(x => x.SmsMobileNumber(person)).Return("0709218001");
			var notificationHeader = new NotificationHeader
			{
				EmailReceiver = person.Email,
				EmailSender = notificationChecker.EmailSender,
				MobileNumber = notificationChecker.SmsMobileNumber(person),
				PersonName = person.Name.ToString()
			};

			notificationSenderFactory.Stub(x => x.GetSender()).Return(notificationSender);

			var target = new Notifier(notificationSenderFactory, notificationChecker);
			target.Notify(messages, person);

			notificationSender.AssertWasCalled(x => x.SendNotification(messages, notificationHeader));
		}

		[Test]
		public void ShouldSendNotificationForPersons()
		{
			var notificationSenderFactory = MockRepository.GenerateMock<INotificationSenderFactory>();
			var notificationSender = MockRepository.GenerateMock<INotificationSender>();
			var notificationChecker = MockRepository.GenerateMock<INotificationChecker>();
			notificationChecker.Stub(x => x.EmailSender).Return("sender@teleopti.com");
			var messages = new NotificationMessage();
			var person1 = PersonFactory.CreatePersonWithGuid("a", "a");
			person1.Email = "aa@teleopti.com";
			notificationChecker.Stub(x => x.SmsMobileNumber(person1)).Return("0709218001");

			var person2 = PersonFactory.CreatePersonWithGuid("b", "b");
			person2.Email = "bb@teleopti.com";
			notificationChecker.Stub(x => x.SmsMobileNumber(person2)).Return("0709218002");

			var notificationHeader1 = new NotificationHeader
			{
				EmailReceiver = person1.Email,
				EmailSender = notificationChecker.EmailSender,
				MobileNumber = notificationChecker.SmsMobileNumber(person1),
				PersonName = person1.Name.ToString()
			};

			var notificationHeader2 = new NotificationHeader
			{
				EmailReceiver = person2.Email,
				EmailSender = notificationChecker.EmailSender,
				MobileNumber = notificationChecker.SmsMobileNumber(person2),
				PersonName = person2.Name.ToString()
			};

			notificationSenderFactory.Stub(x => x.GetSender()).Return(notificationSender);

			var target = new Notifier(notificationSenderFactory, notificationChecker);
			target.Notify(messages, new []{person1, person2});

			notificationSender.AssertWasCalled(x => x.SendNotification(messages, notificationHeader1));
			notificationSender.AssertWasCalled(x => x.SendNotification(messages, notificationHeader2));
		}
	}
}

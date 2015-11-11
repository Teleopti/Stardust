using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Notification
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
			notificationChecker.Stub(x => x.Lookup()).Return(new NotificationLookup(new SmsSettings{EmailFrom = "sender@teleopti.com", OptionalColumnId = Guid.Empty}));

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
			notificationChecker.Stub(x => x.Lookup()).Return(new NotificationLookup(new SmsSettings { EmailFrom = "sender@teleopti.com", OptionalColumnId = Guid.Empty }));

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

			notificationSenderFactory.Stub(x => x.GetSender()).Return(notificationSender);

			var target = new Notifier(notificationSenderFactory, notificationChecker);
			target.Notify(messages, new []{person1, person2});

			notificationSender.AssertWasCalled(x => x.SendNotification(messages, notificationHeader1));
			notificationSender.AssertWasCalled(x => x.SendNotification(messages, notificationHeader2));
		}
	}
}

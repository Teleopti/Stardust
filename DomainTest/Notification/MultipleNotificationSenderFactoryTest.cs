using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture]
	public class MultipleNotificationSenderFactoryTest
	{
		[Test]
		public void ShouldGetEmailSender()
		{
			var notificationChecker = MockRepository.GenerateMock<INotificationChecker>();
			var notificationConfigReader = MockRepository.GenerateMock<INotificationConfigReader>();
			var defaultNotificationSender = MockRepository.GenerateMock<INotificationSender>();
			notificationChecker.Stub(x => x.NotificationType()).Return(NotificationType.Email);

			var target = new MultipleNotificationSenderFactory(notificationChecker, notificationConfigReader, defaultNotificationSender);
			var result = target.GetSender();

			result.Should().Be.SameInstanceAs(defaultNotificationSender);
		}

		[Test]
		public void ShouldGetSmsSender()
		{
			var notificationChecker = MockRepository.GenerateMock<INotificationChecker>();
			var notificationConfigReader = MockRepository.GenerateMock<INotificationConfigReader>();
			var defaultNotificationSender = MockRepository.GenerateMock<INotificationSender>();
			notificationChecker.Stub(x => x.NotificationType()).Return(NotificationType.Sms);
			notificationConfigReader.Stub(x => x.HasLoadedConfig).Return(true);
			notificationConfigReader.Stub(x => x.Assembly).Return("Teleopti.Ccc.Sdk.Notification");
			notificationConfigReader.Stub(x => x.ClassName).Return("Teleopti.Ccc.Sdk.Notification.ClickatellNotificationSender");

			var target = new MultipleNotificationSenderFactory(notificationChecker, notificationConfigReader, defaultNotificationSender);
			var result = target.GetSender();

			result.Should().Not.Be.SameInstanceAs(defaultNotificationSender);
			result.Should().Be.OfType<ClickatellNotificationSender>();
		}
	}
}

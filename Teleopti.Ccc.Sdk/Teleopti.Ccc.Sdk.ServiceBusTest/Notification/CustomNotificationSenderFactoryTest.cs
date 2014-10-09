using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Notification;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Notification
{
		[TestFixture]
		public class CustomNotificationSenderFactoryTest
		{
				private MockRepository _mocks;
				private INotificationConfigReader _notificationConfigReader;
				private CustomNotificationSenderFactory _target;

				[SetUp]
				public void Setup()
				{
						_mocks = new MockRepository();
						_notificationConfigReader = _mocks.StrictMock<INotificationConfigReader>();
						_target = new CustomNotificationSenderFactory(_notificationConfigReader);
				}

				[Test]
				public void ShouldReturnClickatell()
				{
						Expect.Call(_notificationConfigReader.HasLoadedConfig).Return(true);
						Expect.Call(_notificationConfigReader.Assembly).Return("Teleopti.Ccc.Sdk.Notification");
						Expect.Call(_notificationConfigReader.ClassName)
									.Return("Teleopti.Ccc.Sdk.Notification.ClickatellNotificationSender");
						_mocks.ReplayAll();
						var sender = _target.GetSender();
						Assert.That(sender, Is.Not.Null);
						Assert.That(sender.GetType(), Is.EqualTo(typeof (ClickatellNotificationSender)));
						_mocks.VerifyAll();
				}

				[Test]
				public void ShouldReturnNullIfNoConfig()
				{
						Expect.Call(_notificationConfigReader.HasLoadedConfig).Return(false);
						_mocks.ReplayAll();
						var sender = _target.GetSender();
						Assert.That(sender, Is.Null);
						_mocks.VerifyAll();
				}

				[Test]
				public void ShouldNotThrowIfAssemblyIsWrong()
				{
						Expect.Call(_notificationConfigReader.HasLoadedConfig).Return(true);
						Expect.Call(_notificationConfigReader.Assembly).Return("Teleopti.Ccc.Sdk.Nottification");
						_mocks.ReplayAll();
						Assert.DoesNotThrow(() => _target.GetSender());
						_mocks.VerifyAll();
				}

				[Test]
				public void ShouldNotThrowIfClassIsWrong()
				{
						Expect.Call(_notificationConfigReader.HasLoadedConfig).Return(true);
						Expect.Call(_notificationConfigReader.Assembly).Return("Teleopti.Ccc.Sdk.Notification").Repeat.Twice();
						Expect.Call(_notificationConfigReader.ClassName)
									.Return("Teleopti.Ccc.Sdk.Notification.ClickAndTellNotificationSender")
									.Repeat.Twice();
						_mocks.ReplayAll();
						Assert.DoesNotThrow(() => _target.GetSender());
						_mocks.VerifyAll();
				}
		}
}
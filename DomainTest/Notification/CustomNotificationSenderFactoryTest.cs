using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Notification
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
						Expect.Call(_notificationConfigReader.Assembly).Return("Teleopti.Ccc.Domain");
						Expect.Call(_notificationConfigReader.ClassName)
									.Return("Teleopti.Ccc.Domain.Notification.ClickatellNotificationSender");
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
						Expect.Call(_notificationConfigReader.Assembly).Return("Teleopti.Ccc.Domain").Repeat.Twice();
						Expect.Call(_notificationConfigReader.ClassName)
									.Return("Teleopti.Ccc.Domain.Notification.ClickAndTellNotificationSender")
									.Repeat.Twice();
						_mocks.ReplayAll();
						Assert.DoesNotThrow(() => _target.GetSender());
						_mocks.VerifyAll();
				}
		}
}
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus.Container;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Container
{
	public class ContainerConfigurationTest
	{
		
		[Test]
		public void ShouldResolveNotificationValidationCheck()
		{
			var builder = new ContainerBuilder();
			using (var container = builder.Build())
			{
				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
				container.Resolve<INotificationValidationCheck>().Should().Be.OfType<NotificationValidationCheck>();
			}
		}

		[Test]
		public void ShouldResolveEmailConfiguration()
		{
			var builder = new ContainerBuilder();
			using (var container = builder.Build())
			{
				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
				container.Resolve<IEmailConfiguration>().Should().Be.OfType<EmailConfiguration>();
			}
		}

	}
}
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Module = Autofac.Module;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class NotificationModule : Module
	{
		private readonly IToggleManager _toggleManager;

		public NotificationModule(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EmailSender>().As<INotificationSender>();
			builder.RegisterType<NotificationValidationCheck>().As<INotificationValidationCheck>();
			builder.RegisterType<EmailConfiguration>().As<IEmailConfiguration>();
			builder.RegisterType<NotificationChecker>().As<INotificationChecker>();
			if (!_toggleManager.IsEnabled(Toggles.Settings_AlertViaEmailFromSMSLink_30444))
				builder.RegisterType<CustomNotificationSenderFactory>().As<INotificationSenderFactory>();
			else
				builder.RegisterType<MultipleNotificationSenderFactory>().As<INotificationSenderFactory>();
			builder.RegisterType<NotificationConfigReader>().As<INotificationConfigReader>();
			builder.RegisterType<Notifier>().As<INotifier>();
		}
	}
}
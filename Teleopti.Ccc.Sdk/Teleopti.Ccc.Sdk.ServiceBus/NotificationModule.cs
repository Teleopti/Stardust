using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Toggle;
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
			builder.RegisterType<MultipleNotificationSenderFactory>().As<INotificationSenderFactory>();
			builder.RegisterType<NotificationConfigReader>().As<INotificationConfigReader>();
			builder.RegisterType<Notifier>().As<INotifier>();
			builder.RegisterType<NotifyAppSubscriptions>().ApplyAspects();
			builder.RegisterType<UserDeviceService>().SingleInstance();
		}
	}
}
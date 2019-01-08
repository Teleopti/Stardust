using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Infrastructure.Aop;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class NotificationModule : Module
	{
		private readonly IocConfiguration _iocConfiguration;

		public NotificationModule(IocConfiguration iocConfiguration)
		{
			_iocConfiguration = iocConfiguration;
		}
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SignificantChangeChecker>().As<ISignificantChangeChecker>();
			builder.RegisterType<EmailSender>().As<INotificationSender>();
			builder.RegisterType<NotificationValidationCheck>().As<INotificationValidationCheck>();
			builder.RegisterType<EmailConfiguration>().As<IEmailConfiguration>();
			builder.RegisterType<NotificationChecker>().As<INotificationChecker>();
			builder.RegisterType<MultipleNotificationSenderFactory>().As<INotificationSenderFactory>();

			if (_iocConfiguration.Toggle(Toggles.Wfm_ReadNotificationConfigurationFromDb_78242))
			{
				builder.RegisterType<NotificationConfigDbReader>().As<INotificationConfigReader>();
			}
			else
			{
				builder.RegisterType<NotificationConfigReader>().As<INotificationConfigReader>();
			}

			builder.RegisterType<Notifier>().As<INotifier>();
			builder.RegisterType<NotifyAppSubscriptions>().ApplyAspects();
			builder.RegisterType<UserDeviceService>().SingleInstance();
		}
	}
}
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Messages.Core.Ioc
{
	public class NotificationModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SignificantChangeChecker>().As<ISignificantChangeChecker>();

			builder.RegisterType<EmailSender>().As<INotificationSender>();
			builder.RegisterType<NotificationValidationCheck>().As<INotificationValidationCheck>();
			builder.RegisterType<EmailConfiguration>().As<IEmailConfiguration>();
			builder.RegisterType<NotificationChecker>().As<INotificationChecker>();
			builder.RegisterType<MultipleNotificationSenderFactory>().As<INotificationSenderFactory>();
			builder.RegisterType<NotificationConfigReader>().As<INotificationConfigReader>();
			builder.RegisterType<Notifier>().As<INotifier>();
			builder.RegisterType<NotifyAppSubscriptions>().ApplyAspects();
		}
	}
}
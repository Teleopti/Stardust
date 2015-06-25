using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.MessageBroker.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Refresh;

namespace Teleopti.Ccc.IocCommon.Configuration
{
    public class ScheduleScreenRefresherModule : Module
    {
	    private readonly IocConfiguration _configuration;

	    public ScheduleScreenRefresherModule(IocConfiguration configuration)
	    {
		    _configuration = configuration;
	    }

	    protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ScheduleDataRefresher>().As<IScheduleDataRefresher>();
            builder.RegisterType<ScheduleRefresher>().As<IScheduleRefresher>();
            builder.RegisterType<MeetingRefresher>().As<IMeetingRefresher>();
            builder.RegisterType<PersonRequestRefresher>().As<IPersonRequestRefresher>();
            builder.RegisterType<ScheduleScreenRefresher>().As<IScheduleScreenRefresher>();

		    if (_configuration.Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733))
			    builder.RegisterType<MailboxSubscriber>().As<IScheduleChangeSubscriber>().SingleInstance();
		    else
				builder.RegisterType<SignalRSubscriber>().As<IScheduleChangeSubscriber>().SingleInstance();
        }
    }
}
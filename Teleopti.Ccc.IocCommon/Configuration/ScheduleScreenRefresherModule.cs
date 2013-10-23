using Autofac;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Refresh;

namespace Teleopti.Ccc.IocCommon.Configuration
{
    public class ScheduleScreenRefresherModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ScheduleDataRefresher>().As<IScheduleDataRefresher>();
            builder.RegisterType<ScheduleRefresher>().As<IScheduleRefresher>();
            builder.RegisterType<MeetingRefresher>().As<IMeetingRefresher>();
            builder.RegisterType<PersonRequestRefresher>().As<IPersonRequestRefresher>();
            builder.RegisterType<ScheduleScreenRefresher>().As<IScheduleScreenRefresher>();
        }
    }
}
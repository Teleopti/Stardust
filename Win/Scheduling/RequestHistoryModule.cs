using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling.Requests;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class RequestHistoryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RequestHistoryView>().As<IRequestHistoryView>().InstancePerLifetimeScope();
            builder.RegisterType<RequestHistoryViewPresenter>().As<IRequestHistoryViewPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<LoadRequestHistoryCommand>().As<ILoadRequestHistoryCommand>().InstancePerLifetimeScope();
            builder.RegisterType<CommonAgentNameProvider>().As<ICommonAgentNameProvider>().InstancePerLifetimeScope();
        }
    }
}
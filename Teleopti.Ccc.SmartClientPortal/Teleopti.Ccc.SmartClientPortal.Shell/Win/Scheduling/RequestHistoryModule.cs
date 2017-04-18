using Autofac;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
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
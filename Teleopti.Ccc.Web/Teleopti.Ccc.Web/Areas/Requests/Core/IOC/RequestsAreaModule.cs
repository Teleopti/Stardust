using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.IOC
{
	public class RequestsAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RequestsProvider>().As<IRequestsProvider>().InstancePerLifetimeScope();
			builder.RegisterType<RequestFilterCreator>().As<IRequestFilterCreator>().InstancePerLifetimeScope();
			builder.RegisterType<RequestsViewModelFactory>().As<IRequestsViewModelFactory>().SingleInstance();
			builder.RegisterType<AgentScheduleViewModelProvider>().As<IAgentScheduleViewModelProvider>().SingleInstance();
			builder.RegisterType<ShiftTradeRequestViewModelFactory>().As<IShiftTradeRequestViewModelFactory>().SingleInstance();
			builder.RegisterType<RequestCommandHandlingProvider>().As<IRequestCommandHandlingProvider>().InstancePerLifetimeScope();
			builder.RegisterType<RequestViewModelMapper>().As<IRequestViewModelMapper<AbsenceAndTextRequestViewModel>>().InstancePerLifetimeScope();
			builder.RegisterType<RequestViewModelMapper>().As<IRequestViewModelMapper<ShiftTradeRequestViewModel>>().InstancePerLifetimeScope();
			builder.RegisterType<RequestViewModelMapper>().As<IRequestViewModelMapper<OvertimeRequestViewModel>>().InstancePerLifetimeScope();
			builder.RegisterType<SwapAndModifyService>().As<ISwapAndModifyService>().InstancePerDependency();
			builder.RegisterType<SwapService>().As<ISwapService>().InstancePerDependency();
			builder.RegisterType<RequestAllowanceViewModelFactory>().As<IRequestAllowanceViewModelFactory>().SingleInstance();
		}
	}
}
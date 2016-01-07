using Autofac;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.IOC
{
	public class RequestsAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RequestsProvider>().As<IRequestsProvider>().InstancePerLifetimeScope();
			builder.RegisterType<RequestsViewModelFactory>().As<IRequestsViewModelFactory>().SingleInstance();
			builder.RegisterType<ReqeustCommandHandlingProvider>().As<IRequestCommandHandlingProvider>().InstancePerLifetimeScope();

			builder.RegisterType<RequestApprovalServiceFactory>().As<IRequestApprovalServiceFactory>().InstancePerDependency();
			builder.RegisterType<SwapAndModifyService>().As<ISwapAndModifyService>().InstancePerDependency();
			builder.RegisterType<SwapService>().As<ISwapService>().InstancePerDependency();
			builder.RegisterType<ResourceCalculationOnlyScheduleDayChangeCallback>().As<IScheduleDayChangeCallback>().InstancePerDependency();
			builder.RegisterType<SaveSchedulePartService>().As<ISaveSchedulePartService>().InstancePerDependency();

		}
	}
}
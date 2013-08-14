using Autofac;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core.IoC
{
	public class AnywhereAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			// TODO: Replace with RegisterHubs when upgrading Autofac
			builder.RegisterType<TeamScheduleHub>().EnableClassInterceptors();
			builder.RegisterType<PersonScheduleHub>().EnableClassInterceptors();

			builder.RegisterType<InterceptorPipelineModule>().As<IHubPipelineModule>();
			builder.RegisterType<ExceptionHandlerPipelineModule>().As<IHubPipelineModule>();

			builder.RegisterType<ScheduleVisibleProvider>().As<IScheduleVisibleProvider>().SingleInstance();
			builder.RegisterType<PersonScheduleViewModelFactory>().As<IPersonScheduleViewModelFactory>().SingleInstance();
			builder.RegisterType<PersonScheduleViewModelMapper>().As<IPersonScheduleViewModelMapper>().SingleInstance();
		}
	}
}
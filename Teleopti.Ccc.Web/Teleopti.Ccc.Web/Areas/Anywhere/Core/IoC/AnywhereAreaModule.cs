using Autofac;
using AutofacContrib.DynamicProxy2;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
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

			builder.RegisterType<PersonScheduleViewModelFactory>().As<IPersonScheduleViewModelFactory>().SingleInstance();
			builder.RegisterType<PersonScheduleViewModelMapper>().As<IPersonScheduleViewModelMapper>().SingleInstance();
		}
	}
}
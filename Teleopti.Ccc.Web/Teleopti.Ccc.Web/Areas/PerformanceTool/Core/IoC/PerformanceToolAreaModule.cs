using Autofac;
using Autofac.Extras.DynamicProxy2;
using Teleopti.Ccc.Infrastructure.PeformanceTool;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Core.IoC
{
	public class PerformanceToolAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ConfigurationController>().EnableClassInterceptors();
			builder.RegisterType<CreateTemporaryPersonsController>().EnableClassInterceptors();
			builder.RegisterType<TestPersonCreator>().As<ITestPersonCreator>().SingleInstance();
		}
	}
}
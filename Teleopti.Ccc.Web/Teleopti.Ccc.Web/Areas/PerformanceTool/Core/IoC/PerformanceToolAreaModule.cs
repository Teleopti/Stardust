using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Core.IoC
{
	public class PerformanceToolAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PerformanceCounter>().As<IPerformanceCounter>().SingleInstance();
		}
	}
}
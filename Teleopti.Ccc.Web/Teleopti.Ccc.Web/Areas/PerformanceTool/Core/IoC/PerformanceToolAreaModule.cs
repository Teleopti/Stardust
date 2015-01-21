using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Infrastructure.PerformanceTool;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Core.IoC
{
	public class PerformanceToolAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PerformanceCounter>().As<IPerformanceCounter>().SingleInstance();
			builder.RegisterType<PersonGenerator>().As<IPersonGenerator>().SingleInstance();
			builder.RegisterType<StateGenerator>().As<IStateGenerator>().SingleInstance();
		}
	}
}
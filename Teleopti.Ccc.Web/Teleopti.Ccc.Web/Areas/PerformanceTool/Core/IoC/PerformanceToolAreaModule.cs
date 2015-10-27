using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Core.IoC
{
	public class PerformanceToolAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PersonGenerator>().As<IPersonGenerator>().SingleInstance();
			builder.RegisterType<StateGenerator>().As<IStateGenerator>().SingleInstance();
			builder.RegisterType<ScheduleGenerator>().As<IScheduleGenerator>().SingleInstance();
			builder.RegisterType<ScheduleDifferenceSaver>().As<IScheduleDifferenceSaver>().SingleInstance();
		}
	}
}
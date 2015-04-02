using Autofac;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ResourcePlannerModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<MissingForecastProvider>()
				.SingleInstance()
				.As<IMissingForecastProvider>();
			builder.RegisterType<NextPlanningPeriodProvider>()
				.SingleInstance()
				.As<INextPlanningPeriodProvider>();
		}
	}
}
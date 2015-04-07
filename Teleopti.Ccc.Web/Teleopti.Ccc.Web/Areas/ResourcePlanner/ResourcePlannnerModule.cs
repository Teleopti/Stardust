using Autofac;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

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
			builder.RegisterType<PeopleAndSkillLoaderDecider>()
				.SingleInstance()
				.As<IPeopleAndSkillLoaderDecider>();
			builder.RegisterType<DisableDeletedFilter>()
				.SingleInstance()
				.As<IDisableDeletedFilter>();
		}
	}
}
using Autofac;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
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
			builder.RegisterType<RestrictionCombiner>()
				.SingleInstance()
				.As<IRestrictionCombiner>();
			builder.RegisterType<RestrictionRetrievalOperation>()
				.SingleInstance()
				.As<IRestrictionRetrievalOperation>();
			builder.RegisterType<RestrictionExtractor>()
				.SingleInstance()
				.As<IRestrictionExtractor>();
		}
	}
}
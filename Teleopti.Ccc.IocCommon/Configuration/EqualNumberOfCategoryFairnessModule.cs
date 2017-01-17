using Autofac;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class EqualNumberOfCategoryFairnessModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EqualNumberOfCategoryFairnessService>().As<IEqualNumberOfCategoryFairnessService>();
			builder.RegisterType<DistributionForPersons>().As<IDistributionForPersons>();
			builder.RegisterType<FilterForEqualNumberOfCategoryFairness>().As<IFilterForEqualNumberOfCategoryFairness>();
			builder.RegisterType<FilterForTeamBlockInSelection>().As<IFilterForTeamBlockInSelection>().SingleInstance();
			builder.RegisterType<FilterOnSwapableTeamBlocks>().As<IFilterOnSwapableTeamBlocks>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockSwapper>().As<ITeamBlockSwapper>();
			builder.RegisterType<EqualCategoryDistributionBestTeamBlockDecider>()
				.As<IEqualCategoryDistributionBestTeamBlockDecider>();
			builder.RegisterType<EqualCategoryDistributionWorstTeamBlockDecider>()
				.As<IEqualCategoryDistributionWorstTeamBlockDecider>();
			builder.RegisterType<FilterPersonsForTotalDistribution>().As<IFilterPersonsForTotalDistribution>();
			builder.RegisterType<DistributionReportService>().As<IDistributionReportService>();
			builder.RegisterType<EqualCategoryDistributionValue>().As<IEqualCategoryDistributionValue>();
			builder.RegisterType<FilterForFullyScheduledBlocks>().As<IFilterForFullyScheduledBlocks>().SingleInstance();
			builder.RegisterType<FilterForNoneLockedTeamBlocks>().As<IFilterForNoneLockedTeamBlocks>().SingleInstance();
			builder.RegisterType<TeamBlockShiftCategoryLimitationValidator>().As<ITeamBlockShiftCategoryLimitationValidator>().InstancePerLifetimeScope();
			//ITeamBlockShiftCategoryLimitationValidator
		}
	}
}
using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle("Can (probably) not be implemented in teamblock code branch", Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public class IntradayOptimizationContext
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly SkillGroupContext _virtualSkillContext;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContext;

		public IntradayOptimizationContext(Func<ISchedulerStateHolder> schedulerStateHolder,
				SkillGroupContext virtualSkillContext,
				CascadingResourceCalculationContextFactory resourceCalculationContext)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_virtualSkillContext = virtualSkillContext;
			_resourceCalculationContext = resourceCalculationContext;
		}

		public IDisposable Create(DateOnlyPeriod period)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var virtualSkillContext = _virtualSkillContext.Create(schedulerStateHolder.SchedulingResultState.PersonsInOrganization, period);
			var resourceContext = _resourceCalculationContext.Create(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.Skills, true, period);
			return new GenericDisposable(() =>
			{
				resourceContext.Dispose();
				virtualSkillContext.Dispose();
			});
		}
	}
}
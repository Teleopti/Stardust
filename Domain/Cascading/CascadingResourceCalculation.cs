using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly VirtualSkillContext _virtualSkillContext;
		private readonly CascadeResources _cascadeResources;

		public CascadingResourceCalculation(IResourceOptimizationHelper resourceOptimizationHelper,
																Func<ISchedulerStateHolder> stateHolder,
																VirtualSkillContext virtualSkillContext,
																CascadeResources cascadeResources)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_stateHolder = stateHolder;
			_virtualSkillContext = virtualSkillContext;
			_cascadeResources = cascadeResources;
		}

		public void ForDay(DateOnly date)
		{
			doForPeriod(new DateOnlyPeriod(date, date));
		}

		public void ForAll()
		{
			doForPeriod(_stateHolder().RequestedPeriod.DateOnlyPeriod);
		}

		private void doForPeriod(DateOnlyPeriod period)
		{
			using (_virtualSkillContext.Create(period))
			{
				using (new ResourceCalculationContextFactory(_stateHolder, () => new CascadingPersonSkillProvider()).Create())
				{
					foreach (var date in period.DayCollection())
					{
						//ska vara true, true (?) - fixa och lägg på test senare
						_resourceOptimizationHelper.ResourceCalculateDate(date, false, false);
						_cascadeResources.Execute(date);
					}
				}
			}
		}
	}
}
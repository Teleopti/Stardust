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
			using (_virtualSkillContext.Create(new DateOnlyPeriod(date, date)))
			{
				//TODO - we don't want to do this for every day, need a DateOnlyPeriod method as well?
				using (new ResourceCalculationContextFactory(_stateHolder, () => new CascadingPersonSkillProvider()).Create())
				{
					_resourceOptimizationHelper.ResourceCalculateDate(date, false, false); //ska vara true, true - fixa och lägg på test senare
				}
				_cascadeResources.Execute(date);
			}
		}
	}
}
using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _stateHolder;

		public CascadingResourceCalculation(IResourceOptimizationHelper resourceOptimizationHelper, 
																Func<ISchedulerStateHolder> stateHolder)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_stateHolder = stateHolder;
		}

		public void ForDay(DateOnly date)
		{
			//fix - we don't want to do this for every day probably...
			using (new ResourceCalculationContextFactory(_stateHolder, () => new CascadingPersonSkillProvider()).Create())
			{
				_resourceOptimizationHelper.ResourceCalculateDate(date, false, false); //check this later
			}
		}
	}
}
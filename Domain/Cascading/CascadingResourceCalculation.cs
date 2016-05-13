using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public CascadingResourceCalculation(IResourceOptimizationHelper resourceOptimizationHelper, Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void ForDay(DateOnly date)
		{
			using (PersonSkillReducerContext.SetReducer(new CascadingPersonSkillReducer()))
			{
				_resourceOptimizationHelper.ResourceCalculateDate(date, false, false); //check this later
			}
		}
	}
}
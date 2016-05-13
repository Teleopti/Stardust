using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public CascadingResourceCalculation(IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
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
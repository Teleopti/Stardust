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
		private readonly IPersonSkillProvider _personSkillProvider;

		public CascadingResourceCalculation(IResourceOptimizationHelper resourceOptimizationHelper, IPersonSkillProvider personSkillProvider)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_personSkillProvider = personSkillProvider;
		}

		public void ForDay(DateOnly date)
		{
			//fix
			using (PersonSkillReducerContext.SetReducer(new CascadingPersonSkillReducer(), _personSkillProvider))
			{
				_resourceOptimizationHelper.ResourceCalculateDate(date, false, false); //check this later
			}
		}
	}
}
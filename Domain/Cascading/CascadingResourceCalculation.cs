using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private Func<ISchedulerStateHolder> _schedulerStateHolder;

		public CascadingResourceCalculation(IResourceOptimizationHelper resourceOptimizationHelper, Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void ForDay(DateOnly date)
		{
			//REMOVE LATER ////
			var persons = _schedulerStateHolder().AllPermittedPersons;

			foreach (var person in persons)
			{
				var personPeriod = person.Period(date);
				var orgPersonSkills = personPeriod.PersonSkillCollection.Where(x => x.Active);
				var prioPersonSkill = orgPersonSkills.OrderBy(x => x.Skill.CascadingIndex)
					.FirstOrDefault(s => s.Skill.CascadingIndex != null);

				if (prioPersonSkill != null)
				{
					var nonCascadingPersonSkills = orgPersonSkills.Where(x => x.Skill.CascadingIndex == null);
					person.ResetPersonSkills(personPeriod);
					person.AddSkill(prioPersonSkill.Skill, date);

					foreach (var nonCasdingSkill in nonCascadingPersonSkills)
					{
						person.AddSkill(nonCasdingSkill.Skill, date);
					}
				}
			}

			/////

			_resourceOptimizationHelper.ResourceCalculateDate(date, false, false); //check this later
		}
	}
}
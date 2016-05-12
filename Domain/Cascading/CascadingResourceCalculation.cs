﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
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
			//REMOVE LATER ////
			var persons = _schedulerStateHolder().AllPermittedPersons;

			foreach (var person in persons)
			{
				var orderedCascadingSkills = person.Period(date).PersonSkillCollection
					.Where(x => x.Skill.IsCascading() && x.Active && !((IDeleteTag)x.Skill).IsDeleted).OrderBy(x => x.Skill.CascadingIndex).ToArray();

				orderedCascadingSkills.ForEach(x => ((IPersonSkillModify) x).Active = false);
				foreach (var activity in orderedCascadingSkills.Select(personSkill => personSkill.Skill.Activity).Distinct())
				{
					var prioritizedCascadingSkillForActivity = (IPersonSkillModify)orderedCascadingSkills.First(s => s.Skill.Activity.Equals(activity));
					prioritizedCascadingSkillForActivity.Active = true;
				}
			}
			/////

			_resourceOptimizationHelper.ResourceCalculateDate(date, false, false); //check this later
		}
	}
}
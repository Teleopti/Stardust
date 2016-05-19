using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadeResources
	{
		private readonly Func<ISchedulerStateHolder> _stateHolder;

		public CascadeResources(Func<ISchedulerStateHolder> stateHolder)
		{
			_stateHolder = stateHolder;
		}

		public void Execute(DateOnly date)
		{
			var schedulingResult = _stateHolder().SchedulingResultState;
			var cascadingSkills = schedulingResult.CascadingSkills().ToArray();
			if (!cascadingSkills.Any())
				return;

			var activities = cascadingSkills.Select(x => x.Activity).Distinct();
			var timeZone = cascadingSkills.First().TimeZone; //not correct really...
			var defaultResolution = cascadingSkills.First().DefaultResolution; //(maybe) correct...
			foreach (var activity in activities)
			{
				var cascadingSkillsForActivity = cascadingSkills.Where(x => x.Activity.Equals(activity)).ToArray();
				foreach (var interval in date.ToDateTimePeriod(timeZone).Intervals(TimeSpan.FromMinutes(defaultResolution)))
				{
					var affectedSkillForSkillGroups = orderedSkillGroups(cascadingSkillsForActivity, ResourceCalculationContext.Fetch().AffectedResources(activity, interval).Values);
					foreach (var skillGroup in affectedSkillForSkillGroups)
					{
						var remainingResourcesInGroup = skillGroup.Resources;
						if (remainingResourcesInGroup.IsZero())
							continue;
						var skillToMoveFrom = skillGroup.PrimarySkill;
						ISkillStaffPeriodDictionary skillStaffPeriodFromDic;
						if (!schedulingResult.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skillToMoveFrom, out skillStaffPeriodFromDic))
							continue;
						var skillStaffPeriodFrom = skillStaffPeriodFromDic.SkillStaffPeriodOrDefault(interval);
						var remainingOverstaff = skillStaffPeriodFrom.AbsoluteDifference;
						if (remainingOverstaff <= 0)
							continue;
						foreach (var skillToMoveTo in skillGroup.CascadingSkills)
						{
							ISkillStaffPeriodDictionary skillStaffPeriodToDic;
							if (!schedulingResult.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skillToMoveTo, out skillStaffPeriodToDic))
								continue;
							var skillStaffPeriodTo = skillStaffPeriodToDic.SkillStaffPeriodOrDefault(interval);
							var skillToMoveToAbsoluteDifference = skillStaffPeriodTo.AbsoluteDifference;
							if (skillToMoveToAbsoluteDifference >= 0)
								continue;
							var resourcesToMove = Math.Min(Math.Min(Math.Abs(skillToMoveToAbsoluteDifference), remainingOverstaff), remainingResourcesInGroup);
							skillStaffPeriodTo.TakeResourcesFrom(skillStaffPeriodFrom, resourcesToMove);
							remainingResourcesInGroup -= resourcesToMove;
							remainingOverstaff -= resourcesToMove;
							if (remainingOverstaff.IsZero() || remainingResourcesInGroup.IsZero())
								break;
						}
					}
				}
			}
		}


		//make its own class - TO BE CONTINUED...///
		private static IEnumerable<CascadingSkillGroup> orderedSkillGroups(IEnumerable<ISkill> cascadingSkills, IEnumerable<AffectedSkills> affectedSkills)
		{
			//TODO: shouldn't just sort by "first" skill
			var ret = new List<CascadingSkillGroup>();
			foreach (var skillGroup in affectedSkills)
			{
				var primarySkill = cascadingSkills.First(x => skillGroup.Skills.Contains(x) && x.IsCascading());
				//TODO: check this - why is this needed? (activity check) - the resources will be wrong also...
				var skillsUsedByPrimarySkill = skillGroup.Skills.Where(x => x.IsCascading() && x.Activity.Equals(primarySkill.Activity));

				ret.Add(new CascadingSkillGroup(primarySkill, skillsUsedByPrimarySkill, skillGroup.Resource));
			}
			var orderedPrimarySkills = ret.Where(x => x.CascadingSkills.Count() > 1).OrderByDescending(x => x.PrimarySkill.CascadingIndex);

			return orderedPrimarySkills;
		}

		public class CascadingSkillGroup
		{
			public CascadingSkillGroup(ISkill primarySkill, IEnumerable<ISkill> cascadingSkills, double resources)
			{
				PrimarySkill = primarySkill;
				CascadingSkills = cascadingSkills;
				Resources = resources;
			}

			public ISkill PrimarySkill { get; private set; }
			public IEnumerable<ISkill> CascadingSkills { get; private set; }
			public double Resources { get; private set; }
		}

		/// 

		private static double resourcesInSkillGroup(IActivity activity, ISkill skill1, ISkill skill2, DateTimePeriod period)
		{
			//TODO: Fix better and put it elsewhere! No need to do "full" affectedResources call
			return ResourceCalculationContext.Fetch()
				.AffectedResources(activity, period)
				.Where(x => x.Key.Contains(skill1.Id.Value.ToString()) && x.Key.Contains(skill2.Id.Value.ToString()))
				.Sum(x => x.Value.Resource);
		}
	}
}
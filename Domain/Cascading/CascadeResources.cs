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
				foreach (var interval in date.ToDateTimePeriod(timeZone).Intervals(TimeSpan.FromMinutes(defaultResolution)))
				{
					var affectedSkillForSkillGroups = ResourceCalculationContext.Fetch().AffectedResources(activity, interval).Select(x => x.Value);
					//fel sortering
					foreach (var skillGroup in affectedSkillForSkillGroups)
					{
						var skillToMoveFrom = cascadingSkills.First(x => skillGroup.Skills.Contains(x));
						ISkillStaffPeriodDictionary skillStaffPeriodFromDic;
						if (!schedulingResult.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skillToMoveFrom, out skillStaffPeriodFromDic))
							continue;
						var skillStaffPeriodFrom = skillStaffPeriodFromDic.SkillStaffPeriodOrDefault(interval);
						var remainingOverstaff = skillStaffPeriodFrom.AbsoluteDifference;
						if (remainingOverstaff <= 0)
							continue;
						//fel sortering
						foreach (var skillToMoveTo in skillGroup.Skills.Where(x => !skillToMoveFrom.Equals(x) && skillToMoveFrom.Activity.Equals(x.Activity) && x.IsCascading()))
						{
							var resourcesInGroup = resourcesInSkillGroup(skillToMoveFrom.Activity, skillToMoveTo, skillToMoveFrom, interval);
							if (resourcesInGroup.IsZero())
								continue;
							ISkillStaffPeriodDictionary skillStaffPeriodToDic;
							if (!schedulingResult.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skillToMoveTo, out skillStaffPeriodToDic))
								continue;
							var skillStaffPeriodTo = skillStaffPeriodToDic.SkillStaffPeriodOrDefault(interval);
							var skillToMoveToAbsoluteDifference = skillStaffPeriodTo.AbsoluteDifference;
							if (skillToMoveToAbsoluteDifference >= 0)
								continue;
							var resourcesToMove = Math.Min(Math.Min(Math.Abs(skillToMoveToAbsoluteDifference), remainingOverstaff), resourcesInGroup);
							skillStaffPeriodTo.TakeResourcesFrom(skillStaffPeriodFrom, resourcesToMove);

							remainingOverstaff -= resourcesToMove;
							if (remainingOverstaff.IsZero())
								break;
						}
					}
				}
			}
		}


		//make its own class - TO BE CONTINUED...///
		private IEnumerable<CascadingSkillGroup> orderedSkillGroups(IEnumerable<ISkill> cascadingSkills, IEnumerable<IEnumerable<ISkill>> skillGroups)
		{
			//ain't correct currently
			var primarySkills = new List<ISkill>();
			foreach (var skillGroup in skillGroups)
			{
				primarySkills.Add(cascadingSkills.First(x => skillGroup.Contains(x)));
			}
			primarySkills.OrderByDescending(x => x.CascadingIndex);

			foreach (var primarySkill in primarySkills)
			{
				foreach (var skillGroup in skillGroups)
				{
					if (skillGroup.Contains(primarySkill))
						yield return new CascadingSkillGroup(primarySkill, skillGroup.Where(x => !x.Equals(primarySkill)));
				}
			}
		}

		public class CascadingSkillGroup
		{
			public CascadingSkillGroup(ISkill primarySkill, IEnumerable<ISkill> cascadingSkills)
			{
				PrimarySkill = primarySkill;
				CascadingSkills = cascadingSkills;
			}

			public ISkill PrimarySkill { get; private set; }
			public IEnumerable<ISkill> CascadingSkills { get; private set; }
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
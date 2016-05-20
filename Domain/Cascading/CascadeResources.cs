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
			//TODO: if doing this for more than one day we might to do this initiazlie stuff (down to interval) only once. Or we wait with that...
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
					//TODO: it seems that AffectedResources gives back skillgroups with skills using different activities -> hack (and incorrect) when we order the skill groups...
					foreach (var skillGroup in orderedSkillGroups(cascadingSkillsForActivity, ResourceCalculationContext.Fetch().AffectedResources(activity, interval).Values))
					{
						var remainingResourcesInGroup = skillGroup.Resources;
						if (remainingResourcesInGroup.IsZero())
							continue;
						ISkillStaffPeriodDictionary skillStaffPeriodFromDic;
						if (!schedulingResult.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skillGroup.PrimarySkill, out skillStaffPeriodFromDic))
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
				var skillsUsedByPrimarySkill = cascadingSkills.Where(x => skillGroup.Skills.Contains(x)).ToArray();
				if (skillsUsedByPrimarySkill.Length > 1)
				{
					var primarySkill = cascadingSkills.First(x => skillGroup.Skills.Contains(x) && x.IsCascading());
					ret.Add(new CascadingSkillGroup(primarySkill, skillsUsedByPrimarySkill.Where(x => !x.Equals(primarySkill)).OrderBy(x => x.CascadingIndex), skillGroup.Resource));
				}
			}

			var skillGroups =  ret.OrderByDescending(x => x.PrimarySkill.CascadingIndex).ToArray();
			var count = skillGroups.Count();

			for (var i = 0; i < count - 1; i++)
			{
				var first = skillGroups[i];
				var second = skillGroups[i + 1];
				var swap = false;

				var firstSkills = first.CascadingSkills.ToArray();
				var secondSkills = second.CascadingSkills.ToArray();

				var firstSkillsCount = firstSkills.Count();
				var secondSkillsCount = secondSkills.Count();

				for (var x = 0; x < firstSkillsCount; x++)
				{
					if (x > secondSkillsCount - 1)
					{
						swap = true;
						break;
					}

					if (firstSkills[x].CascadingIndex > secondSkills[x].CascadingIndex)
					{
						swap = true;
						break;
					}	
				}

				if (!swap) continue;

				skillGroups[i] = second;
				skillGroups[i + 1] = first;
			}

			return skillGroups;
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
	}
}
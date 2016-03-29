using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Shoveling
{
	public class ShovelServicePoc
	{
		private readonly ResourceCalculationContextFactory _resourceCalculationContext;

		public ShovelServicePoc(ResourceCalculationContextFactory resourceCalculationContext)
		{
			_resourceCalculationContext = resourceCalculationContext;
		}

		public void Execute(ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodDictionary, IDateOnlyPeriodAsDateTimePeriod periodToCalculate, IList<ISkill> allOrderedSkillList)
		{
			using (_resourceCalculationContext.CreateForShoveling(() => new PersonSkillProviderForShoveling()))
			{
				var resources = ResourceCalculationContext.Fetch();

				var skillList = skillSkillStaffPeriodDictionary.Keys.Where(skill => skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill).ToList();
				if (!skillList.Any())
					return;

				var activityList = skillList.Select(s => s.Activity).Distinct().ToList();
				foreach (var dateOnly in periodToCalculate.DateOnlyPeriod.DayCollection())
				{
					var dateTimePeriodToCalculate = periodToCalculate.DateTimePeriodForDateOnly(dateOnly);
					foreach (var activity in activityList)
					{
						var defaultResoulution = skillList.First(skill => skill.Activity.Equals(activity)).DefaultResolution;
						executeForActivity(activity, dateTimePeriodToCalculate, defaultResoulution, skillSkillStaffPeriodDictionary, allOrderedSkillList, resources);
					}
				}
			}		
		}

		public IList<ShovelResult> Analyze(ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodDictionary, DateTimePeriod dateTimePeriodToCalculate, IList<ISkill> allOrderedSkillList)
		{
			var result = new List<ShovelResult>();
			using (_resourceCalculationContext.CreateForShoveling(() => new PersonSkillProviderForShoveling()))
			{
				var resources = ResourceCalculationContext.Fetch();

				var skillList =
					skillSkillStaffPeriodDictionary.Keys.Where(skill => skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
						.ToList();
				if (!skillList.Any())
					return result;

				var activityList = skillList.Select(s => s.Activity).Distinct().ToList();
				foreach (var activity in activityList)
				{
					var defaultResoulution = skillList.First(skill => skill.Activity.Equals(activity)).DefaultResolution;
					var activityResult = executeForActivity(activity, dateTimePeriodToCalculate, defaultResoulution,
						skillSkillStaffPeriodDictionary, allOrderedSkillList, resources);
					result.AddRange(activityResult);
				}

			}

			return result;
		}

		// this one can be called in parallel for each activity
		private List<ShovelResult> executeForActivity(IActivity activity, DateTimePeriod utcPeriodToCalculate, int minSkillResoulution,
			ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodDictionary, IList<ISkill> allOrderedSkillList, 
			IResourceCalculationDataContainerWithSingleOperation resources)
		{
			var shovel = new Shovel();
			var skillGroupSorter = new SkillGroupPriotizer();
			var result = new List<ShovelResult>();
			var periodSplit = utcPeriodToCalculate.Intervals(TimeSpan.FromMinutes(minSkillResoulution));
			foreach (var dateTimePeriod in periodSplit)
			{
				var resourcesPerSkillGroup = resources.AffectedResources(activity, dateTimePeriod);
				if(!resourcesPerSkillGroup.Any())
					continue;

				foreach (var skillGroup in skillGroupSorter.Sort(resourcesPerSkillGroup.Values.ToList(), allOrderedSkillList.ToList()))
				{
					var orderedSkills = skillGroup.Skills.Where(skill => skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill).OrderBy(skill => skill.Name);
					var periodForSkillDic = new Dictionary<ISkill, ISkillStaffPeriod>();
					foreach (var skill in orderedSkills)
					{
						ISkillStaffPeriodDictionary periodsForSkill;
						if(!skillSkillStaffPeriodDictionary.TryGetValue(skill, out periodsForSkill))
							continue;

						ISkillStaffPeriod periodForSkill;
						if (!periodsForSkill.TryGetValue(dateTimePeriod, out periodForSkill))
							continue;

						periodForSkillDic.Add(skill, periodForSkill);
					}

					result.Add(shovel.Execute(periodForSkillDic.Values.ToList(), skillGroup.Resource));
				}
			}

			return result;
		}
	}

	public class SkillGroupPriotizer
	{
		public IList<AffectedSkills> Sort(IList<AffectedSkills> unorderedSkillGroupList, IList<ISkill> orderedSkillsByLevel)
		{
			if (orderedSkillsByLevel.Count > 64)
				return slowSort(unorderedSkillGroupList, orderedSkillsByLevel);

			var skillValueDic = new Dictionary<ISkill, ulong>();
			var reversed = orderedSkillsByLevel.Reverse().ToList();
			for (int i = 0; i < reversed.Count(); i++)
			{
				if(i == 0)
					skillValueDic.Add(reversed[0], 1);
				else
				{
					skillValueDic.Add(reversed[i], skillValueDic[reversed[i-1]]*2);
				}			
			}

			var skillGroupValueDic = new SortedDictionary<ulong, AffectedSkills>();
			foreach (var skillGroup in unorderedSkillGroupList)
			{
				ulong totalValue = 0;
				foreach (var skill in skillGroup.Skills)
				{
					ulong value;
					if (!skillValueDic.TryGetValue(skill, out value))
						continue;

					totalValue += value;
				}
				if (!skillGroupValueDic.ContainsKey(totalValue))
				{
					skillGroupValueDic.Add(totalValue, skillGroup);
				}
				else
				{					
					var affected = skillGroupValueDic[totalValue];
					affected.Count = affected.Count + skillGroup.Count;
					affected.Resource = affected.Resource + skillGroup.Resource;
				}
				
			}

			return skillGroupValueDic.Values.ToList();
		}

		private IList<AffectedSkills> slowSort(IList<AffectedSkills> unorderedSkillGroupList, IEnumerable<ISkill> orderedSkillsByLevel)
		{
			var highestSkillAffectedSkillsDic = new Dictionary<ISkill, IList<AffectedSkills>>();
			foreach (var skill in orderedSkillsByLevel)
			{
				foreach (var affectedSkills in unorderedSkillGroupList)
				{
					if (affectedSkills.Skills.First().Equals(skill))
					{
						IList<AffectedSkills> affectedSkillList;
						if(!highestSkillAffectedSkillsDic.TryGetValue(skill, out affectedSkillList))
							highestSkillAffectedSkillsDic.Add(skill, new List<AffectedSkills>{ affectedSkills });
						else
						{
							affectedSkillList.Add(affectedSkills);
						}
					}
				}
			}
			var resultList = new List<AffectedSkills>();
			foreach (var pair in highestSkillAffectedSkillsDic.Reverse())
			{
				resultList.AddRange(pair.Value);
			}

			return resultList;
		}
	}

	public class ShovelResult
	{
		public ISkill PrimarySkill { get; set; }
		public double ResourcesOnSkillGroup { get; set; }
		public IDictionary<ISkill, double> TransferDictionary { get; set; }

		public ShovelResult()
		{
			TransferDictionary = new Dictionary<ISkill, double>();
		}
	}

	public class Shovel
	{
		public ShovelResult Execute(List<ISkillStaffPeriod> periodList, double resourcesOnSkillGroup)
		{
			if (!periodList.Any())
				return null;

			var result = new ShovelResult{PrimarySkill = periodList.First().SkillDay.Skill, ResourcesOnSkillGroup = resourcesOnSkillGroup};

			if (periodList.Count() < 2)
				return result;

			if (periodList.First().AbsoluteDifference <= 0)
				return result;

			var pile = periodList.First();
			var totalTransferred = 0d;
			for (int level = 1; level < periodList.Count(); level++)
			{
				result.TransferDictionary.Add(periodList[level].SkillDay.Skill, 0);
				if ((periodList[level].AbsoluteDifference >= 0))
					continue;

				var pot = periodList[level];
				double toTransfer;
				var potFilled = false;
				var maxTransferReached = false;
				if (pile.AbsoluteDifference + pot.AbsoluteDifference >= 0)
				{
					toTransfer = Math.Abs(pot.AbsoluteDifference);
					potFilled = true;
				}
				else
				{
					toTransfer = pile.AbsoluteDifference;
				}

				if (totalTransferred + toTransfer > resourcesOnSkillGroup)
				{
					toTransfer = resourcesOnSkillGroup - totalTransferred;
					maxTransferReached = true;
				}

				pile.SetCalculatedResource65(pile.CalculatedResource - toTransfer);
				pot.SetCalculatedResource65(pot.CalculatedResource + toTransfer);
				totalTransferred += toTransfer;
				result.TransferDictionary[pot.SkillDay.Skill] = toTransfer;
				if (maxTransferReached || !potFilled)
					return result;
			}

			return result;
		}
	}

	public class PersonSkillProviderForShoveling : IPersonSkillProvider
	{
		private readonly ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>> _personCombination =
			new ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>>();

		public SkillCombination SkillsOnPersonDate(IPerson person, DateOnly date)
		{
			ConcurrentBag<SkillCombination> foundCombinations = _personCombination.GetOrAdd(person, _ => new ConcurrentBag<SkillCombination>());
			foreach (var foundCombination in foundCombinations)
			{
				if (foundCombination.IsValidForDate(date))
				{
					return foundCombination;
				}
			}

			IPersonPeriod personPeriod = person.Period(date);
			if (personPeriod == null) return new SkillCombination(new ISkill[0], new DateOnlyPeriod(), new SkillEffiencyResource[] { });

			var personSkillCollection =
				personPeriod.PersonSkillCollection.Where(personSkill => !((IDeleteTag)personSkill.Skill).IsDeleted).ToArray();

			var skills = personSkillCollection.Where(s => s.SkillPercentage.Value > 0)
				.Concat(personPeriod.PersonMaxSeatSkillCollection.Where(s => s.SkillPercentage.Value > 0))
				.Concat(personPeriod.PersonNonBlendSkillCollection.Where(s => s.SkillPercentage.Value > 0))
				.Select(s => s.Skill)
				.Distinct()
				.ToArray();

			var skillEfficiencies =
				personSkillCollection.Where(
					s => s.Active && s.SkillPercentage.Value > 0)
					.Select(k => new SkillEffiencyResource(k.Skill.Id.GetValueOrDefault(), k.SkillPercentage.Value)).ToArray();

			var combination = new SkillCombination(skills, personPeriod.Period, skillEfficiencies);
			foundCombinations.Add(combination);

			return combination;
		}
	}

	public static class StringExt
	{
		public static string Truncate(this string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) return value;
			return value.Length <= maxLength ? value : value.Substring(0, maxLength);
		}
	}
}
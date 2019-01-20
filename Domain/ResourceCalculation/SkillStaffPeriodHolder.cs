using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Task = Teleopti.Ccc.Domain.Forecasting.Task;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillStaffPeriodHolder : ISkillStaffPeriodHolder
	{
		private ISkillSkillStaffPeriodExtendedDictionary _internalDictionary;
		private readonly object Locker = new object();

		public SkillStaffPeriodHolder(IEnumerable<KeyValuePair<ISkill, IEnumerable<ISkillDay>>> skillDays)
		{
			CreateInternalDictionary(skillDays);
		}

		public IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> SkillStaffDataPerActivity(DateTimePeriod onPeriod, IList<ISkill> onSkills, ISkillPriorityProvider skillPriorityProvider)
		{
			var personSkillSkillStaff = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();

			var activities = onSkills.Select(s => s.Activity).Distinct().ToArray();

			var tmp = new Dictionary<ISkill, Dictionary<DateTime, ISkillStaffPeriodDataHolder>>();
			foreach (ISkill skill in onSkills)
			{
				if (_internalDictionary.TryGetValue(skill, out var skillStaffPeriods))
				{
					var dataHolders = new Dictionary<DateTime, ISkillStaffPeriodDataHolder>();

					foreach (KeyValuePair<DateTimePeriod, ISkillStaffPeriod> pair in skillStaffPeriods)
					{
						if (pair.Key.Intersect(onPeriod))
						{
							ISkillStaffPeriod skillStaffPeriod = pair.Value;
							int maximumPersons = skillStaffPeriod.Payload.SkillPersonData.MaximumPersons;
							int minimumPersons = skillStaffPeriod.Payload.SkillPersonData.MinimumPersons;

							double absoluteDifferenceScheduledHeadsAndMinMaxHeads = skillStaffPeriod.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true);
							var origDemand = skillStaffPeriod.FStaffTime().TotalMinutes;
							var assignedResource = skillStaffPeriod.Payload.CalculatedResource *
											 skillStaffPeriod.Period.ElapsedTime().TotalMinutes;
							ISkillStaffPeriodDataHolder dataHolder = new SkillStaffPeriodDataInfo(origDemand,
																									assignedResource,
																									skillStaffPeriod.
																										Period,
																									minimumPersons,
																									maximumPersons,
																									absoluteDifferenceScheduledHeadsAndMinMaxHeads,
																									skillStaffPeriod.PeriodDistribution,
																									skillPriorityProvider.GetOverstaffingFactor(skill),
																									skillPriorityProvider.GetPriorityValue(skill));

							dataHolders.Add(skillStaffPeriod.Period.StartDateTime, dataHolder);
						}
					}
					if (dataHolders.Count > 0)
					{
						dataHolders = BoostFirstAndLastInterval(dataHolders);
						tmp.Add(skill, dataHolders);
					}
				}
			}
			//Combine those with same activity
			foreach (IActivity activity in activities)
			{
				HashSet<ISkill> oldSkills = new HashSet<ISkill>();
				Dictionary<DateTime, ISkillStaffPeriodDataHolder> theData = null;
				foreach (KeyValuePair<ISkill, Dictionary<DateTime, ISkillStaffPeriodDataHolder>> pair in tmp)
				{
					ISkill skill = pair.Key;
					if (activity.Equals(skill.Activity))
					{
						if (oldSkills.Contains(skill)) break;

						if (theData == null)
							theData = pair.Value;

						oldSkills.Add(skill);
						ISkill nextSkill = FindNextSkillWithSameActivity(activity, oldSkills, tmp);
						if (nextSkill != null)
						{
							Dictionary<DateTime, ISkillStaffPeriodDataHolder> nextSkillData = tmp[nextSkill];
							theData = CombineSkillStaffPeriodDataHolder(theData, nextSkillData);
						}
					}
				}
				if (theData != null)
				{
					personSkillSkillStaff.Add(activity, theData);
				}
			}
			return personSkillSkillStaff;
		}

		public static Dictionary<DateTime, ISkillStaffPeriodDataHolder> BoostFirstAndLastInterval(Dictionary<DateTime, ISkillStaffPeriodDataHolder> dataHolders)
		{
			DateTime? lastKey = null;
			//Boost first and last interval
			// sort first so we get them in correct order
			var sortedDataHolders = new SortedDictionary<DateTime, ISkillStaffPeriodDataHolder>(dataHolders);

			foreach (KeyValuePair<DateTime, ISkillStaffPeriodDataHolder> holder in sortedDataHolders)
			{
				//first
				if (lastKey == null)
					holder.Value.Boost = true;

				lastKey = holder.Key;

				if (!sortedDataHolders.TryGetValue(lastKey.Value.Add(holder.Value.Period.ElapsedTime()), out _))
				{
					//last
					holder.Value.Boost = true;
					lastKey = null;
				}
			}
			return new Dictionary<DateTime, ISkillStaffPeriodDataHolder>(sortedDataHolders);
		}

		private static Dictionary<DateTime, ISkillStaffPeriodDataHolder> CombineSkillStaffPeriodDataHolder(Dictionary<DateTime, ISkillStaffPeriodDataHolder> dictionaryTo, Dictionary<DateTime, ISkillStaffPeriodDataHolder> dictionaryFrom)
		{
			dictionaryTo = ControlIfPeriodIsMissing(dictionaryTo, dictionaryFrom);
			foreach (KeyValuePair<DateTime, ISkillStaffPeriodDataHolder> valuePair in dictionaryTo)
			{
				ISkillStaffPeriodDataHolder skillStaffPeriodDataHolder;
				if (dictionaryFrom.TryGetValue(valuePair.Key, out skillStaffPeriodDataHolder))
				{
					valuePair.Value.OriginalDemandInMinutes +=
					 skillStaffPeriodDataHolder.OriginalDemandInMinutes;
					valuePair.Value.AssignedResourceInMinutes +=
						skillStaffPeriodDataHolder.AssignedResourceInMinutes;
					valuePair.Value.MaximumPersons += skillStaffPeriodDataHolder.MaximumPersons;
					valuePair.Value.MinimumPersons += skillStaffPeriodDataHolder.MinimumPersons;
					valuePair.Value.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads += skillStaffPeriodDataHolder.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads;
					valuePair.Value.TweakedCurrentDemand += skillStaffPeriodDataHolder.TweakedCurrentDemand;
					if (skillStaffPeriodDataHolder.Boost)
						valuePair.Value.Boost = true;
				}
			}
			return dictionaryTo;
		}

		private static Dictionary<DateTime, ISkillStaffPeriodDataHolder> ControlIfPeriodIsMissing(Dictionary<DateTime, ISkillStaffPeriodDataHolder> dictionaryToControl
			, Dictionary<DateTime, ISkillStaffPeriodDataHolder> theOtherDictionary)
		{
			foreach (KeyValuePair<DateTime, ISkillStaffPeriodDataHolder> pair in theOtherDictionary)
			{
				if (!dictionaryToControl.ContainsKey(pair.Key))
				{
					ISkillStaffPeriodDataHolder dataHolder = new SkillStaffPeriodDataInfo(0, 0, pair.Value.Period, 0, 0, 0, null);
					dictionaryToControl.Add(pair.Key, dataHolder);
				}
			}

			return dictionaryToControl;
		}

		private static ISkill FindNextSkillWithSameActivity(IActivity activity, HashSet<ISkill> oldSkills, IEnumerable<KeyValuePair<ISkill, Dictionary<DateTime, ISkillStaffPeriodDataHolder>>> skills)
		{
			foreach (var skill in skills)
			{
				if (activity.Equals(skill.Key.Activity) && !oldSkills.Contains(skill.Key))
					return skill.Key;
			}

			return null;
		}

		public ISkillSkillStaffPeriodExtendedDictionary SkillSkillStaffPeriodDictionary => _internalDictionary;

		private void CreateInternalDictionary(IEnumerable<KeyValuePair<ISkill, IEnumerable<ISkillDay>>> skillDays)
		{
			_internalDictionary = new SkillSkillStaffPeriodExtendedDictionary();

			foreach (var skillDay in skillDays)
			{
				var skillStaffPeriods = new SkillStaffPeriodDictionary(skillDay.Key, skillDay.Value.SelectMany(day => day.SkillStaffPeriodCollection).ToDictionary(s => s.Period));
				if (skillStaffPeriods.Count > 0)
					_internalDictionary.Add(skillDay.Key, skillStaffPeriods);
			}
		}
		public IList<ISkillStaffPeriod> IntersectingSkillStaffPeriodList(IEnumerable<ISkill> skills, DateTimePeriod utcPeriod)
		{
			var skillStaffPeriods = new List<ISkillStaffPeriod>();
			skills.ForEach(skill =>
			{
				if (_internalDictionary.TryGetValue(skill, out var content))
				{
					foreach (var dictionary in content)
					{
						if (utcPeriod.Intersect(dictionary.Key))
							skillStaffPeriods.Add(dictionary.Value);
					}
				}
			});
			return skillStaffPeriods;
		}
		public IList<ISkillStaffPeriod> SkillStaffPeriodList(IEnumerable<ISkill> skills, DateTimePeriod utcPeriod)
		{
			var skillStaffPeriods = new List<ISkillStaffPeriod>();
			skills.ForEach(skill =>
			{
				if (_internalDictionary.TryGetValue(skill, out var content))
				{
					foreach (var dictionary in content)
					{
						if (dictionary.Key.EndDateTime <= utcPeriod.StartDateTime) continue;
						if (dictionary.Key.StartDateTime >= utcPeriod.EndDateTime) break; //perf, will only work when ordered by datetime (which always seems to be the case)

						skillStaffPeriods.Add(dictionary.Value);
					}
				}
			});
			return skillStaffPeriods;
		}

		public IDictionary<ISkill, ISkillStaffPeriodDictionary> SkillStaffPeriodDictionary(IEnumerable<ISkill> skills, DateTimePeriod utcPeriod)
		{
			var skillStaffPeriods = new Dictionary<ISkill, ISkillStaffPeriodDictionary>();
			foreach (ISkill skill in skills)
			{
				if (_internalDictionary.TryGetValue(skill, out var content))
				{
					var newDictionary = new SkillStaffPeriodDictionary(skill,
						content.Where(c => c.Key.EndDateTime > utcPeriod.StartDateTime && c.Key.StartDateTime < utcPeriod.EndDateTime)
							.ToDictionary(k => k.Key, v => v.Value));
					if (newDictionary.Count > 0)
						skillStaffPeriods.Add(skill, newDictionary);
				}
			}
			return skillStaffPeriods;
		}

		public IList<ISkillStaffPeriod> SkillStaffPeriodList(IAggregateSkill skill, DateTimePeriod utcPeriod)
		{
			var minimumResolution = (double)getMinimumResolution(skill);

			lock (Locker)
			{
				IDictionary<DateTimePeriod, IList<ISkillStaffPeriod>> skillStaffPeriods = new Dictionary<DateTimePeriod, IList<ISkillStaffPeriod>>();
				foreach (ISkill aggregateSkill in skill.AggregateSkills)
				{
					if (!_internalDictionary.TryGetValue(aggregateSkill, out var content)) continue;

					if (aggregateSkill.DefaultResolution > minimumResolution)
					{
						var relevantSkillStaffPeriodList = content.Where(c => utcPeriod.Contains(c.Key)).Select(c => c.Value);
						var skillStaffPeriodsSplitList = new List<ISkillStaffPeriod>();
						double factor = minimumResolution / aggregateSkill.DefaultResolution;
						foreach (ISkillStaffPeriod skillStaffPeriod in relevantSkillStaffPeriodList)
						{
							skillStaffPeriodsSplitList.AddRange(SplitSkillStaffPeriod(skillStaffPeriod, factor, TimeSpan.FromMinutes(minimumResolution)));
						}

						foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriodsSplitList)
						{
							if (!skillStaffPeriods.TryGetValue(skillStaffPeriod.Period, out var foundList))
							{
								foundList = new List<ISkillStaffPeriod>();
								skillStaffPeriods.Add(skillStaffPeriod.Period, foundList);
							}

							foundList.Add(skillStaffPeriod);
						}
					}
					else
					{
						foreach (KeyValuePair<DateTimePeriod, ISkillStaffPeriod> pair in content.Where(c => utcPeriod.Contains(c.Key)))
						{
							if (!skillStaffPeriods.TryGetValue(pair.Key, out var foundList))
							{
								foundList = new List<ISkillStaffPeriod>();
								skillStaffPeriods.Add(pair.Key, foundList);
							}

							foundList.Add(pair.Value);
						}
					}
				}

				IList<ISkillStaffPeriod> skillStaffPeriodsToReturn = new List<ISkillStaffPeriod>();

				foreach (KeyValuePair<DateTimePeriod, IList<ISkillStaffPeriod>> keyValuePair in skillStaffPeriods)
				{
					ISkillStaffPeriod tempPeriod = SkillStaffPeriod.Combine(keyValuePair.Value);
					var aggregate = (IAggregateSkillStaffPeriod)tempPeriod;
					aggregate.IsAggregate = true;
					recalculateMinMaxStaffAlarm((SkillStaffPeriod)aggregate);
					foreach (ISkillStaffPeriod staffPeriod in keyValuePair.Value)
					{
						recalculateMinMaxStaffAlarm((SkillStaffPeriod)staffPeriod);
						var asAgg = (IAggregateSkillStaffPeriod)staffPeriod;
						if (!asAgg.IsAggregate)
						{
							HandleAggregate(keyValuePair, aggregate, staffPeriod, asAgg);
						}
						else
						{
							if (keyValuePair.Value.Count > 1)
								aggregate.CombineAggregatedSkillStaffPeriod(asAgg);
						}

					}
					skillStaffPeriodsToReturn.Add(tempPeriod);
				}

				return SortedPeriods(skillStaffPeriodsToReturn);
			}
		}

		private static void recalculateMinMaxStaffAlarm(SkillStaffPeriod staffPeriod)
		{
			staffPeriod.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.Ok;
			if (staffPeriod.Payload.SkillPersonData.MinimumPersons > staffPeriod.CalculatedLoggedOn)
				staffPeriod.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MinStaffBroken;
			if (staffPeriod.Payload.SkillPersonData.MaximumPersons > 0 && staffPeriod.Payload.SkillPersonData.MaximumPersons < staffPeriod.CalculatedLoggedOn)
				staffPeriod.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MaxStaffBroken;
		}

		private static int getMinimumResolution(IAggregateSkill skill)
		{
			int minimumResolution = 15;
			if (skill.AggregateSkills.Any())
			{
				minimumResolution = skill.AggregateSkills.Min(s => s.DefaultResolution);
			}
			return minimumResolution;
		}

		private static IList<ISkillStaffPeriod> SortedPeriods(IEnumerable<ISkillStaffPeriod> periodsToSort)
		{
			return periodsToSort.OrderBy(p => p.Period.StartDateTime).ToList();
		}

		public static void HandleAggregate(KeyValuePair<DateTimePeriod, IList<ISkillStaffPeriod>> keyValuePair, IAggregateSkillStaffPeriod aggregate, ISkillStaffPeriod staffPeriod, IAggregateSkillStaffPeriod asAgg)
		{
			asAgg.AggregatedFStaff = staffPeriod.FStaff;
			asAgg.AggregatedCalculatedResource = staffPeriod.CalculatedResource;
			asAgg.AggregatedForecastedIncomingDemand = staffPeriod.Payload.ForecastedIncomingDemand;
			asAgg.AggregatedEstimatedServiceLevel = staffPeriod.EstimatedServiceLevel;
			asAgg.AggregatedEstimatedServiceLevelShrinkage = staffPeriod.EstimatedServiceLevelShrinkage;
			if (staffPeriod.Payload.SkillPersonData.MinimumPersons > staffPeriod.CalculatedLoggedOn)
				asAgg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MinStaffBroken;
			if (staffPeriod.Payload.SkillPersonData.MaximumPersons > 0 && staffPeriod.Payload.SkillPersonData.MaximumPersons < staffPeriod.CalculatedLoggedOn)
				asAgg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MaxStaffBroken;
			ISkill skill = staffPeriod.SkillDay.Skill;

			asAgg.AggregatedStaffingThreshold = StaffingThreshold.Ok;
			var overstaffing = new IntervalHasOverstaffing(skill);
			if (overstaffing.IsSatisfiedBy(staffPeriod))
				asAgg.AggregatedStaffingThreshold = StaffingThreshold.Overstaffing;
			var understaffing = new IntervalHasUnderstaffing(skill);
			if (understaffing.IsSatisfiedBy(staffPeriod))
				asAgg.AggregatedStaffingThreshold = StaffingThreshold.Understaffing;
			var criticalUnderstaffing = new IntervalHasSeriousUnderstaffing(skill);
			if (criticalUnderstaffing.IsSatisfiedBy(staffPeriod))
				asAgg.AggregatedStaffingThreshold = StaffingThreshold.CriticalUnderstaffing;
			asAgg.IsAggregate = true;
			if (keyValuePair.Value.Count > 1)
				aggregate.CombineAggregatedSkillStaffPeriod(asAgg);
			asAgg.IsAggregate = false;
		}

		public ISkillStaffPeriodDictionary SkillStaffPeriodList(IAggregateSkill skill, DateTimePeriod utcPeriod, bool forDay)
		{
			IList<ISkillStaffPeriod> resultList = SkillStaffPeriodList(skill, utcPeriod);
			ISkillStaffPeriodDictionary returnDictionary = new SkillStaffPeriodDictionary(skill, resultList.ToDictionary(k => k.Period));

			return returnDictionary;
		}

		public static IEnumerable<ISkillStaffPeriod> SplitSkillStaffPeriod(ISkillStaffPeriod skillStaffPeriod, double factor, TimeSpan minimumResolution)
		{
			IList<ISkillStaffPeriod> skillStaffPeriods = new List<ISkillStaffPeriod>();
			for (DateTime currentTime = skillStaffPeriod.Period.StartDateTime; currentTime < skillStaffPeriod.Period.EndDateTime; currentTime = currentTime.Add(minimumResolution))
			{
				ISkillStaffPeriod newShortPeriod =
					new SkillStaffPeriod(new DateTimePeriod(currentTime, currentTime.Add(minimumResolution)),
										 new Task(skillStaffPeriod.Payload.TaskData.Tasks * factor,
													skillStaffPeriod.Payload.TaskData.AverageTaskTime,
													skillStaffPeriod.Payload.TaskData.AverageAfterTaskTime),
										 skillStaffPeriod.Payload.ServiceAgreementData);
				newShortPeriod.SetCalculatedResource65(skillStaffPeriod.Payload.CalculatedResource);
				newShortPeriod.Payload.Shrinkage = skillStaffPeriod.Payload.Shrinkage;
				newShortPeriod.Payload.SkillPersonData = skillStaffPeriod.Payload.SkillPersonData;
				((SkillStaff)newShortPeriod.Payload).ForecastedIncomingDemand = skillStaffPeriod.Payload.ManualAgents ?? skillStaffPeriod.Payload.ForecastedIncomingDemand;
				newShortPeriod.Payload.ManualAgents = skillStaffPeriod.Payload.ManualAgents;

				var aggregate = (IAggregateSkillStaffPeriod)newShortPeriod;
				aggregate.IsAggregate = true;
				aggregate.AggregatedFStaff = skillStaffPeriod.FStaff;
				aggregate.AggregatedCalculatedResource = skillStaffPeriod.CalculatedResource;

				skillStaffPeriods.Add(newShortPeriod);
			}
			return skillStaffPeriods;
		}

		public bool GuessResourceCalculationHasBeenMade()
		{
			return _internalDictionary.GuessResourceCalculationHasBeenMade();
		}
	}
}


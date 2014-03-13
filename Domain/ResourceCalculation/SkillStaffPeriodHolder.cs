using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class SkillStaffPeriodHolder : ISkillStaffPeriodHolder
    {
        private ISkillSkillStaffPeriodExtendedDictionary _internalDictionary;
        private static readonly object Locker = new object();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public SkillStaffPeriodHolder(IEnumerable<KeyValuePair<ISkill, IList<ISkillDay>>> skillDays)
        {
            CreateInternalDictionary(skillDays);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> SkillStaffDataPerActivity(DateTimePeriod onPeriod, IList<ISkill> onSkills)
        {
            var personSkillSkillStaff = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();

            IList<IActivity> activities = new List<IActivity>();
            foreach (ISkill skill in onSkills)
            {
                if (!activities.Contains(skill.Activity))
                {
                    activities.Add(skill.Activity);
                }
            }

            var tmp = new Dictionary<ISkill, Dictionary<DateTime, ISkillStaffPeriodDataHolder>>();

            foreach (ISkill skill in onSkills)
            {
                ISkillStaffPeriodDictionary skillStaffPeriods;
                if (_internalDictionary.TryGetValue(skill, out skillStaffPeriods))
                {
                    var dataHolders = new Dictionary<DateTime, ISkillStaffPeriodDataHolder>(new DateTimeAsLongEqualityComparer());

                    foreach (KeyValuePair<DateTimePeriod, ISkillStaffPeriod> pair in skillStaffPeriods)
                    {
                        if (pair.Key.Intersect(onPeriod))
                        {
                            ISkillStaffPeriod skillStaffPeriod = pair.Value;
                            int maximumPersons = skillStaffPeriod.Payload.SkillPersonData.MaximumPersons;
                            int minimumPersons = skillStaffPeriod.Payload.SkillPersonData.MinimumPersons;

                            //if (skill.Name == "B2C Orders Spanish")
                            //{
                                //if (skillStaffPeriod.Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(11))
                                //{
                                //    string foo = string.Empty;
                                //}
                            //}
							//skill.SkillType.ForecastSource
                            double absoluteDifferenceScheduledHeadsAndMinMaxHeads = skillStaffPeriod.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true);
                            //int origDemand = (int) Math.Round(skillStaffPeriod.ForecastedIncomingDemand().TotalMinutes);
                            var origDemand = skillStaffPeriod.FStaffTime().TotalMinutes;
                            var assignedResource =skillStaffPeriod.Payload.CalculatedResource*
                                           skillStaffPeriod.Period.ElapsedTime().TotalMinutes;
                            ISkillStaffPeriodDataHolder dataHolder = new SkillStaffPeriodDataHolder(origDemand,
                                                                                                    assignedResource,
                                                                                                    skillStaffPeriod.
                                                                                                        Period,
                                                                                                    minimumPersons,
                                                                                                    maximumPersons,
                                                                                                    absoluteDifferenceScheduledHeadsAndMinMaxHeads,
                                                                                                    skillStaffPeriod.PeriodDistribution,skill.OverstaffingFactor,skill.PriorityValue);

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
                IList<ISkill> oldSkills = new List<ISkill>();
                Dictionary<DateTime, ISkillStaffPeriodDataHolder> theData = null;
                foreach (KeyValuePair<ISkill, Dictionary<DateTime, ISkillStaffPeriodDataHolder>> pair in tmp)
                {
                    ISkill skill = pair.Key;
                    if (skill.Activity == activity)
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

        public static Dictionary<DateTime, ISkillStaffPeriodDataHolder> BoostFirstAndLastInterval(Dictionary<DateTime, ISkillStaffPeriodDataHolder> dataHolders )
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

                ISkillStaffPeriodDataHolder nextHolder;
                if (!sortedDataHolders.TryGetValue(lastKey.Value.Add(holder.Value.Period.ElapsedTime()), out nextHolder))
                {
                    //last
                    holder.Value.Boost = true;
                    lastKey = null;
                }
            }
            return new Dictionary<DateTime, ISkillStaffPeriodDataHolder>(sortedDataHolders, new DateTimeAsLongEqualityComparer());
        }

        private static Dictionary<DateTime, ISkillStaffPeriodDataHolder> CombineSkillStaffPeriodDataHolder(Dictionary<DateTime, ISkillStaffPeriodDataHolder> dictionaryTo, Dictionary<DateTime, ISkillStaffPeriodDataHolder> dictionaryFrom)
        {
            dictionaryTo = ControlIfPeriodIsMissing(dictionaryTo, dictionaryFrom);
            foreach (KeyValuePair<DateTime, ISkillStaffPeriodDataHolder> valuePair in dictionaryTo)
            {
                ISkillStaffPeriodDataHolder skillStaffPeriodDataHolder;
                if (dictionaryFrom.TryGetValue(valuePair.Key,out skillStaffPeriodDataHolder))
                {
                    valuePair.Value.OriginalDemandInMinutes +=
                     skillStaffPeriodDataHolder.OriginalDemandInMinutes;
                    valuePair.Value.AssignedResourceInMinutes +=
                        skillStaffPeriodDataHolder.AssignedResourceInMinutes;
                    valuePair.Value.MaximumPersons += skillStaffPeriodDataHolder.MaximumPersons;
                    valuePair.Value.MinimumPersons += skillStaffPeriodDataHolder.MinimumPersons;
                    valuePair.Value.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads += skillStaffPeriodDataHolder.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads;
                    valuePair.Value.TweakedCurrentDemand += skillStaffPeriodDataHolder.TweakedCurrentDemand;
                    if(skillStaffPeriodDataHolder.Boost)
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
                    ISkillStaffPeriodDataHolder dataHolder = new SkillStaffPeriodDataHolder(0, 0, pair.Value.Period, 0, 0,0, null);
                    dictionaryToControl.Add(pair.Key, dataHolder);
                }
            }

            return dictionaryToControl;
        }

        private static ISkill FindNextSkillWithSameActivity(IActivity activity, ICollection<ISkill> oldSkills, IEnumerable<KeyValuePair<ISkill, Dictionary<DateTime, ISkillStaffPeriodDataHolder>>> skills)
        {
            foreach (var skill in skills)
            {
                if (skill.Key.Activity == activity && !oldSkills.Contains(skill.Key))
                    return skill.Key;
            }

            return null;
        }

        public ISkillSkillStaffPeriodExtendedDictionary SkillSkillStaffPeriodDictionary
        {
            get { return _internalDictionary; }
        }

        private void CreateInternalDictionary(IEnumerable<KeyValuePair<ISkill, IList<ISkillDay>>> skillDays)
        {
            _internalDictionary = new SkillSkillStaffPeriodExtendedDictionary();

            foreach (KeyValuePair<ISkill, IList<ISkillDay>> skillDay in skillDays)
            {
                ISkillStaffPeriodDictionary skillStaffPeriods = new SkillStaffPeriodDictionary(skillDay.Key);
                foreach (ISkillDay day in skillDay.Value)
                {
                    foreach (ISkillStaffPeriod skillStaffPeriod in day.SkillStaffPeriodCollection)
                    {
                        skillStaffPeriods.Add(skillStaffPeriod.Period, skillStaffPeriod);
                    }
                }
                if (skillStaffPeriods.Count>0)
                    _internalDictionary.Add(skillDay.Key, skillStaffPeriods);
            }
        }
		public IList<ISkillStaffPeriod> IntersectingSkillStaffPeriodList(IEnumerable<ISkill> skills, DateTimePeriod utcPeriod)
		{
			var skillStaffPeriods = new List<ISkillStaffPeriod>();
			foreach (ISkill skill in skills)
			{
				ISkillStaffPeriodDictionary content;
				if (_internalDictionary.TryGetValue(skill, out content))
				{
					foreach (var dictionary in content)
					{
						if (utcPeriod.Intersect(dictionary.Key))
							skillStaffPeriods.Add(dictionary.Value);
					}
				}
			}
			return skillStaffPeriods;
		}
        public IList<ISkillStaffPeriod> SkillStaffPeriodList(IEnumerable<ISkill> skills, DateTimePeriod utcPeriod)
        {
            var skillStaffPeriods = new List<ISkillStaffPeriod>();
            foreach (ISkill skill in skills)
            {
                ISkillStaffPeriodDictionary content;
                if (_internalDictionary.TryGetValue(skill,out content))
                {
                    foreach (var dictionary in content)
                    {
	                    if (dictionary.Key.EndDateTime <= utcPeriod.StartDateTime) continue;
	                    if (dictionary.Key.StartDateTime >= utcPeriod.EndDateTime) continue;

                        skillStaffPeriods.Add(dictionary.Value);
                    }
                }
            }
            return skillStaffPeriods;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IDictionary<ISkill, ISkillStaffPeriodDictionary> SkillStaffPeriodDictionary(IEnumerable<ISkill> skills, DateTimePeriod utcPeriod)
		{
			var skillStaffPeriods = new Dictionary<ISkill, ISkillStaffPeriodDictionary>();
			foreach (ISkill skill in skills)
			{
				ISkillStaffPeriodDictionary content;
				if (_internalDictionary.TryGetValue(skill, out content))
				{
					var newDictionary = new SkillStaffPeriodDictionary(skill);
					foreach (var dictionary in content)
					{
						if (dictionary.Key.EndDateTime <= utcPeriod.StartDateTime) continue;
						if (dictionary.Key.StartDateTime >= utcPeriod.EndDateTime) continue;
						
						newDictionary.Add(dictionary.Key, dictionary.Value);
					}
					if(newDictionary.Count > 0 )
						skillStaffPeriods.Add(skill, newDictionary);
				}
			}
			return skillStaffPeriods;
		}

        public IList<ISkillStaffPeriod> SkillStaffPeriodList(IAggregateSkill skill, DateTimePeriod utcPeriod)
        {
        	int minimumResolution = getMinimumResolution(skill);
            
            lock(Locker)
            {
        	IDictionary<DateTimePeriod, IList<ISkillStaffPeriod>> skillStaffPeriods = new Dictionary<DateTimePeriod,IList<ISkillStaffPeriod>>();
            foreach (ISkill aggregateSkill in skill.AggregateSkills)
            {
                ISkillStaffPeriodDictionary content;
                if (!_internalDictionary.TryGetValue(aggregateSkill, out content)) continue;

                if (aggregateSkill.DefaultResolution>minimumResolution)
                {
                    var relevantSkillStaffPeriodList = content.Where(c => utcPeriod.Contains(c.Key)).Select(c => c.Value);
                    var skillStaffPeriodsSplitList = new List<ISkillStaffPeriod>();
                    double factor = (double)minimumResolution / aggregateSkill.DefaultResolution;
                    foreach (ISkillStaffPeriod skillStaffPeriod in relevantSkillStaffPeriodList)
                    {
                        skillStaffPeriodsSplitList.AddRange(SplitSkillStaffPeriod(skillStaffPeriod,factor,TimeSpan.FromMinutes(minimumResolution)));
                    }

                    foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriodsSplitList)
                    {
                        IList<ISkillStaffPeriod> foundList;
                        if (!skillStaffPeriods.TryGetValue(skillStaffPeriod.Period, out foundList))
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
                        IList<ISkillStaffPeriod> foundList;
                        if (!skillStaffPeriods.TryGetValue(pair.Key, out foundList))
                        {
                            foundList = new List<ISkillStaffPeriod>();
                            skillStaffPeriods.Add(pair.Key,foundList);
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

                foreach (ISkillStaffPeriod staffPeriod in keyValuePair.Value)
                {
                    var asAgg = (IAggregateSkillStaffPeriod) staffPeriod;
                    if(!asAgg.IsAggregate)
                    {
                        HandleAggregate(keyValuePair, aggregate, staffPeriod, asAgg);
                    }
                    else
                    {
						if(keyValuePair.Value.Count > 1)
							aggregate.CombineAggregatedSkillStaffPeriod(asAgg);
                    }
                    
                }
                skillStaffPeriodsToReturn.Add(tempPeriod);
            }

            return SortedPeriods(skillStaffPeriodsToReturn);
            }
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
            if (keyValuePair.Value.Count >1)
                aggregate.CombineAggregatedSkillStaffPeriod(asAgg);
            asAgg.IsAggregate = false;
        }

        public ISkillStaffPeriodDictionary SkillStaffPeriodList(IAggregateSkill skill, DateTimePeriod utcPeriod, bool forDay)
        {
            IList<ISkillStaffPeriod> resultList = SkillStaffPeriodList(skill, utcPeriod);
            ISkillStaffPeriodDictionary returnDictionary = new SkillStaffPeriodDictionary(skill);

            foreach (ISkillStaffPeriod skillStaffPeriod in resultList)
            {
                returnDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
            }
            return returnDictionary;
        }

		public static IEnumerable<ISkillStaffPeriod> SplitSkillStaffPeriod(ISkillStaffPeriod skillStaffPeriod, double factor, TimeSpan minimumResolution)
        {
            IList<ISkillStaffPeriod> skillStaffPeriods = new List<ISkillStaffPeriod>();
            for (DateTime currentTime = skillStaffPeriod.Period.StartDateTime; currentTime < skillStaffPeriod.Period.EndDateTime; currentTime = currentTime.Add(minimumResolution))
            {
                ISkillStaffPeriod newShortPeriod =
                    new SkillStaffPeriod(new DateTimePeriod(currentTime, currentTime.Add(minimumResolution)),
                                         new Task(skillStaffPeriod.Payload.TaskData.Tasks*factor,
                                                  skillStaffPeriod.Payload.TaskData.AverageTaskTime,
                                                  skillStaffPeriod.Payload.TaskData.AverageAfterTaskTime),
                                         skillStaffPeriod.Payload.ServiceAgreementData,
                                         skillStaffPeriod.StaffingCalculatorService);
                newShortPeriod.SetCalculatedResource65(skillStaffPeriod.Payload.CalculatedResource);
                newShortPeriod.Payload.Shrinkage = skillStaffPeriod.Payload.Shrinkage;
                newShortPeriod.Payload.SkillPersonData = (SkillPersonData) skillStaffPeriod.Payload.SkillPersonData.Clone();
                ((SkillStaff)newShortPeriod.Payload).ForecastedIncomingDemand = skillStaffPeriod.Payload.ForecastedIncomingDemand*
                                                                  factor;

                var aggregate = (IAggregateSkillStaffPeriod) newShortPeriod;
                aggregate.IsAggregate = true;
                aggregate.AggregatedFStaff = skillStaffPeriod.FStaff;
                aggregate.AggregatedCalculatedResource = skillStaffPeriod.CalculatedResource;

                skillStaffPeriods.Add(newShortPeriod);
            }
            return skillStaffPeriods;
        }
    }

    
}

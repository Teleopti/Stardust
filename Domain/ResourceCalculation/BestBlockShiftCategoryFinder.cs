using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class BestShiftCategoryResult :IBestShiftCategoryResult
	{
		private readonly FailureCause _failureCause;
		private readonly IShiftCategory _shiftCategory;

		public BestShiftCategoryResult(IShiftCategory shiftCategory, FailureCause failureCause)
		{
			_failureCause = failureCause;
			_shiftCategory = shiftCategory;
		}
		public IShiftCategory BestShiftCategory
		{
			get { return _shiftCategory; }
		}

		public FailureCause FailureCause
		{
			get { return _failureCause; }
		}
	}

	public class BestBlockShiftCategoryFinder :  IBestBlockShiftCategoryFinder
    {
	    private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;
	    private readonly IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
	    private readonly IPersonSkillPeriodsDataHolderManager _personSkillPeriodsDataHolderManager;
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IBlockSchedulingWorkShiftFinderService _workShiftFinderService;
	    private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IGroupShiftCategoryFairnessCreator _groupShiftCategoryFairnessCreator;
		private readonly IPossibleCombinationsOfStartEndCategoryRunner _possibleCombinationsOfStartEndCategoryRunner;
		private readonly IPossibleCombinationsOfStartEndCategoryCreator _possibleCombinationsOfStartEndCategoryCreator;

		public BestBlockShiftCategoryFinder(
            IShiftProjectionCacheManager shiftProjectionCacheManager,
            IShiftProjectionCacheFilter shiftProjectionCacheFilter,
            IPersonSkillPeriodsDataHolderManager personSkillPeriodsDataHolderManager,
            ISchedulingResultStateHolder schedulingResultStateHolder,
            IBlockSchedulingWorkShiftFinderService workShiftFinderService,
            IEffectiveRestrictionCreator effectiveRestrictionCreator, 
            IGroupShiftCategoryFairnessCreator groupShiftCategoryFairnessCreator,
			IPossibleCombinationsOfStartEndCategoryRunner possibleCombinationsOfStartEndCategoryRunner,
			IPossibleCombinationsOfStartEndCategoryCreator possibleCombinationsOfStartEndCategoryCreator)
        {
		    _shiftProjectionCacheManager = shiftProjectionCacheManager;
		    _shiftProjectionCacheFilter = shiftProjectionCacheFilter;
		    _personSkillPeriodsDataHolderManager = personSkillPeriodsDataHolderManager;
		    _schedulingResultStateHolder = schedulingResultStateHolder;
		    _workShiftFinderService = workShiftFinderService;
		    _effectiveRestrictionCreator = effectiveRestrictionCreator;
			_groupShiftCategoryFairnessCreator = groupShiftCategoryFairnessCreator;
			_possibleCombinationsOfStartEndCategoryRunner = possibleCombinationsOfStartEndCategoryRunner;
			_possibleCombinationsOfStartEndCategoryCreator = possibleCombinationsOfStartEndCategoryCreator;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.DateOnly.ToShortDateString"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public IBestShiftCategoryResult BestShiftCategoryForDays(IBlockFinderResult result, IPerson person, IFairnessValueResult totalFairness, 
			IFairnessValueResult agentFairness, ISchedulingOptions schedulingOptions)
        {
			InParameter.NotNull("result",result);
			InParameter.NotNull("person", person);

            IList<DateOnly> dates = result.BlockDays;
			var dictionary = ScheduleDictionary;
            
            IShiftCategory highestvalueCategory = null;
            double highestValue = double.MinValue;
            ICccTimeZoneInfo agentTimeZone = person.PermissionInformation.DefaultTimeZone();
            var schedulePeriodFound = false;

            DateTimePeriod validPeriod = extendedVisiblePeriod();
            var shiftCategoryList = _schedulingResultStateHolder.ShiftCategories;
            foreach (IShiftCategory shiftCategory in shiftCategoryList)
            {
                if(schedulingOptions.NotAllowedShiftCategories.Contains(shiftCategory))
                    continue;

				// one more than double.MinValue because double.MinValue +1 still is double.MinValue, strange but true 
				double? categoryValue = null;
                
                foreach (var dateOnly in dates)
                {
                	IVirtualSchedulePeriod currentSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);

                    if (!validPeriod.Contains(dateOnly.Date))
                        continue;

                    if (!currentSchedulePeriod.IsValid)
                        continue;

                    schedulePeriodFound = true;
                        
					IList<IPerson> persons;
					var groupPerson = person as IGroupPerson;
					if (groupPerson != null)
					{
						persons = new List<IPerson>();
						foreach (var member in groupPerson.GroupMembers)
						{
							IScheduleDay scheduleDay = dictionary[member].ScheduledDay(dateOnly);
							if (!scheduleDay.IsScheduled())
								persons.Add(member);
						}

						if(persons.Count == 0)
							return new BestShiftCategoryResult(null, FailureCause.AlreadyAssigned);
					}
					else
					{
						persons = new List<IPerson> { person };
					}

					var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(persons, dateOnly, schedulingOptions, dictionary);
					if (effectiveRestriction == null)
						return new BestShiftCategoryResult(null, FailureCause.ConflictingRestrictions);

                    var personPeriod = currentSchedulePeriod.Person.Period(dateOnly);
                    IRuleSetBag bag = personPeriod.RuleSetBag;
                    var shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, agentTimeZone, bag, false);
                	
					

                    if (shiftList == null)
                        continue;

					var minmax = bag.MinMaxWorkTime(new RuleSetProjectionService(new ShiftCreatorService()), dateOnly,
					                               effectiveRestriction);

					var combinations = _possibleCombinationsOfStartEndCategoryCreator.FindCombinations(minmax, schedulingOptions);

					_possibleCombinationsOfStartEndCategoryRunner.RunTheList(combinations.ToList(), shiftList, dateOnly, groupPerson, schedulingOptions);

                	IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolders =
                		_personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(dateOnly,
                		                                                                               currentSchedulePeriod);
                    //if closed dataholders.count == 0 and we should skip this day
                    if(dataHolders.Count == 0)
                        continue;

                    //TODO Handle all results 
                    IWorkShiftFinderResult finderResult;
                	double tempValue = getBestValueForShifts(person, shiftCategory, dateOnly, currentSchedulePeriod,
                	                                         dictionary, persons, effectiveRestriction, agentTimeZone,
                	                                         shiftList, out finderResult, totalFairness, agentFairness,
                	                                         dataHolders, schedulingOptions);

                    if (tempValue == double.MinValue && effectiveRestriction.IsRestriction)
                    {
                        finderResult.Successful = true;
                        shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, agentTimeZone, bag, true);
                    	tempValue = getBestValueForShifts(person, shiftCategory, dateOnly, currentSchedulePeriod,
                    	                                  dictionary, persons, effectiveRestriction, agentTimeZone,
                    	                                  shiftList, out finderResult, totalFairness, agentFairness,
                    	                                  dataHolders, schedulingOptions);
                    }
                    if(!finderResult.Successful)
                    {
                        string key = Resources.ScheduleBlockDayWith + " " + shiftCategory.Description.Name + ", " +
                                     dateOnly.ToShortDateString();
                        if(!result.WorkShiftFinderResult.ContainsKey(key))
                            result.WorkShiftFinderResult.Add(key, finderResult);
                    }

                	if (tempValue == double.MinValue)
                	{
                	    goto nextCategory;
                	}

					if (categoryValue.HasValue && categoryValue.Value + tempValue < double.MinValue)
                	{
                		categoryValue = tempValue; 
                	}

					if (!categoryValue.HasValue)
					{
						categoryValue = tempValue;
					}
					else
					{
						categoryValue += tempValue;
					}
                }

				if (categoryValue.HasValue && categoryValue.Value > highestValue)
            	{
					highestValue = categoryValue.Value;
					highestvalueCategory = shiftCategory;
            	}

            nextCategory:;
            }
            if (!schedulePeriodFound)
                return new BestShiftCategoryResult(null, FailureCause.NoValidPeriod);

            return new BestShiftCategoryResult(highestvalueCategory,FailureCause.NoFailure); 
        }

	    private DateTimePeriod extendedVisiblePeriod()
	    {
	        DateTimePeriod basePeriod = _schedulingResultStateHolder.Schedules.Period.VisiblePeriod; 
            var ret = new DateTimePeriod(basePeriod.StartDateTime.AddDays(-10), basePeriod.EndDateTime.AddDays(10));
	        return ret;
	    }

	    private double getBestValueForShifts(IPerson person, IShiftCategory shiftCategory, DateOnly dateOnly, IVirtualSchedulePeriod currentSchedulePeriod, 
            IScheduleDictionary dictionary, IList<IPerson> persons, IEffectiveRestriction effectiveRestriction, ICccTimeZoneInfo agentTimeZone, 
            IList<IShiftProjectionCache> shiftList, out IWorkShiftFinderResult finderResult, IFairnessValueResult totalFairness, IFairnessValueResult agentFairness,
			IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolders, ISchedulingOptions schedulingOptions)
	    {
	        finderResult = new WorkShiftFinderResult(person, dateOnly);

	        shiftList = _shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(shiftList);
	        shiftList = _shiftProjectionCacheFilter.FilterOnShiftCategory(shiftCategory, shiftList, finderResult);
	        shiftList = _shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(dateOnly, agentTimeZone, shiftList, effectiveRestriction,
	                                                                                                schedulingOptions.NotAllowedShiftCategories, finderResult);

	        shiftList = _shiftProjectionCacheFilter.FilterOnBusinessRules(persons, dictionary, dateOnly, shiftList, finderResult);
	        shiftList = _shiftProjectionCacheFilter.FilterOnPersonalShifts(persons, dictionary, dateOnly, shiftList, finderResult);

            if (shiftList.Count == 0)
                return double.MinValue;
	        


	        TimeSpan averageWorkTime = currentSchedulePeriod.AverageWorkTimePerDay;
	        bool useShiftCategoryFairness = false;
	        IShiftCategoryFairnessFactors shiftCategoryFairnessFactors = null;
	        //new ShiftCategoryFairnessFactors(new Dictionary<IShiftCategory, double>());
	        if (person.WorkflowControlSet != null)
	        {
	            useShiftCategoryFairness = person.WorkflowControlSet.UseShiftCategoryFairness;

	            IScheduleRange range = ScheduleDictionary[person];
	            IShiftCategoryFairnessCalculator calculator = new ShiftCategoryFairnessCalculator(range, person, dateOnly,
	                                                                                              _groupShiftCategoryFairnessCreator);

	            shiftCategoryFairnessFactors = calculator.ShiftCategoryFairnessFactors();
	        }

	        return _workShiftFinderService.BestShiftValue(dateOnly,
	                                                      shiftList,
	                                                      dataHolders,
	                                                      totalFairness,
	                                                      agentFairness, 5,
	                                                      averageWorkTime,
	                                                      useShiftCategoryFairness,
	                                                      shiftCategoryFairnessFactors, 
	                                                      (double)schedulingOptions.WorkShiftLengthHintOption,
	                                                      schedulingOptions.UseMinimumPersons,
	                                                      schedulingOptions.UseMaximumPersons, 
														  schedulingOptions);
	    }

	    public IScheduleDictionary ScheduleDictionary
    	{
            get { return _schedulingResultStateHolder.Schedules; }
    	}
    }
}

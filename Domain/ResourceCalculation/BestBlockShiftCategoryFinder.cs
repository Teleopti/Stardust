using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class BestShiftCategoryResult :IBestShiftCategoryResult
	{
		private readonly IPossibleStartEndCategory _possibleStartEndCategory;
		private readonly FailureCause _failureCause;
		

		public BestShiftCategoryResult(IPossibleStartEndCategory possibleStartEndCategory, FailureCause failureCause)
		{
			_possibleStartEndCategory = possibleStartEndCategory;
			_failureCause = failureCause;
		}

		public IPossibleStartEndCategory BestPossible
		{
			get { return _possibleStartEndCategory; }
		}

		public FailureCause FailureCause
		{
			get { return _failureCause; }
		}
	}

	public class BestBlockShiftCategoryFinder :  IBestBlockShiftCategoryFinder
    {
		private readonly IRuleSetProjectionService _ruleSetProjectionService;
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IPossibleCombinationsOfStartEndCategoryRunner _possibleCombinationsOfStartEndCategoryRunner;
		private readonly IPossibleCombinationsOfStartEndCategoryCreator _possibleCombinationsOfStartEndCategoryCreator;

		public BestBlockShiftCategoryFinder( IRuleSetProjectionService ruleSetProjectionService,
            IShiftProjectionCacheManager shiftProjectionCacheManager,
            ISchedulingResultStateHolder schedulingResultStateHolder,
            IEffectiveRestrictionCreator effectiveRestrictionCreator, 
			IPossibleCombinationsOfStartEndCategoryRunner possibleCombinationsOfStartEndCategoryRunner,
			IPossibleCombinationsOfStartEndCategoryCreator possibleCombinationsOfStartEndCategoryCreator)
        {
			_ruleSetProjectionService = ruleSetProjectionService;
			_shiftProjectionCacheManager = shiftProjectionCacheManager;
		    _schedulingResultStateHolder = schedulingResultStateHolder;
		    _effectiveRestrictionCreator = effectiveRestrictionCreator;

			_possibleCombinationsOfStartEndCategoryRunner = possibleCombinationsOfStartEndCategoryRunner;
			_possibleCombinationsOfStartEndCategoryCreator = possibleCombinationsOfStartEndCategoryCreator;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IBestShiftCategoryResult BestShiftCategoryForDays(IBlockFinderResult result, IPerson person, ISchedulingOptions schedulingOptions)
        {
			InParameter.NotNull("result",result);
			InParameter.NotNull("person", person);
			InParameter.NotNull("schedulingOptions", schedulingOptions);

            var dates = result.BlockDays;
			var dictionary = ScheduleDictionary;
			var bestPossible = new HashSet<IPossibleStartEndCategory>();

            var agentTimeZone = person.PermissionInformation.DefaultTimeZone();
            var schedulePeriodFound = false;

            var validPeriod = extendedVisiblePeriod();
    
			foreach (var dateOnly in dates)
			{
				var currentSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);

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
				var bag = personPeriod.RuleSetBag;
				var shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, agentTimeZone, bag, false);
                	
				if (shiftList == null)
					continue;

				var minmax = bag.MinMaxWorkTime(_ruleSetProjectionService, dateOnly, effectiveRestriction);
                	
				//REMOVE WHEN IN GUI
				schedulingOptions.UseGroupSchedulingCommonCategory = true;
				schedulingOptions.UseGroupSchedulingCommonEnd = true;

				var combinations = _possibleCombinationsOfStartEndCategoryCreator.FindCombinations(minmax, schedulingOptions);
				// CONTINUE TO NEXT IF EMPTY??
				_possibleCombinationsOfStartEndCategoryRunner.RunTheList(combinations.ToList(), shiftList, dateOnly, groupPerson, schedulingOptions);
 
				IWorkShiftFinderResult finderResult = new WorkShiftFinderResult(person, dateOnly);

				IPossibleStartEndCategory tmpBestPossible = null;
				if (!combinations.IsEmpty())
					tmpBestPossible = combinations.OrderBy(c => c.ShiftValue).Last();
				

				if (tmpBestPossible == null && effectiveRestriction.IsRestriction)
				{
					//finderResult.Successful = true;
					shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, agentTimeZone, bag, true);
					_possibleCombinationsOfStartEndCategoryRunner.RunTheList(combinations.ToList(), shiftList, dateOnly, groupPerson, schedulingOptions);
					if (!combinations.IsEmpty())
						tmpBestPossible = combinations.OrderBy(c => c.ShiftValue).Last();
						
				}
				if(!finderResult.Successful)
				{
					string key = Resources.ScheduleBlockDayWith + dateOnly.ToShortDateString(CultureInfo.CurrentCulture);
					if(!result.WorkShiftFinderResult.ContainsKey(key))
						result.WorkShiftFinderResult.Add(key, finderResult);
				}

				if (tmpBestPossible != null)
					bestPossible.Add(tmpBestPossible);

			}
            if (!schedulePeriodFound)
                return new BestShiftCategoryResult(null, FailureCause.NoValidPeriod);

			IPossibleStartEndCategory best = null;
			if (bestPossible.Count > 0)
				best = bestPossible.OrderBy(c => c.ShiftValue).Last();

			return new BestShiftCategoryResult(best, FailureCause.NoFailure); 
        }

	    private DateTimePeriod extendedVisiblePeriod()
	    {
	        DateTimePeriod basePeriod = _schedulingResultStateHolder.Schedules.Period.VisiblePeriod; 
            var ret = new DateTimePeriod(basePeriod.StartDateTime.AddDays(-10), basePeriod.EndDateTime.AddDays(10));
	        return ret;
	    }

		//private double getBestValueForShifts(IPerson person, IShiftCategory shiftCategory, DateOnly dateOnly, IVirtualSchedulePeriod currentSchedulePeriod, 
		//    IScheduleDictionary dictionary, IList<IPerson> persons, IEffectiveRestriction effectiveRestriction, ICccTimeZoneInfo agentTimeZone, 
		//    IList<IShiftProjectionCache> shiftList, out IWorkShiftFinderResult finderResult, IFairnessValueResult totalFairness, IFairnessValueResult agentFairness,
		//    IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolders, ISchedulingOptions schedulingOptions)
		//{
		//    finderResult = new WorkShiftFinderResult(person, dateOnly);

		//    shiftList = _shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(shiftList);
		//    shiftList = _shiftProjectionCacheFilter.FilterOnShiftCategory(shiftCategory, shiftList, finderResult);
		//    shiftList = _shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(dateOnly, agentTimeZone, shiftList, effectiveRestriction,
		//                                                                                            schedulingOptions.NotAllowedShiftCategories, finderResult);

		//    shiftList = _shiftProjectionCacheFilter.FilterOnBusinessRules(persons, dictionary, dateOnly, shiftList, finderResult);
		//    shiftList = _shiftProjectionCacheFilter.FilterOnPersonalShifts(persons, dictionary, dateOnly, shiftList, finderResult);

		//    if (shiftList.Count == 0)
		//        return double.MinValue;
	        


		//    TimeSpan averageWorkTime = currentSchedulePeriod.AverageWorkTimePerDay;
		//    bool useShiftCategoryFairness = false;
		//    IShiftCategoryFairnessFactors shiftCategoryFairnessFactors = null;
		//    //new ShiftCategoryFairnessFactors(new Dictionary<IShiftCategory, double>());
		//    if (person.WorkflowControlSet != null)
		//    {
		//        useShiftCategoryFairness = person.WorkflowControlSet.UseShiftCategoryFairness;

		//        IScheduleRange range = ScheduleDictionary[person];
		//        IShiftCategoryFairnessCalculator calculator = new ShiftCategoryFairnessCalculator(_groupShiftCategoryFairnessCreator);

		//        shiftCategoryFairnessFactors = calculator.ShiftCategoryFairnessFactors(range, person, dateOnly );
		//    }

		//    return _workShiftFinderService.BestShiftValue(dateOnly,
		//                                                  shiftList,
		//                                                  dataHolders,
		//                                                  totalFairness,
		//                                                  agentFairness, 5,
		//                                                  averageWorkTime,
		//                                                  useShiftCategoryFairness,
		//                                                  shiftCategoryFairnessFactors, 
		//                                                  (double)schedulingOptions.WorkShiftLengthHintOption,
		//                                                  schedulingOptions.UseMinimumPersons,
		//                                                  schedulingOptions.UseMaximumPersons, 
		//                                                  schedulingOptions);
		//}

	    public IScheduleDictionary ScheduleDictionary
    	{
            get { return _schedulingResultStateHolder.Schedules; }
    	}
    }
}

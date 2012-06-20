using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IShiftCategoryPeriodValueExtractorThread
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		void ExtractShiftCategoryPeriodValue(object possibleStartEndCategory);

		ManualResetEvent ManualResetEvent { get; }
	}

	public class ShiftCategoryPeriodValueExtractorThread : IShiftCategoryPeriodValueExtractorThread
	{
    	private readonly ManualResetEvent _manualResetEvent;
    	private IList<IShiftProjectionCache> _shiftProjectionList;
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly IBlockSchedulingWorkShiftFinderService _workShiftFinderService;
        private readonly DateOnly _dateOnly;
        private readonly IPerson _person;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IPersonSkillPeriodsDataHolderManager _personSkillPeriodsDataHolderManager;
        private readonly IGroupShiftCategoryFairnessCreator _groupShiftCategoryFairnessCreator;
        private readonly IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;

        //constructor
        public ShiftCategoryPeriodValueExtractorThread( IList<IShiftProjectionCache> shiftProjectionList, ISchedulingOptions schedulingOptions, 
			IBlockSchedulingWorkShiftFinderService workShiftFinderService, DateOnly dateOnly, IPerson person, ISchedulingResultStateHolder schedulingResultStateHolder, IPersonSkillPeriodsDataHolderManager personSkillPeriodsDataHolderManager, IGroupShiftCategoryFairnessCreator groupShiftCategoryFairnessCreator, IShiftProjectionCacheFilter shiftProjectionCacheFilter, IEffectiveRestrictionCreator effectiveRestrictionCreator)
        {
        	_manualResetEvent = new ManualResetEvent(false);
            _shiftProjectionList = shiftProjectionList;
            _schedulingOptions = schedulingOptions;
            _workShiftFinderService = workShiftFinderService;
            _dateOnly = dateOnly;
            _person = person;
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _personSkillPeriodsDataHolderManager = personSkillPeriodsDataHolderManager;
            _groupShiftCategoryFairnessCreator = groupShiftCategoryFairnessCreator;
            _shiftProjectionCacheFilter = shiftProjectionCacheFilter;
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
        }

        public IScheduleDictionary ScheduleDictionary
        {
            get { return _schedulingResultStateHolder.Schedules; }
        }

        // This method will be called when the thread is started.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void ExtractShiftCategoryPeriodValue(object possibleStartEndCategory)
        {
			var possible = possibleStartEndCategory as PossibleStartEndCategory;
			if (possible == null)
			{
				_manualResetEvent.Set();
				return;
			}

			var scheduleDictionary = ScheduleDictionary;
			IList<IPerson> persons;
			var groupPerson = _person as IGroupPerson;
			if (groupPerson != null)
			{
				persons = new List<IPerson>();
				foreach (var member in groupPerson.GroupMembers)
				{
					var scheduleDay = scheduleDictionary[member].ScheduledDay(_dateOnly);
					if (!scheduleDay.IsScheduled())
						persons.Add(member);
				}

				if (persons.Count == 0)
					return;
			}
			else
			{
				persons = new List<IPerson> { _person };
			}

			var agentFairness = scheduleDictionary.AverageFairnessPoints(persons);
            var currentSchedulePeriod = _person.VirtualSchedulePeriod(_dateOnly);
              
            var dataHolders =
                    _personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(_dateOnly,
                    currentSchedulePeriod);

            var averageWorkTime = currentSchedulePeriod.AverageWorkTimePerDay;
            var useShiftCategoryFairness = false;
            
            var shiftCategoryFairnessFactors = extractShiftCategoryFairnessFactor(_person ) ;
            
           if (_person.WorkflowControlSet != null)
            {
                useShiftCategoryFairness = _person.WorkflowControlSet.UseShiftCategoryFairness;
                
            }


		   var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(persons, _dateOnly, _schedulingOptions, scheduleDictionary);
            var finderResult = new WorkShiftFinderResult(_person, _dateOnly);

            //applying the filters
            var shiftProjectionList = FilterShiftCategoryPeriodOnSchedulingOptions(_person.PermissionInformation.DefaultTimeZone(),
				effectiveRestriction, persons, finderResult, possible);

            var shiftValue = _workShiftFinderService.BestShiftValue(_dateOnly,
                                                        shiftProjectionList,
                                                        dataHolders,
                                                        scheduleDictionary.FairnessPoints(),
                                                        agentFairness, 5,
                                                        averageWorkTime,
                                                        useShiftCategoryFairness,
                                                        shiftCategoryFairnessFactors,
                                                        (double)_schedulingOptions.WorkShiftLengthHintOption,
                                                        _schedulingOptions.UseMinimumPersons,
                                                        _schedulingOptions.UseMaximumPersons,
                                                        _schedulingOptions);

			possible.ShiftValue = shiftValue;
        	_manualResetEvent.Set();
        }

		public ManualResetEvent ManualResetEvent
		{
			get { return _manualResetEvent; }
		}

		private IShiftCategoryFairnessFactors extractShiftCategoryFairnessFactor(IPerson person)
        {
			lock (ScheduleDictionary)
			{
				var range = ScheduleDictionary[person];
				IShiftCategoryFairnessCalculator calculator = new ShiftCategoryFairnessCalculator(_groupShiftCategoryFairnessCreator);
				return calculator.ShiftCategoryFairnessFactors(range, person, _dateOnly);
			}
        }



		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
		public IList<IShiftProjectionCache> FilterShiftCategoryPeriodOnSchedulingOptions(ICccTimeZoneInfo agentTimeZone, 
            IEffectiveRestriction effectiveRestriction,IList<IPerson>persons,
			IWorkShiftFinderResult finderResult, IPossibleStartEndCategory possibleStartEndCategory)
        {
            var dictionary = ScheduleDictionary;
            _shiftProjectionList = _shiftProjectionCacheFilter.
                FilterOnMainShiftOptimizeActivitiesSpecification(_shiftProjectionList);
            
            _shiftProjectionList = _shiftProjectionCacheFilter.
                FilterOnRestrictionAndNotAllowedShiftCategories(_dateOnly, agentTimeZone, _shiftProjectionList,
                                                                effectiveRestriction,_schedulingOptions.NotAllowedShiftCategories, finderResult);
            
            _shiftProjectionList = _shiftProjectionCacheFilter.
                FilterOnBusinessRules(persons, dictionary, _dateOnly, _shiftProjectionList, finderResult);
            
            _shiftProjectionList = _shiftProjectionCacheFilter.
                FilterOnPersonalShifts(persons, dictionary, _dateOnly, _shiftProjectionList, finderResult);

            
      _shiftProjectionList =
                _shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonStartEnd(
                    _shiftProjectionList,
                    possibleStartEndCategory,
                    _schedulingOptions);

            if (_schedulingOptions.UseGroupSchedulingCommonCategory)
            {
                _shiftProjectionList = _shiftProjectionCacheFilter.
                    FilterOnShiftCategory(possibleStartEndCategory.ShiftCategory, 
                                          _shiftProjectionList, finderResult); 

            }
            return _shiftProjectionList;
        }


    }
}

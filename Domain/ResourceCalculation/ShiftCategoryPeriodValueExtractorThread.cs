﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class ShiftCategoryPeriodValueExtractorThread
    {

        private readonly IPossibleStartEndCategory _possibleStartEndCategory;
        private IList<IShiftProjectionCache> _shiftProjectionList;
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly IBlockSchedulingWorkShiftFinderService _workShiftFinderService;
        private readonly DateOnly _dateOnly;
        private readonly IGroupPerson _groupPerson;
        private readonly ISchedulingResultStateHolder _resultStateHolder;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IPersonSkillPeriodsDataHolderManager _personSkillPeriodsDataHolderManager;
        private readonly IGroupShiftCategoryFairnessCreator _groupShiftCategoryFairnessCreator;
        private readonly IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;

        //constructor
        public ShiftCategoryPeriodValueExtractorThread( IPossibleStartEndCategory possibleStartEndCategory,
                                                        IList<IShiftProjectionCache> shiftProjectionList,
                                                        ISchedulingOptions schedulingOptions,
                                                        IBlockSchedulingWorkShiftFinderService workShiftFinderService,
                                                        DateOnly dateOnly,
                                                        IGroupPerson groupPerson,
                                                        ISchedulingResultStateHolder resultStateHolder,
                                                        ISchedulingResultStateHolder  schedulingResultStateHolder,
                                                        IPersonSkillPeriodsDataHolderManager personSkillPeriodsDataHolderManager,
                                                        IGroupShiftCategoryFairnessCreator groupShiftCategoryFairnessCreator,
                                                        IShiftProjectionCacheFilter  shiftProjectionCacheFilter,
                                                        IEffectiveRestrictionCreator  effectiveRestrictionCreator
                                                        )
        {
            _possibleStartEndCategory = possibleStartEndCategory;
            _shiftProjectionList = shiftProjectionList;
            _schedulingOptions = schedulingOptions;
            _workShiftFinderService = workShiftFinderService;
            _dateOnly = dateOnly;
            _groupPerson = groupPerson;
            _resultStateHolder = resultStateHolder;
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
        public void ExtractShiftCategoryPeriodValue()
        {
            var person = (IPerson)_groupPerson;
            var scheduleDictionary = _resultStateHolder.Schedules;
            var members = _groupPerson.GroupMembers;
            

            
            var agentFairness = scheduleDictionary.AverageFairnessPoints(members);
            var currentSchedulePeriod = person.VirtualSchedulePeriod(_dateOnly);
              
            var dataHolders =
                    _personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(_dateOnly,
                    currentSchedulePeriod);

            var averageWorkTime = currentSchedulePeriod.AverageWorkTimePerDay;
            var useShiftCategoryFairness = false;
            
            IShiftCategoryFairnessFactors shiftCategoryFairnessFactors = ExtractShiftCategoryFairnessFactor(person ) ;
            
           if (person.WorkflowControlSet != null)
            {
                useShiftCategoryFairness = person.WorkflowControlSet.UseShiftCategoryFairness;
                
            }
                
            var groupPerson = person as IGroupPerson;
            var dictionary = ScheduleDictionary;
            IList<IPerson> persons = (from member in groupPerson.GroupMembers let scheduleDay = 
                                            dictionary[member].ScheduledDay(_dateOnly) where !scheduleDay.IsScheduled() select member).ToList();

            var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(persons, _dateOnly, _schedulingOptions, dictionary);
            var finderResult = new WorkShiftFinderResult(person, _dateOnly);

            //applying the filters
            var shiftProjectionList = FilterShiftCategoryPeriodOnSchedulingOptions(person.PermissionInformation.DefaultTimeZone(),effectiveRestriction,persons,finderResult  );

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

            _possibleStartEndCategory.ShiftValue = shiftValue;
 
        }

        private IShiftCategoryFairnessFactors ExtractShiftCategoryFairnessFactor(IPerson person)
        {
            var range = ScheduleDictionary[person];
            IShiftCategoryFairnessCalculator calculator = new ShiftCategoryFairnessCalculator(range, person, _dateOnly, _groupShiftCategoryFairnessCreator);
            return calculator.ShiftCategoryFairnessFactors();
        }


       
        public IList<IShiftProjectionCache> FilterShiftCategoryPeriodOnSchedulingOptions(ICccTimeZoneInfo agentTimeZone, 
            IEffectiveRestriction effectiveRestriction,
            IList<IPerson>persons,
            IWorkShiftFinderResult finderResult  )
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
                    _possibleStartEndCategory,
                    _schedulingOptions);

            if (_schedulingOptions.UseGroupSchedulingCommonCategory)
            {
                _shiftProjectionList = _shiftProjectionCacheFilter.
                    FilterOnShiftCategory(_possibleStartEndCategory.ShiftCategory, 
                                          _shiftProjectionList, finderResult); 

            }
            return _shiftProjectionList;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IShiftCategoryPeriodValueExtractorThread
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		void ExtractShiftCategoryPeriodValue(object possibleStartEndCategory);
	}

	public class ShiftCategoryPeriodValueExtractorThread : IShiftCategoryPeriodValueExtractorThread, IDisposable
	{
    	private ManualResetEvent _manualResetEvent;
    	private IList<IShiftProjectionCache> _shiftProjectionList;
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly IBlockSchedulingWorkShiftFinderService _workShiftFinderService;
        private readonly DateOnly _dateOnly;
        private readonly IPerson _person;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IPersonSkillPeriodsDataHolderManager _personSkillPeriodsDataHolderManager;
        private readonly IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
		private readonly bool _useShiftCategoryFairness;
		private readonly IShiftCategoryFairnessFactors _shiftCategoryFairnessFactors;
		private readonly IFairnessValueResult _totalFairness;
		private readonly IFairnessValueResult _agentFairness;
		private readonly IList<IPerson> _persons;
		private readonly IEffectiveRestriction _effectiveRestriction;


		//constructor
        public ShiftCategoryPeriodValueExtractorThread( IList<IShiftProjectionCache> shiftProjectionList, ISchedulingOptions schedulingOptions, 
			IBlockSchedulingWorkShiftFinderService workShiftFinderService, DateOnly dateOnly, IPerson person,
			ISchedulingResultStateHolder schedulingResultStateHolder, IPersonSkillPeriodsDataHolderManager personSkillPeriodsDataHolderManager, 
			 IShiftProjectionCacheFilter shiftProjectionCacheFilter, bool useShiftCategoryFairness, IShiftCategoryFairnessFactors shiftCategoryFairnessFactors,
			IFairnessValueResult totalFairness, IFairnessValueResult agentFairness, IList<IPerson> persons,
			IEffectiveRestriction effectiveRestriction)
        {
        	_manualResetEvent = new ManualResetEvent(false);
            _shiftProjectionList = shiftProjectionList;
            _schedulingOptions = schedulingOptions;
            _workShiftFinderService = workShiftFinderService;
            _dateOnly = dateOnly;
            _person = person;
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _personSkillPeriodsDataHolderManager = personSkillPeriodsDataHolderManager;
            _shiftProjectionCacheFilter = shiftProjectionCacheFilter;
        	_useShiftCategoryFairness = useShiftCategoryFairness;
        	_shiftCategoryFairnessFactors = shiftCategoryFairnessFactors;
        	_totalFairness = totalFairness;
        	_agentFairness = agentFairness;
        	_persons = persons;
        	_effectiveRestriction = effectiveRestriction;
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
				if (_manualResetEvent != null)
					_manualResetEvent.Set();
				return;
			}

			
            var currentSchedulePeriod = _person.VirtualSchedulePeriod(_dateOnly);
              
            var dataHolders =
                    _personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(_dateOnly,
                    currentSchedulePeriod);

            var averageWorkTime = currentSchedulePeriod.AverageWorkTimePerDay;
            
            var finderResult = new WorkShiftFinderResult(_person, _dateOnly);

            //applying the filters
            var shiftProjectionList = FilterShiftCategoryPeriodOnSchedulingOptions(_person.PermissionInformation.DefaultTimeZone(),
				_effectiveRestriction, _persons, finderResult, possible);

            var shiftValue = _workShiftFinderService.BestShiftValue(_dateOnly,
                                                        shiftProjectionList,
                                                        dataHolders,
                                                        _totalFairness,
                                                        _agentFairness, 5,
                                                        averageWorkTime,
                                                        _useShiftCategoryFairness,
                                                        _shiftCategoryFairnessFactors,
                                                        _schedulingOptions);
            if (_schedulingOptions.UseCommonActivity)
                possible.ActivityPeriods = shiftValue.ActivityPeriods ;
			possible.ShiftValue = shiftValue.Value ;
			if (_manualResetEvent != null)
        		_manualResetEvent.Set();
        }

		public ManualResetEvent ManualResetEvent
		{
			get { return _manualResetEvent; }
		}

		


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
		public IList<IShiftProjectionCache> FilterShiftCategoryPeriodOnSchedulingOptions(TimeZoneInfo agentTimeZone, 
            IEffectiveRestriction effectiveRestriction,IList<IPerson>persons,
			IWorkShiftFinderResult finderResult, IPossibleStartEndCategory possibleStartEndCategory)
        {
            var dictionary = ScheduleDictionary;
            _shiftProjectionList = _shiftProjectionCacheFilter.
                FilterOnMainShiftOptimizeActivitiesSpecification(_shiftProjectionList, _schedulingOptions.MainShiftOptimizeActivitySpecification);
            
            _shiftProjectionList = _shiftProjectionCacheFilter.
                FilterOnRestrictionAndNotAllowedShiftCategories(_dateOnly, agentTimeZone, _shiftProjectionList,
                                                                effectiveRestriction,_schedulingOptions.NotAllowedShiftCategories, finderResult);
            
            _shiftProjectionList = _shiftProjectionCacheFilter.
                FilterOnBusinessRules(persons, dictionary, _dateOnly, _shiftProjectionList, finderResult);
            
			_shiftProjectionList =
                _shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonStartEnd(
                    _shiftProjectionList,
                    possibleStartEndCategory,
                    _schedulingOptions,
					finderResult);

            if (_schedulingOptions.UseGroupSchedulingCommonCategory)
            {
                _shiftProjectionList = _shiftProjectionCacheFilter.
                    FilterOnShiftCategory(possibleStartEndCategory.ShiftCategory, 
                                          _shiftProjectionList, finderResult); 

            }
            return _shiftProjectionList;
        }


		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if(_manualResetEvent != null)
					_manualResetEvent.Close();
				_manualResetEvent = null;
			}
		}
	}
}

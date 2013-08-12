using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses"), TestFixture]
    public class ShiftCategoryPeriodValueExtractorThreadTest : IDisposable
    {
        private MockRepository _mocks;
        private ISchedulingOptions _schedulingOptions;
        private ShiftCategoryPeriodValueExtractorThread _target;
        private IPossibleStartEndCategory _possibleStartEndCategory;
        private IList<IShiftProjectionCache> _shiftProjectionList;
        private IBlockSchedulingWorkShiftFinderService _workShiftFinderService;
        private DateOnly _dateOnly;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IPersonSkillPeriodsDataHolderManager _personSkillPeriodDataHolderManager;
        private IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private IGroupPerson _groupPerson;
        private IEffectiveRestriction _effectiveRestriction;

        private IShiftProjectionCache _cashe1;
        private IShiftProjectionCache _cashe2;
        private IShiftProjectionCache _cashe3;
        private IShiftProjectionCache _cashe4;
    	private Guid _guid;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            
            _schedulingOptions = new SchedulingOptions { UseGroupScheduling = true, UseGroupSchedulingCommonStart = true };
            
            _shiftProjectionList = _mocks.DynamicMock<IList<IShiftProjectionCache>>();
            _workShiftFinderService = _mocks.StrictMock<IBlockSchedulingWorkShiftFinderService>();
            _dateOnly = new DateOnly(new DateTime(2012, 6, 8, 8, 0, 0));
            _schedulingResultStateHolder = _mocks.DynamicMock<ISchedulingResultStateHolder>();
            _personSkillPeriodDataHolderManager = _mocks.DynamicMock<IPersonSkillPeriodsDataHolderManager>();
            _shiftProjectionCacheFilter = _mocks.DynamicMock<IShiftProjectionCacheFilter>();
            _effectiveRestrictionCreator = _mocks.DynamicMock<IEffectiveRestrictionCreator>();
            _possibleStartEndCategory = new PossibleStartEndCategory();
            _effectiveRestriction = _mocks.DynamicMock<IEffectiveRestriction>();
        	_guid = Guid.NewGuid();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void TestDifferentFilterShiftCategory()
        {
            IPerson person = new Person();
            TimeZoneInfo agentTimeZone = TimeZoneInfo.Utc;
            var personList = new List<IPerson> { person };
            IWorkShiftFinderResult finderResult = new WorkShiftFinderResult(person, _dateOnly);
            var notAllowedShiftCategory = new List<IShiftCategory> { new ShiftCategory("test") };
            _shiftProjectionList = getCashes();

            IScenario scenario = new Scenario("sc");
            var period = new DateTimePeriod(new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc));
            IScheduleDateTimePeriod period1 = new ScheduleDateTimePeriod(period);
            var scheduleDictionary = new ScheduleDictionary(scenario, period1);
        	var fairness = _mocks.DynamicMock<IShiftCategoryFairnessFactors>();
			using (_mocks.Record())
            {
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(_shiftProjectionList, new All<IEditableShift>())).IgnoreArguments().Return
                        (_shiftProjectionList).Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(_dateOnly,
                                                                                                        agentTimeZone,
                                                                                                        _shiftProjectionList,
                                                                                                        _effectiveRestriction,
                                                                                                        notAllowedShiftCategory,
                                                                                                        finderResult)).
                    IgnoreArguments().Return(_shiftProjectionList).Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheFilter.FilterOnBusinessRules(personList, scheduleDictionary, _dateOnly,
                                                                              _shiftProjectionList, finderResult)).
                    IgnoreArguments().Return(_shiftProjectionList).Repeat.AtLeastOnce();
              
                Expect.Call(_shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonStartEnd(_shiftProjectionList,
                                                                                              _possibleStartEndCategory,
                                                                                              _schedulingOptions,
																							  finderResult)).
                    IgnoreArguments().Return(_shiftProjectionList).Repeat.AtLeastOnce();
            }


            using (_mocks.Playback())
            {
				_target = new ShiftCategoryPeriodValueExtractorThread( _shiftProjectionList, _schedulingOptions, _workShiftFinderService, _dateOnly,
                _groupPerson,  _schedulingResultStateHolder, _personSkillPeriodDataHolderManager, 
                _shiftProjectionCacheFilter, false, fairness, null, null,new List<IPerson>(),_effectiveRestriction );

                var result = _target.FilterShiftCategoryPeriodOnSchedulingOptions(agentTimeZone,
                                                                               _effectiveRestrictionCreator.
                                                                                   GetEffectiveRestriction(personList,
                                                                                                           _dateOnly,
                                                                                                           _schedulingOptions,
                                                                                                           scheduleDictionary),
																			   personList, finderResult, _possibleStartEndCategory);

                Assert.That(result.Count, Is.EqualTo(4));
            }   

}


        private IList<IShiftProjectionCache> getCashes()
        {
            _cashe1 = _mocks.DynamicMock<IShiftProjectionCache>();
            _cashe2 = _mocks.DynamicMock<IShiftProjectionCache>();
            _cashe3 = _mocks.DynamicMock<IShiftProjectionCache>();
            _cashe4 = _mocks.DynamicMock<IShiftProjectionCache>();
            return new List<IShiftProjectionCache> { _cashe1, _cashe2, _cashe3, _cashe4 };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void TestExtractShiftCategoryPeriodValue()
        {
           
            _shiftProjectionList = getCashes();

            IPerson person = new Person();
            var personList = new List<IPerson> { person };
            _groupPerson = new GroupPerson(personList,_dateOnly,"Grp", _guid);

            IDictionary<IShiftCategory, int> shiftDictionary = new Dictionary<IShiftCategory, int>();
            _possibleStartEndCategory.ShiftCategory = new ShiftCategory("newshiftcat");
            shiftDictionary.Add(_possibleStartEndCategory.ShiftCategory,25 );
             IScenario scenario = new Scenario("sc");

            var period = new DateTimePeriod(new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc));
            IScheduleDateTimePeriod period1 = new ScheduleDateTimePeriod(period);
            var scheduleDictionary = new ScheduleDictionary(scenario, period1);
			var fairness = _mocks.DynamicMock<IShiftCategoryFairnessFactors>();
            using (_mocks.Record())
            {
               Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
               Expect.Call(_workShiftFinderService.BestShiftValue(_dateOnly, _shiftProjectionList, null, null, null, 5,
                                                                   TimeSpan.FromHours(8), false, null,
                                                                   _schedulingOptions)).IgnoreArguments().Return(
                                                                       new ShiftProjectionShiftValue
                                                                           {
                                                                               ShiftProjection = _shiftProjectionList[0],
                                                                               Value = 100
                                                                           });
            }
            _schedulingOptions.UseCommonActivity = true;
            
            using (_mocks.Playback())
            {
				_target = new ShiftCategoryPeriodValueExtractorThread(_shiftProjectionList, _schedulingOptions, _workShiftFinderService, _dateOnly,
               _groupPerson,  _schedulingResultStateHolder, _personSkillPeriodDataHolderManager,
			   _shiftProjectionCacheFilter, false, fairness, null, null, new List<IPerson>(), _effectiveRestriction);

				_target.ExtractShiftCategoryPeriodValue(_possibleStartEndCategory);
            }
            
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
				if (_target != null)
					_target.Dispose();
				_target = null;
			}
		}
     
    }
}

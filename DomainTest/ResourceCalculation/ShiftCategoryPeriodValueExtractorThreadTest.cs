using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    class ShiftCategoryPeriodValueExtractorThreadTest
    {
        private MockRepository _mocks;
        private ISchedulingOptions _schedulingOptions;
        private ShiftCategoryPeriodValueExtractorThread _target;
        private IPossibleStartEndCategory _possibleStartEndCategory;
        private IList<IShiftProjectionCache> _shiftProjectionList;
        private IBlockSchedulingWorkShiftFinderService _workShiftFinderService;
        private DateOnly _dateOnly;
        private ISchedulingResultStateHolder _resultStateHolder;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IPersonSkillPeriodsDataHolderManager _personSkillPeriodDataHolderManager;
        private IGroupShiftCategoryFairnessCreator _groupShiftCategoryFairnessCreater;
        private IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private IGroupPerson _groupPerson;
        private IEffectiveRestriction _effectiveRestriction;

        private IShiftProjectionCache _cashe1;
        private IShiftProjectionCache _cashe2;
        private IShiftProjectionCache _cashe3;
        private IShiftProjectionCache _cashe4;

       
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            
            _schedulingOptions = new SchedulingOptions { UseGroupScheduling = true, UseGroupSchedulingCommonStart = true };
            
            _shiftProjectionList = _mocks.DynamicMock<IList<IShiftProjectionCache>>();
            _workShiftFinderService = _mocks.DynamicMock<IBlockSchedulingWorkShiftFinderService>();
            _dateOnly = new DateOnly(new DateTime(2012, 6, 8, 8, 0, 0));
            _resultStateHolder = _mocks.DynamicMock<ISchedulingResultStateHolder>();
            _schedulingResultStateHolder = _mocks.DynamicMock<ISchedulingResultStateHolder>();
            _personSkillPeriodDataHolderManager = _mocks.DynamicMock<IPersonSkillPeriodsDataHolderManager>();
            _groupShiftCategoryFairnessCreater = _mocks.DynamicMock<IGroupShiftCategoryFairnessCreator>();
            _shiftProjectionCacheFilter = _mocks.DynamicMock<IShiftProjectionCacheFilter>();
            _effectiveRestrictionCreator = _mocks.DynamicMock<IEffectiveRestrictionCreator>();
            _possibleStartEndCategory = new PossibleStartEndCategory();
            _effectiveRestriction = _mocks.DynamicMock<IEffectiveRestriction>();
        }

        [Test]
        public void TestDifferentFilterShiftCategory()
        {
            IPerson person = new Person();
            ICccTimeZoneInfo agentTimeZone = new CccTimeZoneInfo();
            var personList = new List<IPerson> { person };
            IWorkShiftFinderResult finderResult = new WorkShiftFinderResult(person, _dateOnly);
            var notAllowedShiftCategory = new List<IShiftCategory> { new ShiftCategory("test") };
            _shiftProjectionList = getCashes();

            IScenario scenario = new Scenario("sc");
            var period = new DateTimePeriod(new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc));
            IScheduleDateTimePeriod period1 = new ScheduleDateTimePeriod(period);
            var scheduleDictionary = new ScheduleDictionary(scenario, period1);

            using (_mocks.Record())
            {
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
                Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(_shiftProjectionList)).IgnoreArguments().Return
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
                Expect.Call(_shiftProjectionCacheFilter.FilterOnPersonalShifts(personList, scheduleDictionary, _dateOnly,
                                                                              _shiftProjectionList, finderResult)).
                    IgnoreArguments().Return(_shiftProjectionList).Repeat.AtLeastOnce();

                Expect.Call(_shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonStartEnd(_shiftProjectionList,
                                                                                              _possibleStartEndCategory,
                                                                                              _schedulingOptions)).
                    IgnoreArguments().Return(_shiftProjectionList).Repeat.AtLeastOnce();
            }


            using (_mocks.Playback())
            {
                _target = new ShiftCategoryPeriodValueExtractorThread(_possibleStartEndCategory, _shiftProjectionList, _schedulingOptions, _workShiftFinderService, _dateOnly,
                _groupPerson, _resultStateHolder, _schedulingResultStateHolder, _personSkillPeriodDataHolderManager, _groupShiftCategoryFairnessCreater,
                _shiftProjectionCacheFilter, _effectiveRestrictionCreator);

                var result = _target.FilterShiftCategoryPeriodOnSchedulingOptions(agentTimeZone,
                                                                               _effectiveRestrictionCreator.
                                                                                   GetEffectiveRestriction(personList,
                                                                                                           _dateOnly,
                                                                                                           _schedulingOptions,
                                                                                                           scheduleDictionary),
                                                                               personList, finderResult);

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

        [Test ]
        public void TestExtractShiftCategoryPeriodValue()
        {
           
            _shiftProjectionList = getCashes();

            IPerson person = new Person();
            var personList = new List<IPerson> { person };
            _groupPerson = new GroupPerson(personList,_dateOnly,"Grp");

            IDictionary<IShiftCategory, int> shiftDictionary = new Dictionary<IShiftCategory, int>();
            _possibleStartEndCategory.ShiftCategory = new ShiftCategory("newshiftcat");
            shiftDictionary.Add(_possibleStartEndCategory.ShiftCategory,25 );
            IFairnessValueResult fairnessValueResult = new FairnessValueResult();
            IShiftCategoryFairness objToReturn = new ShiftCategoryFairness(shiftDictionary, fairnessValueResult);
            IScenario scenario = new Scenario("sc");

            var period = new DateTimePeriod(new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc));
            IScheduleDateTimePeriod period1 = new ScheduleDateTimePeriod(period);
            var scheduleDictionary = new ScheduleDictionary(scenario, period1);


            using (_mocks.Record())
            {
                Expect.Call(_groupShiftCategoryFairnessCreater.CalculateGroupShiftCategoryFairness(person, _dateOnly)).IgnoreArguments().
                Return(objToReturn);

               
                Expect.Call(_resultStateHolder.Schedules).Return(scheduleDictionary);
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
            }

            using (_mocks.Playback())
            {
                _target = new ShiftCategoryPeriodValueExtractorThread(_possibleStartEndCategory, _shiftProjectionList, _schedulingOptions, _workShiftFinderService, _dateOnly,
               _groupPerson, _resultStateHolder, _schedulingResultStateHolder, _personSkillPeriodDataHolderManager, _groupShiftCategoryFairnessCreater,
               _shiftProjectionCacheFilter, _effectiveRestrictionCreator);

                _target.ExtractShiftCategoryPeriodValue();
            }

           

        

        }

     
    }
}

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
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.DomainTest.ResourceCalculation;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest
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

        readonly DateTime _start1 = new DateTime(2012, 6, 8, 8, 0, 0);
        readonly DateTime _start2 = new DateTime(2012, 6, 8, 7, 0, 0);
        readonly DateTime _start3 = new DateTime(2012, 6, 8, 8, 0, 0);
        readonly DateTime _start4 = new DateTime(2012, 6, 8, 10, 0, 0);
        readonly DateTime _start5 = new DateTime(2012, 6, 8, 10, 0, 0);
       
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            
            _schedulingOptions = new SchedulingOptions { UseGroupScheduling = true, UseGroupSchedulingCommonStart = true };
            _possibleStartEndCategory = new PossibleStartEndCategory();
            _shiftProjectionList = _mocks.DynamicMock<IList<IShiftProjectionCache>>();
            _workShiftFinderService = _mocks.DynamicMock<IBlockSchedulingWorkShiftFinderService>();
            _dateOnly = new DateOnly(new DateTime(2012, 6, 8, 8, 0, 0));
            _resultStateHolder = _mocks.DynamicMock<ISchedulingResultStateHolder>();
            _schedulingResultStateHolder = _mocks.DynamicMock<ISchedulingResultStateHolder>();
            _personSkillPeriodDataHolderManager = _mocks.DynamicMock<IPersonSkillPeriodsDataHolderManager>();
            _groupShiftCategoryFairnessCreater = _mocks.DynamicMock<IGroupShiftCategoryFairnessCreator>();
            _shiftProjectionCacheFilter = _mocks.DynamicMock<IShiftProjectionCacheFilter>();
            _effectiveRestrictionCreator = _mocks.DynamicMock<IEffectiveRestrictionCreator>();
            
            

            //_target = new ShiftCategoryPeriodValueExtractorThread(_possibleStartEndCategory,_shiftProjectionList,_schedulingOptions,_workShiftFinderService,_dateOnly,
            //    _groupPerson,_resultStateHolder,_schedulingResultStateHolder,_personSkillPeriodDataHolderManager,_groupShiftCategoryFairnessCreater,
            //    _shiftProjectionCacheFilter,_effectiveRestrictionCreator);
        }

        [Test]
        public void FilterShiftCategoryTest()
        {
            IPerson person = new Person();
            ICccTimeZoneInfo agentTimeZone = new CccTimeZoneInfo();
            var personList = new List<IPerson> {person};
            IWorkShiftFinderResult finderResult = new WorkShiftFinderResult(person, _dateOnly);
            var notAllowedShiftCategory = new List<IShiftCategory> {new ShiftCategory("test")};

            _shiftProjectionList  = getCashes();
            Expect.Call(_cashe1.MainShiftStartDateTime).Return(_start1);
            Expect.Call(_cashe2.MainShiftStartDateTime).Return(_start2);
            Expect.Call(_cashe3.MainShiftStartDateTime).Return(_start3);
            Expect.Call(_cashe4.MainShiftStartDateTime).Return(_start4);
           

            var scheduleDictionary = _mocks.DynamicMock<IScheduleDictionary>();

            Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
            Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(_shiftProjectionList )).IgnoreArguments().Return
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

            _possibleStartEndCategory.StartTime= new DateTime(2012, 6, 8, 8, 0, 0);

            _target = new ShiftCategoryPeriodValueExtractorThread(_possibleStartEndCategory, _shiftProjectionList, _schedulingOptions, _workShiftFinderService, _dateOnly,
                _groupPerson, _resultStateHolder, _schedulingResultStateHolder, _personSkillPeriodDataHolderManager, _groupShiftCategoryFairnessCreater,
                _shiftProjectionCacheFilter, _effectiveRestrictionCreator);

            _mocks.ReplayAll();
            var result = _target.FilterShiftCategoryPeriodOnSchedulingOptions(agentTimeZone,
                                                                              _effectiveRestrictionCreator.
                                                                                  GetEffectiveRestriction(personList,
                                                                                                          _dateOnly,
                                                                                                          _schedulingOptions,
                                                                                                          scheduleDictionary),
                                                                              personList, finderResult);

            Assert.That(result.Count, Is.EqualTo(2));
            _mocks.VerifyAll();
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
            Expect.Call(_cashe1.MainShiftStartDateTime).Return(_start1);
            Expect.Call(_cashe2.MainShiftStartDateTime).Return(_start2);
            Expect.Call(_cashe3.MainShiftStartDateTime).Return(_start3);
            Expect.Call(_cashe4.MainShiftStartDateTime).Return(_start4);

            IPerson person = new Person();
            var personList = new List<IPerson> { person };
            _groupPerson = new GroupPerson(personList,_dateOnly,"Grp");
            

            _possibleStartEndCategory.StartTime = new DateTime(2012, 6, 8, 8, 0, 0);

            _target = new ShiftCategoryPeriodValueExtractorThread(_possibleStartEndCategory, _shiftProjectionList, _schedulingOptions, _workShiftFinderService, _dateOnly,
                _groupPerson, _resultStateHolder, _schedulingResultStateHolder, _personSkillPeriodDataHolderManager, _groupShiftCategoryFairnessCreater,
                _shiftProjectionCacheFilter, _effectiveRestrictionCreator);

            var result = _target.ExtractShiftCategoryPeriodValue();

            Assert.That(result,Is.EqualTo(5.8));
            _mocks.VerifyAll();

        }

     
    }
}

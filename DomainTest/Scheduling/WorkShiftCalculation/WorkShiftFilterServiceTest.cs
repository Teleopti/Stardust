using System;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class WorkShiftFilterServiceTest
    {
        private IWorkShiftFilterService _target;
        private MockRepository _mocks;
        private IShiftProjectionCacheManager _shiftProjectionCacheManager;
        private IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
        private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private ISchedulingResultStateHolder _stateHolder;
        private IShiftLengthDecider _shiftLengthDecider;
        private ISchedulingOptions _schedulingOptions;
        private TimeZoneInfo _timeZoneInfo;
        private IScheduleMatrixPro _matrix;
        private IPerson _person;
        private IVirtualSchedulePeriod _schedulePeriod;
        private IPersonPeriod _personPeriod;
        private IPermissionInformation _info;
        private IWorkShift _workShift1;
        private IWorkShift _workShift2;
        private IWorkShift _workShift3;
        private IShiftCategory _category;
        private IActivity _activity;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _shiftProjectionCacheManager = _mocks.StrictMock<IShiftProjectionCacheManager>();
            _shiftProjectionCacheFilter = _mocks.StrictMock<IShiftProjectionCacheFilter>();
            _workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _shiftLengthDecider = _mocks.StrictMock<IShiftLengthDecider>();
            _schedulingOptions = new SchedulingOptions();
            var zone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
            _timeZoneInfo = (zone);
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _personPeriod = _mocks.StrictMock<IPersonPeriod>();
            _person = _mocks.StrictMock<IPerson>();
            _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _info = new PermissionInformation(_person);
            _info.SetDefaultTimeZone(_timeZoneInfo);
            _target = new WorkShiftFilterService(_shiftProjectionCacheManager, _shiftProjectionCacheFilter,
                                                 _workShiftMinMaxCalculator, _stateHolder, _shiftLengthDecider);
        }

        [Test]
        public void ShouldFilterWorkShifts()
        {
            var bag = _mocks.StrictMock<IRuleSetBag>();
            var dictionary = _mocks.StrictMock<IScheduleDictionary>();
            var range = _mocks.StrictMock<IScheduleRange>();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            var caches = getCashes();
            var dateOnly = new DateOnly(2012, 12, 12);
            _schedulingOptions.ShiftCategory = _category;
            using (_mocks.Record())
            {
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_stateHolder.Schedules).Return(dictionary);
                Expect.Call(_person.VirtualSchedulePeriod(dateOnly)).Return(_schedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_person.Period(dateOnly)).Return(_personPeriod);
                Expect.Call(_personPeriod.RuleSetBag).Return(bag);
                Expect.Call(dictionary[_person]).Return(range).Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromAdjustedRuleSetBag(dateOnly, _timeZoneInfo, bag, false, effectiveRestriction)).Return(caches);
                Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(caches, new Domain.Specification.All<IMainShift>())).
                    IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
                //Expect.Call(_shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonStartEnd(caches, null, _schedulingOptions, null)).
                //    IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
                //Expect.Call(_shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonActivity(caches, _schedulingOptions, null, null)).
                //    IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
                effectiveRestriction.ShiftCategory = _category;
                Expect.Call(_shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(new DateOnly(), null, null, null, null, null)).
                    IgnoreArguments().Return(caches);
                Expect.Call(_shiftProjectionCacheFilter.CheckRestrictions(_schedulingOptions, effectiveRestriction, null)).IgnoreArguments().Return(
                    true);
                Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, _matrix, _schedulingOptions)).Return(
                    new MinMax<TimeSpan>(new TimeSpan(0, 6, 0, 0), new TimeSpan(0, 12, 0, 0)));
                Expect.Call(_shiftProjectionCacheFilter.Filter(new MinMax<TimeSpan>(), caches, dateOnly,
                                                               range, null)).IgnoreArguments().Return(caches);
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_person.PermissionInformation).Return(_info).Repeat.AtLeastOnce();
                Expect.Call(_shiftLengthDecider.FilterList(caches, _workShiftMinMaxCalculator, _matrix, _schedulingOptions)).Return(caches);
            }
            using (_mocks.Playback())
            {
                var retShift = _target.Filter(dateOnly, _person, new List<IScheduleMatrixPro>{_matrix}, effectiveRestriction, _schedulingOptions);
                Assert.IsNotNull(retShift);
            }
        }

        private IList<IShiftProjectionCache> getCashes()
        {
            var dateOnly = new DateOnly(2009, 2, 2);
            var tmpList = getWorkShifts();
            var retList = new List<IShiftProjectionCache>();
            foreach (IWorkShift shift in tmpList)
            {
                var cache = new ShiftProjectionCache(shift);
                cache.SetDate(dateOnly, _timeZoneInfo);
                retList.Add(cache);
            }
            return retList;
        }

        private IEnumerable<IWorkShift> getWorkShifts()
        {
            _activity = ActivityFactory.CreateActivity("sd");
            _category = ShiftCategoryFactory.CreateShiftCategory("dv");
            _workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
                                                          _activity, _category);
            _workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
                                                          _activity, _category);
            _workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
                                                                      _activity, _category);

            return new List<IWorkShift> { _workShift1, _workShift2, _workShift3 };
        }
    }
}

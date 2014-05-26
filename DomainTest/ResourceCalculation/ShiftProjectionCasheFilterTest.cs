using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestFixture]
    public class ShiftProjectionCacheFilterTest
    {
        private ShiftProjectionCacheFilter _target;
        private DateOnly _dateOnly;
        private ILongestPeriodForAssignmentCalculator _rules;
        private IWorkShift _workShift1;
        private IWorkShift _workShift2;
        private IWorkShift _workShift3;
        private IShiftCategory _category;
        private IActivity _activity;
        private IEffectiveRestriction _effectiveRestriction;
        private IWorkShiftFinderResult _finderResult;
        private MockRepository _mocks;
        private TimeZoneInfo _TimeZoneInfo;
        private IScheduleRange _scheduleRange;
        private IScheduleDay _part;
        private IPersonAssignment _personAssignment;
        private IScheduleDictionary _scheduleDictionary;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _dateOnly = new DateOnly(2009, 2, 2);
            _TimeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time"));
            //_scheduleDayUtc = TimeZoneHelper.ConvertToUtc(_dateOnly.Date, _TimeZoneInfo);
            _effectiveRestriction = new EffectiveRestriction(
                new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
                new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0)),
                new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(8, 0, 0)),
                null, null, null, new List<IActivityRestriction>());
            _finderResult = new WorkShiftFinderResult(new Person(), new DateOnly(2009, 2, 3));
            _rules = _mocks.StrictMock<ILongestPeriodForAssignmentCalculator>();
            _target = new ShiftProjectionCacheFilter(_rules);
            _part = _mocks.StrictMock<IScheduleDay>();
            _scheduleRange = _mocks.StrictMock<IScheduleRange>();
            _personAssignment = _mocks.StrictMock<IPersonAssignment>();

            _scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
        	_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
        }

        [Test]
        public void VerifyRestrictionCheck()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            var result = _mocks.StrictMock<IWorkShiftFinderResult>();

            using (_mocks.Record())
            {
                Expect.Call(effectiveRestriction.ShiftCategory).Return(null);
                Expect.Call(effectiveRestriction.IsRotationDay).Return(false);
                Expect.Call(effectiveRestriction.IsAvailabilityDay).Return(false);
                Expect.Call(effectiveRestriction.IsPreferenceDay).Return(false);
                Expect.Call(effectiveRestriction.IsStudentAvailabilityDay).Return(false);
                Expect.Call(() => result.AddFilterResults(null)).IgnoreArguments();
            }

            schedulingOptions.UseRotations = true;
            schedulingOptions.RotationDaysOnly = true;
            schedulingOptions.UsePreferences = true;
            schedulingOptions.PreferencesDaysOnly = true;
            schedulingOptions.UseAvailability = true;
            schedulingOptions.AvailabilityDaysOnly = true;
            schedulingOptions.UseStudentAvailability = true;

            using (_mocks.Playback())
            {
                bool ret = _target.CheckRestrictions(schedulingOptions, effectiveRestriction, result);
                Assert.IsFalse(ret);
            }
        }

        [Test]
        public void ShouldReturnCorrectIfUsePreferenceMustHavesOnly()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            var result = _mocks.StrictMock<IWorkShiftFinderResult>();

            using (_mocks.Record())
            {
                Expect.Call(effectiveRestriction.ShiftCategory).Return(null);
                Expect.Call(effectiveRestriction.IsPreferenceDay).Return(false);
                Expect.Call(() => result.AddFilterResults(null)).IgnoreArguments();
            }

            schedulingOptions.UseRotations = false;
            schedulingOptions.RotationDaysOnly = false;
            schedulingOptions.UsePreferences = true;
            schedulingOptions.PreferencesDaysOnly = false;
            schedulingOptions.UseAvailability = false;
            schedulingOptions.AvailabilityDaysOnly = false;
            schedulingOptions.UseStudentAvailability = false;
            schedulingOptions.UsePreferencesMustHaveOnly = true;

            using (_mocks.Playback())
            {
                bool ret = _target.CheckRestrictions(schedulingOptions, effectiveRestriction, result);
                Assert.IsFalse(ret);
            }
        }

        [Test]
        public void VerifyRestrictionCheckWhenTrue()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            var result = _mocks.StrictMock<IWorkShiftFinderResult>();
            IList<IWorkShiftFilterResult> lstResult = new List<IWorkShiftFilterResult>();
            using (_mocks.Record())
            {
                Expect.Call(effectiveRestriction.ShiftCategory).Return(null);
                Expect.Call(effectiveRestriction.IsRotationDay).Return(true);
            }

            schedulingOptions.UseRotations = true;
            schedulingOptions.RotationDaysOnly = true;
            schedulingOptions.UsePreferences = false;
            schedulingOptions.PreferencesDaysOnly = false;
            schedulingOptions.UseAvailability = false;
            schedulingOptions.AvailabilityDaysOnly = false;
            schedulingOptions.UseStudentAvailability = false;

            using (_mocks.Playback())
            {
                bool ret = _target.CheckRestrictions(schedulingOptions, effectiveRestriction, result);
                Assert.IsTrue(ret);
                Assert.AreEqual(0, lstResult.Count);
            }
        }

        [Test]
        public void VerifyRestrictionCheckWithNullEffectiveReturnsFalse()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions();
            IEffectiveRestriction effectiveRestriction = null;
            var result = _mocks.StrictMock<IWorkShiftFinderResult>();

            using (_mocks.Record())
            {
                Expect.Call(() => result.AddFilterResults(null)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                bool ret = _target.CheckRestrictions(schedulingOptions, effectiveRestriction, result);
                Assert.IsFalse(ret);
            }
        }

        [Test]
        public void CanFilterOnEffectiveRestrictionAndNotAllowedShiftCategories()
        {
            var ret = _target.FilterOnRestrictionAndNotAllowedShiftCategories(_dateOnly, _TimeZoneInfo, GetCashes(), _effectiveRestriction, new List<IShiftCategory>(), _finderResult);
            Assert.AreEqual(0, ret.Count);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void CanFilterOnActivityRestriction()
        {
            IActivity testActivity = ActivityFactory.CreateActivity("test");
            IActivity breakActivity = ActivityFactory.CreateActivity("lunch");
            var breakPeriod = new DateTimePeriod(new DateTime(1800, 1, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 13, 0, 0, DateTimeKind.Utc));


            IWorkShift ws1 = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(8), TimeSpan.FromHours(21), testActivity);
            ws1.LayerCollection.Add(new WorkShiftActivityLayer(breakActivity, breakPeriod));
            IWorkShift ws2 = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(9), TimeSpan.FromHours(22), testActivity);
            ws2.LayerCollection.Add(new WorkShiftActivityLayer(breakActivity, breakPeriod.MovePeriod(new TimeSpan(1, 0, 0))));
            IList<IWorkShift> listOfWorkShifts = new List<IWorkShift> { ws1, ws2 };

            _TimeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            var casheList = new List<IShiftProjectionCache>();
            foreach (IWorkShift shift in listOfWorkShifts)
            {
                var cache = new ShiftProjectionCache(shift, _personalShiftMeetingTimeChecker);
                cache.SetDate(_dateOnly, _TimeZoneInfo);
                casheList.Add(cache);
            }

            IList<IActivityRestriction> activityRestrictions = new List<IActivityRestriction>();
            IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                                  new EndTimeLimitation(),
                                                                                  new WorkTimeLimitation(), null, null, null,
                                                                                  activityRestrictions);
            var ret = ShiftProjectionCacheFilter.FilterOnActivityRestrictions(_dateOnly, _TimeZoneInfo, casheList, effectiveRestriction, _finderResult);
            Assert.AreEqual(2, ret.Count);

            var activityRestriction = new ActivityRestriction(breakActivity)
                                        {
                                            StartTimeLimitation = new StartTimeLimitation(new TimeSpan(12, 0, 0), null),
                                            EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(13, 0, 0))
                                        };
            activityRestrictions = new List<IActivityRestriction> { activityRestriction };
            effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                                  new EndTimeLimitation(),
                                                                                  new WorkTimeLimitation(), null, null, null,
                                                                                  activityRestrictions);

            ret = ShiftProjectionCacheFilter.FilterOnActivityRestrictions(_dateOnly, _TimeZoneInfo, casheList, effectiveRestriction, _finderResult);
            Assert.AreEqual(0, ret.Count);
        }

        [Test]
        public void CanFilterOnEffectiveRestriction()
        {
            var ret = _target.FilterOnRestrictionTimeLimits(_dateOnly, _TimeZoneInfo, GetCashes(), _effectiveRestriction, _finderResult);
            Assert.IsNotNull(ret);
        }
        [Test]
        public void CanFilterOnRestrictionTimeLimitsWithEmptyList()
        {
            var ret = _target.FilterOnRestrictionTimeLimits(_dateOnly, _TimeZoneInfo, new List<IShiftProjectionCache>(), _effectiveRestriction, _finderResult);
            Assert.IsNotNull(ret);
        }

        [Test]
        public void CanFilterOnRestrictionMinMaxWorkTimeWithEmptyList()
        {
            var ret = _target.FilterOnRestrictionMinMaxWorkTime(new List<IShiftProjectionCache>(), _effectiveRestriction, _finderResult);
            Assert.IsNotNull(ret);
        }
        [Test]
        public void CanFilterOnRestrictionMinMaxWorkTimeWithNoRestrictions()
        {
            var effectiveRestriction = new EffectiveRestriction(
                new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
                new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0)),
                new WorkTimeLimitation(null, null),
                null, null, null, new List<IActivityRestriction>());
            var ret = _target.FilterOnRestrictionMinMaxWorkTime(GetCashes(), effectiveRestriction, _finderResult);
            Assert.AreEqual(3, ret.Count);
        }

        [Test]
        public void CanFilterOnCategoryWithEmptyList()
        {
            var ret = _target.FilterOnShiftCategory(_category, new List<IShiftProjectionCache>(), _finderResult);
            Assert.IsNotNull(ret);
        }

        [Test]
        public void CanFilterOnCategoryWithCategoryIsNull()
        {
            var ret = _target.FilterOnShiftCategory(null, GetCashes(), _finderResult);
            Assert.AreEqual(3, ret.Count);
        }

        [Test]
        public void CanFilterOnNotAllowedCategoriesWithEmptyList()
        {
            var ret = _target.FilterOnNotAllowedShiftCategories(new List<IShiftCategory> { _category }, new List<IShiftProjectionCache>(), _finderResult);
            Assert.IsNotNull(ret);
        }

        [Test]
        public void CanFilterOnRestrictionMinMaxWorkTime()
        {
            IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
            _workShift1 = _mocks.StrictMock<IWorkShift>();
            _workShift2 = _mocks.StrictMock<IWorkShift>();
            var mainshift1 = _mocks.StrictMock<IEditableShift>();
			var mainshift2 = _mocks.StrictMock<IEditableShift>();
            var ps1 = _mocks.StrictMock<IProjectionService>();
            var ps2 = _mocks.StrictMock<IProjectionService>();
            var lc1 = _mocks.StrictMock<IVisualLayerCollection>();
            var lc2 = _mocks.StrictMock<IVisualLayerCollection>();

            using (_mocks.Record())
            {
                Expect.Call(_workShift1.ToEditorShift(new DateTime(2009, 1, 1), _TimeZoneInfo)).Return(mainshift1);
                Expect.Call(_workShift2.ToEditorShift(new DateTime(2009, 1, 1), _TimeZoneInfo)).Return(mainshift2);
                Expect.Call(_workShift1.ProjectionService()).Return(ps1);
                Expect.Call(_workShift2.ProjectionService()).Return(ps2);
                Expect.Call(ps1.CreateProjection()).Return(lc1);
                Expect.Call(ps2.CreateProjection()).Return(lc2);

                Expect.Call(lc1.ContractTime()).Return(new TimeSpan(7, 0, 0));
                Expect.Call(lc2.ContractTime()).Return(new TimeSpan(10, 0, 0));
            }

            IList<IShiftProjectionCache> retShifts;
            ShiftProjectionCache c1;
            ShiftProjectionCache c2;

            using (_mocks.Playback())
            {
                c1 = new ShiftProjectionCache(_workShift1, _personalShiftMeetingTimeChecker);
                c1.SetDate(new DateOnly(2009, 1, 1), _TimeZoneInfo);
                shifts.Add(c1);
                c2 = new ShiftProjectionCache(_workShift2, _personalShiftMeetingTimeChecker);
                c2.SetDate(new DateOnly(2009, 1, 1), _TimeZoneInfo);
                shifts.Add(c2);
                retShifts = _target.FilterOnRestrictionMinMaxWorkTime(shifts, _effectiveRestriction, new WorkShiftFinderResultForTest());

            }
            retShifts.Should().Contain(c1);
            retShifts.Count.Should().Be.EqualTo(1);
        }

        [Test]
        public void CanFilterOnContractTime()
        {
            IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
            _workShift1 = _mocks.StrictMock<IWorkShift>();
            _workShift2 = _mocks.StrictMock<IWorkShift>();
			var mainshift1 = _mocks.StrictMock<IEditableShift>();
			var mainshift2 = _mocks.StrictMock<IEditableShift>();
            var ps1 = _mocks.StrictMock<IProjectionService>();
            var ps2 = _mocks.StrictMock<IProjectionService>();
            var lc1 = _mocks.StrictMock<IVisualLayerCollection>();
            var lc2 = _mocks.StrictMock<IVisualLayerCollection>();

            var minMaxcontractTime = new MinMax<TimeSpan>(new TimeSpan(7, 0, 0), new TimeSpan(8, 0, 0));
            using (_mocks.Record())
            {
                Expect.Call(_workShift1.ToEditorShift(new DateTime(2009, 1, 1), _TimeZoneInfo)).Return(mainshift1);
                Expect.Call(_workShift2.ToEditorShift(new DateTime(2009, 1, 1), _TimeZoneInfo)).Return(mainshift2);
                Expect.Call(_workShift1.ProjectionService()).Return(ps1);
                Expect.Call(_workShift2.ProjectionService()).Return(ps2);
                Expect.Call(ps1.CreateProjection()).Return(lc1);
                Expect.Call(ps2.CreateProjection()).Return(lc2);

                Expect.Call(lc1.ContractTime()).Return(new TimeSpan(7, 0, 0));
                Expect.Call(lc2.ContractTime()).Return(new TimeSpan(10, 0, 0));
            }

            IList<IShiftProjectionCache> retShifts;
            ShiftProjectionCache c1;
            ShiftProjectionCache c2;

            using (_mocks.Playback())
            {
                c1 = new ShiftProjectionCache(_workShift1, _personalShiftMeetingTimeChecker);
                c1.SetDate(new DateOnly(2009, 1, 1), _TimeZoneInfo);
                shifts.Add(c1);
                c2 = new ShiftProjectionCache(_workShift2, _personalShiftMeetingTimeChecker);
                c2.SetDate(new DateOnly(2009, 1, 1), _TimeZoneInfo);
                shifts.Add(c2);
                retShifts = _target.FilterOnContractTime(minMaxcontractTime, shifts, new WorkShiftFinderResultForTest());

            }
            retShifts.Should().Contain(c1);
            retShifts.Count.Should().Be.EqualTo(1);
        }

        [Test]
        public void CanFilterOnShiftCategories()
        {
            IShiftCategory shiftCategory1 = new ShiftCategory("wanted");
            IShiftCategory shiftCategory2 = new ShiftCategory("not wanted");

            var workShift1 = _mocks.StrictMock<IWorkShift>();
            var workShift2 = _mocks.StrictMock<IWorkShift>();
            var workShift3 = _mocks.StrictMock<IWorkShift>();

            var cache1 = _mocks.StrictMock<IShiftProjectionCache>();
            var cache2 = _mocks.StrictMock<IShiftProjectionCache>();
            var cache3 = _mocks.StrictMock<IShiftProjectionCache>();

            IList<IShiftProjectionCache> caches = new List<IShiftProjectionCache> { cache1, cache2, cache3 };
            IWorkShiftFinderResult finderResult = new WorkShiftFinderResultForTest();
            using (_mocks.Record())
            {
                Expect.Call(cache1.TheWorkShift).Return(workShift1).Repeat.AtLeastOnce();
                Expect.Call(cache2.TheWorkShift).Return(workShift2).Repeat.AtLeastOnce();
                Expect.Call(cache3.TheWorkShift).Return(workShift3).Repeat.AtLeastOnce();

                Expect.Call(workShift1.ShiftCategory).Return(shiftCategory1).Repeat.AtLeastOnce();
                Expect.Call(workShift2.ShiftCategory).Return(shiftCategory2).Repeat.AtLeastOnce();
                Expect.Call(workShift3.ShiftCategory).Return(shiftCategory2).Repeat.AtLeastOnce();

            }

            using (_mocks.Playback())
            {
                var ret = _target.FilterOnShiftCategory(shiftCategory1, caches, finderResult);
                Assert.AreEqual(1, ret.Count);
                Assert.AreEqual(shiftCategory1, ret[0].TheWorkShift.ShiftCategory);
                ret = _target.FilterOnShiftCategory(shiftCategory2, caches, finderResult);
                Assert.AreEqual(2, ret.Count);
            }
        }

        [Test]
        public void CanFilterOnNotAllowedShiftCategories()
        {
            IShiftCategory shiftCategory1 = new ShiftCategory("allowed");
            IShiftCategory shiftCategory2 = new ShiftCategory("not allowed");
            IShiftCategory shiftCategory3 = new ShiftCategory("not allowed 2");

            var workShift1 = _mocks.StrictMock<IWorkShift>();
            var workShift2 = _mocks.StrictMock<IWorkShift>();
            var workShift3 = _mocks.StrictMock<IWorkShift>();

            var cache1 = _mocks.StrictMock<IShiftProjectionCache>();
            var cache2 = _mocks.StrictMock<IShiftProjectionCache>();
            var cache3 = _mocks.StrictMock<IShiftProjectionCache>();

            IList<IShiftProjectionCache> caches = new List<IShiftProjectionCache> { cache1, cache2, cache3 };
            IList<IShiftCategory> categoriesNotAllowed = new List<IShiftCategory> { shiftCategory2, shiftCategory3 };
            IWorkShiftFinderResult finderResult = new WorkShiftFinderResultForTest();
            using (_mocks.Record())
            {
                Expect.Call(cache1.TheWorkShift).Return(workShift1).Repeat.AtLeastOnce();
                Expect.Call(cache2.TheWorkShift).Return(workShift2).Repeat.AtLeastOnce();
                Expect.Call(cache3.TheWorkShift).Return(workShift3).Repeat.AtLeastOnce();

                Expect.Call(workShift1.ShiftCategory).Return(shiftCategory1).Repeat.AtLeastOnce();
                Expect.Call(workShift2.ShiftCategory).Return(shiftCategory2).Repeat.AtLeastOnce();
                Expect.Call(workShift3.ShiftCategory).Return(shiftCategory3).Repeat.AtLeastOnce();

            }

            using (_mocks.Playback())
            {
                var ret = _target.FilterOnNotAllowedShiftCategories(categoriesNotAllowed, caches, finderResult);
                Assert.AreEqual(1, ret.Count);
                Assert.AreEqual(shiftCategory1, ret[0].TheWorkShift.ShiftCategory);
                ret = _target.FilterOnNotAllowedShiftCategories(new List<IShiftCategory>(), caches, finderResult);
                Assert.AreEqual(3, ret.Count);
            }
        }

        [Test]
        public void FilterBusinessRulesReturnsEmptyListWhenPeriodIsNull()
        {
            using (_mocks.Record())
            {
                Expect.Call(_rules.PossiblePeriod(_scheduleRange, _dateOnly)).Return(null);
            }
            using (_mocks.Playback())
            {
                var ret = _target.FilterOnBusinessRules(_scheduleRange, GetCashes(), _dateOnly, _finderResult);
                Assert.AreEqual(0, ret.Count);
            }
        }

        [Test]
        public void CanFilterOnBusinessRules()
        {
            var startTime = new DateTime(2009, 2, 2, 11, 0, 0, DateTimeKind.Utc);
            var endTime = new DateTime(2009, 2, 2, 21, 0, 0, DateTimeKind.Utc);

            var scheduleDayPeriod = new DateTimePeriod(startTime, endTime);
            using (_mocks.Record())
            {
                Expect.Call(_rules.PossiblePeriod(_scheduleRange, _dateOnly)).Return(scheduleDayPeriod);
            }
            using (_mocks.Playback())
            {
                var ret = _target.FilterOnBusinessRules(_scheduleRange, GetCashes(), _dateOnly, _finderResult);
                Assert.AreEqual(2, ret.Count);
            }
        }

        [Test]
        public void CanFilterOnBusinessRulesThroughGroupOfPersons()
        {
            var person1 = new Person();
            var person2 = new Person();
            var persons = new List<IPerson> { person1, person2 };

            var startTime = new DateTime(2009, 2, 2, 11, 0, 0, DateTimeKind.Utc);
            var endTime = new DateTime(2009, 2, 2, 21, 0, 0, DateTimeKind.Utc);

            var scheduleDayPeriod = new DateTimePeriod(startTime, endTime);

            Expect.Call(_scheduleDictionary[person1]).Return(_scheduleRange);
            Expect.Call(_scheduleDictionary[person2]).Return(_scheduleRange);

            Expect.Call(_rules.PossiblePeriod(_scheduleRange, _dateOnly)).Return(scheduleDayPeriod).Repeat.Twice();
            _mocks.ReplayAll();
            var ret = _target.FilterOnBusinessRules(persons, _scheduleDictionary, _dateOnly, GetCashes(), _finderResult);
            Assert.AreEqual(2, ret.Count);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyFilterOnStartAndEndTime()
        {
            var scheduleDayPeriod = new DateTimePeriod(2009, 1, 1, 2009, 1, 2);
            IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
            _workShift1 = _mocks.StrictMock<IWorkShift>();
            _workShift2 = _mocks.StrictMock<IWorkShift>();
			var mainshift1 = _mocks.StrictMock<IEditableShift>();
			var mainshift2 = _mocks.StrictMock<IEditableShift>();
            var ps1 = _mocks.StrictMock<IProjectionService>();
            var ps2 = _mocks.StrictMock<IProjectionService>();
            var lc1 = _mocks.StrictMock<IVisualLayerCollection>();
            var lc2 = _mocks.StrictMock<IVisualLayerCollection>();


            using (_mocks.Record())
            {
                Expect.Call(_workShift1.ToEditorShift(new DateTime(2009, 1, 1), _TimeZoneInfo)).Return(mainshift1);
                Expect.Call(_workShift2.ToEditorShift(new DateTime(2009, 1, 1), _TimeZoneInfo)).Return(mainshift2);
                Expect.Call(mainshift1.ProjectionService()).Return(ps1);
                Expect.Call(mainshift2.ProjectionService()).Return(ps2);
                Expect.Call(ps1.CreateProjection()).Return(lc1);
                Expect.Call(ps2.CreateProjection()).Return(lc2);
                Expect.Call(lc1.Period()).Return(scheduleDayPeriod).Repeat.Twice();
                Expect.Call(lc2.Period()).Return(scheduleDayPeriod.MovePeriod(TimeSpan.FromMinutes(1))).Repeat.Twice();

            }

            IList<IShiftProjectionCache> retShifts;
            ShiftProjectionCache c1;
            ShiftProjectionCache c2;

            using (_mocks.Playback())
            {
                c1 = new ShiftProjectionCache(_workShift1, _personalShiftMeetingTimeChecker);
                c1.SetDate(new DateOnly(2009, 1, 1), _TimeZoneInfo);
                shifts.Add(c1);
                c2 = new ShiftProjectionCache(_workShift2, _personalShiftMeetingTimeChecker);
                c2.SetDate(new DateOnly(2009, 1, 1), _TimeZoneInfo);
                shifts.Add(c2);
                retShifts = _target.FilterOnStartAndEndTime(scheduleDayPeriod, shifts, new WorkShiftFinderResultForTest());

            }

            retShifts.Should().Contain(c1);
            retShifts.Count.Should().Be.EqualTo(1);

        }
        [Test]
        public void CanGetMaximumPeriodForMeetings()
        {
            var resultPeriod = new DateTimePeriod(new DateTime(2009, 2, 2, 10, 0, 0, DateTimeKind.Utc),
                                new DateTime(2009, 2, 2, 14, 30, 0, DateTimeKind.Utc));

            ReadOnlyCollection<IPersonMeeting> meetings = GetMeetings();

            using (_mocks.Record())
            {
                Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
                Expect.Call(_personAssignment.PersonalActivities()).Return(Enumerable.Empty<IPersonalShiftLayer>()).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                var retPeriod = _target.GetMaximumPeriodForPersonalShiftsAndMeetings(_part);
                Assert.AreEqual(resultPeriod, retPeriod);
            }

        }

        [Test]
        public void CanGetMaximumPeriodForPersonalShifts()
        {
            var resultPeriod = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
                                new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc));

            var period = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2009, 2, 2, 9, 30, 0, DateTimeKind.Utc));
            var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc),
                                             new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc));


            var retList = new List<IPersonMeeting>();
            var meetings = new ReadOnlyCollection<IPersonMeeting>(retList);

            using (_mocks.Record())
            {
                Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
	            Expect.Call(_personAssignment.PersonalActivities())
	                  .Return(new []
		                  {
			                  new PersonalShiftLayer(new Activity("sdf"), period), 
			                  new PersonalShiftLayer(new Activity("sdf"), period2)
		                  })
	                  .Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                var retPeriod = _target.GetMaximumPeriodForPersonalShiftsAndMeetings(_part);
                Assert.AreEqual(resultPeriod, retPeriod);
            }

        }

        [Test]
        public void CanGetMaximumPeriodForPersonalShiftsAndMeetings()
        {
            var resultPeriod = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
                                new DateTime(2009, 2, 2, 14, 30, 0, DateTimeKind.Utc));

            var period = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2009, 2, 2, 9, 30, 0, DateTimeKind.Utc));
            var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc),
                                             new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc));


            var meetings = GetMeetings();

            using (_mocks.Record())
            {
                Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
                Expect.Call(_personAssignment.PersonalActivities()).Return(new[]
	                {
		                new PersonalShiftLayer(new Activity("d"), period), 
		                new PersonalShiftLayer(new Activity("d"), period2)
	                }).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                var retPeriod = _target.GetMaximumPeriodForPersonalShiftsAndMeetings(_part);
                Assert.AreEqual(resultPeriod, retPeriod);
            }

        }

        [Test]
        public void GetMaximumPeriodForPersonalShiftsAndMeetingsReturnsNullWhenEmpty()
        {
            var retList = new List<IPersonMeeting>();
            var meetings = new ReadOnlyCollection<IPersonMeeting>(retList);

            using (_mocks.Record())
            {
                Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonAssignment()).Return(null);
            }

            using (_mocks.Playback())
            {
                var retPeriod = _target.GetMaximumPeriodForPersonalShiftsAndMeetings(_part);
                Assert.IsNull(retPeriod);
            }
        }

        [Test]
        public void ShouldCheckIfCategoryInRestrictionConflictsWithOptions()
        {
            var effective = _mocks.StrictMock<IEffectiveRestriction>();
            _category = new ShiftCategory("effCat");
            _category.SetId(Guid.NewGuid());
            IShiftCategory category = new ShiftCategory("optCat");
            category.SetId(Guid.NewGuid());
            var options = _mocks.StrictMock<ISchedulingOptions>();

            Expect.Call(effective.ShiftCategory).Return(_category).Repeat.Twice();
            Expect.Call(options.ShiftCategory).Return(category).Repeat.Twice();

            _mocks.ReplayAll();
            var ret = _target.CheckRestrictions(options, effective, _finderResult);
            Assert.That(ret, Is.False);
            Assert.That(_finderResult.FilterResults.Count, Is.GreaterThan(0));
            _mocks.VerifyAll();

        }

        private ReadOnlyCollection<IPersonMeeting> GetMeetings()
        {
            var period = new DateTimePeriod(new DateTime(2009, 2, 2, 10, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2009, 2, 2, 10, 30, 0, DateTimeKind.Utc));
            var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc),
                                             new DateTime(2009, 2, 2, 14, 30, 0, DateTimeKind.Utc));

            var retList = new List<IPersonMeeting>();
            var meetingPerson = _mocks.StrictMock<IMeetingPerson>();
            var meeting = _mocks.StrictMock<IMeeting>();
            var personMeeting = new PersonMeeting(meeting, meetingPerson, period);
            var personMeeting2 = new PersonMeeting(meeting, meetingPerson, period2);
            retList.Add(personMeeting);
            retList.Add(personMeeting2);
            return new ReadOnlyCollection<IPersonMeeting>(retList);
        }

        private IList<IShiftProjectionCache> GetCashes()
        {
            var tmpList = GetWorkShifts();
            var retList = new List<IShiftProjectionCache>();
            foreach (IWorkShift shift in tmpList)
            {
                var cache = new ShiftProjectionCache(shift, _personalShiftMeetingTimeChecker);
                cache.SetDate(_dateOnly, _TimeZoneInfo);
                retList.Add(cache);
            }
            return retList;
        }

        private IEnumerable<IWorkShift> GetWorkShifts()
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

        private IShiftProjectionCache _cashe1;
        private IShiftProjectionCache _cashe2;
        private IShiftProjectionCache _cashe3;
        private IShiftProjectionCache _cashe4;

        readonly TimeSpan _start1 = TimeSpan.FromHours(8);
		readonly TimeSpan _start2 = TimeSpan.FromHours(7);
		readonly TimeSpan _start3 = TimeSpan.FromHours(8);
		readonly TimeSpan _start4 = TimeSpan.FromHours(10);

		readonly TimeSpan _end1 = TimeSpan.FromHours(15);
		readonly TimeSpan _end2 = TimeSpan.FromHours(16);
		readonly TimeSpan _end3 = TimeSpan.FromHours(17);
		readonly TimeSpan _end4 = TimeSpan.FromHours(16);

        private IList<IShiftProjectionCache> getCashes()
        {
            _cashe1 = _mocks.DynamicMock<IShiftProjectionCache>();
            _cashe2 = _mocks.DynamicMock<IShiftProjectionCache>();
            _cashe3 = _mocks.DynamicMock<IShiftProjectionCache>();
            _cashe4 = _mocks.DynamicMock<IShiftProjectionCache>();
            return new List<IShiftProjectionCache> { _cashe1, _cashe2, _cashe3, _cashe4 };
        }

        [Test]
        public void CanFilterOutShiftsWhichCannotBeOverwritten()
        {
            _personAssignment = _mocks.StrictMock<IPersonAssignment>();
            
            var currentDate = new DateTime(2009, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            var lunch = ActivityFactory.CreateActivity("lunch");
            lunch.AllowOverwrite = false;
            IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
            var readOnlymeetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>());
            var c1 = _mocks.StrictMock<IShiftProjectionCache>();
            shifts.Add(c1);
            Expect.Call(c1.MainShiftProjection).Return(new VisualLayerCollection(null, GetLunchLayer(currentDate, lunch), new ProjectionPayloadMerger())).Repeat.AtLeastOnce();
            Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
            Expect.Call(_part.PersonMeetingCollection()).Return(readOnlymeetings).Repeat.AtLeastOnce();
						Expect.Call(_personAssignment.PersonalActivities()).Return(GetPersonalLayers(currentDate)).Repeat.AtLeastOnce();
            _mocks.ReplayAll();
            var retShifts = _target.FilterOnNotOverWritableActivities(shifts,_part,  _finderResult);
            retShifts.Count.Should().Be.EqualTo(0);
            _mocks.VerifyAll();
        }

		[Test]
		public void ShouldNotFilterIfNoMeetingOrPersonAssignment()
		{
			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.AllowOverwrite = false;
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var readOnlymeetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>());
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);
			
			Expect.Call(_part.PersonAssignment()).Return(null).Repeat.AtLeastOnce();
			Expect.Call(_part.PersonMeetingCollection()).Return(readOnlymeetings).Repeat.AtLeastOnce();
			
			_mocks.ReplayAll();
			var retShifts = _target.FilterOnNotOverWritableActivities(shifts, _part, _finderResult);
			retShifts.Count.Should().Be.EqualTo(1);
			_mocks.VerifyAll();
		}

        private static IEnumerable<IPersonalShiftLayer> GetPersonalLayers(DateTime currentDate)
        {
            var personalLayers = new []
                                     {
                                         new PersonalShiftLayer(ActivityFactory.CreateActivity("personal"),
                                                                        new DateTimePeriod(currentDate.AddHours(10),
                                                                                           currentDate.AddHours(13)))
                                     };
            return personalLayers;
        }

        private static List<IVisualLayer> GetLunchLayer(DateTime currentDate, Activity lunch)
        {
            var lunchLayer = new List<IVisualLayer>
                                 {
                                     new VisualLayer(lunch,
                                                     new DateTimePeriod(currentDate.AddHours(11), currentDate.AddHours(12)),
                                                     lunch, null)
                                 };
            return lunchLayer;
        }

        [Test]
        public void VerifyIfPersonalShiftCannotOverrideActivity()
        {
            _personAssignment = _mocks.StrictMock<IPersonAssignment>();
            
            var currentDate = new DateTime(2009, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            var lunch = ActivityFactory.CreateActivity("lunch");
            lunch.AllowOverwrite = true;
            var lunchLayer = new List<IVisualLayer>
                                 {
                                     new VisualLayer(lunch, new DateTimePeriod(currentDate.AddHours(11), currentDate.AddHours(12)),
                                                     lunch, null)
                                 };
            var layerCollection1 = new VisualLayerCollection(null, lunchLayer, new ProjectionPayloadMerger());
            
            IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
            var meeting = _mocks.StrictMock<IPersonMeeting>();
            var readOnlymeetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> {meeting});
            var c1 = _mocks.StrictMock<IShiftProjectionCache>();
            shifts.Add(c1);
            Expect.Call(c1.MainShiftProjection).Return(layerCollection1).Repeat.AtLeastOnce();
            Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
            Expect.Call(_part.PersonMeetingCollection()).Return(readOnlymeetings).Repeat.AtLeastOnce();
            _mocks.ReplayAll();
            var retShifts = _target.FilterOnNotOverWritableActivities(shifts, _part, _finderResult);
            retShifts.Count.Should().Be.EqualTo(1);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCoverMeetingAndPersonalShiftsWhenItIsPossible()
        {
            var period = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2009, 2, 2, 9, 30, 0, DateTimeKind.Utc));
            var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc),
                                             new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc));
            
            var meeting = _mocks.StrictMock<IPersonMeeting>();
            var meetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { meeting });
            _personAssignment = _mocks.StrictMock<IPersonAssignment>();

            var currentDate = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            var phone = ActivityFactory.CreateActivity("phone");
            phone.AllowOverwrite = true;
            phone.InWorkTime = true;
            var phoneLayer = new List<IVisualLayer>
                                 {
                                     new VisualLayer(phone, new DateTimePeriod(currentDate.AddHours(8), currentDate.AddHours(17)),
                                                     phone, null)
                                 };
            var layerCollection1 = new VisualLayerCollection(null, phoneLayer, new ProjectionPayloadMerger());

            IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
            var c1 = _mocks.StrictMock<IShiftProjectionCache>();
            shifts.Add(c1);

            using (_mocks.Record())
            {
                Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
                Expect.Call(_personAssignment.PersonalActivities()).Return(new[]
	                {
		                new PersonalShiftLayer(new Activity("d"), period)
	                }).Repeat.AtLeastOnce();
                Expect.Call(meeting.Period).Return(period2).Repeat.AtLeastOnce();
                Expect.Call(c1.MainShiftProjection).Return(layerCollection1).Repeat.AtLeastOnce();
                Expect.Call(c1.PersonalShiftsAndMeetingsAreInWorkTime(new ReadOnlyCollection<IPersonMeeting>(meetings),
																	  _personAssignment)).Return(true);
            }

            using (_mocks.Playback())
            {
                var shiftsList = _target.FilterOnPersonalShifts(shifts, _part, _finderResult);
                Assert.That(shiftsList.Count, Is.EqualTo(1));
            }
        }


        [Test]
        public void ShouldGetNoShiftWhenNoMainShiftCanCoverMeetingAndPersonalShifts()
        {
            var period = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2009, 2, 2, 9, 30, 0, DateTimeKind.Utc));
            var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc),
                                             new DateTime(2009, 2, 2, 17, 15, 0, DateTimeKind.Utc));

            var meeting = _mocks.StrictMock<IPersonMeeting>();
            var meetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { meeting });
            _personAssignment = _mocks.StrictMock<IPersonAssignment>();

            var currentDate = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            var phone = ActivityFactory.CreateActivity("phone");
            phone.AllowOverwrite = true;
            phone.InWorkTime = true;
            var phoneLayer = new List<IVisualLayer>
                                 {
                                     new VisualLayer(phone, new DateTimePeriod(currentDate.AddHours(8), currentDate.AddHours(17)),
                                                     phone, null)
                                 };
            var layerCollection1 = new VisualLayerCollection(null, phoneLayer, new ProjectionPayloadMerger());

            IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
            var c1 = _mocks.StrictMock<IShiftProjectionCache>();
            shifts.Add(c1);

            using (_mocks.Record())
            {
                Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
                Expect.Call(_personAssignment.PersonalActivities()).Return(new[]
	                {
		                new PersonalShiftLayer(new Activity("d"), period)
	                }).Repeat.AtLeastOnce();
                Expect.Call(meeting.Period).Return(period2).Repeat.AtLeastOnce();
                Expect.Call(c1.MainShiftProjection).Return(layerCollection1).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                var shiftsList = _target.FilterOnPersonalShifts(shifts, _part, _finderResult);
                Assert.That(shiftsList.Count, Is.EqualTo(0));
            }
        }
    }
}

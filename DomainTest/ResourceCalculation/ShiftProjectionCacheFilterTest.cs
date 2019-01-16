using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
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
        private MockRepository _mocks;
        private TimeZoneInfo _timeZoneInfo;
        private IScheduleRange _scheduleRange;
	    private IPersonalShiftAndMeetingFilter _personalShiftAndMeetingFilter;
		private INotOverWritableActivitiesShiftFilter _notOverWritableActivitiesShiftFilter;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _dateOnly = new DateOnly(2009, 2, 2);
            _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
            _effectiveRestriction = new EffectiveRestriction(
                new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
                new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0)),
                new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(8, 0, 0)),
                null, null, null, new List<IActivityRestriction>());
            _rules = _mocks.StrictMock<ILongestPeriodForAssignmentCalculator>();
	        _personalShiftAndMeetingFilter = _mocks.StrictMock<IPersonalShiftAndMeetingFilter>();
	        _notOverWritableActivitiesShiftFilter = _mocks.StrictMock<INotOverWritableActivitiesShiftFilter>();
			_target = new ShiftProjectionCacheFilter(_rules, _personalShiftAndMeetingFilter, _notOverWritableActivitiesShiftFilter);
            _scheduleRange = _mocks.StrictMock<IScheduleRange>();
        }

        [Test]
        public void VerifyRestrictionCheck()
        {
            var schedulingOptions = new SchedulingOptions();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();

            using (_mocks.Record())
            {
                Expect.Call(effectiveRestriction.ShiftCategory).Return(null);
                Expect.Call(effectiveRestriction.IsRotationDay).Return(false);
                Expect.Call(effectiveRestriction.IsAvailabilityDay).Return(false);
                Expect.Call(effectiveRestriction.IsPreferenceDay).Return(false);
                Expect.Call(effectiveRestriction.IsStudentAvailabilityDay).Return(false);
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
                bool ret = _target.CheckRestrictions(schedulingOptions, effectiveRestriction);
                Assert.IsFalse(ret);
            }
        }

        [Test]
        public void ShouldReturnCorrectIfUsePreferenceMustHavesOnly()
        {
            SchedulingOptions schedulingOptions = new SchedulingOptions();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();

			using (_mocks.Record())
            {
                Expect.Call(effectiveRestriction.ShiftCategory).Return(null);
                Expect.Call(effectiveRestriction.IsPreferenceDay).Return(false);
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
                bool ret = _target.CheckRestrictions(schedulingOptions, effectiveRestriction);
				Assert.IsFalse(ret);
            }
        }

        [Test]
        public void VerifyRestrictionCheckWhenTrue()
        {
            SchedulingOptions schedulingOptions = new SchedulingOptions();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
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
                bool ret = _target.CheckRestrictions(schedulingOptions, effectiveRestriction);
                Assert.IsTrue(ret);
            }
        }

	    [Test]
	    public void VerifyRestrictionCheckWithNullEffectiveReturnsFalse()
	    {
		    SchedulingOptions schedulingOptions = new SchedulingOptions();
		    IEffectiveRestriction effectiveRestriction = null;

		    bool ret = _target.CheckRestrictions(schedulingOptions, effectiveRestriction);
		    Assert.IsFalse(ret);
	    }

	    [Test]
        public void CanFilterOnEffectiveRestrictionAndNotAllowedShiftCategories()
        {
            var ret = _target.FilterOnRestrictionAndNotAllowedShiftCategories(_dateOnly, _timeZoneInfo, GetCashes(), _effectiveRestriction, new HashSet<IShiftCategory>());
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

            var dateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(_dateOnly, _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            var casheList = new List<ShiftProjectionCache>();
            foreach (IWorkShift shift in listOfWorkShifts)
            {
                var cache = new ShiftProjectionCache(shift, dateOnlyAsPeriod);
                casheList.Add(cache);
            }

            IList<IActivityRestriction> activityRestrictions = new List<IActivityRestriction>();
            IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                                  new EndTimeLimitation(),
                                                                                  new WorkTimeLimitation(), null, null, null,
                                                                                  activityRestrictions);
            var ret = ShiftProjectionCacheFilter.FilterOnActivityRestrictions(_dateOnly, _timeZoneInfo, casheList, effectiveRestriction);
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

            ret = ShiftProjectionCacheFilter.FilterOnActivityRestrictions(_dateOnly, _timeZoneInfo, casheList, effectiveRestriction);
            Assert.AreEqual(0, ret.Count);
        }

        [Test]
        public void CanFilterOnEffectiveRestriction()
        {
            var ret = _target.FilterOnRestrictionTimeLimits(_dateOnly, _timeZoneInfo, GetCashes(), _effectiveRestriction);
            Assert.IsNotNull(ret);
        }
        [Test]
        public void CanFilterOnRestrictionTimeLimitsWithEmptyList()
        {
            var ret = _target.FilterOnRestrictionTimeLimits(_dateOnly, _timeZoneInfo, new List<ShiftProjectionCache>(), _effectiveRestriction);
            Assert.IsNotNull(ret);
        }

        [Test]
        public void CanFilterOnRestrictionMinMaxWorkTimeWithEmptyList()
        {
            var ret = _target.FilterOnRestrictionMinMaxWorkTime(new List<ShiftProjectionCache>(), _effectiveRestriction);
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
            var ret = _target.FilterOnRestrictionMinMaxWorkTime(GetCashes(), effectiveRestriction);
            Assert.AreEqual(3, ret.Count);
        }

        [Test]
        public void CanFilterOnCategoryWithEmptyList()
        {
            var ret = _target.FilterOnShiftCategory(_category, new List<ShiftProjectionCache>());
            Assert.IsNotNull(ret);
        }

        [Test]
        public void CanFilterOnCategoryWithCategoryIsNull()
        {
            var ret = _target.FilterOnShiftCategory(null, GetCashes());
            Assert.AreEqual(3, ret.Count);
        }

        [Test]
        public void CanFilterOnNotAllowedCategoriesWithEmptyList()
        {
            var ret = _target.FilterOnNotAllowedShiftCategories(new HashSet<IShiftCategory> { _category }, new List<ShiftProjectionCache>());
            Assert.IsNotNull(ret);
        }

        [Test]
        public void CanFilterOnRestrictionMinMaxWorkTime()
        {
            IList<ShiftProjectionCache> shifts = new List<ShiftProjectionCache>();
            _workShift1 = _mocks.StrictMock<IWorkShift>();
            _workShift2 = _mocks.StrictMock<IWorkShift>();
			
            var lc1 = _mocks.StrictMock<IVisualLayerCollection>();
            var lc2 = _mocks.StrictMock<IVisualLayerCollection>();

            using (_mocks.Record())
            {
                Expect.Call(_workShift1.Projection).Return(lc1);
                Expect.Call(_workShift2.Projection).Return(lc2);

                Expect.Call(lc1.ContractTime()).Return(new TimeSpan(7, 0, 0));
                Expect.Call(lc2.ContractTime()).Return(new TimeSpan(10, 0, 0));
            }

            IList<ShiftProjectionCache> retShifts;
            ShiftProjectionCache c1;
            ShiftProjectionCache c2;

            using (_mocks.Playback())
			{
				var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 1, 1), _timeZoneInfo);
				c1 = new ShiftProjectionCache(_workShift1, dateOnlyAsDateTimePeriod);
                shifts.Add(c1);
                c2 = new ShiftProjectionCache(_workShift2, dateOnlyAsDateTimePeriod);
                shifts.Add(c2);
                retShifts = _target.FilterOnRestrictionMinMaxWorkTime(shifts, _effectiveRestriction);

            }
            retShifts.Should().Contain(c1);
            retShifts.Count.Should().Be.EqualTo(1);
        }

        [Test]
        public void CanFilterOnContractTime()
        {
            IList<ShiftProjectionCache> shifts = new List<ShiftProjectionCache>();
            _workShift1 = _mocks.StrictMock<IWorkShift>();
            _workShift2 = _mocks.StrictMock<IWorkShift>();
            var lc1 = _mocks.StrictMock<IVisualLayerCollection>();
            var lc2 = _mocks.StrictMock<IVisualLayerCollection>();

            var minMaxcontractTime = new MinMax<TimeSpan>(new TimeSpan(7, 0, 0), new TimeSpan(8, 0, 0));
            using (_mocks.Record())
            {
                Expect.Call(_workShift1.Projection).Return(lc1);
                Expect.Call(_workShift2.Projection).Return(lc2);

                Expect.Call(lc1.ContractTime()).Return(new TimeSpan(7, 0, 0));
                Expect.Call(lc2.ContractTime()).Return(new TimeSpan(10, 0, 0));
            }

            IList<ShiftProjectionCache> retShifts;
            ShiftProjectionCache c1;
            ShiftProjectionCache c2;

            using (_mocks.Playback())
            {
				var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 1, 1), _timeZoneInfo);
				c1 = new ShiftProjectionCache(_workShift1, dateOnlyAsDateTimePeriod);
                shifts.Add(c1);
                c2 = new ShiftProjectionCache(_workShift2, dateOnlyAsDateTimePeriod);
                shifts.Add(c2);
                retShifts = _target.FilterOnContractTime(minMaxcontractTime, shifts);

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

	        var cache1 = new ShiftProjectionCache(workShift1, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
	        var cache2 = new ShiftProjectionCache(workShift2, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
	        var cache3 = new ShiftProjectionCache(workShift3, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
            
            IList<ShiftProjectionCache> caches = new List<ShiftProjectionCache> { cache1, cache2, cache3 };
            using (_mocks.Record())
            {
                Expect.Call(workShift1.ShiftCategory).Return(shiftCategory1).Repeat.AtLeastOnce();
                Expect.Call(workShift2.ShiftCategory).Return(shiftCategory2).Repeat.AtLeastOnce();
                Expect.Call(workShift3.ShiftCategory).Return(shiftCategory2).Repeat.AtLeastOnce();

            }

            using (_mocks.Playback())
            {
                var ret = _target.FilterOnShiftCategory(shiftCategory1, caches);
                Assert.AreEqual(1, ret.Count);
                Assert.AreEqual(shiftCategory1, ret[0].TheWorkShift.ShiftCategory);
                ret = _target.FilterOnShiftCategory(shiftCategory2, caches);
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

	        var cache1 = new ShiftProjectionCache(workShift1, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
	        var cache2 = new ShiftProjectionCache(workShift2, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
	        var cache3 = new ShiftProjectionCache(workShift3, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));

            var caches = new List<ShiftProjectionCache> { cache1, cache2, cache3 };
            var categoriesNotAllowed = new HashSet<IShiftCategory> { shiftCategory2, shiftCategory3 };
			using (_mocks.Record())
            {
                Expect.Call(workShift1.ShiftCategory).Return(shiftCategory1).Repeat.AtLeastOnce();
                Expect.Call(workShift2.ShiftCategory).Return(shiftCategory2).Repeat.AtLeastOnce();
                Expect.Call(workShift3.ShiftCategory).Return(shiftCategory3).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                var ret = _target.FilterOnNotAllowedShiftCategories(categoriesNotAllowed, caches);
                Assert.AreEqual(1, ret.Count);
                Assert.AreEqual(shiftCategory1, ret[0].TheWorkShift.ShiftCategory);
                ret = _target.FilterOnNotAllowedShiftCategories(new HashSet<IShiftCategory>(), caches);
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
                var ret = _target.FilterOnBusinessRules(_scheduleRange, GetCashes(), _dateOnly);
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
                var ret = _target.FilterOnBusinessRules(_scheduleRange, GetCashes(), _dateOnly);
                Assert.AreEqual(2, ret.Count);
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
	        var options = new SchedulingOptions {ShiftCategory = category};

            Expect.Call(effective.ShiftCategory).Return(_category).Repeat.Twice();
            
            _mocks.ReplayAll();
            var ret = _target.CheckRestrictions(options, effective);
            Assert.That(ret, Is.False);
            _mocks.VerifyAll();

        }

        private IList<ShiftProjectionCache> GetCashes()
        {
            var tmpList = GetWorkShifts();
            var retList = new List<ShiftProjectionCache>();

			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(_dateOnly, _timeZoneInfo);
			foreach (IWorkShift shift in tmpList)
            {
                var cache = new ShiftProjectionCache(shift, dateOnlyAsDateTimePeriod);
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
    }
}

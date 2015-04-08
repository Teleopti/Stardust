using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    [TestFixture]
    public class ScheduleDayRotationRestrictionExtractorTest
    {
        private ScheduleDayRotationRestrictionExtractor _target;
        private MockRepository _mock;
        private IRestrictionExtractor _restrictionExtractor;
        private IList<IScheduleDay> _scheduleDays;
        private IScheduleDay _scheduleDay;
        private IList<IRotationRestriction> _rotationRestrictions;
        private IRotationRestriction _rotationRestriction;
        private IDayOffTemplate _dayOffTemplate;
        private IShiftCategory _shiftCategory;
        private ICheckerRestriction _restrictionChecker;
	    private IExtractedRestrictionResult _extractedRestrictionResult;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _restrictionExtractor = _mock.StrictMock<IRestrictionExtractor>();
	        _extractedRestrictionResult = _mock.StrictMock<IExtractedRestrictionResult>();
            _target = new ScheduleDayRotationRestrictionExtractor(_restrictionExtractor);
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _scheduleDays = new List<IScheduleDay>{_scheduleDay};
            _rotationRestriction = _mock.StrictMock<IRotationRestriction>();
            _rotationRestrictions = new List<IRotationRestriction> { _rotationRestriction };
            _dayOffTemplate = _mock.StrictMock<IDayOffTemplate>();
            _shiftCategory = _mock.StrictMock<IShiftCategory>();
            _restrictionChecker = _mock.StrictMock<ICheckerRestriction>();
        }

        [Test]
        public void ShouldGetAllDaysWithRotationsOnStartTimeLimitiations()
        {
	        var startTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(11));
            using(_mock.Record())
            {
                Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
                Expect.Call(_extractedRestrictionResult.RotationList).Return(_rotationRestrictions);
	            Expect.Call(_rotationRestriction.StartTimeLimitation).Return(startTimeLimitation);
            }

            using(_mock.Playback())
            {
                var restrictedDays = _target.AllRestrictedDays(_scheduleDays);
                Assert.AreEqual(1, restrictedDays.Count);
                Assert.AreEqual(_scheduleDay, restrictedDays.First());
            }
        }

		[Test]
		public void ShouldGetAllDaysWithRotationsOnEndTimeLimitiations()
		{
			var startTimeLimitation = new StartTimeLimitation(null, null);
			var endTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(11));
			
			using (_mock.Record())
			{
				Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
				Expect.Call(_extractedRestrictionResult.RotationList).Return(_rotationRestrictions);
				Expect.Call(_rotationRestriction.StartTimeLimitation).Return(startTimeLimitation);
				Expect.Call(_rotationRestriction.EndTimeLimitation).Return(endTimeLimitation);
			}

			using (_mock.Playback())
			{
				var restrictedDays = _target.AllRestrictedDays(_scheduleDays);
				Assert.AreEqual(1, restrictedDays.Count);
				Assert.AreEqual(_scheduleDay, restrictedDays.First());
			}
		}

		[Test]
		public void ShouldGetAllDaysWithRotationsOnWorkTimeLimitiations()
		{
			var startTimeLimitation = new StartTimeLimitation(null, null);
			var endTimeLimitation = new EndTimeLimitation(null, null);
			var workTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(11));

			using (_mock.Record())
			{
				Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
				Expect.Call(_extractedRestrictionResult.RotationList).Return(_rotationRestrictions);
				Expect.Call(_rotationRestriction.StartTimeLimitation).Return(startTimeLimitation);
				Expect.Call(_rotationRestriction.EndTimeLimitation).Return(endTimeLimitation);
				Expect.Call(_rotationRestriction.WorkTimeLimitation).Return(workTimeLimitation);
			}

			using (_mock.Playback())
			{
				var restrictedDays = _target.AllRestrictedDays(_scheduleDays);
				Assert.AreEqual(1, restrictedDays.Count);
				Assert.AreEqual(_scheduleDay, restrictedDays.First());
			}
		}

		[Test]
		public void ShouldGetAllDaysWithRotationsOnShiftCategory()
		{
			var startTimeLimitation = new StartTimeLimitation(null, null);
			var endTimeLimitation = new EndTimeLimitation(null, null);
			var workTimeLimitation = new WorkTimeLimitation(null, null);

			using (_mock.Record())
			{
				Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
				Expect.Call(_extractedRestrictionResult.RotationList).Return(_rotationRestrictions);
				Expect.Call(_rotationRestriction.StartTimeLimitation).Return(startTimeLimitation);
				Expect.Call(_rotationRestriction.EndTimeLimitation).Return(endTimeLimitation);
				Expect.Call(_rotationRestriction.WorkTimeLimitation).Return(workTimeLimitation);
				Expect.Call(_rotationRestriction.ShiftCategory).Return(new ShiftCategory("shiftCategory"));
			}

			using (_mock.Playback())
			{
				var restrictedDays = _target.AllRestrictedDays(_scheduleDays);
				Assert.AreEqual(1, restrictedDays.Count);
				Assert.AreEqual(_scheduleDay, restrictedDays.First());
			}
		}

		[Test]
		public void ShouldGetAllDaysWithRotationsOnDayOff()
		{
			var startTimeLimitation = new StartTimeLimitation(null, null);
			var endTimeLimitation = new EndTimeLimitation(null, null);
			var workTimeLimitation = new WorkTimeLimitation(null, null);

			using (_mock.Record())
			{
				Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
				Expect.Call(_extractedRestrictionResult.RotationList).Return(_rotationRestrictions);
				Expect.Call(_rotationRestriction.StartTimeLimitation).Return(startTimeLimitation);
				Expect.Call(_rotationRestriction.EndTimeLimitation).Return(endTimeLimitation);
				Expect.Call(_rotationRestriction.WorkTimeLimitation).Return(workTimeLimitation);
				Expect.Call(_rotationRestriction.ShiftCategory).Return(null);
				Expect.Call(_rotationRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("dayOff")));
			}

			using (_mock.Playback())
			{
				var restrictedDays = _target.AllRestrictedDays(_scheduleDays);
				Assert.AreEqual(1, restrictedDays.Count);
				Assert.AreEqual(_scheduleDay, restrictedDays.First());
			}
		}

		[Test]
		public void ShouldNotGetAnyDaysWhenNoRotationRestrictionOnDay()
		{
			var startTimeLimitation = new StartTimeLimitation(null, null);
			var endTimeLimitation = new EndTimeLimitation(null, null);
			var workTimeLimitation = new WorkTimeLimitation(null, null);

			using (_mock.Record())
			{
				Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
				Expect.Call(_extractedRestrictionResult.RotationList).Return(_rotationRestrictions);
				Expect.Call(_rotationRestriction.StartTimeLimitation).Return(startTimeLimitation);
				Expect.Call(_rotationRestriction.EndTimeLimitation).Return(endTimeLimitation);
				Expect.Call(_rotationRestriction.WorkTimeLimitation).Return(workTimeLimitation);
				Expect.Call(_rotationRestriction.ShiftCategory).Return(null);
				Expect.Call(_rotationRestriction.DayOffTemplate).Return(null);
			}

			using (_mock.Playback())
			{
				var restrictedDays = _target.AllRestrictedDays(_scheduleDays);
				Assert.AreEqual(0, restrictedDays.Count);
			}
		}

        [Test]
        public void ShouldGetAllDaysWithDayOffRotation()
        {
            using(_mock.Record())
            {
                Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
                Expect.Call(_extractedRestrictionResult.RotationList).Return(_rotationRestrictions);
                Expect.Call(_rotationRestriction.DayOffTemplate).Return(_dayOffTemplate);
            }

            using(_mock.Playback())
            {
                var restrictedDays = _target.AllRestrictedDayOffs(_scheduleDays);
                Assert.AreEqual(1, restrictedDays.Count);
                Assert.AreEqual(_scheduleDay, restrictedDays.First());
            }
        }

        [Test]
        public void ShouldGetAllDaysWithShiftRotation()
        {
            using(_mock.Record())
            {
                Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
                Expect.Call(_extractedRestrictionResult.RotationList).Return(_rotationRestrictions);
                Expect.Call(_rotationRestriction.ShiftCategory).Return(_shiftCategory);
            }

            using(_mock.Playback())
            {
                var restrictedDays = _target.AllRestrictedShifts(_scheduleDays);
                Assert.AreEqual(1, restrictedDays.Count);
                Assert.AreEqual(_scheduleDay, restrictedDays.First());
            }
        }

        [Test]
        public void ShouldGetFulfilledRotation()
        {
            using(_mock.Record())
            {
                Expect.Call(_restrictionChecker.CheckRotations(_scheduleDay)).Return(PermissionState.Satisfied);
            }

            using(_mock.Playback())
            {
                var scheduleDay = _target.RestrictionFulfilled(_restrictionChecker, _scheduleDay);
                Assert.AreEqual(_scheduleDay, scheduleDay);
            }
        }

        [Test]
        public void ShouldGetFulfilledRotationDayOff()
        {
            using(_mock.Record())
            {
                Expect.Call(_restrictionChecker.CheckRotationDayOff(_scheduleDay)).Return(PermissionState.Satisfied);
            }

            using(_mock.Playback())
            {
                var scheduleDay = _target.RestrictionFulfilledDayOff(_restrictionChecker, _scheduleDay);
                Assert.AreEqual(_scheduleDay, scheduleDay);
            }
        }

        [Test]
        public void ShouldGetFulfilledRotationShift()
        {
            using(_mock.Record())
            {
                Expect.Call(_restrictionChecker.CheckRotationShift(_scheduleDay)).Return(PermissionState.Satisfied);
            }

            using(_mock.Playback())
            {
                var scheduleDay = _target.RestrictionFulfilledShift(_restrictionChecker, _scheduleDay);
                Assert.AreEqual(_scheduleDay, scheduleDay);
            }
        }
    }
}

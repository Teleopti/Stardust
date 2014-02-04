using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
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

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _restrictionExtractor = _mock.StrictMock<IRestrictionExtractor>();
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
        public void ShouldGetAllDaysWithRotations()
        {
            using(_mock.Record())
            {
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.RotationList).Return(_rotationRestrictions);
            }

            using(_mock.Playback())
            {
                var restrictedDays = _target.AllRestrictedDays(_scheduleDays);
                Assert.AreEqual(1, restrictedDays.Count);
                Assert.AreEqual(_scheduleDay, restrictedDays.First());
            }
        }

        [Test]
        public void ShouldGetAllDaysWithDayOffRotation()
        {
            using(_mock.Record())
            {
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.RotationList).Return(_rotationRestrictions);
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
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.RotationList).Return(_rotationRestrictions);
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

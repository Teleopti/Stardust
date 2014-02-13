using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    [TestFixture]
    public class ScheduleDayPreferenceRestrictionExtractorTest
    {
        private ScheduleDayPreferenceRestrictionExtractor _target;
        private IRestrictionExtractor _restrictionExtractor;
        private ICheckerRestriction _restrictionChecker;
        private MockRepository _mock;
        private IList<IScheduleDay> _scheduleDays;
        private IScheduleDay _scheduleDay;
        private IList<IPreferenceRestriction> _preferenceRestrictions;
        private IPreferenceRestriction _preferenceRestriction;
        private IDayOffTemplate _dayOffTemplate;
        private IShiftCategory _shiftCategory;
        private IAbsence _absence;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _scheduleDays = new List<IScheduleDay> { _scheduleDay };
            _restrictionExtractor = _mock.StrictMock<IRestrictionExtractor>();
            _restrictionChecker = _mock.StrictMock<ICheckerRestriction>();
            _target = new ScheduleDayPreferenceRestrictionExtractor(_restrictionExtractor);
            _preferenceRestriction = _mock.StrictMock<IPreferenceRestriction>();
            _preferenceRestrictions = new List<IPreferenceRestriction> { _preferenceRestriction };
            _dayOffTemplate = _mock.StrictMock<IDayOffTemplate>();
            _shiftCategory = _mock.StrictMock<IShiftCategory>();
            _absence = _mock.StrictMock<IAbsence>();
        }

        [Test]
        public void ShouldGetAllDaysWithPreferences()
        {
            using(_mock.Record())
            {
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.PreferenceList).Return(_preferenceRestrictions);
            }

            using(_mock.Playback())
            {
                var restrictedDays = _target.AllRestrictedDays(_scheduleDays);
                Assert.AreEqual(1, restrictedDays.Count);
                Assert.AreEqual(_scheduleDay, restrictedDays.First());
            }
        }

        [Test]
        public void ShouldGetAllDaysWithMustHavePreference()
        {
            using(_mock.Record())
            {
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.PreferenceList).Return(_preferenceRestrictions);
                Expect.Call(_preferenceRestriction.MustHave).Return(true);
            }

            using(_mock.Playback())
            {
                var restrictedDays = _target.AllRestrictedDaysMustHave(_scheduleDays);
                Assert.AreEqual(1, restrictedDays.Count);
                Assert.AreEqual(_scheduleDay, restrictedDays.First());
            }
        }

        [Test]
        public void ShouldGetAllDaysWithDayOffsPreferences()
        {
            using(_mock.Record())
            {
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.PreferenceList).Return(_preferenceRestrictions);
                Expect.Call(_preferenceRestriction.DayOffTemplate).Return(_dayOffTemplate);
            }

            using(_mock.Playback())
            {
                var restrictedDays = _target.AllRestrictedDayOffs(_scheduleDays);
                Assert.AreEqual(1, restrictedDays.Count);
                Assert.AreEqual(_scheduleDay,restrictedDays.First());
            }
        }

        [Test]
        public void ShouldGetAllDaysWithAbsencePreferences()
        {
            using(_mock.Record())
            {
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.PreferenceList).Return(_preferenceRestrictions);
                Expect.Call(_preferenceRestriction.Absence).Return(_absence);
            }

            using(_mock.Playback())
            {
                var restrictedDays = _target.AllRestrictedAbsences(_scheduleDays);
                Assert.AreEqual(1, restrictedDays.Count);
                Assert.AreEqual(_scheduleDay, restrictedDays.First());
            }
        }

        [Test]
        public void ShouldGetAllDaysWithShiftPreferences()
        {
            using(_mock.Record())
            {
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.PreferenceList).Return(_preferenceRestrictions);
                Expect.Call(_preferenceRestriction.ShiftCategory).Return(_shiftCategory);
            }

            using(_mock.Playback())
            {
                var restrictedDays = _target.AllRestrictedShifts(_scheduleDays);
                Assert.AreEqual(1, restrictedDays.Count);
                Assert.AreEqual(_scheduleDay, restrictedDays.First());
            }
        }

        [Test]
        public void ShouldGetFulfilledPreference()
        {
            using(_mock.Record())
            {
                Expect.Call(_restrictionChecker.CheckPreference(_scheduleDay)).Return(PermissionState.Satisfied);
            }

            using(_mock.Playback())
            {
                var scheduleDay = _target.RestrictionFulfilled(_restrictionChecker, _scheduleDay);
                Assert.AreEqual(_scheduleDay, scheduleDay);
            }
        }

        [Test]
        public void ShouldGetFulfilledPreferenceDayOff()
        {
            using(_mock.Record())
            {
                Expect.Call(_restrictionChecker.CheckPreferenceDayOff(_scheduleDay)).Return(PermissionState.Satisfied);
            }

            using(_mock.Playback())
            {
                var scheduleDay = _target.RestrictionFulfilledDayOff(_restrictionChecker, _scheduleDay);
                Assert.AreEqual(_scheduleDay, scheduleDay);
            }
        }

        [Test]
        public void ShouldGetFulfilledPreferenceAbsence()
        {
            using(_mock.Record())
            {
                Expect.Call(_restrictionChecker.CheckPreferenceAbsence(PermissionState.Unspecified, _scheduleDay)).Return(PermissionState.Satisfied);
            }

            using(_mock.Playback())
            {
                var scheduleDay = _target.RestrictionFulfilledAbsence(_restrictionChecker, _scheduleDay);
                Assert.AreEqual(_scheduleDay, scheduleDay);
            }
        }

        [Test]
        public void ShouldGetFulfilledPreferenceShift()
        {
            using(_mock.Record())
            {
                Expect.Call(_restrictionChecker.CheckPreferenceShift(_scheduleDay)).Return(PermissionState.Satisfied);
            }

            using(_mock.Playback())
            {
                var scheduleDay = _target.RestrictionFulfilledShift(_restrictionChecker, _scheduleDay);
                Assert.AreEqual(_scheduleDay, scheduleDay);
            }
        }

        [Test]
        public void ShouldGetFulfilledMustHavePreferences()
        {
            using (_mock.Record())
            {
                Expect.Call(_restrictionChecker.CheckPreferenceMustHave(_scheduleDay)).Return(PermissionState.Satisfied);
            }

            using (_mock.Playback())
            {
                var scheduleDay = _target.RestrictionFulfilledMustHave(_restrictionChecker, _scheduleDay);
                Assert.AreEqual(_scheduleDay, scheduleDay);
            }
        }
    }
}

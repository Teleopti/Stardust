﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    [TestFixture]
    public class ScheduleDayRestrictionExtractorTest
    {
        private ScheduleDayRestrictionExtractor _target;
        private MockRepository _mock;
        private IRestrictionExtractor _restrictionExtractor;
        private IList<IPreferenceRestriction> _preferenceRestrictions;
        private IPreferenceRestriction _preferenceRestriction;
        private IList<IRotationRestriction> _rotationRestrictions;
        private IRotationRestriction _rotationRestriction;
        private IList<IStudentAvailabilityDay> _studentAvailabilityRestrictions;
        private IStudentAvailabilityDay _studentAvailabilityDay;
        private IList<IAvailabilityRestriction> _availabilityRestrictions;
        private IAvailabilityRestriction _availabilityRestriction;
        private IList<IScheduleDay> _scheduleDays;
        private IScheduleDay _scheduleDay;
       
        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _scheduleDays = new List<IScheduleDay> { _scheduleDay };
            _restrictionExtractor = _mock.StrictMock<IRestrictionExtractor>();
            _target = new ScheduleDayRestrictionExtractor(_restrictionExtractor);
            _preferenceRestriction = _mock.StrictMock<IPreferenceRestriction>();
            _preferenceRestrictions = new List<IPreferenceRestriction>{_preferenceRestriction};
            _rotationRestriction = _mock.StrictMock<IRotationRestriction>();
            _rotationRestrictions = new List<IRotationRestriction>{_rotationRestriction};
            _studentAvailabilityDay = _mock.StrictMock<IStudentAvailabilityDay>();
            _studentAvailabilityRestrictions = new List<IStudentAvailabilityDay>{_studentAvailabilityDay};
            _availabilityRestriction = _mock.StrictMock<IAvailabilityRestriction>();
            _availabilityRestrictions = new List<IAvailabilityRestriction>{_availabilityRestriction};
            
        }

        [Test]
        public void ShouldGetAllDaysWithPreferenceRestrictions()
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
            }
        }

        [Test]
        public void ShouldGetAllDaysWithRotationRestrictions()
        {
            using (_mock.Record())
            {
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.PreferenceList).Return(new List<IPreferenceRestriction>());
                Expect.Call(_restrictionExtractor.RotationList).Return(_rotationRestrictions);
            }

            using (_mock.Playback())
            {
                var restrictedDays = _target.AllRestrictedDays(_scheduleDays);
                Assert.AreEqual(1, restrictedDays.Count);
            }
        }

        [Test]
        public void ShouldGetAllDaysWithStudentAvailabilityRestrictions()
        {
            using (_mock.Record())
            {
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.PreferenceList).Return(new List<IPreferenceRestriction>());
                Expect.Call(_restrictionExtractor.RotationList).Return(new List<IRotationRestriction>());
                Expect.Call(_restrictionExtractor.StudentAvailabilityList).Return(_studentAvailabilityRestrictions);
            }

            using (_mock.Playback())
            {
                var restrictedDays = _target.AllRestrictedDays(_scheduleDays);
                Assert.AreEqual(1, restrictedDays.Count);
            }
        }

        [Test]
        public void ShouldGetAllDaysWithAvailabilityRestrictions()
        {
            using (_mock.Record())
            {
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.PreferenceList).Return(new List<IPreferenceRestriction>());
                Expect.Call(_restrictionExtractor.RotationList).Return(new List<IRotationRestriction>());
                Expect.Call(_restrictionExtractor.StudentAvailabilityList).Return(new List<IStudentAvailabilityDay>());
                Expect.Call(_restrictionExtractor.AvailabilityList).Return(_availabilityRestrictions);
            }

            using (_mock.Playback())
            {
                var restrictedDays = _target.AllRestrictedDays(_scheduleDays);
                Assert.AreEqual(1, restrictedDays.Count);
            }
        }

        [Test]
        public void ShouldGetNoDaysOnDaysWithNoRestrictions()
        {
            using (_mock.Record())
            {
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.PreferenceList).Return(new List<IPreferenceRestriction>());
                Expect.Call(_restrictionExtractor.RotationList).Return(new List<IRotationRestriction>());
                Expect.Call(_restrictionExtractor.StudentAvailabilityList).Return(new List<IStudentAvailabilityDay>());
                Expect.Call(_restrictionExtractor.AvailabilityList).Return(new List<IAvailabilityRestriction>());
            }

            using (_mock.Playback())
            {
                var restrictedDays = _target.AllRestrictedDays(_scheduleDays);
                Assert.AreEqual(0, restrictedDays.Count);
            }
        }
    }
}

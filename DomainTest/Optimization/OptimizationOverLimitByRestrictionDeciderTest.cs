﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class OptimizationOverLimitByRestrictionDeciderTest
    {
        private OptimizationOverLimitByRestrictionDecider _target;
        private MockRepository _mocks;

        private IScheduleMatrixPro _matrix;
        private ICheckerRestriction _restrictionChecker;
        private OptimizationPreferences _optimizationPreferences;

        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDayPro _scheduleDayPro3;
        private IScheduleDayPro _scheduleDayPro4;

        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDay _scheduleDay3;
        private IScheduleDay _scheduleDay4;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _restrictionChecker = _mocks.StrictMock<ICheckerRestriction>();
            _optimizationPreferences = new OptimizationPreferences();
            resetPreferences();

            _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro3 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro4 = _mocks.StrictMock<IScheduleDayPro>();

            _scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDay4 = _mocks.StrictMock<IScheduleDay>();

        }

        [Test]
        public void VerifyInstantiate()
        {
            _target = new OptimizationOverLimitByRestrictionDecider(
                _matrix,
                _restrictionChecker,
                _optimizationPreferences);
            Assert.IsNotNull(_target);
        }

        #region PreferencesTests

        [Test]
        public void VerifyPreferencesUnderLimit()
        {
            _optimizationPreferences.General.UsePreferences = true;
            _optimizationPreferences.General.PreferencesValue = 0;

            using (_mocks.Record())
            {
                addPreferencesMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyPreferencesEqualLimit()
        {
            _optimizationPreferences.General.UsePreferences = true;
            _optimizationPreferences.General.PreferencesValue = 0.5d;

            using (_mocks.Record())
            {
                addPreferencesMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyPreferencesOverLimit()
        {
            _optimizationPreferences.General.UsePreferences = true;
            _optimizationPreferences.General.PreferencesValue = 0.6d;

            using (_mocks.Record())
            {
                addPreferencesMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void VerifyPreferencesIsUnderLimitIfUsePreferencesIsFalse()
        {
            _optimizationPreferences.General.UsePreferences = false;
            _optimizationPreferences.General.PreferencesValue = 0.4d;

            _target = new OptimizationOverLimitByRestrictionDecider(
                _matrix,
                _restrictionChecker,
                _optimizationPreferences);
            bool result = _target.OverLimit();
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifyPreferencesUnderLimitWhenNoPreferencesInPeriod()
        {
            _optimizationPreferences.General.UsePreferences = true;
            _optimizationPreferences.General.PreferencesValue = 0.0d;

            using (_mocks.Record())
            {

                commonMocks();

                Expect.Call(_restrictionChecker.CheckPreference())
                    .Return(PermissionState.None)
                    .Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        private void addPreferencesMockExpectation()
        {
            addMockExpectation(_restrictionChecker.CheckPreference);
        }


        #endregion

        #region MustHavesTests

        [Test]
        public void VerifyMustHavesUnderLimit()
        {
            _optimizationPreferences.General.UseMustHaves = true;
            _optimizationPreferences.General.MustHavesValue = 0;

            using (_mocks.Record())
            {
                addMustHaveMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyMustHavesEqualLimit()
        {
            _optimizationPreferences.General.UseMustHaves = true;
            _optimizationPreferences.General.MustHavesValue = 0.5d;

            using (_mocks.Record())
            {
                addMustHaveMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyMustHavesOverLimit()
        {
            _optimizationPreferences.General.UseMustHaves = true;
            _optimizationPreferences.General.MustHavesValue = 0.6d;

            using (_mocks.Record())
            {
                addMustHaveMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void VerifyMustHavesIsUnderLimitIfUseRotationsIsFalse()
        {
            _optimizationPreferences.General.UseMustHaves = false;
            _optimizationPreferences.General.MustHavesValue = 0.4d;

            _target = new OptimizationOverLimitByRestrictionDecider(
                _matrix,
                _restrictionChecker,
                _optimizationPreferences);
            bool result = _target.OverLimit();
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifyMustHavesNoMustHavesInPeriod()
        {
            _optimizationPreferences.General.UseMustHaves = true;
            _optimizationPreferences.General.RotationsValue = 0.0d;

            using (_mocks.Record())
            {

                commonMocks();

                Expect.Call(_restrictionChecker.CheckPreferenceMustHave())
                    .Return(PermissionState.None)
                    .Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        private void addMustHaveMockExpectation()
        {
            addMockExpectation(_restrictionChecker.CheckPreferenceMustHave);
        }


        #endregion

        #region RotationTests

        [Test]
        public void VerifyRotationUnderLimit()
        {
            _optimizationPreferences.General.UseRotations = true;
            _optimizationPreferences.General.RotationsValue = 0;

            using (_mocks.Record())
            {
                addRotationMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyRotationEqualLimit()
        {
            _optimizationPreferences.General.UseRotations = true;
            _optimizationPreferences.General.RotationsValue = 0.5d;

            using (_mocks.Record())
            {
                addRotationMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyRotationOverLimit()
        {
            _optimizationPreferences.General.UseRotations = true;
            _optimizationPreferences.General.RotationsValue = 0.6d;

            using (_mocks.Record())
            {
                addRotationMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void VerifyRotationIsUnderLimitIfUseRotationsIsFalse()
        {
            _optimizationPreferences.General.UseRotations = false;
            _optimizationPreferences.General.RotationsValue = 0.4d;

            using (_mocks.Record())
            {
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyRotationNoRotationInPeriod()
        {
            _optimizationPreferences.General.UseRotations = true;
            _optimizationPreferences.General.RotationsValue = 0.0d;

            using (_mocks.Record())
            {

                commonMocks();

                Expect.Call(_restrictionChecker.CheckRotations())
                    .Return(PermissionState.None)
                    .Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        private void addRotationMockExpectation()
        {
            addMockExpectation(_restrictionChecker.CheckRotations);
        }

        #endregion

        #region AvailabilitiesTests

        [Test]
        public void VerifyAvailabilitiesUnderLimit()
        {
            _optimizationPreferences.General.UseAvailabilities = true;
            _optimizationPreferences.General.AvailabilitiesValue = 0;

            using (_mocks.Record())
            {
                addAvailabilityMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyAvailabilitiesEqualLimit()
        {
            _optimizationPreferences.General.UseAvailabilities = true;
            _optimizationPreferences.General.AvailabilitiesValue = 0.5d;

            using (_mocks.Record())
            {
                addAvailabilityMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyAvailabilitiesOverLimit()
        {
            _optimizationPreferences.General.UseAvailabilities = true;
            _optimizationPreferences.General.AvailabilitiesValue = 0.6d;

            using (_mocks.Record())
            {
                addAvailabilityMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void VerifyAvailabilitiesIsUnderLimitIfUseAvailabilitiesIsFalse()
        {
            _optimizationPreferences.General.UseAvailabilities = false;
            _optimizationPreferences.General.AvailabilitiesValue = 0.4d;

            using (_mocks.Record())
            {
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyAvailabilitiesIsUnderLimitIfNoAvailabilitiesInPeriod()
        {
            _optimizationPreferences.General.UseAvailabilities = true;
            _optimizationPreferences.General.AvailabilitiesValue = 0.0d;

            using (_mocks.Record())
            {

                commonMocks();

                Expect.Call(_restrictionChecker.CheckAvailability())
                    .Return(PermissionState.None)
                    .Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        private void addAvailabilityMockExpectation()
        {
            addMockExpectation(_restrictionChecker.CheckAvailability);
        }

        #endregion

        #region StudentAvailabilitiesTests

        [Test]
        public void VerifyStudentAvailabilitiesUnderLimit()
        {
            _optimizationPreferences.General.UseStudentAvailabilities = true;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0;

            using (_mocks.Record())
            {
                addStudentAvailabilityMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyStudentAvailabilitiesEqualLimit()
        {
            _optimizationPreferences.General.UseStudentAvailabilities = true;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0.5d;

            using (_mocks.Record())
            {
                addStudentAvailabilityMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyStudentAvailabilitiesOverLimit()
        {
            _optimizationPreferences.General.UseStudentAvailabilities = true;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0.6d;

            using (_mocks.Record())
            {
                addStudentAvailabilityMockExpectation();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void VerifyStudentAvailabilitiesIsUnderLimitIfUseStudentAvailabilitiesIsFalse()
        {
            _optimizationPreferences.General.UseStudentAvailabilities = false;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0.4d;

            using (_mocks.Record())
            {
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyStudentAvailabilitiesIsUnderLimitIfNoStudentAvailabilitiesInPeriod()
        {
            _optimizationPreferences.General.UseStudentAvailabilities = true;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0.0d;

            using (_mocks.Record())
            {
                commonMocks();

                Expect.Call(_restrictionChecker.CheckStudentAvailability())
                    .Return(PermissionState.None)
                    .Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _matrix,
                    _restrictionChecker,
                    _optimizationPreferences);
                bool result = _target.OverLimit();
                Assert.IsFalse(result);
            }
        }

        private void addStudentAvailabilityMockExpectation()
        {
            addMockExpectation(_restrictionChecker.CheckStudentAvailability);
        }

        #endregion

        private void resetPreferences()
        {
            _optimizationPreferences.General.UsePreferences = false;
            _optimizationPreferences.General.UseMustHaves = false;
            _optimizationPreferences.General.UseRotations = false;
            _optimizationPreferences.General.UseAvailabilities = false;
            _optimizationPreferences.General.UseStudentAvailabilities = false;
        }

        private void addMockExpectation(Func<PermissionState> checkMethod)
        {

            commonMocks();

            Expect.Call(checkMethod())
                .Return(PermissionState.None);
            Expect.Call(checkMethod())
                .Return(PermissionState.Unspecified);
            Expect.Call(checkMethod())
                .Return(PermissionState.Broken);
            Expect.Call(checkMethod())
                .Return(PermissionState.None);
        }

        private void commonMocks()
        {
            Expect.Call(() => _restrictionChecker.ScheduleDay = _scheduleDay1);
            Expect.Call(() => _restrictionChecker.ScheduleDay = _scheduleDay2);
            Expect.Call(() => _restrictionChecker.ScheduleDay = _scheduleDay3);
            Expect.Call(() => _restrictionChecker.ScheduleDay = _scheduleDay4);

            Expect.Call(_matrix.EffectivePeriodDays).Return(
                new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>
                                                            {
                                                                _scheduleDayPro1,
                                                                _scheduleDayPro2,
                                                                _scheduleDayPro3,
                                                                _scheduleDayPro4
                                                            }));

            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_scheduleDay3);
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_scheduleDay4);
        }
    }
}

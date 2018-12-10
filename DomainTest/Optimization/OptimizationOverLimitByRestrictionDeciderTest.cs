using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class OptimizationOverLimitByRestrictionDeciderTest
    {
        private IOptimizationOverLimitByRestrictionDecider _target;
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

        private IScheduleMatrixOriginalStateContainer _originalStateContainer;

	    private IDaysOffPreferences _daysOffPreferences;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
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

			_daysOffPreferences = new DaysOffPreferences();

        }

        [Test]
        public void VerifyInstantiate()
        {
            _target = new OptimizationOverLimitByRestrictionDecider(
                _restrictionChecker,
                _optimizationPreferences,
                _originalStateContainer,
				_daysOffPreferences);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
                Assert.IsTrue(result.PreferencesOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
                Assert.IsTrue(result.PreferencesOverLimit == 0);

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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsFalse(result.PreferencesOverLimit == 0);
            }
        }

        [Test]
        public void VerifyPreferencesIsUnderLimitIfUsePreferencesIsFalse()
        {
            _optimizationPreferences.General.UsePreferences = false;
            _optimizationPreferences.General.PreferencesValue = 0.4d;

            _target = new OptimizationOverLimitByRestrictionDecider(
                _restrictionChecker,
                _optimizationPreferences,
                _originalStateContainer,
				_daysOffPreferences);
            var result = _target.OverLimitsCounts(_matrix);
			Assert.IsTrue(result.PreferencesOverLimit == 0);
        }

        [Test]
        public void VerifyPreferencesUnderLimitWhenNoPreferencesInPeriod()
        {
            _optimizationPreferences.General.UsePreferences = true;
            _optimizationPreferences.General.PreferencesValue = 0.0d;

            using (_mocks.Record())
            {

                commonMocks();

                Expect.Call(_restrictionChecker.CheckPreference(_scheduleDay1)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckPreference(_scheduleDay2)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckPreference(_scheduleDay3)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckPreference(_scheduleDay4)).Return(PermissionState.None);
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsTrue(result.PreferencesOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
                Assert.IsTrue(result.MustHavesOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsTrue(result.MustHavesOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsFalse(result.MustHavesOverLimit == 0);
            }
        }

        [Test]
        public void VerifyMustHavesIsUnderLimitIfUseRotationsIsFalse()
        {
            _optimizationPreferences.General.UseMustHaves = false;
            _optimizationPreferences.General.MustHavesValue = 0.4d;

            _target = new OptimizationOverLimitByRestrictionDecider(
                _restrictionChecker,
                _optimizationPreferences,
                _originalStateContainer,
				_daysOffPreferences);
            var result = _target.OverLimitsCounts(_matrix);
			Assert.IsTrue(result.MustHavesOverLimit == 0);
        }

        [Test]
        public void VerifyMustHavesNoMustHavesInPeriod()
        {
            _optimizationPreferences.General.UseMustHaves = true;
            _optimizationPreferences.General.RotationsValue = 0.0d;

            using (_mocks.Record())
            {

                commonMocks();

                Expect.Call(_restrictionChecker.CheckPreferenceMustHave(_scheduleDay1)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckPreferenceMustHave(_scheduleDay2)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckPreferenceMustHave(_scheduleDay3)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckPreferenceMustHave(_scheduleDay4)).Return(PermissionState.None);
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsTrue(result.MustHavesOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
                Assert.IsTrue(result.RotationsOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsTrue(result.RotationsOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsFalse(result.RotationsOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsTrue(result.RotationsOverLimit == 0);
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

                Expect.Call(_restrictionChecker.CheckRotations(_scheduleDay1)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckRotations(_scheduleDay2)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckRotations(_scheduleDay3)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckRotations(_scheduleDay4)).Return(PermissionState.None);
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsTrue(result.RotationsOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
                Assert.IsTrue(result.AvailabilitiesOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsTrue(result.AvailabilitiesOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsFalse(result.AvailabilitiesOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsTrue(result.AvailabilitiesOverLimit == 0);
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

                Expect.Call(_restrictionChecker.CheckAvailability(_scheduleDay1)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckAvailability(_scheduleDay2)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckAvailability(_scheduleDay3)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckAvailability(_scheduleDay4)).Return(PermissionState.None);
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsTrue(result.AvailabilitiesOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
                Assert.IsTrue(result.StudentAvailabilitiesOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsTrue(result.StudentAvailabilitiesOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsFalse(result.StudentAvailabilitiesOverLimit == 0);
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
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsTrue(result.StudentAvailabilitiesOverLimit == 0);
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

	            Expect.Call(_restrictionChecker.CheckStudentAvailability(_scheduleDay1)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckStudentAvailability(_scheduleDay2)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckStudentAvailability(_scheduleDay3)).Return(PermissionState.None);
				Expect.Call(_restrictionChecker.CheckStudentAvailability(_scheduleDay4)).Return(PermissionState.None);
            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByRestrictionDecider(
                    _restrictionChecker,
                    _optimizationPreferences,
                    _originalStateContainer,
					_daysOffPreferences);
                var result = _target.OverLimitsCounts(_matrix);
				Assert.IsTrue(result.StudentAvailabilitiesOverLimit == 0);
            }
        }

        private void addStudentAvailabilityMockExpectation()
        {
            addMockExpectation(_restrictionChecker.CheckStudentAvailability);
        }

        #endregion

		[Test]
		public void MoveMaxDaysOverLimitShouldReturnFalseIfEverythingOk()
		{
			_daysOffPreferences.UseKeepExistingDaysOff = true;
			_daysOffPreferences.KeepExistingDaysOffValue = 1;

			using (_mocks.Record())
			{
				Expect.Call(_originalStateContainer.ChangedDayOffsPercent()).Return(0);
			}

			bool ret;

			using (_mocks.Playback())
			{
				_target = new OptimizationOverLimitByRestrictionDecider(
					_restrictionChecker,
					_optimizationPreferences,
					_originalStateContainer,
					_daysOffPreferences);

				ret = _target.MoveMaxDaysOverLimit();
			}

			Assert.IsFalse(ret);
		}

		[Test]
		public void MoveMaxDaysOverLimitShouldReturnTrueIfDaysOffOverLimit()
		{
			_daysOffPreferences.UseKeepExistingDaysOff = true;
			_daysOffPreferences.KeepExistingDaysOffValue = 1;

			using (_mocks.Record())
			{
				Expect.Call(_originalStateContainer.ChangedDayOffsPercent()).Return(.5);
			}

			bool ret;

			using (_mocks.Playback())
			{
				_target = new OptimizationOverLimitByRestrictionDecider(
					_restrictionChecker,
					_optimizationPreferences,
					_originalStateContainer,
					_daysOffPreferences);

				ret = _target.MoveMaxDaysOverLimit();
			}

			Assert.IsTrue(ret);
		}

	    [Test]
	    public void ShouldReportFalseIfBrokenHasNotIncreased()
	    {
			_optimizationPreferences.General.UsePreferences = true;
			_optimizationPreferences.General.PreferencesValue = 1;
			_optimizationPreferences.General.UseMustHaves = true;
			_optimizationPreferences.General.MustHavesValue = 1;
			_optimizationPreferences.General.UseRotations = true;
			_optimizationPreferences.General.RotationsValue = 1;
			_optimizationPreferences.General.UseAvailabilities = true;
			_optimizationPreferences.General.AvailabilitiesValue = 1;
			_optimizationPreferences.General.UseStudentAvailabilities = true;
			_optimizationPreferences.General.StudentAvailabilitiesValue = 1;

			using (_mocks.Record())
			{
				permissionStateMocks();
			}

			bool ret;

			using (_mocks.Playback())
			{
				_target = new OptimizationOverLimitByRestrictionDecider(
					_restrictionChecker,
					_optimizationPreferences,
					_originalStateContainer,
					_daysOffPreferences);

				var last = new OverLimitResults(1, 2, 3, 4, 0);
				ret = _target.HasOverLimitIncreased(last, _matrix);
				Assert.IsFalse(ret);
			}
	    }

	    
	    [Test]
		public void ShouldReportTrueIfBrokenHasIncreased()
		{
			_optimizationPreferences.General.UsePreferences = true;
			_optimizationPreferences.General.PreferencesValue = 1;
			_optimizationPreferences.General.UseMustHaves = true;
			_optimizationPreferences.General.MustHavesValue = 1;
			_optimizationPreferences.General.UseRotations = true;
			_optimizationPreferences.General.RotationsValue = 1;
			_optimizationPreferences.General.UseAvailabilities = true;
			_optimizationPreferences.General.AvailabilitiesValue = 1;
			_optimizationPreferences.General.UseStudentAvailabilities = true;
			_optimizationPreferences.General.StudentAvailabilitiesValue = 1;

			using (_mocks.Record())
			{
				permissionStateMocks();
			}

			bool ret;

			using (_mocks.Playback())
			{
				_target = new OptimizationOverLimitByRestrictionDecider(
					_restrictionChecker,
					_optimizationPreferences,
					_originalStateContainer,
					_daysOffPreferences);

				var last = new OverLimitResults(0, 1, 2, 3, 0);
				ret = _target.HasOverLimitIncreased(last, _matrix);
				Assert.IsTrue(ret);
			}
		}

	    [Test]
	    public void ShouldReportCorrectBrokenCount()
	    {
			_optimizationPreferences.General.UsePreferences = true;
		    _optimizationPreferences.General.PreferencesValue = 1;
			_optimizationPreferences.General.UseMustHaves = true;
		    _optimizationPreferences.General.MustHavesValue = 1;
			_optimizationPreferences.General.UseRotations = true;
		    _optimizationPreferences.General.RotationsValue = 1;
			_optimizationPreferences.General.UseAvailabilities = true;
			_optimizationPreferences.General.AvailabilitiesValue = 1;
			_optimizationPreferences.General.UseStudentAvailabilities = true;
			_optimizationPreferences.General.StudentAvailabilitiesValue = 1;
			using (_mocks.Record())
			{
				permissionStateMocks();
			}

			OverLimitResults ret;

			using (_mocks.Playback())
			{
				_target = new OptimizationOverLimitByRestrictionDecider(
					_restrictionChecker,
					_optimizationPreferences,
					_originalStateContainer,
					_daysOffPreferences);

				ret = _target.OverLimitsCounts(_matrix);
			}

			Assert.AreEqual(1, ret.PreferencesOverLimit);
			Assert.AreEqual(2, ret.MustHavesOverLimit);
			Assert.AreEqual(3, ret.RotationsOverLimit);
			Assert.AreEqual(4, ret.AvailabilitiesOverLimit);
			Assert.AreEqual(0, ret.StudentAvailabilitiesOverLimit);
	    }

        private void resetPreferences()
        {
            _optimizationPreferences.General.UsePreferences = false;
            _optimizationPreferences.General.UseMustHaves = false;
            _optimizationPreferences.General.UseRotations = false;
            _optimizationPreferences.General.UseAvailabilities = false;
            _optimizationPreferences.General.UseStudentAvailabilities = false;
        }

        private void addMockExpectation(Func<IScheduleDay, PermissionState> checkMethod)
        {

            commonMocks();

            Expect.Call(checkMethod(_scheduleDay1)).Return(PermissionState.None);
            Expect.Call(checkMethod(_scheduleDay2)).Return(PermissionState.Unspecified);
            Expect.Call(checkMethod(_scheduleDay3)).Return(PermissionState.Broken);
            Expect.Call(checkMethod(_scheduleDay4)).Return(PermissionState.None);
        }

        private void commonMocks()
        {
	        Expect.Call(_matrix.EffectivePeriodDays).Return(
		        new[]
		        {
			        _scheduleDayPro1,
			        _scheduleDayPro2,
			        _scheduleDayPro3,
			        _scheduleDayPro4
		        });

            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_scheduleDay3);
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_scheduleDay4);

            Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2012, 01, 01)).Repeat.Any();
            Expect.Call(_scheduleDayPro2.Day).Return(new DateOnly(2012, 01, 01)).Repeat.Any();
            Expect.Call(_scheduleDayPro3.Day).Return(new DateOnly(2012, 01, 01)).Repeat.Any();
            Expect.Call(_scheduleDayPro4.Day).Return(new DateOnly(2012, 01, 01)).Repeat.Any();
        }

		private void commonMocks2()
		{
			Expect.Call(_matrix.EffectivePeriodDays).Return(new[]
			{
				_scheduleDayPro1,
				_scheduleDayPro2,
				_scheduleDayPro3,
				_scheduleDayPro4
			});

			Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
			Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
			Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_scheduleDay3);
			Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_scheduleDay4);

			Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2012, 01, 01)).Repeat.Any();
			Expect.Call(_scheduleDayPro2.Day).Return(new DateOnly(2012, 01, 01)).Repeat.Any();
			Expect.Call(_scheduleDayPro3.Day).Return(new DateOnly(2012, 01, 01)).Repeat.Any();
			Expect.Call(_scheduleDayPro4.Day).Return(new DateOnly(2012, 01, 01)).Repeat.Any();
		}

		private void commonMocks3()
		{
			Expect.Call(_matrix.EffectivePeriodDays).Return(
				new[]
				{
					_scheduleDayPro1,
					_scheduleDayPro2,
					_scheduleDayPro3,
					_scheduleDayPro4
				});

			Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
			Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
			Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_scheduleDay3);
			Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_scheduleDay4);

		}

		private void permissionStateMocks()
		{
			commonMocks2();
			Expect.Call(_restrictionChecker.CheckPreference(_scheduleDay1)).Return(PermissionState.Broken);
			Expect.Call(_restrictionChecker.CheckPreference(_scheduleDay2)).Return(PermissionState.Satisfied);
			Expect.Call(_restrictionChecker.CheckPreference(_scheduleDay3)).Return(PermissionState.Satisfied);
			Expect.Call(_restrictionChecker.CheckPreference(_scheduleDay4)).Return(PermissionState.Satisfied);

			commonMocks3();
			Expect.Call(_restrictionChecker.CheckPreferenceMustHave(_scheduleDay1)).Return(PermissionState.Broken);
			Expect.Call(_restrictionChecker.CheckPreferenceMustHave(_scheduleDay2)).Return(PermissionState.Broken);
			Expect.Call(_restrictionChecker.CheckPreferenceMustHave(_scheduleDay3)).Return(PermissionState.Satisfied);
			Expect.Call(_restrictionChecker.CheckPreferenceMustHave(_scheduleDay4)).Return(PermissionState.Satisfied);

			commonMocks3();
			Expect.Call(_restrictionChecker.CheckRotations(_scheduleDay1)).Return(PermissionState.Broken);
			Expect.Call(_restrictionChecker.CheckRotations(_scheduleDay2)).Return(PermissionState.Broken);
			Expect.Call(_restrictionChecker.CheckRotations(_scheduleDay3)).Return(PermissionState.Broken);
			Expect.Call(_restrictionChecker.CheckRotations(_scheduleDay4)).Return(PermissionState.Satisfied);

			commonMocks3();
			Expect.Call(_restrictionChecker.CheckAvailability(_scheduleDay1)).Return(PermissionState.Broken);
			Expect.Call(_restrictionChecker.CheckAvailability(_scheduleDay2)).Return(PermissionState.Broken);
			Expect.Call(_restrictionChecker.CheckAvailability(_scheduleDay3)).Return(PermissionState.Broken);
			Expect.Call(_restrictionChecker.CheckAvailability(_scheduleDay4)).Return(PermissionState.Broken);

			commonMocks3();
			Expect.Call(_restrictionChecker.CheckStudentAvailability(_scheduleDay1)).Return(PermissionState.Satisfied);
			Expect.Call(_restrictionChecker.CheckStudentAvailability(_scheduleDay2)).Return(PermissionState.Satisfied);
			Expect.Call(_restrictionChecker.CheckStudentAvailability(_scheduleDay3)).Return(PermissionState.Satisfied);
			Expect.Call(_restrictionChecker.CheckStudentAvailability(_scheduleDay4)).Return(PermissionState.Satisfied);
		}

    }
}

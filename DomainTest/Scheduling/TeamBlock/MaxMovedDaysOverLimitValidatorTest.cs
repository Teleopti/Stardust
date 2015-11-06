using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class MaxMovedDaysOverLimitValidatorTest
	{
		private MockRepository _mocks;
		private IMaxMovedDaysOverLimitValidator _target;
		private IDictionary<IPerson, IScheduleRange> _allSelectedScheduleRangeClones;
		private IScheduleDayEquator _scheduleDayEquator;
		private IOptimizationPreferences _optimizationPreferences;
		private IScheduleMatrixPro _matrix;
		private IPerson _person;
		private IScheduleRange _range;
		private IScheduleDayPro _scheduleDayPro;
		private IScheduleDay _currentScheduleDay;
		private IScheduleDay _originalScheduleDay;
		private IDaysOffPreferences _daysOffPreferences;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			
			_optimizationPreferences = new OptimizationPreferences();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_allSelectedScheduleRangeClones = new Dictionary<IPerson, IScheduleRange>();
			_range = _mocks.StrictMock<IScheduleRange>();
			_person = PersonFactory.CreatePerson();
			_allSelectedScheduleRangeClones.Add(_person, _range);
			_optimizationPreferences.Rescheduling.AllSelectedScheduleRangeClones = _allSelectedScheduleRangeClones;
			_scheduleDayEquator = _mocks.StrictMock<IScheduleDayEquator>();
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_currentScheduleDay = _mocks.StrictMock<IScheduleDay>();
			_originalScheduleDay = _mocks.StrictMock<IScheduleDay>();

			_target = new MaxMovedDaysOverLimitValidator(_scheduleDayEquator);
			_daysOffPreferences = new DaysOffPreferences();
			
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MaxIs"), Test]
		public void ShouldReturnTrueIfMoveMaxIsNotUsed()
		{

			_optimizationPreferences.Shifts.KeepShifts = false;
			_daysOffPreferences.UseKeepExistingDaysOff = false;
			bool result = _target.ValidateMatrix(_matrix, _optimizationPreferences, _daysOffPreferences);
			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldReturnTrueIfAllKeepShiftsWithinLimit()
		{
			_optimizationPreferences.Shifts.KeepShifts = true;
			_optimizationPreferences.Shifts.KeepShiftsValue = 0;

			using (_mocks.Record())
			{
				commonMocks();

				Expect.Call(_originalScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDayEquator.MainShiftEquals(_originalScheduleDay, _currentScheduleDay)).Return(true);
			}

			using (_mocks.Playback())
			{
				bool result = _target.ValidateMatrix(_matrix, _optimizationPreferences, _daysOffPreferences);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfNotKeepShiftsWithinLimit()
		{
			_optimizationPreferences.Shifts.KeepShifts = true;
			_optimizationPreferences.Shifts.KeepShiftsValue = 1;

			using (_mocks.Record())
			{
				commonMocks();

				Expect.Call(_originalScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDayEquator.MainShiftEquals(_originalScheduleDay, _currentScheduleDay)).Return(false);
			}

			using (_mocks.Playback())
			{
				bool result = _target.ValidateMatrix(_matrix, _optimizationPreferences, _daysOffPreferences);
				Assert.IsFalse(result);
			}
		}

		private void commonMocks()
		{
			Expect.Call(_matrix.Person).Return(_person);
			Expect.Call(_matrix.EffectivePeriodDays)
			      .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_scheduleDayPro}));
			Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_currentScheduleDay);
			Expect.Call(_scheduleDayPro.Day).Return(DateOnly.MinValue);
			Expect.Call(_range.ScheduledDay(DateOnly.MinValue)).Return(_originalScheduleDay);
		}

        [Test]
        public void ShouldAddOriginalNumberOfDaysOff()
        {
            
            var optimizerPrefrences = _optimizationPreferences;
            optimizerPrefrences.Shifts.KeepShifts = true;
            optimizerPrefrences.Shifts.KeepShiftsValue = 0;
            _daysOffPreferences.UseKeepExistingDaysOff = false;

            using (_mocks.Record())
            {
                commonMocks();

                Expect.Call(_originalScheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);
                Expect.Call(_scheduleDayEquator.MainShiftEquals(_originalScheduleDay, _currentScheduleDay)).Return(true);
            }

            using (_mocks.Playback())
            {
                bool result = _target.ValidateMatrix(_matrix, _optimizationPreferences, _daysOffPreferences);
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void ShouldAddChangedDaysOff()
        {

            var optimizerPrefrences = _optimizationPreferences;
            _daysOffPreferences.UseKeepExistingDaysOff = true;
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            using (_mocks.Record())
            {
                commonMocks();

                Expect.Call(_originalScheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);
                //Expect.Call(_scheduleDayEquator.MainShiftEquals(_originalScheduleDay, _currentScheduleDay)).Return(true);
                Expect.Call(_scheduleDayEquator.DayOffEquals(scheduleDay, scheduleDay)).IgnoreArguments().Return(false );
            }

            using (_mocks.Playback())
            {
                bool result = _target.ValidateMatrix(_matrix, _optimizationPreferences, _daysOffPreferences);
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void ShouldReturnFalseWhenUseKeepExistingDaysOffIsUsed()
        {

            var optimizerPrefrences = _optimizationPreferences;
            _daysOffPreferences.UseKeepExistingDaysOff = true;
            _daysOffPreferences.KeepExistingDaysOffValue = 50;
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            using (_mocks.Record())
            {
                commonMocks();

                Expect.Call(_originalScheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);
               Expect.Call(_scheduleDayEquator.DayOffEquals(scheduleDay, scheduleDay)).IgnoreArguments().Return(false);
            }

            using (_mocks.Playback())
            {
                bool result = _target.ValidateMatrix(_matrix, _optimizationPreferences, _daysOffPreferences);
                Assert.IsFalse(result);
            }
        }
	}
}
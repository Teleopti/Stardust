using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class MatrixRestrictionLockerTest
    {
        private IMatrixRestrictionLocker _target;
        private MockRepository _mocks;
        private ISchedulingOptions _schedulingOptions;
        private IRestrictionExtractor _extractor;
        private IScheduleMatrixPro _matrix;
        private DateOnly _dayToCheck;
        private IScheduleDay _schedulePart;
        private IRotationRestriction _rotationRestriction;
        private IAvailabilityRestriction _availabilityRestriction;
        private IPreferenceRestriction _preferenceRestriction;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _mocks.StrictMock<ISchedulingResultStateHolder>();
            _schedulingOptions = new SchedulingOptions();
            _schedulingOptions.UseAvailability = false;
            _schedulingOptions.UseRotations = false;
            _schedulingOptions.UseAvailability = false;
            _schedulingOptions.UseStudentAvailability = false;
            _schedulingOptions.UsePreferencesMustHaveOnly = false;
            _schedulingOptions.UsePreferences = false;
			_extractor = _mocks.StrictMock<IRestrictionExtractor>();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _target = new MatrixRestrictionLocker(_schedulingOptions, _extractor);
            _dayToCheck = new DateOnly(2010, 1, 1);
			_schedulePart = _mocks.StrictMock<IScheduleDay>();
        }

        [Test]
        public void VerifyGetLockedIfRotationRestrictionIsDayOffAndDayOffIsScheduled()
        {
            using(_mocks.Record())
            {
                mockExpectationsForAll();
                rotationExpectationsForAll();
                Expect.Call(_rotationRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("hej"))).Repeat.Once();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Once();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix);
            }
            Assert.AreEqual(_dayToCheck, result[0]);
        }

        [Test]
        public void VerifyNotGetLockedIfRotationRestrictionIsDayOffAndShiftIsScheduled()
        {
            using (_mocks.Record())
            {
                mockExpectationsForAll();
                rotationExpectationsForAll();
                Expect.Call(_rotationRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("hej"))).Repeat.Once();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Once();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix);
            }
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void VerifyNotLockedIfRotationRestrictionIsShiftAndScheduledWithDayOff()
        {
            using (_mocks.Record())
            {
                mockExpectationsForAll();
                rotationExpectationsForAll();
                Expect.Call(_rotationRestriction.DayOffTemplate).Return(null).Repeat.Once();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Once();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix);
            }
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void VerifyGetLockedIfRotationRestrictionIsShiftAndScheduledWithShift()
        {
            using (_mocks.Record())
            {
                mockExpectationsForAll();
                rotationExpectationsForAll();
                Expect.Call(_rotationRestriction.DayOffTemplate).Return(null).Repeat.Once();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Once();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix);
            }
            Assert.AreEqual(_dayToCheck, result[0]);
        }

        [Test]
        public void VerifyGetLockedIfAvailabilityRestrictionIsDayOffAndDayOffIsScheduled()
        {
            using (_mocks.Record())
            {
                mockExpectationsForAll();
                availabilityExpectationsForAll();
                Expect.Call(_availabilityRestriction.NotAvailable).Return(true).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix);
            }
            Assert.AreEqual(_dayToCheck, result[0]);
        }

        [Test]
        public void VerifyNotGetLockedIfAvailabilityRestrictionIsDayOffAndShiftIsScheduled()
        {
            using (_mocks.Record())
            {
                mockExpectationsForAll();
                availabilityExpectationsForAll();
                Expect.Call(_availabilityRestriction.NotAvailable).Return(true).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix);
            }
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void VerifyNotLockedIfAvailabilityRestrictionIsShiftAndScheduledWithDayOff()
        {
            using (_mocks.Record())
            {
                mockExpectationsForAll();
                availabilityExpectationsForAll();
                Expect.Call(_availabilityRestriction.NotAvailable).Return(false).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix);
            }
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void VerifyGetLockedIfPreferenceIsDayOffAndScheduledWithDayOff()
        {
            using (_mocks.Record())
            {
                mockExpectationsForAll();
                preferenceExpectationsForAll();
                Expect.Call(_preferenceRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("hej"))).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix);
            }
            Assert.AreEqual(_dayToCheck, result[0]);
        }

		[Test]
		public void VerifyGetLockedIfPreferenceIsDayOffAndScheduledWithVacationOnContractDayOff()
		{
			using (_mocks.Record())
			{
				mockExpectationsForAll();
				preferenceExpectationsForAll();
				Expect.Call(_preferenceRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("hej"))).Repeat.Any();
				Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.ContractDayOff).Repeat.Any();
			}
			IList<DateOnly> result;
			using (_mocks.Playback())
			{
				result = _target.Execute(_matrix);
			}
			Assert.AreEqual(_dayToCheck, result[0]);
		}

        [Test]
        public void VerifyNotGetLockedIfPreferenceIsDayOffAndScheduledWithShift()
        {
            using (_mocks.Record())
            {
                mockExpectationsForAll();
                preferenceExpectationsForAll();
                Expect.Call(_preferenceRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("hej"))).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix);
            }
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void VerifyGetLockedIfPreferenceIsShiftAndScheduledWithShift()
        {
            using (_mocks.Record())
            {
                mockExpectationsForAll();
                preferenceExpectationsForAll();
                Expect.Call(_preferenceRestriction.DayOffTemplate).Return(null).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix);
            }
            Assert.AreEqual(_dayToCheck, result[0]);
        }

        [Test]
        public void VerifyNotGetLockedIfPreferenceIsShiftAndScheduledWithDayOff()
        {
            using (_mocks.Record())
            {
                mockExpectationsForAll();
                preferenceExpectationsForAll();
                Expect.Call(_preferenceRestriction.DayOffTemplate).Return(null).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix);
            }
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void VerifyGetRightsDaysLockedWithAvailability()
        {
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();

            _extractor = _mocks.StrictMock<IRestrictionExtractor>();

            IScheduleDay dayOff = _mocks.StrictMock<IScheduleDay>();
            IScheduleDay mainShift = _mocks.StrictMock<IScheduleDay>();

            IScheduleDayPro day1 = _mocks.StrictMock<IScheduleDayPro>();
            IScheduleDayPro day2 = _mocks.StrictMock<IScheduleDayPro>();
            IScheduleDayPro day3 = _mocks.StrictMock<IScheduleDayPro>();
            IScheduleDayPro day4 = _mocks.StrictMock<IScheduleDayPro>();

            IAvailabilityRestriction notAvailable = _mocks.StrictMock<IAvailabilityRestriction>();
            IAvailabilityRestriction available = _mocks.StrictMock<IAvailabilityRestriction>();

            _schedulingOptions.UseAvailability = true;
            _target = new MatrixRestrictionLocker(_schedulingOptions, _extractor);

            DateOnly expectedDateOnly = new DateOnly(2001, 01, 01);

            using(_mocks.Record())
            {
                Expect.Call(_matrix.UnlockedDays)
                    .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { day1, day2, day3, day4 }));

                Expect.Call(day1.DaySchedulePart())
                    .Return(dayOff);
                Expect.Call(day2.DaySchedulePart())
                    .Return(dayOff);
                Expect.Call(day3.DaySchedulePart())
                    .Return(mainShift);
                Expect.Call(day4.DaySchedulePart())
                    .Return(mainShift);

                Expect.Call(dayOff.SignificantPart())
                    .Return(SchedulePartView.DayOff).Repeat.AtLeastOnce();
                Expect.Call(mainShift.SignificantPart())
                    .Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();

                Expect.Call(() => _extractor.Extract(dayOff));
                Expect.Call(() => _extractor.Extract(dayOff));
                Expect.Call(() => _extractor.Extract(mainShift));
                Expect.Call(() => _extractor.Extract(mainShift));

                Expect.Call(_extractor.AvailabilityList)
                    .Return(new List<IAvailabilityRestriction> {notAvailable});
                Expect.Call(_extractor.AvailabilityList)
                    .Return(new List<IAvailabilityRestriction> { available });
                Expect.Call(_extractor.AvailabilityList)
                    .Return(new List<IAvailabilityRestriction> { notAvailable });
                Expect.Call(_extractor.AvailabilityList)
                    .Return(new List<IAvailabilityRestriction> { available });

                Expect.Call(notAvailable.IsRestriction())
                    .Return(true).Repeat.AtLeastOnce();
                Expect.Call(available.IsRestriction())
                    .Return(true).Repeat.AtLeastOnce();

                Expect.Call(notAvailable.NotAvailable)
                    .Return(true).Repeat.AtLeastOnce();
                Expect.Call(available.NotAvailable)
                    .Return(false).Repeat.AtLeastOnce();

                Expect.Call(day1.Day)
                    .Return(expectedDateOnly).Repeat.Any();
                Expect.Call(day2.Day)
                    .Return(expectedDateOnly.AddDays(1)).Repeat.Any(); 
                Expect.Call(day3.Day)
                    .Return(expectedDateOnly.AddDays(2)).Repeat.Any();
                Expect.Call(day4.Day)
                    .Return(expectedDateOnly.AddDays(3)).Repeat.Any();

            }
            using (_mocks.Playback())
            {
                IList<DateOnly> result = _target.Execute(_matrix);
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(expectedDateOnly, result[0]);
            }
        }

        private void mockExpectationsForAll()
        {
			IScheduleDayPro scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            IList<IScheduleDayPro> unlockedDays = new List<IScheduleDayPro>{scheduleDayPro};
            Expect.Call(_matrix.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(unlockedDays)).Repeat.Any();
            Expect.Call(scheduleDayPro.DaySchedulePart()).Return(_schedulePart).Repeat.Any();
            _extractor.Extract(_schedulePart);
            LastCall.Repeat.Any();
            Expect.Call(scheduleDayPro.Day).Return(_dayToCheck).Repeat.Any();
        }

        private void rotationExpectationsForAll()
        {
            _schedulingOptions.UseRotations = true;
			_rotationRestriction = _mocks.StrictMock<IRotationRestriction>();
            Expect.Call(_extractor.RotationList).Return(new List<IRotationRestriction> { _rotationRestriction }).Repeat.Any();
            Expect.Call(_rotationRestriction.IsRestriction()).Return(true).Repeat.Once();
        }

        private void availabilityExpectationsForAll()
        {
            _schedulingOptions.UseAvailability = true;
            _availabilityRestriction = _mocks.StrictMock<IAvailabilityRestriction>();
            Expect.Call(_extractor.AvailabilityList).Return(new List<IAvailabilityRestriction> { _availabilityRestriction }).Repeat.Any();
            Expect.Call(_availabilityRestriction.IsRestriction()).Return(true).Repeat.Once();
        }

        private void preferenceExpectationsForAll()
        {
            _schedulingOptions.UsePreferences = true;
			_preferenceRestriction = _mocks.StrictMock<IPreferenceRestriction>();
            Expect.Call(_extractor.PreferenceList).Return(new List<IPreferenceRestriction> { _preferenceRestriction }).Repeat.Any();
            Expect.Call(_preferenceRestriction.IsRestriction()).Return(true).Repeat.Any();
            Expect.Call(_preferenceRestriction.MustHave).Return(false).Repeat.Any();
        }
    }
}
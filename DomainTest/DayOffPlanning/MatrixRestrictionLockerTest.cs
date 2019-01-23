using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class MatrixRestrictionLockerTest
    {
        private MatrixRestrictionLocker _target;
        private MockRepository _mocks;
        private SchedulingOptions _schedulingOptions;
        private IRestrictionExtractor _extractor;
        private IScheduleMatrixPro _matrix;
        private DateOnly _dayToCheck;
        private IScheduleDay _schedulePart;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _mocks.StrictMock<ISchedulingResultStateHolder>();
            _schedulingOptions = new SchedulingOptions
            {
	            UseAvailability = false,
	            UseRotations = false,
	            UseStudentAvailability = false,
	            UsePreferencesMustHaveOnly = false,
	            UsePreferences = false
            };
	        _extractor = _mocks.StrictMock<IRestrictionExtractor>();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _target = new MatrixRestrictionLocker(_extractor);
            _dayToCheck = new DateOnly(2010, 1, 1);
			_schedulePart = _mocks.StrictMock<IScheduleDay>();
        }

        [Test]
        public void VerifyGetLockedIfRotationRestrictionIsDayOffAndDayOffIsScheduled()
        {
			var rotationRestriction = _mocks.StrictMock<IRotationRestriction>();
            using(_mocks.Record())
            {
				mockExpectationsForAll(new ExtractedRestrictionResult(new RestrictionCombiner(), new List<IRotationRestriction> { rotationRestriction },
						Enumerable.Empty<IAvailabilityRestriction>(), Enumerable.Empty<IPreferenceRestriction>(),
						Enumerable.Empty<IStudentAvailabilityDay>()));
				
				Expect.Call(rotationRestriction.IsRestriction()).Return(true).Repeat.Once();
                Expect.Call(rotationRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("hej"))).Repeat.Once();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Once();
            }
            IEnumerable<DateOnly> result;
            using (_mocks.Playback())
			{
				_schedulingOptions.UseRotations = true;
                result = _target.Execute(_matrix,_schedulingOptions);
            }
            Assert.AreEqual(_dayToCheck, result.First());
        }

        [Test]
        public void VerifyNotGetLockedIfRotationRestrictionIsDayOffAndShiftIsScheduled()
		{
			var rotationRestriction = _mocks.StrictMock<IRotationRestriction>();
            using (_mocks.Record())
            {
				mockExpectationsForAll(new ExtractedRestrictionResult(new RestrictionCombiner(), new List<IRotationRestriction> { rotationRestriction },
						Enumerable.Empty<IAvailabilityRestriction>(), Enumerable.Empty<IPreferenceRestriction>(),
						Enumerable.Empty<IStudentAvailabilityDay>()));

				Expect.Call(rotationRestriction.IsRestriction()).Return(true).Repeat.Once();
                Expect.Call(rotationRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("hej"))).Repeat.Once();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Once();
            }
			IEnumerable<DateOnly> result;
            using (_mocks.Playback())
            {
				_schedulingOptions.UseRotations = true;
                result = _target.Execute(_matrix,_schedulingOptions);
            }
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void VerifyNotLockedIfRotationRestrictionIsShiftAndScheduledWithDayOff()
		{
			var rotationRestriction = _mocks.StrictMock<IRotationRestriction>();
            using (_mocks.Record())
            {
				mockExpectationsForAll(new ExtractedRestrictionResult(new RestrictionCombiner(), new List<IRotationRestriction> { rotationRestriction },
						Enumerable.Empty<IAvailabilityRestriction>(), Enumerable.Empty<IPreferenceRestriction>(),
						Enumerable.Empty<IStudentAvailabilityDay>()));

				Expect.Call(rotationRestriction.IsRestriction()).Return(true).Repeat.Once();
                Expect.Call(rotationRestriction.DayOffTemplate).Return(null).Repeat.Once();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Once();
            }
			IEnumerable<DateOnly> result;
            using (_mocks.Playback())
			{
				_schedulingOptions.UseRotations = true;
                result = _target.Execute(_matrix,_schedulingOptions);
            }
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void VerifyGetLockedIfRotationRestrictionIsShiftAndScheduledWithShift()
		{
			var rotationRestriction = _mocks.StrictMock<IRotationRestriction>();
            using (_mocks.Record())
            {
				mockExpectationsForAll(new ExtractedRestrictionResult(new RestrictionCombiner(), new List<IRotationRestriction> { rotationRestriction },
						Enumerable.Empty<IAvailabilityRestriction>(), Enumerable.Empty<IPreferenceRestriction>(),
						Enumerable.Empty<IStudentAvailabilityDay>()));

				Expect.Call(rotationRestriction.IsRestriction()).Return(true).Repeat.Once();
                Expect.Call(rotationRestriction.DayOffTemplate).Return(null).Repeat.Once();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Once();
            }
			IEnumerable<DateOnly> result;
            using (_mocks.Playback())
			{
				_schedulingOptions.UseRotations = true;
                result = _target.Execute(_matrix, _schedulingOptions);
            }
            Assert.AreEqual(_dayToCheck, result.First());
        }

        [Test]
        public void VerifyGetLockedIfAvailabilityRestrictionIsDayOffAndDayOffIsScheduled()
        {
			var availabilityRestriction = _mocks.StrictMock<IAvailabilityRestriction>();
			using (_mocks.Record())
            {
				mockExpectationsForAll(new ExtractedRestrictionResult(new RestrictionCombiner(), Enumerable.Empty<IRotationRestriction>(),
						new List<IAvailabilityRestriction> { availabilityRestriction }, Enumerable.Empty<IPreferenceRestriction>(),
						Enumerable.Empty<IStudentAvailabilityDay>()));
				
				Expect.Call(availabilityRestriction.IsRestriction()).Return(true).Repeat.Once();
				Expect.Call(availabilityRestriction.NotAvailable).Return(true).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            }
			IEnumerable<DateOnly> result;
            using (_mocks.Playback())
            {
				_schedulingOptions.UseAvailability = true;
				result = _target.Execute(_matrix, _schedulingOptions);
            }
            Assert.AreEqual(_dayToCheck, result.First());
        }

        [Test]
        public void VerifyNotGetLockedIfAvailabilityRestrictionIsDayOffAndShiftIsScheduled()
		{
			var availabilityRestriction = _mocks.StrictMock<IAvailabilityRestriction>();
			using (_mocks.Record())
			{
				mockExpectationsForAll(new ExtractedRestrictionResult(new RestrictionCombiner(), Enumerable.Empty<IRotationRestriction>(),
						new List<IAvailabilityRestriction> { availabilityRestriction }, Enumerable.Empty<IPreferenceRestriction>(),
						Enumerable.Empty<IStudentAvailabilityDay>()));

				Expect.Call(availabilityRestriction.IsRestriction()).Return(true).Repeat.Once();
                Expect.Call(availabilityRestriction.NotAvailable).Return(true).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            }
			IEnumerable<DateOnly> result;
            using (_mocks.Playback())
            {
				_schedulingOptions.UseAvailability = true;
				result = _target.Execute(_matrix, _schedulingOptions);
            }
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void VerifyNotLockedIfAvailabilityRestrictionIsShiftAndScheduledWithDayOff()
		{
			var availabilityRestriction = _mocks.StrictMock<IAvailabilityRestriction>();
			using (_mocks.Record())
			{
				mockExpectationsForAll(new ExtractedRestrictionResult(new RestrictionCombiner(), Enumerable.Empty<IRotationRestriction>(),
						new List<IAvailabilityRestriction> { availabilityRestriction }, Enumerable.Empty<IPreferenceRestriction>(),
						Enumerable.Empty<IStudentAvailabilityDay>()));

				Expect.Call(availabilityRestriction.IsRestriction()).Return(true).Repeat.Once();
                Expect.Call(availabilityRestriction.NotAvailable).Return(false).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            }
			IEnumerable<DateOnly> result;
            using (_mocks.Playback())
            {
				_schedulingOptions.UseAvailability = true;
				result = _target.Execute(_matrix, _schedulingOptions);
            }
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void VerifyGetLockedIfPreferenceIsDayOffAndScheduledWithDayOff()
        {
			var preferenceRestriction = _mocks.StrictMock<IPreferenceRestriction>();
			using (_mocks.Record())
            {
				mockExpectationsForAll(new ExtractedRestrictionResult(new RestrictionCombiner(), Enumerable.Empty<IRotationRestriction>(),
						Enumerable.Empty<IAvailabilityRestriction>(), new List<IPreferenceRestriction> { preferenceRestriction },
						Enumerable.Empty<IStudentAvailabilityDay>()));

				Expect.Call(preferenceRestriction.IsRestriction()).Return(true).Repeat.Any();
				Expect.Call(preferenceRestriction.MustHave).Return(false).Repeat.Any();
                Expect.Call(preferenceRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("hej"))).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            }
			IEnumerable<DateOnly> result;
            using (_mocks.Playback())
			{
				_schedulingOptions.UsePreferences = true;
                result = _target.Execute(_matrix, _schedulingOptions);
            }
            Assert.AreEqual(_dayToCheck, result.First());
        }

		[Test]
		public void VerifyGetLockedIfPreferenceIsDayOffAndScheduledWithVacationOnContractDayOff()
		{
			var preferenceRestriction = _mocks.StrictMock<IPreferenceRestriction>();
			using (_mocks.Record())
			{
				mockExpectationsForAll(new ExtractedRestrictionResult(new RestrictionCombiner(), Enumerable.Empty<IRotationRestriction>(),
						Enumerable.Empty<IAvailabilityRestriction>(), new List<IPreferenceRestriction> { preferenceRestriction },
						Enumerable.Empty<IStudentAvailabilityDay>()));

				Expect.Call(preferenceRestriction.IsRestriction()).Return(true).Repeat.Any();
				Expect.Call(preferenceRestriction.MustHave).Return(false).Repeat.Any();
				Expect.Call(preferenceRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("hej"))).Repeat.Any();
				Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.ContractDayOff).Repeat.Any();
			}
			IEnumerable<DateOnly> result;
			using (_mocks.Playback())
			{
				_schedulingOptions.UsePreferences = true;
				result = _target.Execute(_matrix, _schedulingOptions);
			}
			Assert.AreEqual(_dayToCheck, result.First());
		}

        [Test]
        public void VerifyNotGetLockedIfPreferenceIsDayOffAndScheduledWithShift()
		{
			var preferenceRestriction = _mocks.StrictMock<IPreferenceRestriction>();
			using (_mocks.Record())
			{
				mockExpectationsForAll(new ExtractedRestrictionResult(new RestrictionCombiner(), Enumerable.Empty<IRotationRestriction>(),
						Enumerable.Empty<IAvailabilityRestriction>(), new List<IPreferenceRestriction> { preferenceRestriction },
						Enumerable.Empty<IStudentAvailabilityDay>()));

				Expect.Call(preferenceRestriction.IsRestriction()).Return(true).Repeat.Any();
				Expect.Call(preferenceRestriction.MustHave).Return(false).Repeat.Any();
                Expect.Call(preferenceRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("hej"))).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            }
			IEnumerable<DateOnly> result;
            using (_mocks.Playback())
			{
				_schedulingOptions.UsePreferences = true;
                result = _target.Execute(_matrix, _schedulingOptions);
            }
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void VerifyGetLockedIfPreferenceIsShiftAndScheduledWithShift()
		{
			var preferenceRestriction = _mocks.StrictMock<IPreferenceRestriction>();
			using (_mocks.Record())
			{
				mockExpectationsForAll(new ExtractedRestrictionResult(new RestrictionCombiner(), Enumerable.Empty<IRotationRestriction>(),
						Enumerable.Empty<IAvailabilityRestriction>(), new List<IPreferenceRestriction> { preferenceRestriction },
						Enumerable.Empty<IStudentAvailabilityDay>()));

				Expect.Call(preferenceRestriction.IsRestriction()).Return(true).Repeat.Any();
				Expect.Call(preferenceRestriction.MustHave).Return(false).Repeat.Any();
                Expect.Call(preferenceRestriction.DayOffTemplate).Return(null).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            }
			IEnumerable<DateOnly> result;
            using (_mocks.Playback())
			{
				_schedulingOptions.UsePreferences = true;
                result = _target.Execute(_matrix, _schedulingOptions);
            }
            Assert.AreEqual(_dayToCheck, result.First());
        }

        [Test]
        public void VerifyNotGetLockedIfPreferenceIsShiftAndScheduledWithDayOff()
		{
			var preferenceRestriction = _mocks.StrictMock<IPreferenceRestriction>();
			using (_mocks.Record())
			{
				mockExpectationsForAll(new ExtractedRestrictionResult(new RestrictionCombiner(), Enumerable.Empty<IRotationRestriction>(),
						Enumerable.Empty<IAvailabilityRestriction>(), new List<IPreferenceRestriction> { preferenceRestriction },
						Enumerable.Empty<IStudentAvailabilityDay>()));

				Expect.Call(preferenceRestriction.IsRestriction()).Return(true).Repeat.Any();
				Expect.Call(preferenceRestriction.MustHave).Return(false).Repeat.Any();
                Expect.Call(preferenceRestriction.DayOffTemplate).Return(null).Repeat.Any();
                Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            }
			IEnumerable<DateOnly> result;
            using (_mocks.Playback())
			{
				_schedulingOptions.UsePreferences = true;
                result = _target.Execute(_matrix,_schedulingOptions);
            }
            Assert.AreEqual(0, result.Count());
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
            _target = new MatrixRestrictionLocker(_extractor);

            DateOnly expectedDateOnly = new DateOnly(2001, 01, 01);

            using(_mocks.Record())
            {
                Expect.Call(_matrix.UnlockedDays)
                    .Return(new HashSet<IScheduleDayPro> { day1, day2, day3, day4 });

                Expect.Call(day1.DaySchedulePart()).Return(dayOff);
                Expect.Call(day2.DaySchedulePart()).Return(dayOff);
                Expect.Call(day3.DaySchedulePart()).Return(mainShift);
                Expect.Call(day4.DaySchedulePart()).Return(mainShift);

                Expect.Call(dayOff.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.AtLeastOnce();
                Expect.Call(mainShift.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();

	            Expect.Call(_extractor.Extract(dayOff))
		            .Return(new ExtractedRestrictionResult(new RestrictionCombiner(), Enumerable.Empty<IRotationRestriction>(),
						new List<IAvailabilityRestriction> { notAvailable }, Enumerable.Empty<IPreferenceRestriction>(),
			            Enumerable.Empty<IStudentAvailabilityDay>()));
	            Expect.Call(_extractor.Extract(dayOff))
		            .Return(new ExtractedRestrictionResult(new RestrictionCombiner(), Enumerable.Empty<IRotationRestriction>(),
						new List<IAvailabilityRestriction> { available }, Enumerable.Empty<IPreferenceRestriction>(),
			            Enumerable.Empty<IStudentAvailabilityDay>()));
				Expect.Call(_extractor.Extract(mainShift))
		            .Return(new ExtractedRestrictionResult(new RestrictionCombiner(), Enumerable.Empty<IRotationRestriction>(),
						new List<IAvailabilityRestriction> { notAvailable }, Enumerable.Empty<IPreferenceRestriction>(),
			            Enumerable.Empty<IStudentAvailabilityDay>()));
				Expect.Call(_extractor.Extract(mainShift))
		            .Return(new ExtractedRestrictionResult(new RestrictionCombiner(), Enumerable.Empty<IRotationRestriction>(),
						new List<IAvailabilityRestriction> { available }, Enumerable.Empty<IPreferenceRestriction>(),
			            Enumerable.Empty<IStudentAvailabilityDay>()));
                
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
                var result = _target.Execute(_matrix,_schedulingOptions);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual(expectedDateOnly, result.First());
            }
        }

        private void mockExpectationsForAll(IExtractedRestrictionResult extractedRestrictionResult)
        {
			var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var unlockedDays = new HashSet<IScheduleDayPro> { scheduleDayPro};
            Expect.Call(_matrix.UnlockedDays).Return(unlockedDays).Repeat.Any();
            Expect.Call(scheduleDayPro.DaySchedulePart()).Return(_schedulePart).Repeat.Any();
            Expect.Call(_extractor.Extract(_schedulePart)).Return(extractedRestrictionResult).Repeat.Any();
            Expect.Call(scheduleDayPro.Day).Return(_dayToCheck).Repeat.Any();
        }
    }
}
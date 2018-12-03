using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Overtime;


namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class AdjustOvertimeLengthBasedOnAvailabilityTest
    {
        private AdjustOvertimeLengthBasedOnAvailability _target;
		private IScheduleDay _scheduleDay;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;
		private DateTimePeriod _dateTimePeriod;
		private MockRepository _mock;
		private DateTime _shiftEndingTime;
		private DateTime _shiftStartTime;
        
        [SetUp]
        public void Setup()
        {
            _target = new AdjustOvertimeLengthBasedOnAvailability();
			_mock = new MockRepository();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_projectionService = _mock.StrictMock<IProjectionService>();
			_visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
	        _shiftStartTime = new DateTime(2014, 03, 05, 14, 30, 0, DateTimeKind.Utc);
	        _shiftEndingTime = new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_shiftStartTime, _shiftEndingTime);
        }

		[Test]
	    public void ShouldReturnNullIfAvailabilityNotIntersectWithOvertimePeriod()
	    {
			var overtimeLayerLength = TimeSpan.FromHours(2);
			var overtimePeriod = new DateTimePeriod(_shiftEndingTime, _shiftEndingTime.Add(overtimeLayerLength));
			var overtimeAvailabilityPeriod = new DateTimePeriod(new DateTime(2014, 03, 05, 13, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 15, 0, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriod, overtimePeriod, _scheduleDay);
				Assert.IsFalse(result.HasValue);
			}   
	    }

		[Test]
	    public void ShouldReturnNullIfAvailabilityStartAfterShiftEnd()
	    {
			var overtimeLayerLength = TimeSpan.FromHours(2);
			var overtimePeriod = new DateTimePeriod(_shiftEndingTime, _shiftEndingTime.Add(overtimeLayerLength));
			var overtimeAvailabilityPeriod = new DateTimePeriod(new DateTime(2014, 03, 05, 16, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 16, 30, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriod, overtimePeriod, _scheduleDay);
				Assert.IsFalse(result.HasValue);
			}    
	    }

		[Test]
		public void ShouldReturnNullIfAvailabilityEndBeforeShiftStart()
		{
			var overtimeLayerLength = TimeSpan.FromHours(2);
			var overtimePeriod = new DateTimePeriod(_shiftStartTime.Add(-overtimeLayerLength), _shiftStartTime);
			var overtimeAvailabilityPeriod = new DateTimePeriod(new DateTime(2014, 03, 05, 13, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 14, 0, 0, DateTimeKind.Utc));
			
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriod, overtimePeriod, _scheduleDay);
				Assert.IsFalse(result.HasValue);
			}
		}

		[Test]
	    public void ShouldAdjustOvertimePeriodBeforeShift()
	    {
			var overtimeLayerLength = TimeSpan.FromHours(2);
			var overtimePeriod = new DateTimePeriod(_shiftStartTime.Add(-overtimeLayerLength), _shiftStartTime);
			var overtimeAvailabilityPeriod = new DateTimePeriod(new DateTime(2014, 03, 05, 13, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 15, 0, 0, DateTimeKind.Utc));
			var expected = new DateTimePeriod(new DateTime(2014, 03, 05, 13, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 14, 30, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriod, overtimePeriod, _scheduleDay);
				Assert.IsTrue(result.HasValue);
				Assert.AreEqual(expected, result);
			}   
	    }

		[Test]
		public void ShouldAdjustOvertimePeriodAfterShift()
		{
			var overtimeLayerLength = TimeSpan.FromHours(2);
			var overtimePeriod = new DateTimePeriod(_shiftEndingTime, _shiftEndingTime.Add(overtimeLayerLength));
			var overtimeAvailabilityPeriod = new DateTimePeriod(new DateTime(2014, 03, 05, 13, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 16, 0, 0, DateTimeKind.Utc));
			var expected = new DateTimePeriod(new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 16, 0, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriod, overtimePeriod, _scheduleDay);
				Assert.IsTrue(result.HasValue);
				Assert.AreEqual(expected, result);
			}
		}

		[Test]
		public void ShouldAdjustOvertimePeriodForOverNightShifts()
		{
			_shiftStartTime = new DateTime(2014, 03, 05, 23, 30, 0, DateTimeKind.Utc);
			_shiftEndingTime = new DateTime(2014, 03, 06, 0, 30, 0, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_shiftStartTime, _shiftEndingTime);

			var overtimeLayerLength = TimeSpan.FromHours(1);
			var overtimePeriod = new DateTimePeriod(_shiftEndingTime, _shiftEndingTime.Add(overtimeLayerLength));
			var overtimeAvailabilityPeriod = new DateTimePeriod(new DateTime(2014, 03, 06, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 06, 1, 0, 0, DateTimeKind.Utc));
			var expected = new DateTimePeriod(new DateTime(2014, 03, 06, 0, 30, 0, DateTimeKind.Utc), new DateTime(2014, 03, 06, 1, 0, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriod, overtimePeriod, _scheduleDay);
				Assert.IsTrue(result.HasValue);
				Assert.AreEqual(expected, result);
			}
		}


		[Test]
		public void TestIssue27272()
		{
			TimeSpan overtimeLayerLength = TimeSpan.FromHours(3).Add(TimeSpan.FromMinutes(30));
			var shiftEndTime = new DateTime(2011, 05, 14, 14, 30, 0, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(shiftEndTime.AddHours(-1), shiftEndTime);
			var overtimePeriod = new DateTimePeriod(shiftEndTime, shiftEndTime.Add(overtimeLayerLength));
			var overtimeAvailabilityPeriodUtc = new DateTimePeriod(new DateTime(2011, 05, 14, 14, 30, 0, DateTimeKind.Utc), new DateTime(2011, 05, 14, 20, 0, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);	
			}

			using (_mock.Playback())
			{
				var adjustedOvertimeLength = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriodUtc, overtimePeriod, _scheduleDay);
				Assert.IsTrue(adjustedOvertimeLength.HasValue);
				Assert.AreEqual(TimeSpan.FromHours(3).Add(TimeSpan.FromMinutes(30)), adjustedOvertimeLength.Value.ElapsedTime());	
			}	
		}
    }
}

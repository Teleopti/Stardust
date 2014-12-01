﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class CalculateBestOvertimeTest
    {
        private ICalculateBestOvertime _target;
        private DateTimePeriod _period1;
        private DateTimePeriod _period2;
        private DateTimePeriod _period3;
        private DateTimePeriod _period4;
        private DateTimePeriod _period5;
        private DateTimePeriod _period6;
        private DateTime _shiftEndingTime;
	    private IScheduleDay _scheduleDay;
	    private MockRepository _mock;
        private List<OvertimePeriodValue> _mappedData;
	    private IProjectionService _projectionService;
	    private IVisualLayerCollection _visualLayerCollection;
	    private DateTimePeriod _dateTimePeriod;

        [SetUp]
        public void Setup()
        {
            _target = new CalculateBestOvertime();
            _period1 = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 16, 45, 0, DateTimeKind.Utc));
            _period2 = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 45, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 00, 0, DateTimeKind.Utc));
            _period3 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 00, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 15, 0, DateTimeKind.Utc));
            _period4 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 15, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc));
            _period5 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 45, 0, DateTimeKind.Utc));
            _period6 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 45, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 18, 00, 0, DateTimeKind.Utc));
            _shiftEndingTime = new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc);
            _mappedData = new List<OvertimePeriodValue>();
            _mappedData.Add(new OvertimePeriodValue(_period1, 0.78));
            _mappedData.Add(new OvertimePeriodValue(_period2, 1.52));
            _mappedData.Add(new OvertimePeriodValue(_period3, -5.98));
            _mappedData.Add(new OvertimePeriodValue(_period4, -3.55));
            _mappedData.Add(new OvertimePeriodValue(_period5, -6.75));
            _mappedData.Add(new OvertimePeriodValue(_period6, -4.6));
			_mock = new MockRepository();
	        _scheduleDay = _mock.StrictMock<IScheduleDay>();
	        _projectionService = _mock.StrictMock<IProjectionService>();
	        _visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
			_dateTimePeriod = new DateTimePeriod(_shiftEndingTime.AddHours(-1), _shiftEndingTime);
        }

        [Test]
        public void TestForOvertimeWithExactOrEqualLimit()
        {
            var oneHourTimeSpan = new TimeSpan(0, 1, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(oneHourTimeSpan, oneHourTimeSpan);

	        using (_mock.Record())
	        {
		        Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
		        Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
		        Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
	        }

	        using (_mock.Playback())
	        {
				Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, _mappedData, _scheduleDay, 15).First().ElapsedTime(), oneHourTimeSpan);	   
	        }   
        }

        [Test]
        public void TestForOvertimeFrom0To1Hour()
        {
            var oneHourTimeSpan = new TimeSpan(0, 0, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(oneHourTimeSpan, oneHourTimeSpan.Add(TimeSpan.FromHours(1)));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

	        using (_mock.Playback())
	        {
				Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, _mappedData, _scheduleDay, 15).First().ElapsedTime(), oneHourTimeSpan.Add(TimeSpan.FromHours(1)));   
	        }     
        }

        [Test]
        public void TestForOvertimeFrom15MinutesTo1Hour()
        {
            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 15, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 1, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, _mappedData, _scheduleDay, 15).First().ElapsedTime(), overtimeLimitEndTimeSpan);
			}    
        }

        [Test]
        public void TestForOvertimeFrom15MinutesTo3Hour()
        {
            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 15, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 3, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(new TimeSpan(0, 1, 30, 0), _target.GetBestOvertime(overtimeDurantion, _mappedData, _scheduleDay, 15).First().ElapsedTime());
			}
        }

        [Test]
        public void TestForOvertimeWithLongerLimit()
        {
            var overtimeLimitStartTimeSpan = new TimeSpan(0, 3, 0, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 3, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, _mappedData, _scheduleDay, 15).First().ElapsedTime(), TimeSpan.Zero);
			}    
        }

        [Test]
        public void TestForOvertimeWithNoOvertimeFound()
        {
            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 0, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 3, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

            var mappedData = new List<OvertimePeriodValue>();
            mappedData.Add(new OvertimePeriodValue(_period1, 0.78));
            mappedData.Add(new OvertimePeriodValue(_period2, 1.52));
            mappedData.Add(new OvertimePeriodValue(_period3, 5.98));
            mappedData.Add(new OvertimePeriodValue(_period4, 3.55));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, mappedData, _scheduleDay, 15).First().ElapsedTime(), TimeSpan.Zero);
			}    
        }

        [Test]
        public void TestForOvertimeWithDiffRange()
        {
            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 30, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 0, 45, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

            var mappedData = new List<OvertimePeriodValue>();
            mappedData.Add(new OvertimePeriodValue(_period1, 0.78));
            mappedData.Add(new OvertimePeriodValue(_period2, 1.52));
            mappedData.Add(new OvertimePeriodValue(_period3, -1));
            mappedData.Add(new OvertimePeriodValue(_period4, -3.55));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(TimeSpan.Zero, _target.GetBestOvertime(overtimeDurantion, mappedData, _scheduleDay, 15).First().ElapsedTime());
			}     
        }

        [Test]
        public void ShouldAllwaysConsiderAllInteralsFromEndOfTheShift()
        {
            var mappedData = new List<OvertimePeriodValue>();
            mappedData.Add(new OvertimePeriodValue(_period1, -1));
            mappedData.Add(new OvertimePeriodValue(_period2, 2));
            mappedData.Add(new OvertimePeriodValue(_period3, -5.98));
            mappedData.Add(new OvertimePeriodValue(_period4, -3.55));

            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 0, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 0, 30, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.AtLeastOnce();
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(TimeSpan.FromMinutes(15), _target.GetBestOvertime(overtimeDurantion, mappedData, _scheduleDay, 15).First().ElapsedTime());

				overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 30, 0);
				overtimeLimitEndTimeSpan = new TimeSpan(0, 0, 30, 0);
				overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

				Assert.AreEqual(TimeSpan.Zero, _target.GetBestOvertime(overtimeDurantion, mappedData, _scheduleDay, 15).First().ElapsedTime());
			}  
        }

        [Test]
        public void ShouldOnlyPutOvertimeIfSumOfRelativeDifferencesIsNegative()
        {
            var mappedData = new List<OvertimePeriodValue>();
            mappedData.Add(new OvertimePeriodValue(_period1, 1));
            mappedData.Add(new OvertimePeriodValue(_period2, 2));
            mappedData.Add(new OvertimePeriodValue(_period3, 3));
            mappedData.Add(new OvertimePeriodValue(_period4, -6));

            var overtimeLimitStartTimeSpan = new TimeSpan(0, 1, 0, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 1, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(TimeSpan.Zero, _target.GetBestOvertime(overtimeDurantion, mappedData, _scheduleDay, 15).First().ElapsedTime());
			}
        }

        [Test]
        public void ShouldPutOvertimeIfSumOfRelativeDifferencesIsNegative()
        {
            var mappedData = new List<OvertimePeriodValue>();
            mappedData.Add(new OvertimePeriodValue(_period1, 1));
            mappedData.Add(new OvertimePeriodValue(_period2, 2));
            mappedData.Add(new OvertimePeriodValue(_period3, 3));
            mappedData.Add(new OvertimePeriodValue(_period4, -7));

            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 0, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 1, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(TimeSpan.FromHours(1), _target.GetBestOvertime(overtimeDurantion, mappedData, _scheduleDay, 15).First().ElapsedTime());
			}
        }

        [Test]
        public void ShouldHandleMinSettingOfZeroMaxSettingOfZero()
        {
            var mappedData = new List<OvertimePeriodValue>();
            mappedData.Add(new OvertimePeriodValue(_period1, -1));
            mappedData.Add(new OvertimePeriodValue(_period2, 2));
            mappedData.Add(new OvertimePeriodValue(_period3, -5.98));
            mappedData.Add(new OvertimePeriodValue(_period4, -3.55));

            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 0, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 0, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(TimeSpan.Zero, _target.GetBestOvertime(overtimeDurantion, mappedData, _scheduleDay, 15).First().ElapsedTime());
			}
        }
    }
}

using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class PeriodOffsetCalculatorTest
    {
        private PeriodOffsetCalculator _target;
        private MockRepository _mocks; 
        private IScheduleDay _sourceScheduleDay;
        private IScheduleDay _targetScheduleDay;
        private DateTimePeriod _sourceDateTimePeriod;
        private DateTimePeriod _targetDateTimePeriod;

        private DateTime _sourceStartDateTime; 
        private DateTime _sourceEndDateTime; 
        private DateTime _targetStartDateTime ;
        private DateTime _targetEndDateTime;

        private ICccTimeZoneInfo _singaporeTimeZone;
        private ICccTimeZoneInfo _hawaiiTimeZone;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _sourceScheduleDay = _mocks.StrictMock<IScheduleDay>();
            _targetScheduleDay = _mocks.StrictMock<IScheduleDay>();
            
            _singaporeTimeZone = CccTimeZoneInfoFactory.SingaporeTimeZoneInfo(); // -10
            _hawaiiTimeZone = CccTimeZoneInfoFactory.HawaiiTimeZoneInfo(); // +8

            _target = new PeriodOffsetCalculator();
        }

        [Test]
        public void VerifyCalculatePeriodOffsetIsZeroWithinSameTimeZoneAndSameDay()
        {
            _sourceStartDateTime = new DateTime(2010, 06, 30, 16, 0, 0, DateTimeKind.Utc); // local - 8 => utc
            _sourceEndDateTime = new DateTime(2010, 07, 01, 16, 0, 0, DateTimeKind.Utc);
            _targetStartDateTime = new DateTime(2010, 06, 30, 16, 0, 0, DateTimeKind.Utc); // local - 8 => utc
            _targetEndDateTime = new DateTime(2010, 07, 01, 16, 0, 0, DateTimeKind.Utc);
            _sourceDateTimePeriod = new DateTimePeriod(_sourceStartDateTime, _sourceEndDateTime);
            _targetDateTimePeriod = new DateTimePeriod(_targetStartDateTime, _targetEndDateTime);

            using (_mocks.Record())
            {
                Expect.Call(_sourceScheduleDay.TimeZone).Return(_hawaiiTimeZone).Repeat.AtLeastOnce();
                Expect.Call(_targetScheduleDay.TimeZone).Return(_hawaiiTimeZone).Repeat.AtLeastOnce();
                Expect.Call(_sourceScheduleDay.Period).Return(_sourceDateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_targetScheduleDay.Period).Return(_targetDateTimePeriod).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, true);
                Assert.AreEqual(new TimeSpan(), offSet);
            }
        }

        [Test]
        public void VerifyCalculatePeriodOffsetIsTheDifferenceBetweenSourceAndTargetDayWithinTimeZone()
        {
            _sourceStartDateTime = new DateTime(2010, 06, 30, 16, 0, 0, DateTimeKind.Utc); // local - 8 => utc
            _sourceEndDateTime = new DateTime(2010, 07, 01, 16, 0, 0, DateTimeKind.Utc);
            _targetStartDateTime = new DateTime(2010, 07, 30, 16, 0, 0, DateTimeKind.Utc); // local - 8 => utc
            _targetEndDateTime = new DateTime(2010, 07, 31, 16, 0, 0, DateTimeKind.Utc);
            _sourceDateTimePeriod = new DateTimePeriod(_sourceStartDateTime, _sourceEndDateTime);
            _targetDateTimePeriod = new DateTimePeriod(_targetStartDateTime, _targetEndDateTime);

            using (_mocks.Record())
            {
                Expect.Call(_sourceScheduleDay.TimeZone).Return(_hawaiiTimeZone).Repeat.AtLeastOnce();
                Expect.Call(_targetScheduleDay.TimeZone).Return(_hawaiiTimeZone).Repeat.AtLeastOnce();
                Expect.Call(_sourceScheduleDay.Period).Return(_sourceDateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_targetScheduleDay.Period).Return(_targetDateTimePeriod).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, true);
                Assert.AreEqual(new TimeSpan(30, 0, 0, 0), offSet);
            }
        }

        [Test]
        public void VerifyPeriodOffsetShowsTheTimeZoneChangesEvenWithSameDay()
        {

            _sourceStartDateTime = new DateTime(2010, 06, 30, 16, 0, 0, DateTimeKind.Utc); // local - 8 => utc
            _sourceEndDateTime = new DateTime(2010, 07, 01, 16, 0, 0, DateTimeKind.Utc);
            _targetStartDateTime = new DateTime(2010, 07, 01, 10, 0, 0, DateTimeKind.Utc); // local + 10 => utc
            _targetEndDateTime = new DateTime(2010, 07, 02, 10, 0, 0, DateTimeKind.Utc);
            _sourceDateTimePeriod = new DateTimePeriod(_sourceStartDateTime, _sourceEndDateTime);
            _targetDateTimePeriod = new DateTimePeriod(_targetStartDateTime, _targetEndDateTime);

            using (_mocks.Record())
            {
                Expect.Call(_sourceScheduleDay.Period).Return(_sourceDateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_targetScheduleDay.Period).Return(_targetDateTimePeriod).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, false);
                Assert.AreEqual(new TimeSpan(0, 18, 0, 0), offSet);
            }
        }

        [Test]
        public void VerifyPeriodOffsetBetweenTimeZonesIsZeroInSameDayIfIgnoreTimeZoneChangesParameterTrue()
        {
            _sourceStartDateTime = new DateTime(2010, 06, 30, 16, 0, 0, DateTimeKind.Utc); // local - 8 => utc
            _sourceEndDateTime = new DateTime(2010, 07, 01, 16, 0, 0, DateTimeKind.Utc);
            _targetStartDateTime = new DateTime(2010, 07, 01, 10, 0, 0, DateTimeKind.Utc); // local + 10 => utc
            _targetEndDateTime = new DateTime(2010, 07, 02, 10, 0, 0, DateTimeKind.Utc);
            _sourceDateTimePeriod = new DateTimePeriod(_sourceStartDateTime, _sourceEndDateTime);
            _targetDateTimePeriod = new DateTimePeriod(_targetStartDateTime, _targetEndDateTime);

            using (_mocks.Record())
            {
                Expect.Call(_sourceScheduleDay.TimeZone).Return(_singaporeTimeZone).Repeat.AtLeastOnce();
                Expect.Call(_targetScheduleDay.TimeZone).Return(_hawaiiTimeZone).Repeat.AtLeastOnce();
                Expect.Call(_sourceScheduleDay.Period).Return(_sourceDateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_targetScheduleDay.Period).Return(_targetDateTimePeriod).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, true);
                Assert.AreEqual(new TimeSpan(), offSet);
            }
        }

        [Test]
        public void VerifyCalculatePeriodOffsetIsTheDifferenceBetweenSourceAndTargetDayPlusTimeZoneChanges()
        {
            _sourceStartDateTime = new DateTime(2010, 06, 30, 16, 0, 0, DateTimeKind.Utc); // local - 8 => utc
            _sourceEndDateTime = new DateTime(2010, 07, 01, 16, 0, 0, DateTimeKind.Utc);
            _targetStartDateTime = new DateTime(2010, 07, 31, 10, 0, 0, DateTimeKind.Utc); // local + 10 => utc
            _targetEndDateTime = new DateTime(2010, 08, 1, 10, 0, 0, DateTimeKind.Utc);
            _sourceDateTimePeriod = new DateTimePeriod(_sourceStartDateTime, _sourceEndDateTime);
            _targetDateTimePeriod = new DateTimePeriod(_targetStartDateTime, _targetEndDateTime);

            using (_mocks.Record())
            {
                Expect.Call(_sourceScheduleDay.Period).Return(_sourceDateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_targetScheduleDay.Period).Return(_targetDateTimePeriod).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, false);
                Assert.AreEqual(new TimeSpan(30, 18, 0, 0), offSet);
            }
        }

        [Test]
        public void VerifyCalculatePeriodOffsetIsTheDifferenceBetweenSourceAndTargetDayOnlyIfIgnoreTimeZoneChangesParameterTrue()
        {
            _sourceStartDateTime = new DateTime(2010, 06, 30, 16, 0, 0, DateTimeKind.Utc); // local - 8 => utc
            _sourceEndDateTime = new DateTime(2010, 07, 01, 16, 0, 0, DateTimeKind.Utc);
            _targetStartDateTime = new DateTime(2010, 07, 31, 10, 0, 0, DateTimeKind.Utc); // local + 10 => utc
            _targetEndDateTime = new DateTime(2010, 08, 1, 10, 0, 0, DateTimeKind.Utc);
            _sourceDateTimePeriod = new DateTimePeriod(_sourceStartDateTime, _sourceEndDateTime);
            _targetDateTimePeriod = new DateTimePeriod(_targetStartDateTime, _targetEndDateTime);

            using (_mocks.Record())
            {
                Expect.Call(_sourceScheduleDay.TimeZone).Return(_singaporeTimeZone).Repeat.AtLeastOnce();
                Expect.Call(_targetScheduleDay.TimeZone).Return(_hawaiiTimeZone).Repeat.AtLeastOnce();
                Expect.Call(_sourceScheduleDay.Period).Return(_sourceDateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_targetScheduleDay.Period).Return(_targetDateTimePeriod).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, true);
                Assert.AreEqual(new TimeSpan(30, 0, 0, 0), offSet);
            }
        }

        [Test]
        public void VerifyCalculatePeriodOffsetCalculatesWithDaylightSavingsWithinTimeZone()
        {
            _sourceStartDateTime = new DateTime(2011, 03, 15, 16, 0, 0, DateTimeKind.Utc); // local - 8 => utc
            _sourceEndDateTime = new DateTime(2011, 03, 16, 16, 0, 0, DateTimeKind.Utc);
            _targetStartDateTime = new DateTime(2011, 04, 15, 16, 0, 0, DateTimeKind.Utc); // local - 8 + 1 (summertime) => utc
            _targetEndDateTime = new DateTime(2011, 04, 16, 16, 0, 0, DateTimeKind.Utc);
            _sourceDateTimePeriod = new DateTimePeriod(_sourceStartDateTime, _sourceEndDateTime);
            _targetDateTimePeriod = new DateTimePeriod(_targetStartDateTime, _targetEndDateTime);

            using (_mocks.Record())
            {
                Expect.Call(_sourceScheduleDay.Period).Return(_sourceDateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_targetScheduleDay.Period).Return(_targetDateTimePeriod).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, false);
                Assert.AreEqual(new TimeSpan(31, 0, 0, 0), offSet);
            }
        }

        [Test]
        public void VerifyCalculatePeriodOffsetCalculatesWithDaylightSavingsWithinTimeZoneEvenIfIgnoreTimeZoneChanges()
            {
                CccTimeZoneInfo stockholmTimeZone = CccTimeZoneInfoFactory.StockholmTimeZoneInfo();

            _sourceStartDateTime = new DateTime(2011, 03, 15, 16, 0, 0, DateTimeKind.Utc); // local - 8 => utc
            _sourceEndDateTime = new DateTime(2011, 03, 16, 16, 0, 0, DateTimeKind.Utc);
            _targetStartDateTime = new DateTime(2011, 04, 15, 16, 0, 0, DateTimeKind.Utc); // local - 8 + 1 (summertime) => utc
            _targetEndDateTime = new DateTime(2011, 04, 16, 16, 0, 0, DateTimeKind.Utc);
            _sourceDateTimePeriod = new DateTimePeriod(_sourceStartDateTime, _sourceEndDateTime);
            _targetDateTimePeriod = new DateTimePeriod(_targetStartDateTime, _targetEndDateTime);

            using(_mocks.Record())
            {
                Expect.Call(_sourceScheduleDay.TimeZone).Return(stockholmTimeZone).Repeat.AtLeastOnce();
                Expect.Call(_targetScheduleDay.TimeZone).Return(stockholmTimeZone).Repeat.AtLeastOnce();
                Expect.Call(_sourceScheduleDay.Period).Return(_sourceDateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_targetScheduleDay.Period).Return(_targetDateTimePeriod).Repeat.AtLeastOnce();
            }
            using(_mocks.Playback())
            {
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, true);
                Assert.AreEqual(new TimeSpan(31, 0, 0, 0), offSet);
            }
        }

            [Test]
            public void VerifyCalculatePeriodOffsetCalculatesWithDaylightSavingsBetweenTimeZonesEvenIfIgnoreTimeZoneChanges()
            {
                CccTimeZoneInfo stockholmTimeZone = CccTimeZoneInfoFactory.StockholmTimeZoneInfo();
                CccTimeZoneInfo helsinkiTimeZone = CccTimeZoneInfoFactory.HelsinkiTimeZoneInfo();

                _sourceStartDateTime = new DateTime(2011, 03, 15, 23, 0, 0, DateTimeKind.Utc); // local - 1 => utc
                _sourceEndDateTime = new DateTime(2011, 03, 16, 23, 0, 0, DateTimeKind.Utc);
                _targetStartDateTime = new DateTime(2011, 04, 15, 23, 0, 0, DateTimeKind.Utc); // local - 2 + 1 (summertime) => utc
                _targetEndDateTime = new DateTime(2011, 04, 16, 23, 0, 0, DateTimeKind.Utc);
                _sourceDateTimePeriod = new DateTimePeriod(_sourceStartDateTime, _sourceEndDateTime);
                _targetDateTimePeriod = new DateTimePeriod(_targetStartDateTime, _targetEndDateTime);

                
                using (_mocks.Record())
                {
                    Expect.Call(_sourceScheduleDay.TimeZone).Return(stockholmTimeZone).Repeat.AtLeastOnce();
                    Expect.Call(_targetScheduleDay.TimeZone).Return(helsinkiTimeZone).Repeat.AtLeastOnce();
                    Expect.Call(_sourceScheduleDay.Period).Return(_sourceDateTimePeriod).Repeat.AtLeastOnce();
                    Expect.Call(_targetScheduleDay.Period).Return(_targetDateTimePeriod).Repeat.AtLeastOnce();
                }
                using (_mocks.Playback())
                {
                    TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, true);
                    Assert.AreEqual(new TimeSpan(31, 1, 0, 0), offSet);
                }
            }

    }
}

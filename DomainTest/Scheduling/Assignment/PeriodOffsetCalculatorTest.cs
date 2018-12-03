using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


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

        private TimeZoneInfo _singaporeTimeZone;
        private TimeZoneInfo _hawaiiTimeZone;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _sourceScheduleDay = _mocks.StrictMock<IScheduleDay>();
            _targetScheduleDay = _mocks.StrictMock<IScheduleDay>();

			TimeZoneGuard.Instance.TimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
            _singaporeTimeZone = TimeZoneInfoFactory.SingaporeTimeZoneInfo(); // -10
            _hawaiiTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo(); // +8

            _target = new PeriodOffsetCalculator();
        }

		[TearDown]
		public void Teardown()
		{
			TimeZoneGuard.Instance.TimeZone = null;
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
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, true, _sourceDateTimePeriod);
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
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, true, _sourceDateTimePeriod);
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
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, false, _sourceDateTimePeriod);
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
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, true, _sourceDateTimePeriod);
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
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, false, _sourceDateTimePeriod);
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
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, true, _sourceDateTimePeriod);
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
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, false, _sourceDateTimePeriod);
                Assert.AreEqual(new TimeSpan(31, 0, 0, 0), offSet);
            }
        }

		[Test]
	    public void ShouldCalculatePeriodOffsetWithDaylightSavings()
	    {
			_sourceDateTimePeriod = new DateTimePeriod(2016, 03, 25, 23, 2016, 03, 26, 23);
			_targetDateTimePeriod = new DateTimePeriod(2016, 03, 26, 23, 2016, 03, 27, 22);

			using (_mocks.Record())
			{
				Expect.Call(_sourceScheduleDay.Period).Return(_sourceDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_targetScheduleDay.Period).Return(_targetDateTimePeriod).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				var offSet = _target.CalculatePeriodOffsetConsiderDaylightSavings(_sourceScheduleDay, _targetScheduleDay, new DateTimePeriod(2016, 3, 26, 9, 2016, 3, 26, 10));
				offSet.Should().Be.EqualTo(TimeSpan.FromHours(23));
			}    
	    }

		[Test]
	    public void ShouldUseCurrentViewPointWhenCalculatingPeriodOffsetWithDaylightSaving()
		{
			TimeZoneGuard.Instance.TimeZone = TimeZoneInfoFactory.NewYorkTimeZoneInfo();

			_sourceDateTimePeriod = new DateTimePeriod(2016, 10, 25, 13, 2016, 10, 25, 21);
			_targetDateTimePeriod = new DateTimePeriod(2016, 11, 1, 13, 2016, 11, 1, 21);

			using (_mocks.Record())
			{
				Expect.Call(_sourceScheduleDay.Period).Return(_sourceDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_targetScheduleDay.Period).Return(_targetDateTimePeriod).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				var offSet = _target.CalculatePeriodOffsetConsiderDaylightSavings(_sourceScheduleDay, _targetScheduleDay, new DateTimePeriod(2016, 10, 25, 9, 2016, 10, 25, 10));
				offSet.Should().Be.EqualTo(TimeSpan.FromDays(7));
			}

			TimeZoneGuard.Instance.TimeZone = null;
		}

        [Test]
        public void VerifyCalculatePeriodOffsetCalculatesWithDaylightSavingsWithinTimeZoneEvenIfIgnoreTimeZoneChanges()
            {
                TimeZoneInfo stockholmTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();

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
                TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, true, _sourceDateTimePeriod);
                Assert.AreEqual(new TimeSpan(31, 0, 0, 0), offSet);
            }
        }

            [Test]
            public void VerifyCalculatePeriodOffsetCalculatesWithDaylightSavingsBetweenTimeZonesEvenIfIgnoreTimeZoneChanges()
            {
                TimeZoneInfo stockholmTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
                TimeZoneInfo helsinkiTimeZone = TimeZoneInfoFactory.HelsinkiTimeZoneInfo();

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
                    TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, true, _sourceDateTimePeriod);
                    Assert.AreEqual(new TimeSpan(31, 1, 0, 0), offSet);
                }
            }

            [Test]
            public void ShouldMoveCorrectOnDayOffDaylightSavingChange()
            {
                TimeZoneInfo stockholmTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
                var shiftStart = new DateTime(2012, 03, 24, 3, 0, 0, DateTimeKind.Utc);
                _sourceStartDateTime = new DateTime(2012, 03, 23, 23, 0, 0, DateTimeKind.Utc); // local - 1 => utc
                _sourceEndDateTime = new DateTime(2012, 03, 24, 23, 0, 0, DateTimeKind.Utc);
                // local - 1  1 (summertime) => utc (changes on the 25th if the shift starts before the shift no problem, if after we must remove one hour more
                _targetStartDateTime = new DateTime(2012, 03, 24, 23, 0, 0, DateTimeKind.Utc);
                _targetEndDateTime = new DateTime(2012, 03, 25, 23, 0, 0, DateTimeKind.Utc);
                _sourceDateTimePeriod = new DateTimePeriod(_sourceStartDateTime, _sourceEndDateTime);
                _targetDateTimePeriod = new DateTimePeriod(_targetStartDateTime, _targetEndDateTime);
                var shiftPeriod = new DateTimePeriod(shiftStart, shiftStart.AddHours(8));

                using (_mocks.Record())
                {
                    Expect.Call(_sourceScheduleDay.TimeZone).Return(stockholmTimeZone).Repeat.AtLeastOnce();
                    Expect.Call(_targetScheduleDay.TimeZone).Return(stockholmTimeZone).Repeat.AtLeastOnce();
                    Expect.Call(_sourceScheduleDay.Period).Return(_sourceDateTimePeriod).Repeat.AtLeastOnce();
                    Expect.Call(_targetScheduleDay.Period).Return(_targetDateTimePeriod).Repeat.AtLeastOnce();
                }
                using (_mocks.Playback())
                {
                    TimeSpan offSet = _target.CalculatePeriodOffset(_sourceScheduleDay, _targetScheduleDay, true, shiftPeriod);
                    Assert.AreEqual(new TimeSpan(0, 23, 0, 0), offSet);
                }
            }
    }
}

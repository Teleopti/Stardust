using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Time
{
    [TestFixture]
    public class SetupDateTimePeriodToDefaultWorkHoursTest
    {
        private ISetupDateTimePeriod _target;
        private DateTime _start = new DateTime(2001, 12, 27, 8, 0, 0, DateTimeKind.Utc);
        private DateTime _end = new DateTime(2001, 12, 27, 17, 0, 0, DateTimeKind.Utc);
        private TimeZoneInfo _timeZone;
        private DateTimePeriod _targetDateTimePeriod;
        private TimeZoneInfo _info;
        private IScheduleDay _scheduleDay;

        private DateTimePeriod _defaultLocal;

        [SetUp]
        public void Setup()
        {
            TimeSpan localStartTimeOfDay = TimeSpan.FromHours(6);
            TimeSpan localEndTimeOfDay = TimeSpan.FromHours(13);

            _timeZone = TimeZoneInfo.CreateCustomTimeZone("MyOwn", TimeSpan.FromMinutes(12), "MyOwn", "MyOwn");
            _info = (_timeZone);
            _start = new DateTime(2001, 12, 27, 2, 0, 0, DateTimeKind.Utc);
            _end = new DateTime(2001, 12, 31, 12, 0, 0, DateTimeKind.Utc);
            
            var timePeriod = new DateTimePeriod(_start, _end);
            _defaultLocal = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(timePeriod.LocalStartDateTime.Add(localStartTimeOfDay), timePeriod.LocalStartDateTime.Add(localEndTimeOfDay));
            _target = new SetupDateTimePeriodToDefaultLocalHours(_defaultLocal, null, _info);
            _targetDateTimePeriod = new DateTimePeriod(_start, _end);
            _scheduleDay = new SchedulePartFactoryForDomain().CreatePartWithMainShift();

        }

        [Test]
        public void VerifyGetsEightToFiveUtc()
        {
            _target = new SetupDateTimePeriodToDefaultLocalHours(_defaultLocal,null, (TimeZoneInfo.Utc));

            DateTimePeriod result = _target.Period;
            Assert.AreEqual(_defaultLocal.LocalStartDateTime.TimeOfDay, result.StartDateTime.TimeOfDay);
            Assert.AreEqual(_defaultLocal.LocalEndDateTime.TimeOfDay, result.EndDateTime.TimeOfDay);

        }

        [Test]
        public void TimeIsSetFromLocalTimeZoneInfo()
        {
            _target = new SetupDateTimePeriodToDefaultLocalHours(_defaultLocal,null, _info);

            DateTimePeriod p = _target.Period;
            Assert.AreEqual(_defaultLocal.LocalStartDateTime.TimeOfDay, p.StartDateTimeLocal(_info).TimeOfDay);
            Assert.AreEqual(_defaultLocal.LocalEndDateTime.TimeOfDay, p.EndDateTimeLocal(_info).TimeOfDay);

        }
        [Test]
        public void VerifyDateIsFromTheStartDate()
        {
            _target = new SetupDateTimePeriodToDefaultLocalHours(_defaultLocal,null, _info);

            DateTimePeriod p = _target.Period;
            Assert.AreEqual(_targetDateTimePeriod.StartDateTime.Date, p.StartDateTimeLocal(_info).Date);
            Assert.AreEqual(_targetDateTimePeriod.StartDateTime.Date, p.EndDateTimeLocal(_info).Date);

        }

        [Test]
        public void CanCreateOvertimeOnPeriod()
        {
            _target = new SetupDateTimePeriodToDefaultLocalHours(_defaultLocal,_scheduleDay, _info);
            DateTimePeriod? period = _scheduleDay.PersonAssignmentCollectionDoNotUse().First().Period;
            var newPeriod = new DateTimePeriod(period.Value.EndDateTime, period.Value.EndDateTime.AddHours(1));
            Assert.AreEqual(newPeriod, _target.Period);
        }
    }
}
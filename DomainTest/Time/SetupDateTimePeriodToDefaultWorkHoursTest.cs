using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Time
{
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class SetupDateTimePeriodToDefaultWorkHoursTest
    {
        private ISetupDateTimePeriod _target;
		private readonly DateTimePeriod _targetDateTimePeriod = new DateTimePeriod(2001,12,27,2,2001,12,31,12);
		private readonly TimeZoneInfo _info = TimeZoneInfo.CreateCustomTimeZone("MyOwn", TimeSpan.FromMinutes(12), "MyOwn", "MyOwn");
		private IScheduleDay _scheduleDay;

        private DateTimePeriod _defaultLocal;

        private void setup()
        {
            TimeSpan localStartTimeOfDay = TimeSpan.FromHours(6);
            TimeSpan localEndTimeOfDay = TimeSpan.FromHours(13);
			
	        var startDateTimeLocal = _targetDateTimePeriod.StartDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone);
	        _defaultLocal = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTimeLocal.Add(localStartTimeOfDay), startDateTimeLocal.Add(localEndTimeOfDay), TimeZoneHelper.CurrentSessionTimeZone);
            _target = new SetupDateTimePeriodToDefaultLocalHours(_defaultLocal, null, _info);
            _scheduleDay = new SchedulePartFactoryForDomain().CreatePartWithMainShift();
        }

        [Test]
        public void VerifyGetsEightToFiveUtc()
        {
	        setup();

			_target = new SetupDateTimePeriodToDefaultLocalHours(_defaultLocal,null, (TimeZoneInfo.Utc));

            DateTimePeriod result = _target.Period;
	        var localPeriod = _defaultLocal.TimePeriod(TimeZoneHelper.CurrentSessionTimeZone);
            Assert.AreEqual(localPeriod.StartTime, result.StartDateTime.TimeOfDay);
            Assert.AreEqual(localPeriod.EndTime, result.EndDateTime.TimeOfDay);
        }

        [Test]
        public void TimeIsSetFromLocalTimeZoneInfo()
        {
			setup();
			_target = new SetupDateTimePeriodToDefaultLocalHours(_defaultLocal,null, _info);

            DateTimePeriod p = _target.Period;
	        var localPeriod = _defaultLocal.TimePeriod(TimeZoneHelper.CurrentSessionTimeZone);
            Assert.AreEqual(localPeriod.StartTime, p.StartDateTimeLocal(_info).TimeOfDay);
            Assert.AreEqual(localPeriod.EndTime, p.EndDateTimeLocal(_info).TimeOfDay);
        }

        [Test]
        public void VerifyDateIsFromTheStartDate()
        {
			setup();
			_target = new SetupDateTimePeriodToDefaultLocalHours(_defaultLocal,null, _info);

            DateTimePeriod p = _target.Period;
            Assert.AreEqual(_targetDateTimePeriod.StartDateTime.Date, p.StartDateTimeLocal(_info).Date);
            Assert.AreEqual(_targetDateTimePeriod.StartDateTime.Date, p.EndDateTimeLocal(_info).Date);

        }

        [Test]
        public void CanCreateOvertimeOnPeriod()
        {
			setup();
			_target = new SetupDateTimePeriodToDefaultLocalHours(_defaultLocal,_scheduleDay, _info);
            DateTimePeriod? period = _scheduleDay.PersonAssignment().Period;
            var newPeriod = new DateTimePeriod(period.Value.EndDateTime, period.Value.EndDateTime.AddHours(1));
            Assert.AreEqual(newPeriod, _target.Period);
        }
    }
}
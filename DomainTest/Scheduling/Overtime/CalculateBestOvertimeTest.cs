using System;
using System.Collections.Generic;
using NUnit.Framework;
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
        private List<OvertimePeriodValue> _mappedData;

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
        }

        [Test]
        public void TestForOvertimeWithExactOrEqualLimit()
        {
            var oneHourTimeSpan = new TimeSpan(0, 1, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(oneHourTimeSpan, oneHourTimeSpan);

            Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, _mappedData, _shiftEndingTime, 15), oneHourTimeSpan);
        }

        [Test]
        public void TestForOvertimeFrom0To1Hour()
        {
            var oneHourTimeSpan = new TimeSpan(0, 0, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(oneHourTimeSpan, oneHourTimeSpan.Add(TimeSpan.FromHours(1)));

            Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, _mappedData, _shiftEndingTime, 15), oneHourTimeSpan.Add(TimeSpan.FromHours(1)));
        }

        [Test]
        public void TestForOvertimeFrom15MinutesTo1Hour()
        {
            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 15, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 1, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

            Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, _mappedData, _shiftEndingTime, 15), overtimeLimitEndTimeSpan);
        }

        [Test]
        public void TestForOvertimeFrom15MinutesTo3Hour()
        {
            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 15, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 3, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

            Assert.AreEqual(new TimeSpan(0, 1, 30, 0), _target.GetBestOvertime(overtimeDurantion, _mappedData, _shiftEndingTime, 15));
        }

        [Test]
        public void TestForOvertimeWithLongerLimit()
        {
            var overtimeLimitStartTimeSpan = new TimeSpan(0, 3, 0, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 3, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

            Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, _mappedData, _shiftEndingTime, 15), TimeSpan.Zero);
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

            Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, mappedData, _shiftEndingTime, 15), TimeSpan.Zero);
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

            Assert.AreEqual(TimeSpan.Zero, _target.GetBestOvertime(overtimeDurantion, mappedData, _shiftEndingTime, 15));
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

            Assert.AreEqual(TimeSpan.FromMinutes(15), _target.GetBestOvertime(overtimeDurantion, mappedData, _shiftEndingTime, 15));

            overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 30, 0);
            overtimeLimitEndTimeSpan = new TimeSpan(0, 0, 30, 0);
            overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

            Assert.AreEqual(TimeSpan.Zero, _target.GetBestOvertime(overtimeDurantion, mappedData, _shiftEndingTime, 15));
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

            Assert.AreEqual(TimeSpan.Zero, _target.GetBestOvertime(overtimeDurantion, mappedData, _shiftEndingTime, 15));
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

            Assert.AreEqual(TimeSpan.FromHours(1), _target.GetBestOvertime(overtimeDurantion, mappedData, _shiftEndingTime, 15));
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

            Assert.AreEqual(TimeSpan.Zero, _target.GetBestOvertime(overtimeDurantion, mappedData, _shiftEndingTime, 15));
        }
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class AdjustOvertimeLengthBasedOnAvailabilityTest
    {
        private AdjustOvertimeLengthBasedOnAvailability _target;
        
        [SetUp]
        public void Setup()
        {
            _target = new AdjustOvertimeLengthBasedOnAvailability();
        }

        [Test]
        public void ReturnZeroIfAvailabilityIsBeforeShiftEnds()
        {
            TimeSpan overtimeLayerLength = TimeSpan.FromHours(2);
            var shiftEndTime = new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc);
            var overtimeAvailabilityPeriodUtc = new DateTimePeriod(new DateTime(2014, 03, 05, 13, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 15, 0, 0, DateTimeKind.Utc));
            var adjustedOvertimeLength = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriodUtc , overtimeLayerLength, shiftEndTime);
            Assert.AreEqual(TimeSpan.Zero, adjustedOvertimeLength);
        }

        [Test]
        public void ReturnZeroIfAvailabilityIsAfterTheShiftEndWithAGap()
        {
            TimeSpan overtimeLayerLength = TimeSpan.FromHours(2);
            var shiftEndTime = new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc);
            var overtimeAvailabilityPeriodUtc = new DateTimePeriod(new DateTime(2014, 03, 05, 17, 30, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 18, 30, 0, DateTimeKind.Utc));
            var adjustedOvertimeLength = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriodUtc, overtimeLayerLength, shiftEndTime);
            Assert.AreEqual(TimeSpan.Zero, adjustedOvertimeLength);
        }

        [Test]
        public void ReturnAdjustedDurationIfAvailabilityIsRightAfterTheShiftEnds()
        {
            TimeSpan overtimeLayerLength = TimeSpan.FromHours(3);
            var shiftEndTime = new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc);
            var overtimeAvailabilityPeriodUtc = new DateTimePeriod(new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 16, 30, 0, DateTimeKind.Utc));
            var adjustedOvertimeLength = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriodUtc, overtimeLayerLength, shiftEndTime);
            Assert.AreEqual(TimeSpan.FromHours(1), adjustedOvertimeLength);
        }

        [Test]
        public void ReturnZeroIfAvailabilityEndsWhenShiftShiftEnds()
        {
            TimeSpan overtimeLayerLength = TimeSpan.FromHours(3);
            var shiftEndTime = new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc);
            var overtimeAvailabilityPeriodUtc = new DateTimePeriod(new DateTime(2014, 03, 05, 14, 30, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc));
            var adjustedOvertimeLength = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriodUtc, overtimeLayerLength, shiftEndTime);
            Assert.AreEqual(TimeSpan.Zero, adjustedOvertimeLength);
        }

        [Test]
        public void ReturnAdjustedDurationIfAvailabilityIsIn_ValidRange()
        {
            TimeSpan overtimeLayerLength = TimeSpan.FromHours(1);
            var shiftEndTime = new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc);
            var overtimeAvailabilityPeriodUtc = new DateTimePeriod(new DateTime(2014, 03, 05, 14, 30, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 16, 0, 0, DateTimeKind.Utc));
            var adjustedOvertimeLength = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriodUtc, overtimeLayerLength, shiftEndTime);
            Assert.AreEqual(TimeSpan.FromMinutes(30), adjustedOvertimeLength);
        }

        [Test]
        public void ReturnAdjustedDurationInMinutes()
        {
            TimeSpan overtimeLayerLength = TimeSpan.FromHours(3);
            var shiftEndTime = new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc);
            var overtimeAvailabilityPeriodUtc = new DateTimePeriod(new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 15, 45, 0, DateTimeKind.Utc));
            var adjustedOvertimeLength = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriodUtc, overtimeLayerLength, shiftEndTime);
            Assert.AreEqual(TimeSpan.FromMinutes(15), adjustedOvertimeLength);
        }

        [Test]
        public void ReturnAdjustedDurationForOverNightShifts()
        {
            TimeSpan overtimeLayerLength = TimeSpan.FromHours(3);
            var shiftEndTime = new DateTime(2014, 03, 06, 0, 30, 0, DateTimeKind.Utc);
            var overtimeAvailabilityPeriodUtc = new DateTimePeriod(new DateTime(2014, 03, 05, 23, 30, 0, DateTimeKind.Utc), new DateTime(2014, 03, 06, 02, 0, 0, DateTimeKind.Utc));
            var adjustedOvertimeLength = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriodUtc, overtimeLayerLength, shiftEndTime);
            Assert.AreEqual(new TimeSpan(0,1,30,0), adjustedOvertimeLength);
        }

        [Test]
        public void ReturnAdjustedDurationIfOvertimeLengthEqualsDuration()
        {
            TimeSpan overtimeLayerLength = TimeSpan.FromMinutes(15);
            var shiftEndTime = new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc);
            var overtimeAvailabilityPeriodUtc = new DateTimePeriod(new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc), new DateTime(2014, 03, 05, 16, 0, 0, DateTimeKind.Utc));
            var adjustedOvertimeLength = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriodUtc, overtimeLayerLength, shiftEndTime);
            Assert.AreEqual(TimeSpan.FromMinutes(15), adjustedOvertimeLength);
        }
       
        [Test]
        public void TestIssue27272()
        {
			TimeSpan overtimeLayerLength = TimeSpan.FromHours(3).Add(TimeSpan.FromMinutes(30));
			var shiftEndTime = new DateTime(2011, 05, 14, 14, 30, 0, DateTimeKind.Utc);
			var overtimeAvailabilityPeriodUtc = new DateTimePeriod(new DateTime(2011, 05, 14, 14, 30, 0, DateTimeKind.Utc), new DateTime(2011, 05, 14, 20, 0, 0, DateTimeKind.Utc));
			var adjustedOvertimeLength = _target.AdjustOvertimeDuration(overtimeAvailabilityPeriodUtc, overtimeLayerLength, shiftEndTime);
			Assert.AreEqual(TimeSpan.FromHours(3).Add(TimeSpan.FromMinutes(30)), adjustedOvertimeLength);
        }
    }
}

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Tracking;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Scheduling.Tracking
{
	[DomainTest]
	public class TrackerCalculatorTest
    {
        private TrackerCalculator _target;
	    private IPersistableScheduleDataPermissionChecker _permissionChecker;

	    [SetUp]
        public void Setup()
        {
			_permissionChecker = new PersistableScheduleDataPermissionChecker(new FullPermission());
            _target = new TrackerCalculator();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCanCountAbsenceDaysOverPeriod()
        {
            IPerson person = new Person();
            IScenario scenario = new Scenario("For Test");
			var currentAuthorization = new FullPermission();
			var dictionary = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2000,1,1,2002,1,1)), _permissionChecker, currentAuthorization);
            var baseDateTime = new DateTime(2001, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            IAbsence absenceToCount = new Absence();
            IActivity underlyingActivity = new Activity("For test");
            IScheduleDay schedulePart = ExtractedSchedule.CreateScheduleDay(dictionary, person,new DateOnly(2001,1,2), currentAuthorization);
            IScheduleDay schedulePart2 = ExtractedSchedule.CreateScheduleDay(dictionary, person, new DateOnly(2001, 1, 5), currentAuthorization);
            IScheduleDay schedulePart3 = ExtractedSchedule.CreateScheduleDay(dictionary, person, new DateOnly(2001, 1, 11), currentAuthorization);
            //Periods:
            var period1 = new DateTimePeriod(baseDateTime.AddDays(1),baseDateTime.AddDays(1).AddHours(4));
            var period2 = new DateTimePeriod(baseDateTime.AddDays(4),baseDateTime.AddDays(4).AddHours(4));
            var period3 = new DateTimePeriod(baseDateTime.AddDays(10),baseDateTime.AddDays(10).AddHours(4));

            //Add underlying activities (to show the absences in projection)
            schedulePart.CreateAndAddActivity(underlyingActivity,period1,new ShiftCategory("-"));
            schedulePart2.CreateAndAddActivity(underlyingActivity, period2, new ShiftCategory("-"));
            schedulePart3.CreateAndAddActivity(underlyingActivity, period3, new ShiftCategory("-"));
            IList<IScheduleDay> days = new List<IScheduleDay>{schedulePart, schedulePart2, schedulePart3};

            //Add absences 
            schedulePart.CreateAndAddAbsence(new AbsenceLayer(absenceToCount, period1));
            schedulePart2.CreateAndAddAbsence(new AbsenceLayer(absenceToCount, period2));
            schedulePart3.CreateAndAddAbsence(new AbsenceLayer(absenceToCount, period3));
            
            Assert.AreEqual(TimeSpan.FromDays(3), _target.CalculateNumberOfDaysOnScheduleDays(absenceToCount, days));
            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCanCountAbsenceTimeOverPeriod()
        {
            IPerson person = new Person();
            IScenario scenario = new Scenario("For Test");
			var currentAuthorization = new FullPermission();
			var dictionary = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2002, 1, 1)), _permissionChecker, currentAuthorization);
            var baseDateTime = new DateTime(2001, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            IAbsence absenceToCount = new Absence {InContractTime = true};
            IActivity underlyingActivity = new Activity("For test");
            IScheduleDay schedulePart = ExtractedSchedule.CreateScheduleDay(dictionary, person, new DateOnly(2001, 1, 2), currentAuthorization);
            IScheduleDay schedulePart2 = ExtractedSchedule.CreateScheduleDay(dictionary, person, new DateOnly(2001, 1, 5), currentAuthorization);
            IScheduleDay schedulePart3 = ExtractedSchedule.CreateScheduleDay(dictionary, person, new DateOnly(2001, 1, 11), currentAuthorization);
            //Periods:
            var period1 = new DateTimePeriod(baseDateTime.AddDays(1), baseDateTime.AddDays(1).AddHours(4));
            var period2 = new DateTimePeriod(baseDateTime.AddDays(4), baseDateTime.AddDays(4).AddHours(4));
            var period3 = new DateTimePeriod(baseDateTime.AddDays(10), baseDateTime.AddDays(10).AddHours(4));

            //Add underlying activities (to show the absences in projection)
            schedulePart.CreateAndAddActivity(underlyingActivity, period1, new ShiftCategory("-"));
            schedulePart2.CreateAndAddActivity(underlyingActivity, period2, new ShiftCategory("-"));
            schedulePart3.CreateAndAddActivity(underlyingActivity, period3, new ShiftCategory("-"));
            IList<IScheduleDay> days = new List<IScheduleDay> { schedulePart, schedulePart2, schedulePart3 };

            //Add absences 
            schedulePart.CreateAndAddAbsence(new AbsenceLayer(absenceToCount, period1));
            schedulePart2.CreateAndAddAbsence(new AbsenceLayer(absenceToCount, period2));
            schedulePart3.CreateAndAddAbsence(new AbsenceLayer(absenceToCount, period3));

            Assert.AreEqual(TimeSpan.FromHours(12), _target.CalculateTotalTimeOnScheduleDays(absenceToCount, days));
        }

        [Test]
        public void ShouldReturnZeroIfSignificantPartIsDayOff()
        {
            var mocks = new MockRepository();
            var absenceToCount = new Absence { InContractTime = true };
            var day = mocks.StrictMock<IScheduleDay>();
            var days = new List<IScheduleDay> {day};
            Expect.Call(day.SignificantPart()).Return(SchedulePartView.DayOff);
            mocks.ReplayAll();
            Assert.That(_target.CalculateNumberOfDaysOnScheduleDays(absenceToCount, days), Is.EqualTo(TimeSpan.FromDays(0)));
            mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnZeroIfSignificantPartIsContractDayOff()
        {
            var mocks = new MockRepository();
            var absenceToCount = new Absence { InContractTime = true };
            var day = mocks.StrictMock<IScheduleDay>();
            var days = new List<IScheduleDay> { day };
            Expect.Call(day.SignificantPart()).Return(SchedulePartView.ContractDayOff);
            mocks.ReplayAll();
            Assert.That(_target.CalculateNumberOfDaysOnScheduleDays(absenceToCount, days), Is.EqualTo(TimeSpan.FromDays(0)));
            mocks.VerifyAll();
        }
    }
}

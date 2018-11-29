using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll;


namespace Teleopti.Ccc.WinCodeTest.Payroll
{
    [TestFixture]
    public class MultiplicatorDefinitionViewModelTest
    {
        private MultiplicatorDefinitionViewModel targetDateTime;
        private MultiplicatorDefinitionViewModel targetDayOfWeek;
        private Multiplicator multiplicatorOvertime;
        private Multiplicator multiplicatorOBTime;
        private DateOnly startDate;
        private DateOnly endDate;
        private TimeSpan startTime;
        private TimeSpan endTime;

        [SetUp]
        public void Setup()
        {

            multiplicatorOvertime = new Multiplicator(MultiplicatorType.Overtime);
            multiplicatorOBTime = new Multiplicator(MultiplicatorType.OBTime);
            startDate = new DateOnly(2009,12,27);
            endDate = new DateOnly(2009, 12, 28);
            startTime = TimeSpan.FromHours(8);
            endTime = TimeSpan.FromHours(18);

            IMultiplicatorDefinition dateTimeMultiplicatorDefinition = new DateTimeMultiplicatorDefinition(multiplicatorOvertime,startDate, endDate,startTime, endTime);
            IMultiplicatorDefinition dayOfWeekMultiplicatorDefinition = new DayOfWeekMultiplicatorDefinition(multiplicatorOBTime, DayOfWeek.Wednesday, new TimePeriod("8-18"));

            targetDateTime = new MultiplicatorDefinitionViewModel(dateTimeMultiplicatorDefinition);
            targetDayOfWeek = new MultiplicatorDefinitionViewModel(dayOfWeekMultiplicatorDefinition);
        }

        [Test]
        public void VerifyCanAssignSelectedDayOfWeek()
        {
            Assert.AreEqual(DayOfWeek.Wednesday, targetDayOfWeek.DayOfWeek);
            targetDayOfWeek.DayOfWeek = DayOfWeek.Tuesday;
            Assert.AreEqual(DayOfWeek.Tuesday, targetDayOfWeek.DayOfWeek);
            Assert.IsNull(targetDateTime.DayOfWeek);
        }

        [Test]
        public void VerifyCanAssignMultiplicator()
        {
            Assert.AreEqual(multiplicatorOBTime.Description,targetDateTime.Multiplicator.Description);
            Assert.AreEqual(multiplicatorOvertime.Description,targetDayOfWeek.Multiplicator.Description);

            targetDayOfWeek.Multiplicator = multiplicatorOBTime;
            targetDateTime.Multiplicator = multiplicatorOvertime;

            Assert.AreEqual(multiplicatorOvertime.Description, targetDateTime.Multiplicator.Description);
            Assert.AreEqual(multiplicatorOBTime.Description, targetDayOfWeek.Multiplicator.Description);
        }

        [Test]
        public void VerifyCanAssignFrom()
        {
            Assert.AreEqual(startDate.Date.Add(startTime), targetDateTime.FromDate);
            DateTime newDateTime = new DateTime(2009, 12, 26, 5, 12, 0);
            targetDateTime.FromDate = newDateTime;
            Assert.AreEqual(newDateTime, targetDateTime.FromDate);
            Assert.IsNull(targetDayOfWeek.FromDate);

            targetDateTime.FromDate = newDateTime;
            Assert.AreEqual(newDateTime, targetDateTime.FromDate);
        }

        [Test]
        public void VerifyCanAssignTo()
        {
            Assert.AreEqual(endDate.Date.Add(endTime), targetDateTime.ToDate);
            DateTime newDateTime = new DateTime(2009, 12, 29, 15, 30, 0);
            targetDateTime.ToDate = newDateTime;
            Assert.AreEqual(newDateTime, targetDateTime.ToDate);
            Assert.IsNull(targetDayOfWeek.ToDate);

            targetDateTime.ToDate = newDateTime;
            Assert.AreEqual(newDateTime, targetDateTime.ToDate);

        }

        [Test]
        public void VerifyCanAssignStartTime()
        {
            Assert.AreEqual(startTime, targetDayOfWeek.StartTime);
            targetDayOfWeek.StartTime = TimeSpan.FromHours(22); //Later than end
            Assert.AreEqual(TimeSpan.FromHours(22), targetDayOfWeek.StartTime);

            
            targetDayOfWeek.StartTime = TimeSpan.FromHours(16); //Earlier than end
            Assert.AreEqual(TimeSpan.FromHours(16), targetDayOfWeek.StartTime);

            Assert.IsNull(targetDateTime.StartTime);
        }

        [Test]
        public void VerifyCanAssignEndTime()
        {
            Assert.AreEqual(endTime, targetDayOfWeek.EndTime);

            targetDayOfWeek.EndTime = TimeSpan.FromHours(22); //Later than start
            Assert.AreEqual(TimeSpan.FromHours(22), targetDayOfWeek.EndTime);

            targetDayOfWeek.EndTime = TimeSpan.FromHours(7); //Earlier than start -> means after 8 dvs. 7 the day after
            Assert.AreEqual(new TimeSpan(1,7,0,0), targetDayOfWeek.EndTime);

            Assert.IsNull(targetDateTime.EndTime);

        }

        [Test]
        public void VerifyCanAssignMultiplicatorCollection()
        {
            IList<IMultiplicator> multiplicators = new List<IMultiplicator> { multiplicatorOBTime, multiplicatorOvertime };

            targetDateTime.MultiplicatorCollection = multiplicators;
            targetDayOfWeek.MultiplicatorCollection = multiplicators;

            Assert.AreEqual(2, targetDateTime.MultiplicatorCollection.Count);
            Assert.AreEqual(2, targetDayOfWeek.MultiplicatorCollection.Count);
        }
    }
}

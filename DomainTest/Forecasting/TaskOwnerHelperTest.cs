using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class TaskOwnerHelperTest
    {
        private IList<IWorkloadDay> _workloadDays;
        private readonly DateTime _currentDate = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private TaskOwnerHelper _target;
        private ISkill _skill;
        private IWorkload _workload;

        [SetUp]
        public void Setup()
        {
            _skill = SkillFactory.CreateSkill("testSkill");
            _workload = WorkloadFactory.CreateWorkload(_skill);
        }

        [Test]
        public void CanSplitIntoMonths()
        {
            _workloadDays = WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(_currentDate, CultureInfo.CurrentCulture.Calendar.AddMonths(_currentDate, 5), _workload);
            _target = new TaskOwnerHelper(_workloadDays);

            //This sholud split into 5 whole months (1 day + 5 months)
            ICollection<TaskOwnerPeriod> workloadPeriods = _target.CreateWholeMonthTaskOwnerPeriods();
            
            Assert.AreEqual(5,workloadPeriods.Count);
        }

        [Test, SetCulture("en-GB")]
        public void CanSplitIntoWeeks()
        {
            //This sholud split into 3 whole weeks 1day + 3 weeks equals 3 periods with 7 days and one with 1 day
            _workloadDays = WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(_currentDate, CultureInfo.CurrentCulture.Calendar.AddWeeks(_currentDate, 3), _workload);
            _target = new TaskOwnerHelper(_workloadDays);
            ICollection<TaskOwnerPeriod> workloadPeriods = _target.CreateWholeWeekTaskOwnerPeriods();
            Assert.AreEqual(3, workloadPeriods.Count);
        }

        /// <summary>
        /// Verifies the lock works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        [Test]
        public void VerifyLockWorks()
        {
            _workloadDays = WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(_currentDate, CultureInfo.CurrentCulture.Calendar.AddMonths(_currentDate, 5), _workload);
            _target = new TaskOwnerHelper(_workloadDays);

            ICollection<TaskOwnerPeriod> workloadPeriods = _target.CreateWholeMonthTaskOwnerPeriods();
            var firstWorkloadPeriod = workloadPeriods.First();
            double currentTotalTasks = firstWorkloadPeriod.Tasks;
            double currentFirstDayTotalTasks = firstWorkloadPeriod.TaskOwnerDayCollection[0].Tasks + 50d;

            _target.BeginUpdate();
            ((IWorkloadDay)firstWorkloadPeriod.TaskOwnerDayCollection[0]).MakeOpen24Hours();
            firstWorkloadPeriod.TaskOwnerDayCollection[0].Tasks = currentFirstDayTotalTasks;
            Assert.AreEqual(currentTotalTasks, firstWorkloadPeriod.Tasks);
            Assert.AreEqual(currentFirstDayTotalTasks, Math.Round(firstWorkloadPeriod.TaskOwnerDayCollection[0].Tasks,2));
            _target.EndUpdate();

            Assert.AreEqual(currentTotalTasks + 50d, Math.Round(firstWorkloadPeriod.Tasks,2));
            Assert.AreEqual(currentFirstDayTotalTasks, Math.Round(firstWorkloadPeriod.TaskOwnerDayCollection[0].Tasks,2));
        }

        /// <summary>
        /// Verifies the get workload days work.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-23
        /// </remarks>
        [Test]
        public void VerifyGetWorkloadDaysWork()
        {
            _workloadDays = WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(_currentDate, CultureInfo.CurrentCulture.Calendar.AddMonths(_currentDate, 5),  _workload);
            _target = new TaskOwnerHelper(_workloadDays);

            Assert.AreEqual(_workloadDays.Count, _target.TaskOwnerDays.Count);
            Assert.AreEqual(_workloadDays[0], _target.TaskOwnerDays[0]);
        }

        [Test]
        public void VerifyCanSplitIntoYears()
        {
            //Creates 1097 days 2007-1-1 to 2010-1-1 
            //Equals:  365 days in 2007, 366 days in 2008, 365 days in 2009 and 1 day in 2010

            CultureInfo ci = CultureInfo.GetCultureInfo(1053);
            _workloadDays = WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(_currentDate, ci.Calendar.AddYears(_currentDate, 3), _workload);
            _target = new TaskOwnerHelper(_workloadDays);
            
            Assert.AreEqual(1097,_workloadDays.Count);

            IList<TaskOwnerPeriod> taskOwnerPeriods = _target.CreateYearTaskOwnerPeriods(ci.Calendar);

            Assert.AreEqual(4, taskOwnerPeriods.Count);
            Assert.AreEqual(365, taskOwnerPeriods[0].TaskOwnerDayCollection.Count); //2007
            Assert.AreEqual(366, taskOwnerPeriods[1].TaskOwnerDayCollection.Count); //2008 Skottår
            Assert.AreEqual(365, taskOwnerPeriods[2].TaskOwnerDayCollection.Count); //2009
            Assert.AreEqual(1, taskOwnerPeriods[3].TaskOwnerDayCollection.Count); //2009
        }

        [Test]
        public void VerifyCanHandleSplitIntoYearsWithNoDays()
        {
            CultureInfo ci = CultureInfo.GetCultureInfo(1053);
            _workloadDays = new List<IWorkloadDay>();
            _target = new TaskOwnerHelper(_workloadDays);

            IList<TaskOwnerPeriod> taskOwnerPeriods = _target.CreateYearTaskOwnerPeriods(ci.Calendar);

            Assert.AreEqual(0, taskOwnerPeriods.Count);
        }

        /// <summary>
        /// Verifies the get whole selection of workload days work.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        [Test]
        public void VerifyGetWholeSelectionOfWorkloadDaysWork()
        {
            _workloadDays = WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(_currentDate, CultureInfo.CurrentCulture.Calendar.AddMonths(_currentDate, 5),  _workload);
            _target = new TaskOwnerHelper(_workloadDays);

            TaskOwnerPeriod workloadPeriods = _target.CreateWholeSelectionTaskOwnerPeriod();
            Assert.AreEqual(_workloadDays.Count, workloadPeriods.TaskOwnerDayCollection.Count);
        }

        /// <summary>
        /// Verifies the get periods work with no task owner days.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyGetPeriodsWorkWithNoTaskOwnerDays()
        {
            _target = new TaskOwnerHelper(new List<ITaskOwner>());

            Assert.AreEqual(0, _target.TaskOwnerDays.Count);
            Assert.AreEqual(0, _target.CreateWholeMonthTaskOwnerPeriods().Count);
            Assert.IsNull(_target.CreateWholeSelectionTaskOwnerPeriod());
            Assert.AreEqual(0, _target.CreateWholeWeekTaskOwnerPeriods().Count);
        }

        [Test]
        public void ShouldSplitIntoMonthOfYearTest()
        {
            _workloadDays = WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(_currentDate, CultureInfo.CurrentCulture.Calendar.AddMonths(_currentDate, 25),  _workload);
            _target = new TaskOwnerHelper(_workloadDays);
            IList<TaskOwnerPeriod> taskOwnerPeriods = _target.CreateMonthTaskOwnerPeriods(CultureInfo.GetCultureInfo(1053).Calendar);

            Assert.AreEqual(12, taskOwnerPeriods.Count);
            Assert.AreEqual(93, taskOwnerPeriods[0].TaskOwnerDayCollection.Count); //3 mån
            Assert.AreEqual(58, taskOwnerPeriods[1].TaskOwnerDayCollection.Count);// till 2009-02-01
            Assert.AreEqual(62, taskOwnerPeriods[2].TaskOwnerDayCollection.Count);
            Assert.AreEqual(60, taskOwnerPeriods[3].TaskOwnerDayCollection.Count);
            Assert.AreEqual(62, taskOwnerPeriods[4].TaskOwnerDayCollection.Count);
            Assert.AreEqual(60, taskOwnerPeriods[5].TaskOwnerDayCollection.Count);
            Assert.AreEqual(62, taskOwnerPeriods[6].TaskOwnerDayCollection.Count);
            Assert.AreEqual(62, taskOwnerPeriods[7].TaskOwnerDayCollection.Count);
            Assert.AreEqual(60, taskOwnerPeriods[8].TaskOwnerDayCollection.Count);
            Assert.AreEqual(62, taskOwnerPeriods[9].TaskOwnerDayCollection.Count);
            Assert.AreEqual(60, taskOwnerPeriods[10].TaskOwnerDayCollection.Count);
            Assert.AreEqual(62, taskOwnerPeriods[11].TaskOwnerDayCollection.Count);
        }

        [Test]
        public void ShouldSplitIntoWeekOfMonthTest()
        {
            _workloadDays = WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(_currentDate, CultureInfo.CurrentCulture.Calendar.AddDays(_currentDate, 27), _workload);
            _target = new TaskOwnerHelper(_workloadDays);
            IList<TaskOwnerPeriod> taskOwnerPeriods = _target.CreateWeekTaskOwnerPeriods(CultureInfo.GetCultureInfo(1053).Calendar);

            Assert.AreEqual(5, taskOwnerPeriods.Count);
            Assert.AreEqual(7, taskOwnerPeriods[0].TaskOwnerDayCollection.Count); //3 mån
            Assert.AreEqual(7, taskOwnerPeriods[1].TaskOwnerDayCollection.Count);// till 2009-02-01
            Assert.AreEqual(7, taskOwnerPeriods[2].TaskOwnerDayCollection.Count);
            Assert.AreEqual(7, taskOwnerPeriods[3].TaskOwnerDayCollection.Count);
            Assert.AreEqual(28, taskOwnerPeriods[4].TaskOwnerDayCollection.Count);
        }

        [Test]
        public void ShouldSplitIntoDayOfWeekTest()
        {
            _workloadDays = WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(_currentDate, CultureInfo.CurrentCulture.Calendar.AddDays(_currentDate, 5), _workload);
            _target = new TaskOwnerHelper(_workloadDays);
            IList<TaskOwnerPeriod> taskOwnerPeriods = _target.CreateDayTaskOwnerPeriods(CultureInfo.GetCultureInfo(1053).Calendar);

            Assert.AreEqual(7, taskOwnerPeriods.Count);
            Assert.AreEqual(6, taskOwnerPeriods[0].TaskOwnerDayCollection.Count);
            Assert.AreEqual(1, taskOwnerPeriods[1].TaskOwnerDayCollection.Count);
            Assert.AreEqual(1, taskOwnerPeriods[2].TaskOwnerDayCollection.Count);
            Assert.AreEqual(1, taskOwnerPeriods[3].TaskOwnerDayCollection.Count);
            Assert.AreEqual(1, taskOwnerPeriods[4].TaskOwnerDayCollection.Count);
            Assert.AreEqual(1, taskOwnerPeriods[5].TaskOwnerDayCollection.Count);
            Assert.AreEqual(1, taskOwnerPeriods[6].TaskOwnerDayCollection.Count);
        }

        [Test]
        public void ShouldHandleSplitIntoMonthOfYearFromEmptyListTest()
        {
            _workloadDays = new List<IWorkloadDay>();
            _target = new TaskOwnerHelper(_workloadDays);
            IList<TaskOwnerPeriod> taskOwnerPeriods = _target.CreateMonthTaskOwnerPeriods(CultureInfo.GetCultureInfo(1053).Calendar);

            Assert.AreEqual(0, taskOwnerPeriods.Count);
        }

        [Test]
        public void ShouldHandleSplitIntoWeekOfMonthFromEmptyListTest()
        {
            _workloadDays = new List<IWorkloadDay>();
            _target = new TaskOwnerHelper(_workloadDays);
            IList<TaskOwnerPeriod> taskOwnerPeriods = _target.CreateWeekTaskOwnerPeriods(CultureInfo.GetCultureInfo(1053).Calendar);

            Assert.AreEqual(0, taskOwnerPeriods.Count);
        }

        [Test]
        public void ShouldHandleSplitIntoDayOfWeekFromEmptyListTest()
        {
            _workloadDays = new List<IWorkloadDay>();
            _target = new TaskOwnerHelper(_workloadDays);
            IList<TaskOwnerPeriod> taskOwnerPeriods = _target.CreateDayTaskOwnerPeriods(CultureInfo.GetCultureInfo(1053).Calendar);

            Assert.AreEqual(0, taskOwnerPeriods.Count);
        }

        [Test]
        public void VerifyCanHandleTwoFebruaryDifferentYears()
        {
            _workloadDays = WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(_currentDate, _currentDate.AddDays(25), _workload);
            _workloadDays =
                _workloadDays.Concat(WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(_currentDate.AddYears(1),
                                                                                            _currentDate.AddYears(1).AddDays(25),
                                                                                            _workload)).ToList();

            _target = new TaskOwnerHelper(_workloadDays);
            IList<TaskOwnerPeriod> taskOwnerPeriods = _target.CreateMonthTaskOwnerPeriods(CultureInfo.GetCultureInfo(1053).Calendar);

            Assert.AreEqual(12, taskOwnerPeriods.Count);
        }
    }
}

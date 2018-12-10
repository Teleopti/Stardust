using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using System.Reflection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for the workload period class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-12-17
    /// </remarks>
    [TestFixture]
    public class TaskOwnerPeriodTest
    {
        private TaskOwnerPeriod target;
        private readonly DateOnly currentDate = new DateOnly(2007, 1, 1);
        private IScenario _scenario;
        private ISkill _skill;
        private IWorkload _workload;
        private MockRepository _mockRepository;
        private TaskOwnerPeriod _parent;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _scenario = _mockRepository.StrictMock<IScenario>();
            _skill = SkillFactory.CreateSkill("testSkill");
            _workload = WorkloadFactory.CreateWorkload(_skill);
            _parent = new TaskOwnerPeriod(currentDate, null, TaskOwnerPeriodType.Other);
            target = new TaskOwnerPeriod(currentDate, null, TaskOwnerPeriodType.Month);
            target.AddParent(_parent);
        }

        [Test]
        public void ShouldHaveCorrectWeeklyWorkloadSummaryForNonTelephonySkills()
        {
            var skill = SkillFactory.CreateSkill("Email", SkillTypeFactory.CreateSkillTypeEmail(), 60);
            var workload = WorkloadFactory.CreateWorkload(skill);
            var openHours = new List<TimePeriod>();
            var workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2012, 6, 26), workload, openHours);
            workloadDay.Tasks = 100;

            target.Add(workloadDay);

	        Assert.That(Math.Round(target.Tasks, 2), Is.EqualTo(100));
        }

        /// <summary>
        /// Verifies the is loaded works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        [Test]
        public void VerifyIsLoadedWorks()
        {
            target = new TaskOwnerPeriod(currentDate, null, TaskOwnerPeriodType.Month);
            Assert.IsFalse(target.IsLoaded);
        }

        /// <summary>
        /// Verifies the current date works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        [Test]
        public void VerifyCurrentDateWorks()
        {
            var myDate = new DateOnly(2007, 8, 1);
            target.CurrentDate =myDate;
            Assert.AreEqual(myDate, target.CurrentDate);
        }

        [Test]
        public void VerifySetDirty()
        {
            target.SetDirty();
        }

        /// <summary>
        /// Verifies the distribution of tasks when some days are closed.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        [Test]
        public void VerifyDistributionOfTasksWhenSomeDaysAreClosed()
        {
            target.TypeOfTaskOwnerPeriod = TaskOwnerPeriodType.Week;
            target = new TaskOwnerPeriod(target.CurrentDate,
                                         WorkloadDayFactory.GetWorkloadDaysForTest(target.StartDate.Date, target.EndDate.Date,
                                                                                    _workload).OfType
                                             <ITaskOwner>().ToList(), target.TypeOfTaskOwnerPeriod);

            Assert.AreEqual(7, target.TaskOwnerDayCollection.Count);

            target.TaskOwnerDayCollection[1].Tasks = 35;
            target.TaskOwnerDayCollection[3].Tasks = 35;

            ((WorkloadDay)target.TaskOwnerDayCollection[5]).Close();
            ((WorkloadDay)target.TaskOwnerDayCollection[6]).Close();

            target.Tasks = 1050d;

            Assert.AreEqual(1050d, target.Tasks);
            Assert.AreEqual(196.875d, Math.Round(target.TaskOwnerDayCollection[0].Tasks,3));
            Assert.AreEqual(229.6875d, Math.Round(target.TaskOwnerDayCollection[1].Tasks,4));
            Assert.AreEqual(196.875d, Math.Round(target.TaskOwnerDayCollection[2].Tasks,3));
            Assert.AreEqual(229.6875d, Math.Round(target.TaskOwnerDayCollection[3].Tasks,4));
            Assert.AreEqual(196.875d, Math.Round(target.TaskOwnerDayCollection[4].Tasks, 3));
            Assert.AreEqual(0d, target.TaskOwnerDayCollection[5].Tasks);
            Assert.AreEqual(0d, target.TaskOwnerDayCollection[6].Tasks);
        }

        /// <summary>
        /// Verifies the changing to same workload type works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-19
        /// </remarks>
        [Test]
        public void VerifyChangingToSameWorkloadTypeWorks()
        {
            target.AddRange(WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(target.StartDate.Date, target.EndDate.Date, _workload).OfType<ITaskOwner>());
            target.TypeOfTaskOwnerPeriod = TaskOwnerPeriodType.Month;
            Assert.AreEqual(TaskOwnerPeriodType.Month, target.TypeOfTaskOwnerPeriod);
            Assert.AreEqual(31, target.TaskOwnerDayCollection.Count);
        }

        [Test]
        public void VerifyClear()
        {
            target.AddRange(WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(target.StartDate.Date, target.EndDate.Date,  _workload).OfType<ITaskOwner>());
            Assert.AreEqual(31, target.TaskOwnerDayCollection.Count);
            target.Clear();
            Assert.AreEqual(0,target.TaskOwnerDayCollection.Count);
        }

        /// <summary>
        /// Verifies the workload day collection works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        [Test]
        public void VerifyWorkloadDayCollectionWorks()
        {
            target.AddRange(WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(target.StartDate.Date, target.EndDate.Date,  _workload).OfType<ITaskOwner>());
            Assert.AreEqual(31, target.TaskOwnerDayCollection.Count);
        }

        /// <summary>
        /// Verifies the number of days in month for all cultures.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        [Test]
        public void VerifyNumberOfDaysInMonthForAllCultures()
        {
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach(CultureInfo ci in cultures)
            {
                Thread.CurrentThread.CurrentCulture = ci;
                target = new TaskOwnerPeriod(currentDate,null,TaskOwnerPeriodType.Month);
                target.AddRange(WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(target.StartDate.Date, target.EndDate.Date,_workload).OfType<ITaskOwner>());
                Assert.AreEqual(
                    ci.Calendar.GetDaysInMonth(
                        ci.Calendar.GetYear(currentDate.Date),
                        ci.Calendar.GetMonth(currentDate.Date),
                        ci.Calendar.GetEra(currentDate.Date)), target.TaskOwnerDayCollection.Count);
            }
        }

        /// <summary>
        /// Verifies the add workload day works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        [Test]
        public void VerifyAddWorkloadDayWorks()
        {
            target.CurrentDate =  new DateOnly(2007, 2, 1);

            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(target.EndDate, _workload,  openHours);

            target.Add(workloadDay);

            Assert.IsTrue(target.TaskOwnerDayCollection.Contains(workloadDay));
        }

        [Test]
        public void VerifyAddAndRemoveWorkloadDayWorks()
        {
            target.CurrentDate = new DateOnly(2007, 2, 1);

            IList<TimePeriod> openHours = new List<TimePeriod>
                                              {new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0))};

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(target.EndDate, _workload, openHours);

            target.Add(workloadDay);

            Assert.IsTrue(target.TaskOwnerDayCollection.Contains(workloadDay));
        }

        /// <summary>
        /// Verifies the remove works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        [Test]
        public void VerifyRemoveWorks()
        {
            IList<TimePeriod> openHours = new List<TimePeriod> { new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)) };

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(target.EndDate, _workload,  openHours);

            target.Add(workloadDay);
            Assert.AreEqual(1, target.TaskOwnerDayCollection.Count);

            target.Remove(workloadDay);
            Assert.AreEqual(0, target.TaskOwnerDayCollection.Count);
        }

        /// <summary>
        /// Verifies the remove of workload not in collection works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        [Test]
        public void VerifyRemoveOfWorkloadNotInCollectionWorks()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007,8,1),_workload,  openHours);

            target.Remove(workloadDay);

            Assert.AreEqual(0, target.TaskOwnerDayCollection.Count);
        }

        /// <summary>
        /// Verifies the get average task time works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        [Test]
        public void VerifyGetAverageTaskTime()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload, openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);
            Assert.AreEqual(140, Math.Round(target.AverageTaskTime.TotalSeconds,0));
        }

        /// <summary>
        /// Verifies the set average task time works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        [Test]
        public void VerifySetAverageTaskTime()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload, openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);
            target.AverageTaskTime = TimeSpan.FromSeconds(3);

            Assert.AreEqual(3d, Math.Round(target.AverageTaskTime.TotalSeconds,2));
            Assert.AreEqual(3, Math.Round(target.TaskOwnerDayCollection[0].AverageTaskTime.TotalSeconds,0));
        }

        /// <summary>
        /// Verifies the set average task time with lock.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifySetAverageTaskTimeWithLock()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload, openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);
            target.Lock();
            target.AverageTaskTime = TimeSpan.FromSeconds(3d);
            target.Release();

            Assert.AreEqual(3d, Math.Round(target.AverageTaskTime.TotalSeconds, 2));
            Assert.AreEqual(3d, Math.Round(target.TaskOwnerDayCollection[0].AverageTaskTime.TotalSeconds,0));
        }

        /// <summary>
        /// Verifies the set average after task time works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        [Test]
        public void VerifySetAverageAfterTaskTime()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload, openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);
            target.AverageAfterTaskTime = TimeSpan.FromSeconds(3d);

            Assert.AreEqual(3d, Math.Round(target.AverageAfterTaskTime.TotalSeconds,2));
            Assert.AreEqual(3,Math.Round(target.TaskOwnerDayCollection[0].AverageAfterTaskTime.TotalSeconds,0));
        }

        /// <summary>
        /// Verifies the set average after task time gives exception when closed.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-19
        /// </remarks>
        [Test]
        public void VerifySetAverageAfterTaskTimeGivesExceptionWhenClosed()
        {
	        Assert.Throws<InvalidOperationException>(() =>
		        target = new TaskOwnerPeriod(new DateOnly(2007, 8, 1), null, TaskOwnerPeriodType.Other)
		        {
			        AverageAfterTaskTime = TimeSpan.FromSeconds(3d)
		        });
        }

        /// <summary>
        /// Verifies the update template name not implemented.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        [Test]
       
        public void VerifyUpdateTemplateNameNotImplemented()
        {
	        Assert.Throws<NotImplementedException>(() => target.ClearTemplateName());
        }

        /// <summary>
        /// Verifies the set total tasks gives exception when closed.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-20
        /// </remarks>
        [Test]
        public void VerifySetTasksGivesExceptionWhenClosed()
        {
			Assert.Throws<InvalidOperationException>(() => target = new TaskOwnerPeriod(new DateOnly(2007, 8, 1), null, TaskOwnerPeriodType.Other) {Tasks = 50});
        }

        /// <summary>
        /// Verifies the set average task time gives exception when closed.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-19
        /// </remarks>
        [Test]
        public void VerifySetAverageTaskTimeGivesExceptionWhenClosed()
        {
			Assert.Throws<InvalidOperationException>(() => target = new TaskOwnerPeriod(new DateOnly(2007, 8, 1), null, TaskOwnerPeriodType.Other)
	        {
		        AverageTaskTime = TimeSpan.FromSeconds(3d)
	        });
        }

        /// <summary>
        /// Verifies the set total tasks works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-19
        /// </remarks>
        [Test]
        public void VerifySetTasksWorks()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload, openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);

            double numberOfTasks = target.Tasks + 50d;
            target.Tasks = numberOfTasks;
            Assert.AreEqual(Math.Round(numberOfTasks, 0), Math.Round(target.Tasks,0));
        }

        /// <summary>
        /// Verifies the set tasks works with lock.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifySetTasksWorksWithLock()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload, openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);

            target.Lock();
            
            double numberOfTasks = target.Tasks + 50d;
            target.Tasks = numberOfTasks;
            target.Release();

            Assert.AreEqual(Math.Round(numberOfTasks,0), Math.Round(target.Tasks,0));
        }

        /// <summary>
        /// Verifies the get average after task time works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        [Test]
        public void VerifyGetAverageAfterTaskTime()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload, openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);
            Assert.AreEqual(140,Math.Round(target.AverageAfterTaskTime.TotalSeconds,0));
        }

        /// <summary>
        /// Verifies the parent works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-19
        /// </remarks>
        [Test]
        public void VerifySetParentWorks()
        {
            MockRepository mocks = new MockRepository();
            ITaskOwner taskOwner = mocks.StrictMock<ITaskOwner>();

            target.AddParent(taskOwner);
        }

        /// <summary>
        /// Verifies the changing childs updates values.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-19
        /// </remarks>
        [Test]
        public void VerifyChangingChildsUpdatesValues()
        {
            target = new TaskOwnerPeriod(target.CurrentDate, WorkloadDayFactory.GetWorkloadDaysForTest(target.CurrentDate.Date, target.CurrentDate.Date.AddDays(1), _workload).OfType<ITaskOwner>().ToList(), TaskOwnerPeriodType.Other);

            Assert.AreEqual(61d, Math.Round(target.Tasks,0));

            target.TaskOwnerDayCollection[0].Tasks = 10d;
            target.TaskOwnerDayCollection[1].Tasks = 10d;
            target.TaskOwnerDayCollection[0].AverageTaskTime = TimeSpan.FromSeconds(40);
            target.TaskOwnerDayCollection[1].AverageTaskTime = TimeSpan.FromSeconds(40);
            target.TaskOwnerDayCollection[0].AverageAfterTaskTime = TimeSpan.FromSeconds(80);
            target.TaskOwnerDayCollection[1].AverageAfterTaskTime = TimeSpan.FromSeconds(80);

            Assert.AreEqual(20d, Math.Round(target.Tasks, 0));
            Assert.AreEqual(40d, Math.Round(target.AverageTaskTime.TotalSeconds,2));
            Assert.AreEqual(80d, Math.Round(target.AverageAfterTaskTime.TotalSeconds,2));

            target.TaskOwnerDayCollection[1].AverageTaskTime = TimeSpan.FromSeconds(60);
            target.TaskOwnerDayCollection[1].AverageAfterTaskTime = TimeSpan.FromSeconds(100);
            
            Assert.AreEqual(TimeSpan.FromSeconds(50).TotalSeconds, Math.Round(target.AverageTaskTime.TotalSeconds,0));
            Assert.AreEqual(TimeSpan.FromSeconds(90).TotalSeconds, Math.Round(target.AverageAfterTaskTime.TotalSeconds,0));

            target.TaskOwnerDayCollection[1].Tasks = 20d;

            Assert.AreEqual(30d, Math.Round(target.Tasks, 0));
        }

        /// <summary>
        /// Verifies the start and end date works with other period.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyStartAndEndDateWorksWithOtherPeriod()
        {
            var startDate = new DateOnly(2007, 8, 1);
            var endDate = new DateOnly(2007, 8, 15);

            target.TypeOfTaskOwnerPeriod = TaskOwnerPeriodType.Other;
            target.CurrentDate = startDate;
            target = new TaskOwnerPeriod(target.CurrentDate,
                                         WorkloadDayFactory.GetWorkloadDaysWithoutContentForTest(startDate.Date, endDate.Date,
                                                                                   _workload).OfType
                                             <ITaskOwner>().ToList(), target.TypeOfTaskOwnerPeriod);

            Assert.AreEqual(startDate, target.StartDate);
            Assert.AreEqual(endDate, target.EndDate);
            Assert.AreEqual(TaskOwnerPeriodType.Other, target.TypeOfTaskOwnerPeriod);
        }

        /// <summary>
        /// Verifies the one workload day can have two workload periods as owners.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test, SetCulture("en-GB")]
        public void VerifyOneWorkloadDayCanHaveTwoWorkloadPeriodsAsOwners()
        {
            target.AddRange(
                WorkloadDayFactory.GetWorkloadDaysForTest(target.StartDate.Date, target.EndDate.Date, 
                                                                        _workload).OfType<ITaskOwner>());

            TaskOwnerHelper workloadPeriodHelper = new TaskOwnerHelper(target.TaskOwnerDayCollection);
            IList<TaskOwnerPeriod> weekPeriods = workloadPeriodHelper.CreateWholeWeekTaskOwnerPeriods();
            
            //Original total tasks for first week should be 213
            target.TaskOwnerDayCollection[0].Tasks -= 20;
            Assert.AreEqual(193, Math.Round(weekPeriods[0].Tasks,0));

            //Original total tasks for month should be 945 (=> -20 + 45)
            weekPeriods[0].TaskOwnerDayCollection[0].Tasks += 45;
            Assert.AreEqual(970, Math.Round(target.Tasks,0));
        }

        /// <summary>
        /// Verifies the lock and release.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        [Test]
        public void VerifyLockAndRelease()
        {
            target.Lock();
            target.Release();
        }

        /// <summary>
        /// Verifies the lock and release with parent.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-23
        /// </remarks>
        [Test]
        public void VerifyLockAndReleaseWithParent()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload, openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);

            ITaskOwner taskOwner = _mockRepository.StrictMock<ITaskOwner>();

            taskOwner.Lock();
            LastCall.Repeat.Once();

            taskOwner.Release();
            LastCall.Repeat.Once();

            _mockRepository.ReplayAll();

            target.AddParent(taskOwner);
            target.Lock();
            target.Release();

            _mockRepository.VerifyAll();
        }

        /// <summary>
        /// Verifies the is closed works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyIsClosedWorks()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload, openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);

            //Assert.IsFalse(target.IsClosed);
            Assert.IsTrue(target.OpenForWork.IsOpen);
            workloadDay.Close();
            //Assert.IsTrue(target.IsClosed);
            Assert.IsFalse(target.OpenForWork.IsOpen);
            workloadDay.MakeOpen24Hours();
            //Assert.IsFalse(target.IsClosed);
            Assert.IsTrue(target.OpenForWork.IsOpen);
        }

        /// <summary>
        /// Verifies the is locked works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyIsLockedWorks()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload,  openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);
            Assert.IsFalse(target.IsLocked);
            target.Lock();
            Assert.IsTrue(target.IsLocked);
            target.Release();
            Assert.IsFalse(target.IsLocked);
        }

        /// <summary>
        /// Verifies the remove parent works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyRemoveParentWorks()
        {
            MockRepository mocks = new MockRepository();
            ITaskOwner parent = mocks.StrictMock<ITaskOwner>();

            mocks.ReplayAll();

            target.AddParent(parent);
            target.RemoveParent(parent);
            target.RemoveParent(parent);

            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the recalculate daily task statistics works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        [Test]
        public void VerifyRecalculateDailyTaskStatisticsWorks()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload, openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);

            Assert.AreEqual(0d, target.TotalStatisticCalculatedTasks);
            Assert.AreEqual(0d, target.TotalStatisticAbandonedTasks);
            Assert.AreEqual(0d, target.TotalStatisticAnsweredTasks);
            
            workloadDay.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 100d;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAbandonedTasks = 50d;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAnsweredTasks = 150d;
            workloadDay.RecalculateDailyStatisticTasks();
            target.RecalculateDailyStatisticTasks();
            Assert.AreEqual(100d, target.TotalStatisticCalculatedTasks);
            Assert.AreEqual(50d, target.TotalStatisticAbandonedTasks);
            Assert.AreEqual(150d, target.TotalStatisticAnsweredTasks);
        }

        /// <summary>
        /// Verifies the recalculate daily average statistic times works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        [Test]
        public void VerifyRecalculateDailyAverageStatisticTimesWorks()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            var workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload,  openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);
            Assert.AreEqual(TimeSpan.Zero, target.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.Zero, target.TotalStatisticAverageAfterTaskTime);

            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 120 * 96; //96 periods
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 60 * 96; //96 periods
            workloadDay.RecalculateDailyAverageStatisticTimes();
            target.RecalculateDailyAverageStatisticTimes();
            Assert.AreEqual(TimeSpan.FromSeconds(120), target.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(60), target.TotalStatisticAverageAfterTaskTime);
        }

        [Test]
        public void VerifyRecalculateDailyAverageStatisticTimesUsesWeightedMean()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            var workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2012,11,23), _workload, openHours);
            workloadDay.TotalStatisticAverageTaskTime = TimeSpan.FromSeconds(60000);
            workloadDay.TotalStatisticAverageAfterTaskTime = TimeSpan.FromSeconds(60000);
            workloadDay.TotalStatisticCalculatedTasks = 1;

            var workloadDay2 = new WorkloadDay();
            workloadDay2.Create(new DateOnly(2012, 11, 24), _workload, openHours);
            workloadDay2.TotalStatisticAverageTaskTime = TimeSpan.FromSeconds(6000);
            workloadDay2.TotalStatisticAverageAfterTaskTime = TimeSpan.FromSeconds(6000);
            workloadDay2.TotalStatisticCalculatedTasks = 10;

            target.Add(workloadDay);
            target.Add(workloadDay2);

            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 60000 * 96;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 60000 * 96;
            workloadDay.RecalculateDailyAverageStatisticTimes();        
            workloadDay2.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 6000 * 96;
            workloadDay2.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 6000 * 96;
            workloadDay2.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 6000 * 96;
            workloadDay2.RecalculateDailyAverageStatisticTimes();

            target.RecalculateDailyStatisticTasks();
            Assert.That(Math.Round(target.TotalStatisticAverageTaskTime.TotalMinutes), Is.EqualTo(182));
            Assert.That(Math.Round(target.TotalStatisticAverageAfterTaskTime.TotalMinutes), Is.EqualTo(182));

        }

        /// <summary>
        /// Verifies the recalculate daily average statistic times works with tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        [Test]
        public void VerifyRecalculateDailyAverageStatisticTimesWorksWithTasks()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload,  openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);
            Assert.AreEqual(TimeSpan.Zero, target.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.Zero, target.TotalStatisticAverageAfterTaskTime);

            workloadDay.TaskPeriodList[0].StatisticTask.StatAnsweredTasks = 10d;
            workloadDay.RecalculateDailyStatisticTasks();
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 120;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 60;
            workloadDay.RecalculateDailyAverageStatisticTimes();
            target.RecalculateDailyAverageStatisticTimes();
            Assert.AreEqual(TimeSpan.FromSeconds(120), target.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(60), target.TotalStatisticAverageAfterTaskTime);
        }

        [Test]
        public void VerifyInitializeWorks()
        {
            #region Ordinary

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
            double value = target.Tasks;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
            TimeSpan timeValue = target.AverageTaskTime;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
            timeValue = target.AverageAfterTaskTime;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            #endregion

            #region Campaign

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
            Percent percentValue = target.CampaignTasks;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
            percentValue = target.CampaignTaskTime;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
            percentValue = target.CampaignAfterTaskTime;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            #endregion

            #region Totals

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
            value = target.TotalTasks;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
            timeValue = target.TotalAverageTaskTime;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
           timeValue = target.TotalAverageAfterTaskTime;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            #endregion

            #region Statistics

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
            value = target.TotalStatisticAbandonedTasks;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
            value = target.TotalStatisticAnsweredTasks;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
            value = target.TotalStatisticCalculatedTasks;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
            timeValue = target.TotalStatisticAverageTaskTime;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, target, new object[] { false }, CultureInfo.InvariantCulture);
            timeValue = target.TotalStatisticAverageAfterTaskTime;
            Assert.IsTrue((bool)target.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, target, null, CultureInfo.InvariantCulture));

            #endregion

            Assert.AreEqual(0d, value);
            Assert.AreEqual(TimeSpan.Zero, timeValue);
            Assert.AreEqual(new Percent(), percentValue);
        }

        /// <summary>
        /// Verifies the on campaign tasks changed is triggered with lock and parent.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyOnCampaignTasksChangedIsTriggeredWithLockAndParent()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload,  openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);

            ITaskOwner taskOwnerParent = _mockRepository.StrictMock<ITaskOwner>();

            taskOwnerParent.Lock();
            LastCall.Repeat.Once();

            taskOwnerParent.SetDirty();
            LastCall.Repeat.Once();

            taskOwnerParent.Release();
            LastCall.Repeat.Once();

            taskOwnerParent.RecalculateDailyTasks();
            LastCall.Repeat.Times(2);

            taskOwnerParent.RecalculateDailyAverageTimes();
            LastCall.Repeat.Times(2);

            taskOwnerParent.RecalculateDailyCampaignTasks();
            LastCall.Repeat.Times(2);

            taskOwnerParent.RecalculateDailyAverageCampaignTimes();
            LastCall.Repeat.Times(2);

            _mockRepository.ReplayAll();

            target.AddParent(taskOwnerParent);
            target.Lock();
            target.CampaignTasks = new Percent(0.3d);
            target.Release();

            _mockRepository.VerifyAll();
        }

        /// <summary>
        /// Verifies the on campaign average times changed is triggered with lock and parent.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyOnCampaignAverageTimesChangedIsTriggeredWithLockAndParent()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload, openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);

            ITaskOwner taskOwnerParent = _mockRepository.StrictMock<ITaskOwner>();

            taskOwnerParent.Lock();
            LastCall.Repeat.Once();

            taskOwnerParent.SetDirty();
            LastCall.Repeat.Once();

            taskOwnerParent.Release();
            LastCall.Repeat.Once();

            taskOwnerParent.RecalculateDailyTasks();
            LastCall.Repeat.Times(2);

            taskOwnerParent.RecalculateDailyAverageTimes();
            LastCall.Repeat.Times(2);

            taskOwnerParent.RecalculateDailyCampaignTasks();
            LastCall.Repeat.Times(2);

            taskOwnerParent.RecalculateDailyAverageCampaignTimes();
            LastCall.Repeat.Times(2);

            _mockRepository.ReplayAll();

            target.AddParent(taskOwnerParent);
            target.Lock();
            target.CampaignTaskTime = new Percent(0.3d);
            target.Release();

            _mockRepository.VerifyAll();
        }

        /// <summary>
        /// Verifies the campaign tasks can bet set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyCampaignTasksCanBeSet()
        {
            //945 * 120% = 1134
            target = new TaskOwnerPeriod(target.CurrentDate,
                                         WorkloadDayFactory.GetWorkloadDaysForTest(target.StartDate.Date, target.EndDate.Date,
                                                      _workload).OfType<ITaskOwner>(), target.TypeOfTaskOwnerPeriod);
            Assert.AreEqual(945d, Math.Round(target.Tasks, 0));
            Assert.AreEqual(945d, Math.Round(target.TotalTasks, 0));
            target.CampaignTasks = new Percent(0.2d);
            Assert.AreEqual(945d, Math.Round(target.Tasks, 0));
            Assert.AreEqual(1134d, Math.Round(target.TotalTasks, 0));
        }

        [Test]
        public void VerifyCampaignTasksCanBeSetWhenChildrenAlreadyHaveCampaignSet()
        {
            //945 * 120% = 1134
            target = new TaskOwnerPeriod(target.CurrentDate,
                                         WorkloadDayFactory.GetWorkloadDaysForTest(target.StartDate.Date, target.EndDate.Date,
                                                      _workload).OfType<ITaskOwner>(), target.TypeOfTaskOwnerPeriod);
            Assert.AreEqual(945d, Math.Round(target.Tasks, 0));
            Assert.AreEqual(945d, Math.Round(target.TotalTasks, 0));

            TaskOwnerHelper helper = new TaskOwnerHelper(new List<ITaskOwner>{target});
            helper.BeginUpdate();
            //target.Lock();
            target.Tasks = 1000d;
            //target.Release();
            helper.EndUpdate();
            Assert.AreEqual(1000d, Math.Round(target.Tasks, 0));
            Assert.AreEqual(1000d, Math.Round(target.TotalTasks,0));

            foreach (var taskOwner in target.TaskOwnerDayCollection)
            {
                taskOwner.CampaignTasks = new Percent(0.2d);
            }
            Assert.AreEqual(1000d, Math.Round(target.Tasks, 0));
            Assert.AreEqual(1200d, Math.Round(target.TotalTasks, 0));
            Assert.AreEqual(new Percent(0.2).ToString(), target.CampaignTasks.ToString());
            target.CampaignTasks = new Percent(0.4d);
            Assert.AreEqual(1000d, Math.Round(target.Tasks,0));
            Assert.AreEqual(1400d, Math.Round(target.TotalTasks,0));
            Assert.AreEqual(new Percent(0.4),target.CampaignTasks);
        }

        /// <summary>
        /// Verifies the campaign times can be set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyCampaignTimesCanBeSet()
        {
            target = new TaskOwnerPeriod(target.CurrentDate, WorkloadDayFactory.GetWorkloadDaysForTest(target.CurrentDate.Date, target.CurrentDate.Date.AddDays(1), _workload).OfType<ITaskOwner>().ToList(), TaskOwnerPeriodType.Other);
            target.TaskOwnerDayCollection[0].AverageTaskTime = TimeSpan.FromSeconds(40);
            target.TaskOwnerDayCollection[1].AverageTaskTime = TimeSpan.FromSeconds(40);
            target.TaskOwnerDayCollection[0].AverageAfterTaskTime = TimeSpan.FromSeconds(80);
            target.TaskOwnerDayCollection[1].AverageAfterTaskTime = TimeSpan.FromSeconds(80);

            Assert.AreEqual(TimeSpan.FromSeconds(40).TotalSeconds, Math.Round(target.AverageTaskTime.TotalSeconds,2));
            Assert.AreEqual(TimeSpan.FromSeconds(80).TotalSeconds, Math.Round(target.AverageAfterTaskTime.TotalSeconds,2));
            Assert.AreEqual(TimeSpan.FromSeconds(40).TotalSeconds, Math.Round(target.TotalAverageTaskTime.TotalSeconds, 2));
            Assert.AreEqual(TimeSpan.FromSeconds(80).TotalSeconds, Math.Round(target.TotalAverageAfterTaskTime.TotalSeconds, 2));
            target.CampaignTaskTime = new Percent(0.25d);
            target.CampaignAfterTaskTime = new Percent(0.5d);
            Assert.AreEqual(TimeSpan.FromSeconds(40).TotalSeconds, Math.Round(target.AverageTaskTime.TotalSeconds, 2));
            Assert.AreEqual(TimeSpan.FromSeconds(80).TotalSeconds, Math.Round(target.AverageAfterTaskTime.TotalSeconds, 2));
            Assert.AreEqual(TimeSpan.FromSeconds(50).TotalSeconds, Math.Round(target.TotalAverageTaskTime.TotalSeconds, 2));
            Assert.AreEqual(TimeSpan.FromSeconds(120).TotalSeconds, Math.Round(target.TotalAverageAfterTaskTime.TotalSeconds, 2));
        }

        /// <summary>
        /// Verifies the campaign can be set with closed days.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-05
        /// </remarks>
        [Test]
        public void VerifyCampaignCanBeSetWithClosedDays()
        {
            target = new TaskOwnerPeriod(target.CurrentDate, WorkloadDayFactory.GetWorkloadDaysForTest(target.CurrentDate.Date, target.CurrentDate.Date.AddDays(1),  _workload).OfType<ITaskOwner>().ToList(), TaskOwnerPeriodType.Other);
            target.TaskOwnerDayCollection[0].AverageTaskTime = TimeSpan.FromSeconds(40);
            target.TaskOwnerDayCollection[1].AverageTaskTime = TimeSpan.FromSeconds(40);
            target.TaskOwnerDayCollection[0].AverageAfterTaskTime = TimeSpan.FromSeconds(80);
            target.TaskOwnerDayCollection[1].AverageAfterTaskTime = TimeSpan.FromSeconds(80);

            ((WorkloadDayBase)target.TaskOwnerDayCollection[1]).Close();

            Assert.AreEqual(TimeSpan.FromSeconds(40), target.AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(80), target.AverageAfterTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(40), target.TotalAverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(80), target.TotalAverageAfterTaskTime);
            target.CampaignTasks = new Percent(0d);
            target.CampaignTaskTime = new Percent(0.25d);
            target.CampaignAfterTaskTime = new Percent(0.5d);
            Assert.AreEqual(TimeSpan.FromSeconds(40), target.AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(80), target.AverageAfterTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(50), target.TotalAverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(120), target.TotalAverageAfterTaskTime);
        }

        /// <summary>
        /// Verifies the set campaign tasks when closed gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifySetCampaignTasksWhenClosedGivesException()
        {
            foreach (ITaskOwner wld in target.TaskOwnerDayCollection)
            {
                ((WorkloadDay)wld).Close();
            }
			Assert.Throws<InvalidOperationException>(() => target.CampaignTasks = new Percent());
        }

        /// <summary>
        /// Verifies the set campaign task time when closed gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifySetCampaignTaskTimeWhenClosedGivesException()
        {
            foreach (ITaskOwner wld in target.TaskOwnerDayCollection)
            {
                ((WorkloadDay)wld).Close();
            }
			Assert.Throws<InvalidOperationException>(() => target.CampaignTaskTime = new Percent());
        }

        /// <summary>
        /// Verifies the set campaign after task time when closed gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifySetCampaignAfterTaskTimeWhenClosedGivesException()
        {
            foreach (ITaskOwner wld in target.TaskOwnerDayCollection)
            {
                ((WorkloadDay)wld).Close();
            }
			Assert.Throws<InvalidOperationException>(() => target.CampaignAfterTaskTime = new Percent());
        }

        /// <summary>
        /// Verifies the recalculate campaign when closed.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyRecalculateCampaignWhenClosed()
        {
            foreach (ITaskOwner wld in target.TaskOwnerDayCollection)
            {
                ((WorkloadDay)wld).Close();
            }

            target.RecalculateDailyCampaignTasks();
            Assert.AreEqual(new Percent(), target.CampaignTasks);
            target.RecalculateDailyAverageCampaignTimes();
            Assert.AreEqual(new Percent(), target.CampaignTaskTime);
            Assert.AreEqual(new Percent(), target.CampaignAfterTaskTime);
        }

		[Test]
		public void ShouldRecalculateCampaignTaskTimesCorrect()
		{
			target = new TaskOwnerPeriod(target.CurrentDate, WorkloadDayFactory.GetWorkloadDaysForTest(target.CurrentDate.Date, target.CurrentDate.Date.AddDays(1), _workload).OfType<ITaskOwner>().ToList(), TaskOwnerPeriodType.Other)
			{
				AverageTaskTime = TimeSpan.FromSeconds(30),
				AverageAfterTaskTime = TimeSpan.FromSeconds(60),
				CampaignTaskTime = new Percent(0.5d),
				CampaignAfterTaskTime = new Percent(0.75d)
			};

			target.RecalculateDailyAverageCampaignTimes();

			Assert.AreEqual(new Percent(0.5d).Value, Math.Round(target.CampaignTaskTime.Value, 2));
			Assert.AreEqual(new Percent(0.75d).Value, Math.Round(target.CampaignAfterTaskTime.Value, 2));
		}


        /// <summary>
        /// Verifies the campaign times can be set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-05
        /// </remarks>
        [Test]
        public void VerifyCampaignTimesCanBeSetAndAffectChildren()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload,  openHours);

            workloadDay.AverageTaskTime = TimeSpan.FromSeconds(40);
            workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(80);

            target.Add(workloadDay);

            Assert.AreEqual(TimeSpan.FromSeconds(40).TotalSeconds, Math.Round(target.AverageTaskTime.TotalSeconds, 2));
            Assert.AreEqual(TimeSpan.FromSeconds(80).TotalSeconds, Math.Round(target.AverageAfterTaskTime.TotalSeconds, 2));
            Assert.AreEqual(TimeSpan.FromSeconds(40).TotalSeconds, Math.Round(target.TotalAverageTaskTime.TotalSeconds, 2));
            Assert.AreEqual(TimeSpan.FromSeconds(80).TotalSeconds, Math.Round(target.TotalAverageAfterTaskTime.TotalSeconds, 2));
            target.CampaignTaskTime = new Percent(0.25d);
            target.CampaignAfterTaskTime = new Percent(0.5d);
            Assert.AreEqual(TimeSpan.FromSeconds(40).TotalSeconds, Math.Round(target.AverageTaskTime.TotalSeconds, 2));
            Assert.AreEqual(TimeSpan.FromSeconds(80).TotalSeconds, Math.Round(target.AverageAfterTaskTime.TotalSeconds, 2));
            Assert.AreEqual(TimeSpan.FromSeconds(50).TotalSeconds, Math.Round(target.TotalAverageTaskTime.TotalSeconds, 2));
            Assert.AreEqual(TimeSpan.FromSeconds(120).TotalSeconds, Math.Round(target.TotalAverageAfterTaskTime.TotalSeconds, 2));
        }
        
        [Test]
        public void CanReset()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(new DateOnly(2007, 8, 1), _workload,  openHours);
            workloadDay.Tasks = 100;
            workloadDay.AverageTaskTime = TimeSpan.FromSeconds(140);

            target.Add(workloadDay);
            
            target.CampaignAfterTaskTime = new Percent(2);
            target.CampaignTaskTime = new Percent(2);
            target.CampaignTasks = new Percent(2);
            target.Tasks = 222;
            target.AverageAfterTaskTime = new TimeSpan(222);
            target.AverageTaskTime = new TimeSpan(222);

            Assert.AreNotEqual(0, target.Tasks);
            Assert.AreNotEqual(new TimeSpan(0), target.AverageAfterTaskTime);
            Assert.AreNotEqual(new TimeSpan(0), target.AverageTaskTime);
            Assert.AreNotEqual(new Percent(0), target.CampaignAfterTaskTime);
            Assert.AreNotEqual(new Percent(0), target.CampaignTasks);
            Assert.AreNotEqual(new Percent(0), target.CampaignTaskTime);

            target.ResetTaskOwner();

            Assert.AreEqual(0, target.Tasks);
            Assert.AreEqual(new TimeSpan(0), target.AverageAfterTaskTime);
            Assert.AreEqual(new TimeSpan(0), target.AverageTaskTime);
            Assert.AreEqual(new Percent(0), target.CampaignAfterTaskTime);
            Assert.AreEqual(new Percent(0), target.CampaignTasks);
            Assert.AreEqual(new Percent(0), target.CampaignTaskTime);
        }

        [Test]
        public void VerifyForecastedIncomingDemand()
        {
            var dt = DateTime.SpecifyKind(currentDate.Date, DateTimeKind.Utc);
            DateTimePeriod dtp = new DateTimePeriod(dt, dt.AddMinutes(15));

            SkillDay skillDay1 = new SkillDay(currentDate,_skill,_scenario,new List<IWorkloadDay>(), new List<ISkillDataPeriod>());
            SkillDay skillDay2 = new SkillDay(currentDate.AddDays(1), _skill, _scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod>());

            ISkillStaffPeriod period1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dtp,
                new Task(10d, TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(35)),
                ServiceAgreement.DefaultValues());
			period1.SetSkillDay(skillDay1);
			
            ISkillStaffPeriod period2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(1)),
                new Task(10d, TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(35)),
                ServiceAgreement.DefaultValues());
			period2.SetSkillDay(skillDay2);
            SkillDayCalculator calculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay1, skillDay2 }, new DateOnlyPeriod());
            target = new TaskOwnerPeriod(currentDate,calculator.SkillDays.OfType<ITaskOwner>(),TaskOwnerPeriodType.Other);

            var newPeriod1 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> {period1});
            skillDay1.SetCalculatedStaffCollection(newPeriod1);
            newPeriod1.BatchCompleted();
            var newPeriod2 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { period2 });
            skillDay2.SetCalculatedStaffCollection(newPeriod2);
            newPeriod2.BatchCompleted();

            IList<ISkillStaffPeriod> sortedList = new List<ISkillStaffPeriod>();
            sortedList.Add(period1);
            sortedList.Add(period2);
            period1.CalculateStaff();
            period2.CalculateStaff(sortedList);

            Assert.AreEqual(
                period1.ForecastedIncomingDemand().Add(period2.ForecastedIncomingDemand()),
                target.ForecastedIncomingDemand);
        }

        [Test]
        public void VerifyForecastedIncomingDemandWithInvalidItems()
        {
            Assert.AreEqual(TimeSpan.Zero, target.ForecastedIncomingDemand);
        }

        [Test]
        public void VerifyForecastedIncomingDemandWithShrinkage()
        {
            DateTime dt = DateTime.SpecifyKind(currentDate.Date, DateTimeKind.Utc);
            DateTimePeriod dtp = new DateTimePeriod(dt, dt.AddMinutes(15));
            SkillDay skillDay1 = new SkillDay(currentDate, _skill, _scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod>());
            SkillDay skillDay2 = new SkillDay(currentDate.AddDays(1), _skill, _scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod>());
            SkillDayCalculator calculator = new SkillDayCalculator(_skill, new List<ISkillDay> {skillDay1, skillDay2}, new DateOnlyPeriod());

            ISkillStaffPeriod period1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dtp,
                new Task(10d, TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(35)),
                ServiceAgreement.DefaultValues());
			period1.SetSkillDay(skillDay1);
            ISkillStaffPeriod period2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(1)),
                new Task(10d, TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(35)),
                ServiceAgreement.DefaultValues());
			period2.SetSkillDay(skillDay2);
            target = new TaskOwnerPeriod(currentDate, calculator.SkillDays.OfType<ITaskOwner>(), TaskOwnerPeriodType.Other);

            var newPeriod1 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { period1 });
            skillDay1.SetCalculatedStaffCollection(newPeriod1);
            newPeriod1.BatchCompleted();
            var newPeriod2 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { period2 });
            skillDay2.SetCalculatedStaffCollection(newPeriod2);
            newPeriod2.BatchCompleted();

            period1.CalculateStaff();
            period2.CalculateStaff();

            Assert.AreEqual(
                period1.ForecastedIncomingDemandWithShrinkage().Add(period2.ForecastedIncomingDemandWithShrinkage()),
                target.ForecastedIncomingDemandWithShrinkage);
        }

        [Test]
        public void VerifyForecastedIncomingDemandWithShrinkageWithInvalidItems()
        {
            Assert.AreEqual(TimeSpan.Zero, target.ForecastedIncomingDemandWithShrinkage);
        }

        [Test]
        public void VerifySettingCampaignNumbersAlloverWeekGivesSamePercentageForWeekObject()
        {
            //945 * 120% = 1134
            target = new TaskOwnerPeriod(target.CurrentDate,
                                         WorkloadDayFactory.GetWorkloadDaysForTest(target.StartDate.Date, target.EndDate.Date,
                                                      _workload).OfType<ITaskOwner>(), target.TypeOfTaskOwnerPeriod);
            var partTarget = new TaskOwnerPeriod(target.TaskOwnerDayCollection[8].CurrentDate,
                                                 new List<ITaskOwner>
                                                     {
                                                         target.TaskOwnerDayCollection[8],
                                                         target.TaskOwnerDayCollection[9],
                                                         target.TaskOwnerDayCollection[10]
                                                     }, TaskOwnerPeriodType.Other);
            Assert.AreEqual(91d, Math.Round(partTarget.Tasks,0));
            Assert.AreEqual(91d, Math.Round(partTarget.TotalTasks,0));

            target.TaskOwnerDayCollection[8].AddParent(target);
            target.TaskOwnerDayCollection[8].AddParent(partTarget);
            target.TaskOwnerDayCollection[9].AddParent(target);
            target.TaskOwnerDayCollection[9].AddParent(partTarget);
            target.TaskOwnerDayCollection[10].AddParent(target);
            target.TaskOwnerDayCollection[10].AddParent(partTarget);

            var helper = new TaskOwnerHelper(target.TaskOwnerDayCollection);
            helper.BeginUpdate();
            foreach (var taskOwner in partTarget.TaskOwnerDayCollection)
            {
                taskOwner.CampaignTasks = new Percent(0.1);
            }
            helper.EndUpdate();

            Assert.AreEqual(91d, Math.Round(partTarget.Tasks,0));
            Assert.AreEqual(100d, Math.Round(partTarget.TotalTasks, 0));
            Assert.AreEqual(new Percent(0.1).ToString(), partTarget.CampaignTasks.ToString());

            helper.BeginUpdate();
            foreach (var taskOwner in partTarget.TaskOwnerDayCollection)
            {
                taskOwner.CampaignTasks = new Percent(0.2d);
            }
            helper.EndUpdate();
            Assert.AreEqual(91d, Math.Round(partTarget.Tasks,0));
            Assert.AreEqual(109d, Math.Round(partTarget.TotalTasks,0));
            Assert.AreEqual(new Percent(0.2).ToString(), partTarget.CampaignTasks.ToString());
        }

		[Test]
		public void Tasks_ShouldNotBeRounded_ToZeroDecimals()
		{
			var openHours = new List<TimePeriod> {new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0))};
			var workloadDay = new WorkloadDay();
			workloadDay.Create(new DateOnly(2007, 8, 1), _workload, openHours);
			workloadDay.Tasks = 0.5d;
			target.Add(workloadDay);

			Math.Round(target.Tasks, 9).Should().Be.EqualTo(0.5);
		}

		[Test]
		public void TotalTasks_ShouldNotBeRounded_ToZeroDecimals()
		{
			var openHours = new List<TimePeriod> { new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)) };
			var workloadDay = new WorkloadDay();
			workloadDay.Create(new DateOnly(2007, 8, 1), _workload, openHours);
			workloadDay.Tasks = 0.5;
			target.Add(workloadDay);

			Math.Round(target.TotalTasks, 9).Should().Be.EqualTo(0.5);
		}
    }
}

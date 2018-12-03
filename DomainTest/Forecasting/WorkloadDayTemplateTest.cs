using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
	public class WorkloadDayTemplateTest
    {
        private IWorkloadDayTemplate _workloadDayTemplate;
        private IWorkload _workload;
        private DateTime _dt;
        private ISkill _skill;
        private string _name;
        private IList<TimePeriod> _openHours;

        [SetUp]
        public void Setup()
        {
            _dt = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _name = "JULAFTON";
            _skill = SkillFactory.CreateSkill("testSkill");
            _skill.MidnightBreakOffset = TimeSpan.FromHours(2);
            _workload = new Workload(_skill);
			
            _openHours = new List<TimePeriod>
                         	{
                         		new TimePeriod(new TimeSpan(13, 0, 0), new TimeSpan(1, 2, 0, 0)),
                         		new TimePeriod(new TimeSpan(7, 0, 0), new TimeSpan(11, 0, 0))
                         	};

        	_workloadDayTemplate = new WorkloadDayTemplate();
            _workloadDayTemplate.Create(_name,_dt, _workload, _openHours);

            _workloadDayTemplate.Tasks = 999;
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_workloadDayTemplate);
        }

        [Test]
        public void ShouldChangeToNewInstance()
        {
            _workload.SetId(Guid.NewGuid());
            
            var newWorkloadInstance = WorkloadFactory.CreateWorkload(_skill);
            newWorkloadInstance.SetId(_workload.Id);

            _workloadDayTemplate.SetWorkloadInstance(newWorkloadInstance);
            _workloadDayTemplate.Workload.Should().Be.SameInstanceAs(newWorkloadInstance);
        }

        [Test]
        public void ShouldThrowExceptionWhenNotSameWorkload()
        {
            _workload.SetId(Guid.NewGuid());

            var newWorkloadInstance = WorkloadFactory.CreateWorkload(_skill);

			Assert.Throws<ArgumentException>(() => _workloadDayTemplate.SetWorkloadInstance(newWorkloadInstance));
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_workloadDayTemplate.GetType(), true));
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_dt, _workloadDayTemplate.CreatedDate);
            Assert.AreEqual(_name, _workloadDayTemplate.Name);
            Assert.IsFalse(_workloadDayTemplate.DayOfWeek.HasValue);
        }

        [Test]
        public void VerifyVersionHandling()
        {
            int originalVersionNumber = _workloadDayTemplate.VersionNumber;
            _workloadDayTemplate.IncreaseVersionNumber();
            Assert.Less(originalVersionNumber, _workloadDayTemplate.VersionNumber);

            TestWorkloadDayTemplate testWorkloadDayTemplate = new TestWorkloadDayTemplate();
            Assert.AreEqual(0, testWorkloadDayTemplate.VersionNumber);
            testWorkloadDayTemplate.SetVersionNumber(42);
            Assert.AreEqual(42, testWorkloadDayTemplate.VersionNumber);
        }
        [Test]
        public void VerifyRenameTemplateWorksWithoutIncrementing()
        {
            int origVersionNumber = _workloadDayTemplate.VersionNumber;
            string origName = _workloadDayTemplate.Name;
            string newName = "new " + origName;
            _workloadDayTemplate.Name = newName;
            Assert.AreEqual(origVersionNumber, _workloadDayTemplate.VersionNumber);
            Assert.AreEqual(newName, _workloadDayTemplate.Name);
        }

        [Test]
        public void VerifyLocalWorkloadDateWorks()
        {
            Assert.AreEqual(_dt, _workloadDayTemplate.CreatedDate);
            ISkill skill = SkillFactory.CreateSkill("MySkill");
            skill.TimeZone = (TimeZoneInfo.Local);
            _workloadDayTemplate.Workload.Skill = skill;
            Assert.AreEqual(_dt, _workloadDayTemplate.CreatedDate);
        }

        [Test]
        public void VerifyChangingTaskPeriodTasksChangesDailyTotal()
        {
            _workloadDayTemplate.Close();
            _workloadDayTemplate.MakeOpen24Hours();

            Task t1 = new Task(123, new TimeSpan(0, 0, 0, 120, 500), new TimeSpan(0, 0, 0, 20, 0));
            _workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(t1.Tasks);
            _workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = t1.AverageTaskTime;
            _workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = t1.AverageAfterTaskTime;

            Task t2 = new Task(321, new TimeSpan(0, 0, 0, 120, 500), new TimeSpan(0, 0, 0, 20, 0));
            _workloadDayTemplate.SortedTaskPeriodList[1].SetTasks(t2.Tasks);
            _workloadDayTemplate.SortedTaskPeriodList[1].AverageTaskTime = t2.AverageTaskTime;
            _workloadDayTemplate.SortedTaskPeriodList[1].AverageAfterTaskTime = t2.AverageAfterTaskTime;

            Assert.AreEqual(444, _workloadDayTemplate.TotalTasks);

            _workloadDayTemplate.TaskPeriodList[1].SetTasks(111);
            Assert.AreEqual(234, _workloadDayTemplate.TotalTasks);
        }

        [Test]
        public void VerifyChangingTasksInOnePeriodAffectsAverageAfterTaskTime()
        {
            _workloadDayTemplate.Close();
            _workloadDayTemplate.MakeOpen24Hours();

            Task t1 = new Task(123, new TimeSpan(1), new TimeSpan(0, 0, 0, 120, 500));
            _workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(t1.Tasks);
            _workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = t1.AverageTaskTime;
            _workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = t1.AverageAfterTaskTime;

            Task t2 = new Task(123, new TimeSpan(1), new TimeSpan(0, 0, 0, 120, 500));
            _workloadDayTemplate.SortedTaskPeriodList[1].SetTasks(t2.Tasks);
            _workloadDayTemplate.SortedTaskPeriodList[1].AverageTaskTime = t2.AverageTaskTime;
            _workloadDayTemplate.SortedTaskPeriodList[1].AverageAfterTaskTime = t2.AverageAfterTaskTime;
            Assert.AreEqual(new TimeSpan((long)(((_workloadDayTemplate.SortedTaskPeriodList[0].TotalTasks * 
                _workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime.Ticks) + 
                (_workloadDayTemplate.SortedTaskPeriodList[1].TotalTasks * 
                _workloadDayTemplate.SortedTaskPeriodList[1].AverageAfterTaskTime.Ticks)) / 
                _workloadDayTemplate.TotalTasks)), _workloadDayTemplate.AverageAfterTaskTime);

            _workloadDayTemplate.TaskPeriodList[0].SetTasks(1000);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 120, 500), _workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime);
            long p1TotalTicks = (long)_workloadDayTemplate.SortedTaskPeriodList[0].TotalTasks * _workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime.Ticks;
            long p2TotalTicks = (long)_workloadDayTemplate.SortedTaskPeriodList[1].TotalTasks * _workloadDayTemplate.SortedTaskPeriodList[1].AverageAfterTaskTime.Ticks;
            Assert.AreEqual(new TimeSpan((long)((p1TotalTicks + p2TotalTicks) / _workloadDayTemplate.TotalTasks)), _workloadDayTemplate.AverageAfterTaskTime);
        }

        [Test]
        public void VerifyChangingTasksInOnePeriodAffectsAverageTaskTime()
        {
            _workloadDayTemplate.Close();
            _workloadDayTemplate.MakeOpen24Hours();

            Task t1 = new Task(123, new TimeSpan(0, 0, 0, 120, 500), new TimeSpan(1));
            _workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(t1.Tasks);
            _workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = t1.AverageTaskTime;
            _workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = t1.AverageAfterTaskTime;

            Task t2 = new Task(321, new TimeSpan(0, 0, 0, 240, 500), new TimeSpan(1));
            _workloadDayTemplate.SortedTaskPeriodList[1].SetTasks(t2.Tasks);
            _workloadDayTemplate.SortedTaskPeriodList[1].AverageTaskTime = t2.AverageTaskTime;
            _workloadDayTemplate.SortedTaskPeriodList[1].AverageAfterTaskTime = t2.AverageAfterTaskTime;

            Assert.AreEqual(new TimeSpan((long)(((_workloadDayTemplate.SortedTaskPeriodList[0].TotalTasks * _workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime.Ticks) +
                (_workloadDayTemplate.SortedTaskPeriodList[1].TotalTasks * _workloadDayTemplate.SortedTaskPeriodList[1].AverageTaskTime.Ticks)) / 
                _workloadDayTemplate.TotalTasks)), _workloadDayTemplate.AverageTaskTime);

            _workloadDayTemplate.TaskPeriodList[0].SetTasks(1000);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 120, 500), _workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime);
            long p1TotalTicks = (long)_workloadDayTemplate.SortedTaskPeriodList[0].TotalTasks * _workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime.Ticks;
            long p2TotalTicks = (long)_workloadDayTemplate.SortedTaskPeriodList[1].TotalTasks * _workloadDayTemplate.SortedTaskPeriodList[1].AverageTaskTime.Ticks;

            Assert.AreEqual(new TimeSpan((long)((p1TotalTicks + p2TotalTicks) / _workloadDayTemplate.TotalTasks)), _workloadDayTemplate.AverageTaskTime);

        }

        [Test]
        public void CanSetId()
        {
            Guid newId = Guid.NewGuid();  
            _workloadDayTemplate.SetId(newId);
            Assert.AreEqual(newId, _workloadDayTemplate.Id);
        }

        [Test]
        public void CanMakeParent()
        {
            _workloadDayTemplate.Close();
            _workloadDayTemplate.MakeOpen24Hours();

            Task t1 = new Task(123, new TimeSpan(0, 0, 0, 120, 500), new TimeSpan(0, 0, 0, 20, 0));
            _workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(t1.Tasks);
            _workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = t1.AverageTaskTime;
            _workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = t1.AverageAfterTaskTime;

            Assert.AreEqual(_workloadDayTemplate, _workloadDayTemplate.TaskPeriodList[0].Parent);
        }

        /// <summary>
        /// Verifies the protected template name can be set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        [Test]
        public void VerifyProtectedTemplateNameCanBeSet()
        {
            TestWorkloadDayTemplate testClass = new TestWorkloadDayTemplate();
            testClass.ChangeTemplateName();
            Assert.IsEmpty(testClass.TemplateReference.TemplateName);
        }

        //The following tests checks if things are changed or not, the real Smoothning
        //are tested in the StatisticalSmoothningTest
        [Test]
        public void CanSnapshotTasksToOriginalTemplateTaskList()
        {
            _workloadDayTemplate.SnapshotTemplateTaskPeriodList(TaskPeriodType.All);

            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].Tasks);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageTaskTime);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageAfterTaskTime);

            Task t1 = new Task(123, _workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime, _workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime);
            _workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(t1.Tasks);

            Assert.AreNotEqual(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].Tasks);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageTaskTime);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageAfterTaskTime);

            _workloadDayTemplate.SnapshotTemplateTaskPeriodList(TaskPeriodType.Tasks);

            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].Tasks);
            Assert.AreEqual(123, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].Tasks);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageTaskTime);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageAfterTaskTime);
           
        }

        [Test]
        public void CanSnapshotAverageTaskTimeToOriginalTemplateTaskList()
        {
            _workloadDayTemplate.SnapshotTemplateTaskPeriodList(TaskPeriodType.All);

            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].Tasks);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageTaskTime);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageAfterTaskTime);

            //Change to something else 
            _workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0,32,23);

            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].Tasks);
            Assert.AreNotEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageTaskTime);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageAfterTaskTime);

            _workloadDayTemplate.SnapshotTemplateTaskPeriodList(TaskPeriodType.AverageTaskTime);

            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].Tasks);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageTaskTime);
            Assert.AreEqual(new TimeSpan(0, 32, 23), _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageTaskTime);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageAfterTaskTime);
        }

        [Test]
        public void CanSnapshotAverageAfterTaskTimeToOriginalTemplateTaskList()
        {
            _workloadDayTemplate.SnapshotTemplateTaskPeriodList(TaskPeriodType.All);

            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].Tasks);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageTaskTime);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageAfterTaskTime);

            //Change to something else 
            _workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 99, 23);

            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].Tasks);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageTaskTime);
            Assert.AreNotEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageAfterTaskTime);

            _workloadDayTemplate.SnapshotTemplateTaskPeriodList(TaskPeriodType.AverageAfterTaskTime);
            
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].Tasks);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageTaskTime);
            Assert.AreEqual(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime, _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageAfterTaskTime);
            Assert.AreEqual(new TimeSpan(0, 99, 23), _workloadDayTemplate.SortedSnapshotTaskPeriodList[0].AverageAfterTaskTime);
        }

        [Test]
        public void VerifyDoRunningAverage()
        {
            //First set all periods
            _workloadDayTemplate.AverageTaskTime = new TimeSpan(0,45,23);
            _workloadDayTemplate.AverageAfterTaskTime = new TimeSpan(0, 22, 23);

            _workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 120, 23);
            _workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 124, 22);

            Task t1 = new Task(123, _workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime, _workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime);
            _workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(t1.Tasks);

            //Change first so we can see a difference later
            Assert.AreEqual(123, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks,2));
            Assert.AreEqual(7223, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime.TotalSeconds, 2));
            Assert.AreEqual(7462, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime.TotalSeconds, 2));

            //Snapshot
            _workloadDayTemplate.SnapshotTemplateTaskPeriodList(TaskPeriodType.All);

            //Smooth Tasks
            _workloadDayTemplate.DoRunningSmoothing(3,TaskPeriodType.Tasks);

            //Tasks should be changed
            Assert.AreNotEqual(123, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks, 2));
            Assert.AreEqual(7223, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime.TotalSeconds, 2));
            Assert.AreEqual(7462, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime.TotalSeconds, 2));

            //Smooth AverageTaskTime
            _workloadDayTemplate.DoRunningSmoothing(3, TaskPeriodType.AverageTaskTime);

            Assert.AreNotEqual(123, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks, 2));
            Assert.AreNotEqual(7223, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime.TotalSeconds, 2));
            Assert.AreEqual(7462, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime.TotalSeconds, 2));

            //Smooth AverageTaskTime
            _workloadDayTemplate.DoRunningSmoothing(3, TaskPeriodType.AverageAfterTaskTime);

            Assert.AreNotEqual(123, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks, 2));
            Assert.AreNotEqual(7223, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime.TotalSeconds, 2));
            Assert.AreNotEqual(7462, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime.TotalSeconds, 2));

            //"Un-smooth" All with 1
            _workloadDayTemplate.DoRunningSmoothing(1, TaskPeriodType.Tasks);
            _workloadDayTemplate.DoRunningSmoothing(1, TaskPeriodType.AverageTaskTime);
            _workloadDayTemplate.DoRunningSmoothing(1, TaskPeriodType.AverageAfterTaskTime);

            Assert.AreEqual(123, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].Tasks, 2));
            Assert.AreEqual(7223, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime.TotalSeconds, 2));
            Assert.AreEqual(7462, Math.Round(_workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime.TotalSeconds, 2));
        }

        [Test]
        public void VerifyChangingOpenHoursWorks()
        {
            _skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _workloadDayTemplate = new WorkloadDayTemplate();
            _workloadDayTemplate.Create(_name,_dt, _workload, new List<TimePeriod>{new TimePeriod(8,0,17,0)});
            int versionBefore = _workloadDayTemplate.VersionNumber;

            Assert.AreEqual(36,_workloadDayTemplate.TaskPeriodList.Count);

            _workloadDayTemplate.ChangeOpenHours(new List<TimePeriod> { new TimePeriod(7, 0, 15, 0) });

            Assert.AreEqual(32,_workloadDayTemplate.TaskPeriodList.Count);
            Assert.AreNotEqual(_workloadDayTemplate.SortedTaskPeriodList[0].Period,
                               _workloadDayTemplate.SortedTaskPeriodList[1].Period);
            Assert.Greater(_workloadDayTemplate.VersionNumber, versionBefore);
        }

        [Test]
        public void VerifyCanHaveSplitOpenHours()
        {
            _workloadDayTemplate.Create(_name, _dt, _workload, new List<TimePeriod> {new TimePeriod(16, 0, 24, 0), new TimePeriod(2, 0, 3, 0)});

            Assert.AreEqual(36, _workloadDayTemplate.TaskPeriodList.Count);
        }

        [Test]
        public void VerifyCanCreateWorkloadDayTemplateForEmailWorkload()
        {
            _skill.SkillType = new SkillTypeEmail(new Description("Email"), ForecastSource.Email);
            _skill.DefaultResolution = _skill.SkillType.DefaultResolution;

            IList<TimePeriod> openHours = new List<TimePeriod> {new TimePeriod(8, 0, 17, 0)};
            _workloadDayTemplate = new WorkloadDayTemplate();
            _workloadDayTemplate.Create(_name,_dt,_workload,openHours);

            Assert.AreEqual(24,_workloadDayTemplate.TaskPeriodList.Count);
            Assert.AreEqual(1,_workloadDayTemplate.OpenHourList.Count);
            Assert.AreEqual(openHours[0],_workloadDayTemplate.OpenHourList[0]);
        }

        [Test]
        public void CanClone()
        {
            _workloadDayTemplate.SetId(Guid.NewGuid());
            IWorkloadDayTemplate workloadDayTemplateClone = (IWorkloadDayTemplate)_workloadDayTemplate.Clone();
            Assert.IsFalse(workloadDayTemplateClone.Id.HasValue);
            Assert.AreEqual(_workloadDayTemplate.TaskPeriodList.Count, workloadDayTemplateClone.TaskPeriodList.Count);
            Assert.AreSame(_workloadDayTemplate, _workloadDayTemplate.TaskPeriodList[0].Parent);
            Assert.AreSame(workloadDayTemplateClone, workloadDayTemplateClone.TaskPeriodList[0].Parent);
            Assert.AreEqual(_workloadDayTemplate.Workload, workloadDayTemplateClone.Workload);
            Assert.AreEqual(_workloadDayTemplate.OpenHourList.Count, workloadDayTemplateClone.OpenHourList.Count);
            Assert.AreNotSame(_workloadDayTemplate.OpenHourList, workloadDayTemplateClone.OpenHourList);
            Assert.AreEqual(_workloadDayTemplate.Name, workloadDayTemplateClone.Name);
            Assert.AreEqual(_workloadDayTemplate.CreatedDate, workloadDayTemplateClone.CreatedDate);

            workloadDayTemplateClone = _workloadDayTemplate.NoneEntityClone();
            Assert.IsFalse(workloadDayTemplateClone.Id.HasValue);
            Assert.AreEqual(_workloadDayTemplate.TaskPeriodList.Count, workloadDayTemplateClone.TaskPeriodList.Count);
            Assert.AreSame(_workloadDayTemplate, _workloadDayTemplate.TaskPeriodList[0].Parent);
            Assert.AreSame(workloadDayTemplateClone, workloadDayTemplateClone.TaskPeriodList[0].Parent);
            Assert.AreEqual(_workloadDayTemplate.Workload, workloadDayTemplateClone.Workload);
            Assert.AreEqual(_workloadDayTemplate.OpenHourList.Count, workloadDayTemplateClone.OpenHourList.Count);
            Assert.AreNotSame(_workloadDayTemplate.OpenHourList, workloadDayTemplateClone.OpenHourList);
            Assert.AreEqual(_workloadDayTemplate.Name, workloadDayTemplateClone.Name);
            Assert.AreEqual(_workloadDayTemplate.CreatedDate, workloadDayTemplateClone.CreatedDate);

            workloadDayTemplateClone = _workloadDayTemplate.EntityClone();
            Assert.AreEqual(_workloadDayTemplate.Id.Value, workloadDayTemplateClone.Id.Value);
            Assert.AreEqual(_workloadDayTemplate.TaskPeriodList.Count, workloadDayTemplateClone.TaskPeriodList.Count);
            Assert.AreSame(_workloadDayTemplate, _workloadDayTemplate.TaskPeriodList[0].Parent);
            Assert.AreSame(workloadDayTemplateClone, workloadDayTemplateClone.TaskPeriodList[0].Parent);
            Assert.AreEqual(_workloadDayTemplate.Workload, workloadDayTemplateClone.Workload);
            Assert.AreEqual(_workloadDayTemplate.OpenHourList.Count, workloadDayTemplateClone.OpenHourList.Count);
            Assert.AreNotSame(_workloadDayTemplate.OpenHourList, workloadDayTemplateClone.OpenHourList);
            Assert.AreEqual(_workloadDayTemplate.Name, workloadDayTemplateClone.Name);
            Assert.AreEqual(_workloadDayTemplate.CreatedDate, workloadDayTemplateClone.CreatedDate);
        }

		[Test]
		public void VerifyProtectedTemplateUpdatedDateCanBeSet()
		{
			var updatedDate = TimeZoneHelper.ConvertToUtc(new DateTime(2010, 12, 2), TimeZoneInfoFactory.StockholmTimeZoneInfo());
			var testWorkloadDayTemplate = new TestWorkloadDayTemplate();
			testWorkloadDayTemplate.SetUpdatedDate(updatedDate);
			Assert.AreEqual(updatedDate, testWorkloadDayTemplate.UpdatedDate);
		}

        private class TestWorkloadDayTemplate : WorkloadDayTemplate
        {
            public void ChangeTemplateName()
            {
				TemplateReference = new TemplateReference(Guid.Empty, 0, "test", 0);
            }

            public void SetVersionNumber(int versionNumber)
            {
                VersionNumber = versionNumber;
            } 
			
			public void SetUpdatedDate(DateTime updatedDate)
            {
                UpdatedDate = updatedDate;
            }
        }
    }
}

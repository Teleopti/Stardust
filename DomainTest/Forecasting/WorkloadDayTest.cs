using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture, SetUICulture("en-US")]
    public class WorkloadDayTest
    {
        private IWorkloadDay _workloadDay;
        private IWorkload _workload;
        private DateOnly _dt;
        private ISkill _skill;
        private IList<TimePeriod> _openHours;

        [SetUp]
        public void Setup()
        {
            _dt = new DateOnly(2007, 1, 1);
            _skill = SkillFactory.CreateSkill("testSkill");
            _skill.MidnightBreakOffset = TimeSpan.FromHours(2);
            _workload = WorkloadFactory.CreateWorkload(_skill);

            _openHours = new List<TimePeriod>();
            _openHours.Add(new TimePeriod(new TimeSpan(13, 0, 0), new TimeSpan(1, 2, 0, 0)));
            _openHours.Add(new TimePeriod(new TimeSpan(7, 0, 0), new TimeSpan(11, 0, 0)));

            _workloadDay = new WorkloadDay();
            _workloadDay.Create(_dt, _workload, _openHours);
            _workloadDay.Tasks = 999;
            _workloadDay.Annotation = "Some comments, mostly about nothing.";
        }

        [Test]
        public void ShouldNotifyParentOnChangedAnnotation()
        {
            MockRepository mockRepository = new MockRepository();
            var taskOwner = mockRepository.DynamicMock<ITaskOwner>();
            using (mockRepository.Record())
            {
                Expect.Call(taskOwner.SetDirty);
            }
            using (mockRepository.Playback())
            {
                _workloadDay.Lock();
                _workloadDay.AddParent(taskOwner);
                _workloadDay.Annotation = "test";
                _workloadDay.Release();
            }
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_workloadDay);
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_workloadDay.GetType(), true));
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_dt, _workloadDay.CurrentDate);
            Assert.IsNotEmpty(_workloadDay.Annotation);
        }

        [Test]
        public void VerifyLocalWorkloadDateWorks()
        {
            Assert.AreEqual(_dt, _workloadDay.CurrentDate);
            ISkill skill = SkillFactory.CreateSkill("MySkill");
            skill.TimeZone = (TimeZoneInfo.Local);
            _workloadDay.Workload.Skill = skill;

            Assert.AreEqual(_dt, _workloadDay.CurrentDate);
        }

        [Test]
        public void VerifyChangingTaskPeriodTasksChangesDailyTotal()
        {
            _workloadDay.Close();
            _workloadDay.MakeOpen24Hours();

            Task t1 = new Task(123, new TimeSpan(1), new TimeSpan(0, 0, 0, 120, 500));
            _workloadDay.SortedTaskPeriodList[0].SetTasks(t1.Tasks);
            _workloadDay.SortedTaskPeriodList[0].AverageTaskTime = t1.AverageTaskTime;
            _workloadDay.SortedTaskPeriodList[0].AverageAfterTaskTime = t1.AverageAfterTaskTime;

            Task t2 = new Task(321, new TimeSpan(1), new TimeSpan(0, 0, 0, 120, 500));
            _workloadDay.SortedTaskPeriodList[1].SetTasks(t2.Tasks);
            _workloadDay.SortedTaskPeriodList[1].AverageTaskTime = t2.AverageTaskTime;
            _workloadDay.SortedTaskPeriodList[1].AverageAfterTaskTime = t2.AverageAfterTaskTime;

            Assert.AreEqual(444, _workloadDay.TotalTasks);

            _workloadDay.TaskPeriodList[1].SetTasks(111);
            Assert.AreEqual(234, _workloadDay.TotalTasks);
        }

        [Test]
        public void VerifyChangingTasksInOnePeriodAffectsAverageAfterTaskTime()
        {
            _workloadDay.Close();
            _workloadDay.MakeOpen24Hours();

            Task t1 = new Task(123, new TimeSpan(1), new TimeSpan(0, 0, 0, 120, 500));
            _workloadDay.SortedTaskPeriodList[0].SetTasks(t1.Tasks);
            _workloadDay.SortedTaskPeriodList[0].AverageTaskTime = t1.AverageTaskTime;
            _workloadDay.SortedTaskPeriodList[0].AverageAfterTaskTime = t1.AverageAfterTaskTime;

            Task t2 = new Task(321, new TimeSpan(1), new TimeSpan(0, 0, 0, 240, 500));
            _workloadDay.SortedTaskPeriodList[1].SetTasks(t2.Tasks);
            _workloadDay.SortedTaskPeriodList[1].AverageTaskTime = t2.AverageTaskTime;
            _workloadDay.SortedTaskPeriodList[1].AverageAfterTaskTime = t2.AverageAfterTaskTime;

            Assert.AreEqual(new TimeSpan((long)(((_workloadDay.SortedTaskPeriodList[0].TotalTasks * _workloadDay.SortedTaskPeriodList[0].AverageAfterTaskTime.Ticks) +
                (_workloadDay.SortedTaskPeriodList[1].TotalTasks * _workloadDay.SortedTaskPeriodList[1].AverageAfterTaskTime.Ticks)) /
                _workloadDay.TotalTasks)), _workloadDay.AverageAfterTaskTime);

            _workloadDay.TaskPeriodList[0].SetTasks(1000);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 120, 500), _workloadDay.SortedTaskPeriodList[0].AverageAfterTaskTime);
            long p1TotalTicks = (long)_workloadDay.SortedTaskPeriodList[0].TotalTasks * _workloadDay.SortedTaskPeriodList[0].AverageAfterTaskTime.Ticks;
            long p2TotalTicks = (long)_workloadDay.SortedTaskPeriodList[1].TotalTasks * _workloadDay.SortedTaskPeriodList[1].AverageAfterTaskTime.Ticks;
            Assert.AreEqual(new TimeSpan((long)((p1TotalTicks + p2TotalTicks) / _workloadDay.TotalTasks)), _workloadDay.AverageAfterTaskTime);
        }

        [Test]
        public void VerifyChangingTasksInOnePeriodAffectsAverageTaskTime()
        {
            _workloadDay.Close();
            _workloadDay.MakeOpen24Hours();

            Task t1 = new Task(123, new TimeSpan(0, 0, 0, 120, 500), new TimeSpan(1));
            _workloadDay.SortedTaskPeriodList[0].SetTasks(t1.Tasks);
            _workloadDay.SortedTaskPeriodList[0].AverageTaskTime = t1.AverageTaskTime;
            _workloadDay.SortedTaskPeriodList[0].AverageAfterTaskTime = t1.AverageAfterTaskTime;

            Task t2 = new Task(321, new TimeSpan(0, 0, 0, 240, 500), new TimeSpan(1));
            _workloadDay.SortedTaskPeriodList[1].SetTasks(t2.Tasks);
            _workloadDay.SortedTaskPeriodList[1].AverageTaskTime = t2.AverageTaskTime;
            _workloadDay.SortedTaskPeriodList[1].AverageAfterTaskTime = t2.AverageAfterTaskTime;
            Assert.AreEqual(new TimeSpan((long)(((_workloadDay.SortedTaskPeriodList[0].TotalTasks * _workloadDay.SortedTaskPeriodList[0].AverageTaskTime.Ticks) +
                (_workloadDay.SortedTaskPeriodList[1].TotalTasks * _workloadDay.SortedTaskPeriodList[1].AverageTaskTime.Ticks)) /
                _workloadDay.TotalTasks)), _workloadDay.AverageTaskTime);

            _workloadDay.TaskPeriodList[0].SetTasks(1000);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 120, 500), _workloadDay.SortedTaskPeriodList[0].AverageTaskTime);
            long p1TotalTicks = (long)_workloadDay.SortedTaskPeriodList[0].TotalTasks * _workloadDay.SortedTaskPeriodList[0].AverageTaskTime.Ticks;
            long p2TotalTicks = (long)_workloadDay.SortedTaskPeriodList[1].TotalTasks * _workloadDay.SortedTaskPeriodList[1].AverageTaskTime.Ticks;

            Assert.AreEqual(new TimeSpan((long)((p1TotalTicks + p2TotalTicks) / _workloadDay.TotalTasks)), _workloadDay.AverageTaskTime);

        }

        [Test]
        public void CanMakeParent()
        {
            _workloadDay.Close();
            _workloadDay.MakeOpen24Hours();

            Task t1 = new Task(123, new TimeSpan(0, 0, 0, 120, 500), new TimeSpan(1));
            _workloadDay.SortedTaskPeriodList[0].SetTasks(t1.Tasks);
            _workloadDay.SortedTaskPeriodList[0].AverageTaskTime = t1.AverageTaskTime;
            _workloadDay.SortedTaskPeriodList[0].AverageAfterTaskTime = t1.AverageAfterTaskTime;

            Assert.AreEqual(_workloadDay, _workloadDay.TaskPeriodList[0].Parent);
        }

        [Test]
        public void CanSetId()
        {
            Guid newId = Guid.NewGuid();
            _workloadDay.SetId(newId);
            Assert.AreEqual(newId, _workloadDay.Id);
        }

        [Test]
        public void VerifyToStringWorks()
        {
            Assert.AreEqual("WorkloadDay, no id", _workloadDay.ToString());
        }

        [Test]
        public void VerifyGetHashCodeWorks()
        {
            IWorkloadDay workloadDayCopy = _workloadDay;
            Assert.IsNotNull(workloadDayCopy.GetHashCode());
            Assert.IsNotNull(_workloadDay.GetHashCode());
            Assert.AreEqual(_workloadDay.GetHashCode(), workloadDayCopy.GetHashCode());
        }
        [Test]
        public void VerifyWorkloadDayTemplateReferenceRenamedWithoutOld()
        {
            string name = "Igor";
            string newName = "Today";
            WorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();
            DateOnly wldDate = new DateOnly(2008, 10, 24);
            _openHours = new List<TimePeriod>();
            workloadDayTemplate.Create(wldDate, _workload, _openHours);
            IWorkloadDay workloadDay = CreateWorkloadDay(_openHours, workloadDayTemplate, name, wldDate);
            workloadDayTemplate.Name = newName;
            Assert.AreEqual(newName, workloadDay.TemplateReference.TemplateName);
        }

        [Test]
        public void CanCreateFromTemplate()
        {
            string name = "<YUCATAN>";
            DateTime createDate = new DateTime(2008, 01, 14, 0, 0, 0, DateTimeKind.Utc);
            IWorkloadDayTemplate workloadDayTemplate = CreateWorkloadDayTemplate(name, createDate);

            workloadDayTemplate.TaskPeriodList[0].SetTasks(500);
            workloadDayTemplate.TaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 12, 4);
            workloadDayTemplate.TaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.CreateFromTemplate(new DateOnly(createDate), _workload, workloadDayTemplate);

            Assert.AreEqual(workloadDay.TotalTasks, workloadDayTemplate.TotalTasks);
            Assert.AreEqual(workloadDay.AverageTaskTime, workloadDayTemplate.AverageTaskTime);
            Assert.AreEqual(workloadDay.AverageAfterTaskTime, workloadDayTemplate.AverageAfterTaskTime);
            Assert.AreNotSame(workloadDay, workloadDayTemplate);
            Assert.AreNotSame(workloadDay.TaskPeriodList[0], workloadDayTemplate.TaskPeriodList[0]);
            Assert.AreEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);
            Assert.AreEqual(workloadDayTemplate.Id, workloadDay.TemplateReference.TemplateId);
        }

        [Test]
        public void VerifyTemplateNameIsChangedWhenManipulatingIntradayTask()
        {
            //CreateProjection template
            //This template is 2008-01-14 A nice and shiny Monday in February
            DateTime createDate = new DateTime(2008, 01, 14, 0, 0, 0, DateTimeKind.Utc);
            string templateName = "<JULDAGEN>";
            IWorkloadDayTemplate workloadDayTemplate = CreateWorkloadDayTemplate(templateName, createDate);

            workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 12, 4);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            //CreateProjection from template this will create a standard Monday
            IWorkloadDay workloadDay = new WorkloadDay();
            workloadDay.CreateFromTemplate(new DateOnly(createDate), _workload, workloadDayTemplate);

            Assert.AreEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);

            //Now we will change the intraday on the workload
            workloadDay.SortedTaskPeriodList[0].SetTasks(700);

            Assert.AreNotEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreNotEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);
            Assert.AreNotEqual(workloadDayTemplate.Id, workloadDay.TemplateReference.TemplateId);
        }

        [Test]
        public void VerifyTemplateNameIsChangedWhenManipulatingIntradayAverageTaskTime()
        {
            //CreateProjection template
            //This template is 2008-01-14 A nice and shiny Monday in February
            DateTime createDate = new DateTime(2008, 01, 14, 0, 0, 0, DateTimeKind.Utc);
            string templateName = "<JULDAGEN>";
            IWorkloadDayTemplate workloadDayTemplate = CreateWorkloadDayTemplate(templateName, createDate);

            workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 12, 4);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            //CreateProjection from template this will create a standard Monday
            IWorkloadDay workloadDay = new WorkloadDay();
            workloadDay.CreateFromTemplate(new DateOnly(createDate), _workload, workloadDayTemplate);

            Assert.AreEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);

            //Now we will change the intraday on the workload
            workloadDay.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 13, 7);

            Assert.AreNotEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreNotEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);
        }

        [Test]
        public void VerifyTemplateNameIsChangedWhenManipulatingIntradayAverageAfterTaskTime()
        {
            //CreateProjection template
            //This template is 2008-01-14 A nice and shiny Monday in February
            DateTime createDate = new DateTime(2008, 01, 14, 0, 0, 0, DateTimeKind.Utc);
            string templateName = "<JULDAGEN>";
            IWorkloadDayTemplate workloadDayTemplate = CreateWorkloadDayTemplate(templateName, createDate);

            workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 12, 4);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            //CreateProjection from template this will create a standard Monday
            IWorkloadDay workloadDay = new WorkloadDay();
            workloadDay.CreateFromTemplate(new DateOnly(createDate), _workload, workloadDayTemplate);

            Assert.AreEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);

            //Now we will change the intraday on the workload
            workloadDay.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 14, 17);

            Assert.AreNotEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreNotEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);
        }

        [Test]
        public void VerifyTemplateNameIsChangedWhenManipulatingIntradayButWithTheSameValues()
        {
            //Anders changed spec. Templates should be set to "OLD" even if values are set back to their origin.
            //CreateProjection template
            //This template is 2008-01-14 A nice and shiny Monday in February
            DateTime createDate = new DateTime(2008, 01, 14, 0, 0, 0, DateTimeKind.Utc);
            string templateName = "<JULDAGEN>";
            IWorkloadDayTemplate workloadDayTemplate = CreateWorkloadDayTemplate(templateName, createDate);

            workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 12, 4);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            //CreateProjection from template this will create a standard Monday
            IWorkloadDay workloadDay = new WorkloadDay();
            workloadDay.CreateFromTemplate(new DateOnly(createDate), _workload, workloadDayTemplate);

            Assert.AreEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);

            //Now we will change the intraday on the workload
            workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 12, 4);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            Assert.AreNotEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreNotEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);
            Assert.IsTrue(workloadDayTemplate.VersionNumber > workloadDay.TemplateReference.VersionNumber);
        }

        [Test]
        public void VerifyTemplateNameIsNotChangedWhenManipulatingDayTask()
        {
            //CreateProjection template
            //This template is 2008-01-14 A nice and shiny Monday in February
            DateTime createDate = new DateTime(2008, 01, 14, 0, 0, 0, DateTimeKind.Utc);
            string templateName = "<JULDAGEN>";
            IWorkloadDayTemplate workloadDayTemplate = CreateWorkloadDayTemplate(templateName, createDate);

            workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 12, 4);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            //CreateProjection from template this will create a standard Monday
            IWorkloadDay workloadDay = new WorkloadDay();
            workloadDay.CreateFromTemplate(new DateOnly(createDate), _workload, workloadDayTemplate);

            Assert.AreEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);

            //Now we will change the intraday on the workload
            workloadDay.Tasks = 70000;
            Assert.AreEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);
        }

        [Test]
        public void VerifyTemplateNameIsNotChangedWhenManipulatingAverageTaskTime()
        {
            //CreateProjection template
            //This template is 2008-01-14 A nice and shiny Monday in February
            DateTime createDate = new DateTime(2008, 01, 14, 0, 0, 0, DateTimeKind.Utc);
            string templateName = "<JULDAGEN>";
            IWorkloadDayTemplate workloadDayTemplate = CreateWorkloadDayTemplate(templateName, createDate);

            workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 12, 4);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            //CreateProjection from template this will create a standard Monday
            IWorkloadDay workloadDay = new WorkloadDay();
            workloadDay.CreateFromTemplate(new DateOnly(createDate), _workload, workloadDayTemplate);

            Assert.AreEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);

            //Now we will change the intraday on the workload
            workloadDay.AverageTaskTime = new TimeSpan(0, 15, 4);

            Assert.AreEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);
        }

        [Test]
        public void VerifyTemplateNameIsNotChangedWhenManipulatingAverageAfterTaskTime()
        {
            //CreateProjection template
            //This template is 2008-01-14 A nice and shiny Monday in February
            DateTime createDate = new DateTime(2008, 01, 14, 0, 0, 0, DateTimeKind.Utc);
            string templateName = "<JULDAGEN>";
            IWorkloadDayTemplate workloadDayTemplate = CreateWorkloadDayTemplate(templateName, createDate);

            workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 12, 4);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            //CreateProjection from template this will create a standard Monday
            IWorkloadDay workloadDay = new WorkloadDay();
            workloadDay.CreateFromTemplate(new DateOnly(createDate), _workload, workloadDayTemplate);

            Assert.AreEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);

            //Now we will change the intraday on the workload
            workloadDay.AverageAfterTaskTime = new TimeSpan(0, 17, 4);
            Assert.AreEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreEqual(workloadDayTemplate.Name, workloadDay.TemplateReference.TemplateName);
        }

	    [Test]
	    public void CanDistributeTasks()
	    {
			DateOnly createDate = new DateOnly(2008, 01, 14);
			var createLocalDate = TimeZoneInfo.ConvertTimeFromUtc(createDate.Date, _workload.Skill.TimeZone);
			string templateName = "JULDAGEN";
			IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

			IList<TimePeriod> openHours = new List<TimePeriod>();
			IWorkloadDay workloadDay = CreateWorkloadDay(openHours, workloadDayTemplate, templateName, createDate);
			int originalTemplateVersionNumber = workloadDayTemplate.VersionNumber;
			Assert.AreEqual(originalTemplateVersionNumber, workloadDay.TemplateReference.VersionNumber);

			//change Monday's open hour in workload template
			_workload.TemplateWeekCollection[1].ChangeOpenHours(new[] { openHours[0], openHours[1] });

			//Add som changes to the template
			ChangeTemplate(workloadDayTemplate);

			int latestTemplateVersionNumber = workloadDayTemplate.VersionNumber;
			Assert.Less(originalTemplateVersionNumber, latestTemplateVersionNumber);
			var expectedTemplateName = string.Format(CultureInfo.CurrentUICulture, "<{0} {1} {2}>", templateName, createLocalDate.ToShortDateString(), createLocalDate.ToShortTimeString());
			Assert.AreEqual(expectedTemplateName, workloadDay.TemplateReference.TemplateName);

			//Calculate the sums
			double sumOfTemplateTasks = workloadDayTemplate.TotalTasks;

			//Set some values on the real workload
			workloadDay.ChangeOpenHours(_openHours);
			workloadDay.Tasks = 10000;
			workloadDay.CampaignTasks = new Percent(20);
			workloadDay.AverageTaskTime = TimeSpan.FromSeconds(100);
			workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(33);

			Assert.AreEqual(_openHours[0], workloadDay.OpenHourList[0]);

			workloadDay.DistributeTasks(workloadDayTemplate.SortedTaskPeriodList);

			Assert.AreNotEqual(_openHours[0], workloadDay.OpenHourList[0]);
			Assert.AreEqual(openHours[0], workloadDay.OpenHourList[0]);
			Assert.AreEqual(openHours[1], workloadDay.OpenHourList[1]);

			double newSumOfTasks = workloadDay.Tasks;

			Assert.AreEqual((workloadDayTemplate.SortedTaskPeriodList[3].Tasks / sumOfTemplateTasks) * newSumOfTasks, workloadDay.SortedTaskPeriodList[3].Tasks);
			Assert.AreEqual(workloadDay.CampaignTasks, workloadDay.SortedTaskPeriodList[3].CampaignTasks);
			Assert.AreEqual(workloadDay.AverageTaskTime, workloadDay.SortedTaskPeriodList[3].AverageTaskTime);
			Assert.AreEqual(workloadDay.AverageAfterTaskTime, workloadDay.SortedTaskPeriodList[3].AverageAfterTaskTime);
	    }

	    [Test]
        public void CanApplyTemplate()
        {
            DateOnly createDate = new DateOnly(2008, 01, 14);
            var createLocalDate = TimeZoneInfo.ConvertTimeFromUtc(createDate.Date, _workload.Skill.TimeZone);
            string templateName = "JULDAGEN";
            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            IWorkloadDay workloadDay = CreateWorkloadDay(openHours, workloadDayTemplate, templateName, createDate);
            int originalTemplateVersionNumber = workloadDayTemplate.VersionNumber;
            Assert.AreEqual(originalTemplateVersionNumber, workloadDay.TemplateReference.VersionNumber);

            //Add som changes to the template
            ChangeTemplate(workloadDayTemplate);

            int latestTemplateVersionNumber = workloadDayTemplate.VersionNumber;
            Assert.Less(originalTemplateVersionNumber, latestTemplateVersionNumber);
            var expectedTemplateName = string.Format(CultureInfo.CurrentUICulture, "<{0} {1} {2}>", templateName, createLocalDate.ToShortDateString(), createLocalDate.ToShortTimeString());
            Assert.AreEqual(expectedTemplateName, workloadDay.TemplateReference.TemplateName);

            //Calculate the sums
            double sumOfTemplateTasks = workloadDayTemplate.TotalTasks;
            double averageTaskTime = workloadDayTemplate.TotalAverageTaskTime.Ticks;
            double averageAfterTaskTime = workloadDayTemplate.TotalAverageAfterTaskTime.Ticks;

            //Set some values on the real workload
            workloadDay.Tasks = 10000;
            workloadDay.AverageTaskTime = new TimeSpan(0, 0, 100);
            workloadDay.AverageAfterTaskTime = new TimeSpan(0, 0, 33);
            workloadDay.ChangeOpenHours(_openHours);

            Assert.AreEqual(_openHours[0], workloadDay.OpenHourList[0]);

            //Apply the template
            workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());
            Assert.AreEqual(latestTemplateVersionNumber, workloadDay.TemplateReference.VersionNumber);

            Assert.AreEqual(templateName, workloadDay.TemplateReference.TemplateName);
            Assert.AreNotEqual(_openHours[0], workloadDay.OpenHourList[0]);
            Assert.AreEqual(openHours[0], workloadDay.OpenHourList[0]);
            Assert.AreEqual(openHours[1], workloadDay.OpenHourList[1]);

            double newSumOfTasks = workloadDay.Tasks;
            double newAverageTaskTime = workloadDay.AverageTaskTime.Ticks;
            double newAverageAfterTaskTime = workloadDay.AverageAfterTaskTime.Ticks;

            Assert.AreEqual((workloadDayTemplate.SortedTaskPeriodList[3].Tasks / sumOfTemplateTasks) * newSumOfTasks, workloadDay.SortedTaskPeriodList[3].Tasks);

            //There is a rounding problem here checks that it is not more than 0.01 milliseconds
            TimeSpan averageTaskTimeSpan = new TimeSpan((long)((workloadDayTemplate.SortedTaskPeriodList[2].AverageTaskTime.Ticks / averageTaskTime) * newAverageTaskTime));
            double diff = Math.Abs(averageTaskTimeSpan.TotalMilliseconds - workloadDay.SortedTaskPeriodList[2].AverageTaskTime.TotalMilliseconds);
            Assert.Less(diff, 0.01);

            TimeSpan averageAfterTaskTimeSpan = new TimeSpan((long)((workloadDayTemplate.SortedTaskPeriodList[3].AverageAfterTaskTime.Ticks / averageAfterTaskTime) * newAverageAfterTaskTime));
            diff = Math.Abs(averageAfterTaskTimeSpan.TotalMilliseconds - workloadDay.SortedTaskPeriodList[3].AverageAfterTaskTime.TotalMilliseconds);
            Assert.Less(diff, 0.01);
        }

        [Test]
        public void CanDistributeIfWeHaveZeroTalkTimeAndACWInTemplate()
        {
            DateOnly createDate = new DateOnly(2008, 01, 14);
            string templateName = "JULDAGEN";
            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            IWorkloadDay workloadDay = CreateWorkloadDay(openHours, workloadDayTemplate, templateName, createDate);
            
            //Add som changes to the template
            SetWorkloadDayTemplateTalkTimeAndACW(workloadDayTemplate);
            
            //Set some values on the real workload
            var originalAverageTaskTime = new TimeSpan(0, 0, 100);
            var originalAverageAfterTaskTime = new TimeSpan(0, 0, 33);


            workloadDay.Tasks = 10000;
            workloadDay.AverageTaskTime = new TimeSpan(0, 0, 100);
            workloadDay.AverageAfterTaskTime = new TimeSpan(0, 0, 33);
            workloadDay.ChangeOpenHours(_openHours);

            //Apply the template
            workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());
            
            //There is a rounding problem here checks that it is not more than 0.01 milliseconds
            double diff = Math.Abs(originalAverageTaskTime.Milliseconds - workloadDay.AverageTaskTime.Milliseconds );
            Assert.Less(diff, 0.01);

            diff = Math.Abs(originalAverageAfterTaskTime.TotalMilliseconds - workloadDay.AverageAfterTaskTime.TotalMilliseconds);
            Assert.Less(diff, 0.01);
        }

        private void SetWorkloadDayTemplateTalkTimeAndACW(IWorkloadDayTemplate workloadDayTemplate)
        {
            workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 0, 1);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 0, 2);

            workloadDayTemplate.SortedTaskPeriodList[1].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[1].AverageTaskTime = new TimeSpan(0, 0, 0);
            workloadDayTemplate.SortedTaskPeriodList[1].AverageAfterTaskTime = new TimeSpan(0, 0, 0);

            workloadDayTemplate.SortedTaskPeriodList[2].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[2].AverageTaskTime = new TimeSpan(0, 0, 0);
            workloadDayTemplate.SortedTaskPeriodList[2].AverageAfterTaskTime = new TimeSpan(0, 0, 0);

            workloadDayTemplate.SortedTaskPeriodList[3].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[3].AverageTaskTime = new TimeSpan(0, 0, 0);
            workloadDayTemplate.SortedTaskPeriodList[3].AverageAfterTaskTime = new TimeSpan(0, 0, 0);
        }


        [Test]
        public void CanApplyTemplateIfCallsTalkTimeAndACWIsZERO()
        {
            DateOnly createDate = new DateOnly(2008, 01, 14);
            string templateName = "JULDAGEN";
            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            IWorkloadDay workloadDay = CreateWorkloadDay(openHours, workloadDayTemplate, templateName, createDate);

            //Add som changes to the template
            ChangeTemplate2(workloadDayTemplate);

            //Calculate the sums
            double sumOfTemplateTasks = workloadDayTemplate.TotalTasks;
            double averageTaskTime = workloadDayTemplate.TotalAverageTaskTime.TotalSeconds;
            double averageAfterTaskTime = workloadDayTemplate.TotalAverageAfterTaskTime.TotalSeconds;

            //Set some values on the real workload
            workloadDay.Tasks = 0;
            workloadDay.AverageTaskTime = new TimeSpan(0, 0, 0);
            workloadDay.AverageAfterTaskTime = new TimeSpan(0, 0, 0);
            workloadDay.ChangeOpenHours(_openHours);

            Assert.AreEqual(_openHours[0], workloadDay.OpenHourList[0]);

            //Apply the template
            workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());

            Assert.AreEqual(workloadDay.Tasks, sumOfTemplateTasks);
            Assert.AreEqual(workloadDay.AverageTaskTime.TotalSeconds, averageTaskTime);
            Assert.AreEqual(workloadDay.AverageAfterTaskTime.TotalSeconds, averageAfterTaskTime);

        }

        [Test]
        public void CanApplyTemplateWithPreservedStatistics()
        {
            MockRepository mocks = new MockRepository();
            DateOnly createDate = new DateOnly(2008, 01, 14);
            const string templateName = "JULDAGEN";

            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();
            IList<TimePeriod> openHours = new List<TimePeriod>();
            IWorkloadDay workloadDay = CreateWorkloadDay(openHours, workloadDayTemplate, templateName, createDate);

            IQueueStatisticsProvider provider = mocks.StrictMock<IQueueStatisticsProvider>();

            using (mocks.Record())
            {
                Expect.Call(provider.GetStatisticsForPeriod(workloadDay.SortedTaskPeriodList[4].Period)).Return(new StatisticTask
                {
                    Interval = workloadDay.SortedTaskPeriodList[4].Period.StartDateTime,
                    StatCalculatedTasks = 200
                }).Repeat.AtLeastOnce();
                Expect.Call(provider.GetStatisticsForPeriod(new DateTimePeriod())).IgnoreArguments().Return(new StatisticTask()).Repeat.AtLeastOnce();
            }

            using (mocks.Playback())
            {
                //Set some values on the real workload
                workloadDay.Tasks = 10000;
                workloadDay.AverageTaskTime = new TimeSpan(0, 0, 100);
                workloadDay.AverageAfterTaskTime = new TimeSpan(0, 0, 33);
                workloadDay.SetQueueStatistics(provider);
                workloadDay.ChangeOpenHours(_openHours);
                Assert.AreEqual(200, workloadDay.TotalStatisticCalculatedTasks);

                //Apply the template
                workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());

                Assert.AreEqual(200, workloadDay.TotalStatisticCalculatedTasks);
            }
        }

        [Test]
        public void VerifyApplyTemplateDuringDstSwitch()
        {
            _skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _skill.MidnightBreakOffset = TimeSpan.FromHours(4);

            var createDate = new DateOnly(2008, 10, 25);
            string templateName = "SUNDAY";
            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            IWorkloadDay workloadDay = CreateWorkloadDay(openHours, workloadDayTemplate, templateName, createDate);
            workloadDay.Close();
            workloadDayTemplate.Close();
            workloadDayTemplate.MakeOpen24Hours();

            workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());
            Assert.AreEqual(100, workloadDay.TaskPeriodList.Count);

            createDate = new DateOnly(2009, 3, 28);
            workloadDay = CreateWorkloadDay(openHours, workloadDayTemplate, templateName, createDate);
            workloadDay.Close();
            workloadDayTemplate.Close();
            workloadDayTemplate.MakeOpen24Hours();

            workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());
            Assert.AreEqual(92, workloadDay.TaskPeriodList.Count);
        }

        [Test]
        public void VerifyApplyTemplateDuringDstSwitchWithMidnightBreak()
        {
            _skill.MidnightBreakOffset = TimeSpan.FromHours(2);
            _skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

            var createDate = new DateOnly(2009, 10, 24);
            string templateName = "SUNDAY";
            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            _openHours = new List<TimePeriod> { new TimePeriod(2, 0, 26, 0) };
            IWorkloadDay workloadDay = CreateWorkloadDay(_openHours, workloadDayTemplate, templateName, createDate);
            workloadDay.Close();
            workloadDayTemplate.Close();
            workloadDayTemplate.MakeOpen24Hours();

            workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());
            Assert.AreEqual(100, workloadDay.TaskPeriodList.Count);

            createDate = new DateOnly(2009, 3, 29);
            workloadDay = CreateWorkloadDay(_openHours, workloadDayTemplate, templateName, createDate);
            workloadDay.Close();
            workloadDayTemplate.Close();
            workloadDayTemplate.MakeOpen24Hours();

            workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());
            Assert.AreEqual(92, workloadDay.TaskPeriodList.Count);
        }

        [Test]
        public void VerifyApplyTemplateDuringDstSwitchWithNotAffectedOpenHours()
        {
            _skill.MidnightBreakOffset = TimeSpan.FromHours(2);
            _skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

            var createDate = new DateOnly(2009, 10, 24);
            const string templateName = "SUNDAY";

            _openHours = new List<TimePeriod> { new TimePeriod(7, 0, 18, 0) };
            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();
            workloadDayTemplate.Create(templateName, DateTime.SpecifyKind(createDate.Date, DateTimeKind.Utc), _workload, _openHours);
            workloadDayTemplate.Tasks = 440;
            workloadDayTemplate.AverageTaskTime = TimeSpan.FromSeconds(120);
            workloadDayTemplate.SetId(Guid.NewGuid());
            if (!_workload.TemplateWeekCollection.Values.Contains(workloadDayTemplate)) _workload.AddTemplate(workloadDayTemplate);

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.CreateFromTemplate(createDate, _workload, workloadDayTemplate);

            Assert.AreEqual(44, workloadDay.TaskPeriodList.Count);
            Assert.IsTrue(workloadDay.TaskPeriodList.All(t => t.Tasks == 10));

            createDate = new DateOnly(2009, 3, 29);
            workloadDay = new WorkloadDay();
            workloadDay.CreateFromTemplate(createDate, _workload, workloadDayTemplate);

            Assert.AreEqual(44, workloadDay.TaskPeriodList.Count);
            Assert.AreEqual(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2009, 03, 29, 7, 0, 0), _skill.TimeZone),
                workloadDay.SortedTaskPeriodList[0].Period.StartDateTime);
            Assert.IsTrue(workloadDay.TaskPeriodList.All(t => t.Tasks == 10));
        }

        [Test]
        public void VerifyCanApplyTemplateAndStatisticsNotTouched()
        {
            MockRepository mocks = new MockRepository();
            DateOnly createDate = new DateOnly(2008, 01, 14);
            string templateName = "<JULDAGEN>";
            WorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();

            WorkloadDay workloadDay = CreateWorkloadDay(openHours, workloadDayTemplate, templateName, createDate);
            workloadDay.CreateFromTemplate(createDate, _workload, workloadDayTemplate);

            IQueueStatisticsProvider provider = mocks.StrictMock<IQueueStatisticsProvider>();

            using (mocks.Record())
            {
                Expect.Call(provider.GetStatisticsForPeriod(workloadDay.TaskPeriodList[0].Period)).Return(new StatisticTask
                {
                    Interval = workloadDay.TaskPeriodList[0].Period.StartDateTime,
                    StatCalculatedTasks = 1000,
                    StatAbandonedTasks = 50,
                    StatAnsweredTasks = 950,
                    StatAverageTaskTimeSeconds = 60,
                    StatAverageAfterTaskTimeSeconds = 120
                }).Repeat.AtLeastOnce();
                Expect.Call(provider.GetStatisticsForPeriod(new DateTimePeriod())).IgnoreArguments().Return(new StatisticTask()).Repeat.AtLeastOnce();
            }

            using (mocks.Playback())
            {
                workloadDay.SetQueueStatistics(provider);

                //Add som changes to the template
                ChangeTemplate(workloadDayTemplate);
                //Apply the template
                workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());

                Assert.AreEqual(1000, workloadDay.TotalStatisticCalculatedTasks);
                Assert.AreEqual(50, workloadDay.TotalStatisticAbandonedTasks);
                Assert.AreEqual(950, workloadDay.TotalStatisticAnsweredTasks);
                Assert.AreEqual(new TimeSpan(0, 0, 1, 0), workloadDay.TotalStatisticAverageTaskTime);
                Assert.AreEqual(new TimeSpan(0, 0, 2, 0), workloadDay.TotalStatisticAverageAfterTaskTime);
            }
        }

        [Test]
        public void VerifyCanApplyTemplateAndDayAverageHandlingTimeNotTouched()
        {
            DateOnly createDate = new DateOnly(2008, 01, 14);
            string templateName = "<JULDAGEN>";
            WorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(7, 0, 0), new TimeSpan(11, 0, 0)));
            workloadDayTemplate.Create(templateName, DateTime.SpecifyKind(createDate.Date, DateTimeKind.Utc), _workload, openHours);
            foreach (ITemplateTaskPeriod taskPeriod in workloadDayTemplate.SortedTaskPeriodList)
            {
                taskPeriod.AverageTaskTime = new TimeSpan(0, 1, 0);
                taskPeriod.Tasks = 1;
            }

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(createDate, _workload, openHours);

            TimeSpan expectedAverageTaskTime = new TimeSpan(0, 0, 2, 0);
            workloadDay.AverageTaskTime = expectedAverageTaskTime;
            workloadDay.Tasks = 1;

            //Apply the template
            workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());

            Assert.AreEqual(expectedAverageTaskTime, workloadDay.AverageTaskTime);
        }

        [Test]
        public void VerifyCanApplyTemplateAndDayAverageAfterHandlingTimeNotTouched()
        {
            DateOnly createDate = new DateOnly(2008, 01, 14);
            string templateName = "<JULDAGEN>";
            WorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(7, 0, 0), new TimeSpan(11, 0, 0)));
            workloadDayTemplate.Create(templateName, DateTime.SpecifyKind(createDate.Date, DateTimeKind.Utc), _workload, openHours);
            foreach (ITemplateTaskPeriod taskPeriod in workloadDayTemplate.TaskPeriodList)
            {
                taskPeriod.AverageAfterTaskTime = new TimeSpan(0, 1, 0);
                taskPeriod.AverageTaskTime = new TimeSpan(0, 1, 0);
                taskPeriod.Tasks = 1;
            }

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(createDate, _workload, openHours);
            workloadDay.Tasks = 1;

            TimeSpan expectedAverageAfterTaskTime = new TimeSpan(0, 0, 2, 0);
            workloadDay.AverageAfterTaskTime = expectedAverageAfterTaskTime;

            //Apply the template
            workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());

            Assert.AreEqual(expectedAverageAfterTaskTime, workloadDay.AverageAfterTaskTime);
        }
        [Test]
        public void VerifyCanApplyTemplateAndDayAverageAfterHandlingTimeNotTouchedWhenAllZeros()
        {
            DateOnly createDate = new DateOnly(2008, 01, 14);
            string templateName = "<JULDAGEN>";
            WorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(7, 0, 0), new TimeSpan(11, 0, 0)));
            workloadDayTemplate.Create(templateName, DateTime.SpecifyKind(createDate.Date, DateTimeKind.Utc), _workload, openHours);
            foreach (ITemplateTaskPeriod taskPeriod in workloadDayTemplate.TaskPeriodList)
            {
                taskPeriod.AverageAfterTaskTime = new TimeSpan(0, 0, 0);
                taskPeriod.AverageTaskTime = new TimeSpan(0, 0, 0);
                taskPeriod.Tasks = 0;
            }

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(createDate, _workload, openHours);
            workloadDay.Tasks = 0;

            TimeSpan expectedAverageAfterTaskTime = new TimeSpan(0, 0, 2, 0);
            workloadDay.AverageAfterTaskTime = expectedAverageAfterTaskTime;

            //Apply the template
            workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());

            Assert.AreEqual(expectedAverageAfterTaskTime, workloadDay.AverageAfterTaskTime);
        }

        [Test]
        public void VerifyChangingOpenHoursWithNoExistingTemplateTaskPeriods()
        {
            _skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            IWorkloadDayTemplate template1 = new WorkloadDayTemplate();
            template1.Create("Template1", DateTime.SpecifyKind(_dt.Date, DateTimeKind.Utc), _workload, new List<TimePeriod>());
            _workloadDay.ApplyTemplate(template1, day => day.Lock(), day => day.Release());

            Assert.IsFalse(_workloadDay.OpenForWork.IsOpen);
            Assert.IsFalse(template1.OpenForWork.IsOpen);
            Assert.AreEqual(0, template1.TaskPeriodList.Count);
            Assert.AreEqual(0, _workloadDay.TaskPeriodList.Count);

            template1.ChangeOpenHours(new List<TimePeriod> { new TimePeriod(7, 0, 15, 0) });
            _workloadDay.ApplyTemplate(template1, day => day.Lock(), day => day.Release());

            Assert.IsTrue(_workloadDay.OpenForWork.IsOpen);
            Assert.IsTrue(template1.OpenForWork.IsOpen);
            Assert.AreEqual(32, template1.TaskPeriodList.Count);
            Assert.AreEqual(32, _workloadDay.TaskPeriodList.Count);
        }

        [Test]
        public void VerifyCanSetDifferentTaskTimeDistributionDependingOnSkillType()
        {
            _workloadDay.ChangeOpenHours(new List<TimePeriod> { new TimePeriod(7, 0, 7, 30) });

            Assert.IsInstanceOf<TaskTimePhoneDistributionService>(_skill.SkillType.TaskTimeDistributionService);

            _workloadDay.Tasks = 20;
            _workloadDay.TaskPeriodList[0].AverageTaskTime = TimeSpan.FromSeconds(20);
            _workloadDay.TaskPeriodList[1].AverageTaskTime = TimeSpan.FromSeconds(40);

            Assert.AreEqual(TimeSpan.FromSeconds(30), _workloadDay.AverageTaskTime);

            _workloadDay.AverageTaskTime = TimeSpan.FromSeconds(60);

            Assert.AreEqual(TimeSpan.FromSeconds(40), _workloadDay.TaskPeriodList[0].AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(80), _workloadDay.TaskPeriodList[1].AverageTaskTime);

            _skill.SkillType = new SkillTypeEmail(new Description("Test"), ForecastSource.Email);

            _workloadDay.AverageTaskTime = TimeSpan.FromSeconds(30);

            Assert.AreEqual(TimeSpan.FromSeconds(30), _workloadDay.TaskPeriodList[0].AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(30), _workloadDay.TaskPeriodList[1].AverageTaskTime);
        }

        [Test]
        public void VerifyCanSetStatistics()
        {
            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();
            DateTime createDate = new DateTime(2008, 01, 14, 0, 0, 0, DateTimeKind.Utc);

            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(1, 2, 0, 0)));
            openHours.Add(new TimePeriod(new TimeSpan(6, 0, 0), new TimeSpan(9, 0, 0)));
            workloadDayTemplate.Create("Kalle", createDate, _workload, openHours);
            _workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());
            var realWorkloadDay = (WorkloadDayBase)_workloadDay;
            realWorkloadDay.TotalStatisticCalculatedTasks = 1000;
            realWorkloadDay.TotalStatisticAbandonedTasks = 50;
            realWorkloadDay.TotalStatisticAnsweredTasks = 950;
            realWorkloadDay.TotalStatisticAverageTaskTime = new TimeSpan(0, 0, 1, 0);
            realWorkloadDay.TotalStatisticAverageAfterTaskTime = new TimeSpan(0, 0, 2, 0);
            Assert.AreEqual(1000, _workloadDay.TotalStatisticCalculatedTasks);
            Assert.AreEqual(50, _workloadDay.TotalStatisticAbandonedTasks);
            Assert.AreEqual(950, _workloadDay.TotalStatisticAnsweredTasks);
            Assert.AreEqual(new TimeSpan(0, 0, 1, 0), _workloadDay.TotalStatisticAverageTaskTime);
            Assert.AreEqual(new TimeSpan(0, 0, 2, 0), _workloadDay.TotalStatisticAverageAfterTaskTime);
        }

        [Test]
        public void VerifyCanCreateEmailWorkloadDay()
        {
            SetupEmailWorkload();

            IList<TimePeriod> openHours = new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) };
            _workloadDay = new WorkloadDay();
            _workloadDay.Create(_dt, _workload, openHours);

            Assert.AreEqual(24, _workloadDay.TaskPeriodList.Count);
            Assert.AreEqual(1, _workloadDay.OpenHourList.Count);
            Assert.AreEqual(openHours[0], _workloadDay.OpenHourList[0]);
        }

        [Test]
        public void VerifyCanSetValuesOnClosedEmailWorkloadDay()
        {
            SetupEmailWorkload();

            IList<TimePeriod> openHours = new List<TimePeriod> { new TimePeriod(TimeSpan.FromHours(2), TimeSpan.FromHours(2)) };
            _workloadDay = new WorkloadDay();
            _workloadDay.Create(_dt, _workload, openHours);

            Assert.AreEqual(24, _workloadDay.TaskPeriodList.Count);
            Assert.IsFalse(_workloadDay.OpenForWork.IsOpen);

            _workloadDay.Tasks = 240;

            Assert.AreEqual(10, _workloadDay.TaskPeriodList[0].Tasks);
            Assert.AreEqual(10, _workloadDay.TaskPeriodList[23].Tasks);
        }

        [Test]
        public void ShouldKeepNumberOfEmailsWhenClosingWorkloadDayWithTemplate()
        {
            SetupEmailWorkload();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            DateTime createDate = new DateTime(2008, 01, 14, 0, 0, 0, DateTimeKind.Utc);

            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();
            workloadDayTemplate.Create("Kalle", createDate, _workload, openHours);

            _workloadDay = new WorkloadDay();
            _workloadDay.Create(_dt, _workload, openHours);
            _workloadDay.MakeOpen24Hours();
            _workloadDay.Tasks = 240;

            Assert.AreEqual(24, _workloadDay.TaskPeriodList.Count);

            _workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());

            Assert.IsFalse(_workloadDay.OpenForWork.IsOpen);
            Assert.AreEqual(10, _workloadDay.TaskPeriodList[0].Tasks);
            Assert.AreEqual(10, _workloadDay.TaskPeriodList[23].Tasks);
        }

        [Test]
        public void VerifyCanApplyEmailTemplate()
        {
            SetupEmailWorkload();

            IList<TimePeriod> openHours = new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) };
            DateTime createDate = new DateTime(2008, 01, 14, 0, 0, 0, DateTimeKind.Utc);

            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();
            workloadDayTemplate.Create("Kalle", createDate, _workload, openHours);
            _workloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());

            Assert.AreEqual(24, _workloadDay.TaskPeriodList.Count);
            Assert.AreEqual(1, _workloadDay.OpenHourList.Count);
            Assert.AreEqual(openHours[0], _workloadDay.OpenHourList[0]);
        }

        [Test]
        public void VerifyCanCreateFromEmailTemplate()
        {
            SetupEmailWorkload();

            _workloadDay = new WorkloadDay();
            _workloadDay.Create(_dt, _workload, _openHours);
            _workloadDay.Tasks = 999;

            IList<TimePeriod> openHours = new List<TimePeriod>();
            var createDate = new DateOnly(2008, 01, 14);

            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();
            workloadDayTemplate.Create("Kalle", DateTime.SpecifyKind(createDate.Date, DateTimeKind.Utc), _workload, openHours);
            _workloadDay.CreateFromTemplate(createDate, _workload, workloadDayTemplate);

            Assert.AreEqual(24, _workloadDay.TaskPeriodList.Count);
            Assert.IsFalse(_workloadDay.OpenForWork.IsOpen);
        }

        [Test]
        public void VerifyCanCloseDayAndStillHaveTaskPeriods()
        {
            SetupEmailWorkload();

            _workloadDay = new WorkloadDay();
            _workloadDay.Create(_dt, _workload, _openHours);
            _workloadDay.Close();

            Assert.IsFalse(_workloadDay.OpenForWork.IsOpen);
            Assert.AreEqual(24, _workloadDay.TaskPeriodList.Count);
        }

        [Test]
        public void VerifyCanSetNewOpenHoursStillHaveTaskPeriods()
        {
            SetupEmailWorkload();

            IList<TimePeriod> openHours = new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) };

            _workloadDay = new WorkloadDay();
            _workloadDay.Create(_dt, _workload, _openHours);
            _workloadDay.ChangeOpenHours(openHours);

            Assert.AreEqual(24, _workloadDay.TaskPeriodList.Count);
            Assert.AreEqual(1, _workloadDay.OpenHourList.Count);
            Assert.AreEqual(openHours[0], _workloadDay.OpenHourList[0]);
        }

        [Test]
        public void VerifyCanGetIsOnlyIncomingInterval()
        {
            SetupEmailWorkload();

            IList<TimePeriod> openHours = new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) };

            _workloadDay = new WorkloadDay();
            _workloadDay.Create(_dt, _workload, _openHours);
            _workloadDay.ChangeOpenHours(openHours);

            Assert.IsTrue(_workloadDay.IsOnlyIncoming(_workloadDay.SortedTaskPeriodList[5]));
            Assert.IsFalse(_workloadDay.IsOnlyIncoming(_workloadDay.SortedTaskPeriodList[6]));
            Assert.IsFalse(_workloadDay.IsOnlyIncoming(_workloadDay.SortedTaskPeriodList[14]));
            Assert.IsTrue(_workloadDay.IsOnlyIncoming(_workloadDay.SortedTaskPeriodList[15]));
        }

        [Test]
        public void VerifyCannotMergeOverOpenHours()
        {
            SetupEmailWorkload();

            IList<TimePeriod> openHours = new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) };

            _workloadDay = new WorkloadDay();
            _workloadDay.Create(_dt, _workload, _openHours);
            _workloadDay.ChangeOpenHours(openHours);

            _workloadDay.MergeTemplateTaskPeriods(new List<ITemplateTaskPeriod>
                                                      {
                                                          _workloadDay.SortedTaskPeriodList[5],
                                                          _workloadDay.SortedTaskPeriodList[6],
                                                          _workloadDay.SortedTaskPeriodList[7]
                                                      });

            Assert.AreEqual(23, _workloadDay.TaskPeriodList.Count);
            Assert.AreEqual(TimeSpan.FromHours(1), _workloadDay.SortedTaskPeriodList[5].Period.ElapsedTime());
            Assert.AreEqual(TimeSpan.FromHours(2), _workloadDay.SortedTaskPeriodList[6].Period.ElapsedTime());
        }

        [Test]
        public void VerifyCanGetTaskPeriodsWithinOpenHours()
        {
            SetupEmailWorkload();

            IList<TimePeriod> openHours = new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) };

            _workloadDay = new WorkloadDay();
            _workloadDay.Create(_dt, _workload, openHours);

            Assert.AreEqual(24, _workloadDay.TaskPeriodList.Count);
            Assert.AreEqual(9, _workloadDay.OpenTaskPeriodList.Count);
        }

        private void SetupEmailWorkload()
        {
            _skill.SkillType = new SkillTypeEmail(new Description("Email"), ForecastSource.Email);
            _skill.DefaultResolution = _skill.SkillType.DefaultResolution;
        }
        private static void ChangeTemplate2(IWorkloadDayTemplate workloadDayTemplate)
        {
            workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 20, 00);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 40, 00);

            workloadDayTemplate.SortedTaskPeriodList[1].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[1].AverageTaskTime = new TimeSpan(0, 20, 00);
            workloadDayTemplate.SortedTaskPeriodList[1].AverageAfterTaskTime = new TimeSpan(0, 40, 0);

            workloadDayTemplate.SortedTaskPeriodList[2].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[2].AverageTaskTime = new TimeSpan(0, 20, 0);
            workloadDayTemplate.SortedTaskPeriodList[2].AverageAfterTaskTime = new TimeSpan(0, 40, 0);

            workloadDayTemplate.SortedTaskPeriodList[3].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[3].AverageTaskTime = new TimeSpan(0, 20, 0);
            workloadDayTemplate.SortedTaskPeriodList[3].AverageAfterTaskTime = new TimeSpan(0, 40, 0);
        }

        private static void ChangeTemplate(IWorkloadDayTemplate workloadDayTemplate)
        {
            workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 0, 10);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            workloadDayTemplate.SortedTaskPeriodList[1].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[1].AverageTaskTime = new TimeSpan(0, 0, 10);
            workloadDayTemplate.SortedTaskPeriodList[1].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            workloadDayTemplate.SortedTaskPeriodList[2].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[2].AverageTaskTime = new TimeSpan(0, 0, 10);
            workloadDayTemplate.SortedTaskPeriodList[2].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            workloadDayTemplate.SortedTaskPeriodList[3].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[3].AverageTaskTime = new TimeSpan(0, 0, 10);
            workloadDayTemplate.SortedTaskPeriodList[3].AverageAfterTaskTime = new TimeSpan(0, 8, 4);
        }


        private WorkloadDay CreateWorkloadDay(IList<TimePeriod> openHours, IWorkloadDayTemplate workloadDayTemplate, string templateName, DateOnly createDate)
        {
            if (openHours.Count == 0)
            {
                openHours.Add(new TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(1, 2, 0, 0)));
                openHours.Add(new TimePeriod(new TimeSpan(6, 0, 0), new TimeSpan(9, 0, 0)));
            }
            workloadDayTemplate.Create(templateName, DateTime.SpecifyKind(createDate.Date, DateTimeKind.Utc), _workload, openHours);
            workloadDayTemplate.SetId(Guid.NewGuid());
            if (!_workload.TemplateWeekCollection.Values.Contains(workloadDayTemplate)) _workload.AddTemplate(workloadDayTemplate);

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.CreateFromTemplate(createDate, _workload, workloadDayTemplate);
            return workloadDay;
        }

        private IWorkloadDayTemplate CreateWorkloadDayTemplate(string name, DateTime createDate)
        {
            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();
            workloadDayTemplate.Create(name, createDate, _workload, _openHours);
            Guid? id = Guid.NewGuid();
            workloadDayTemplate.SetId(id);
            _workload.AddTemplate(workloadDayTemplate);
            return workloadDayTemplate;
        }

        [Test]
        public void VerifyNoneEntityClone()
        {
            _workloadDay.TaskPeriodList[0].SetId(Guid.NewGuid());
            _workloadDay.SetId(Guid.NewGuid());
            WorkloadDay clonedWorkloadDay = (WorkloadDay)_workloadDay.NoneEntityClone();
            Assert.IsNotNull(clonedWorkloadDay);
            Assert.AreNotSame(clonedWorkloadDay, _workloadDay);
            Assert.IsNull(clonedWorkloadDay.Id);
            Assert.AreEqual(_workloadDay.Annotation, clonedWorkloadDay.Annotation);
            Assert.AreEqual(_workloadDay.AverageAfterTaskTime, clonedWorkloadDay.AverageAfterTaskTime);
            Assert.AreEqual(_workloadDay.AverageTaskTime, clonedWorkloadDay.AverageTaskTime);
            Assert.AreEqual(_workloadDay.CampaignAfterTaskTime, clonedWorkloadDay.CampaignAfterTaskTime);

            Assert.AreEqual(_workloadDay.CampaignTasks, clonedWorkloadDay.CampaignTasks);
            Assert.AreEqual(_workloadDay.CampaignTaskTime, clonedWorkloadDay.CampaignTaskTime);
            Assert.AreEqual(_workloadDay.CurrentDate, clonedWorkloadDay.CurrentDate);
            Assert.AreEqual(_workloadDay.OpenForWork, clonedWorkloadDay.OpenForWork);
            Assert.AreEqual(_workloadDay.IsLocked, clonedWorkloadDay.IsLocked);
            Assert.AreEqual(_workloadDay.TaskPeriodListPeriod, clonedWorkloadDay.TaskPeriodListPeriod);
            Assert.AreEqual(_workloadDay.Tasks, clonedWorkloadDay.Tasks);
            Assert.AreEqual(_workloadDay.TemplateReference, clonedWorkloadDay.TemplateReference);
            Assert.AreEqual(_workloadDay.TotalAverageAfterTaskTime, clonedWorkloadDay.TotalAverageAfterTaskTime);
            Assert.AreEqual(_workloadDay.TotalAverageTaskTime, clonedWorkloadDay.TotalAverageTaskTime);
            Assert.AreEqual(_workloadDay.TotalStatisticAbandonedTasks, clonedWorkloadDay.TotalStatisticAbandonedTasks);
            Assert.AreEqual(_workloadDay.TotalStatisticAnsweredTasks, clonedWorkloadDay.TotalStatisticAnsweredTasks);
            Assert.AreEqual(_workloadDay.TotalStatisticAverageAfterTaskTime, clonedWorkloadDay.TotalStatisticAverageAfterTaskTime);
            Assert.AreEqual(_workloadDay.TotalStatisticAverageTaskTime, clonedWorkloadDay.TotalStatisticAverageTaskTime);
            Assert.AreEqual(_workloadDay.TotalStatisticCalculatedTasks, clonedWorkloadDay.TotalStatisticCalculatedTasks);
            Assert.AreEqual(_workloadDay.TotalTasks, clonedWorkloadDay.TotalTasks);
            Assert.AreEqual(_workloadDay.Workload, clonedWorkloadDay.Workload);
            Assert.AreNotSame(_workloadDay.TaskPeriodList, clonedWorkloadDay.TaskPeriodList);
            Assert.AreNotSame(_workloadDay.TaskPeriodList[0], clonedWorkloadDay.TaskPeriodList[0]);
            Assert.IsNull(clonedWorkloadDay.TaskPeriodList[0].Id);
        }

        [Test]
        public void VerifyEntityClone()
        {
            _workloadDay.TaskPeriodList[0].SetId(Guid.NewGuid());
            _workloadDay.SetId(Guid.NewGuid());
            WorkloadDay clonedWorkloadDay = (WorkloadDay)_workloadDay.EntityClone();
            Assert.IsNotNull(clonedWorkloadDay);
            Assert.AreNotSame(clonedWorkloadDay, _workloadDay);
            Assert.AreEqual(_workloadDay.Id, clonedWorkloadDay.Id);
            Assert.AreEqual(_workloadDay.Annotation, clonedWorkloadDay.Annotation);
            Assert.AreEqual(_workloadDay.AverageAfterTaskTime, clonedWorkloadDay.AverageAfterTaskTime);
            Assert.AreEqual(_workloadDay.AverageTaskTime, clonedWorkloadDay.AverageTaskTime);
            Assert.AreEqual(_workloadDay.CampaignAfterTaskTime, clonedWorkloadDay.CampaignAfterTaskTime);

            Assert.AreEqual(_workloadDay.CampaignTasks, clonedWorkloadDay.CampaignTasks);
            Assert.AreEqual(_workloadDay.CampaignTaskTime, clonedWorkloadDay.CampaignTaskTime);
            Assert.AreEqual(_workloadDay.CurrentDate, clonedWorkloadDay.CurrentDate);
            Assert.AreEqual(_workloadDay.OpenForWork, clonedWorkloadDay.OpenForWork);
            Assert.AreEqual(_workloadDay.IsLocked, clonedWorkloadDay.IsLocked);
            Assert.AreEqual(_workloadDay.TaskPeriodListPeriod, clonedWorkloadDay.TaskPeriodListPeriod);
            Assert.AreEqual(_workloadDay.Tasks, clonedWorkloadDay.Tasks);
            Assert.AreEqual(_workloadDay.TemplateReference, clonedWorkloadDay.TemplateReference);
            Assert.AreEqual(_workloadDay.TotalAverageAfterTaskTime, clonedWorkloadDay.TotalAverageAfterTaskTime);
            Assert.AreEqual(_workloadDay.TotalAverageTaskTime, clonedWorkloadDay.TotalAverageTaskTime);
            Assert.AreEqual(_workloadDay.TotalStatisticAbandonedTasks, clonedWorkloadDay.TotalStatisticAbandonedTasks);
            Assert.AreEqual(_workloadDay.TotalStatisticAnsweredTasks, clonedWorkloadDay.TotalStatisticAnsweredTasks);
            Assert.AreEqual(_workloadDay.TotalStatisticAverageAfterTaskTime, clonedWorkloadDay.TotalStatisticAverageAfterTaskTime);
            Assert.AreEqual(_workloadDay.TotalStatisticAverageTaskTime, clonedWorkloadDay.TotalStatisticAverageTaskTime);
            Assert.AreEqual(_workloadDay.TotalStatisticCalculatedTasks, clonedWorkloadDay.TotalStatisticCalculatedTasks);
            Assert.AreEqual(_workloadDay.TotalTasks, clonedWorkloadDay.TotalTasks);
            Assert.AreEqual(_workloadDay.Workload, clonedWorkloadDay.Workload);
            Assert.AreNotSame(_workloadDay.TaskPeriodList, clonedWorkloadDay.TaskPeriodList);
            Assert.AreNotSame(_workloadDay.TaskPeriodList[0], clonedWorkloadDay.TaskPeriodList[0]);
            Assert.AreEqual(_workloadDay.TaskPeriodList[0].Id, clonedWorkloadDay.TaskPeriodList[0].Id);
        }

        [Test]
        public void VerifyClone()
        {
            WorkloadDay clonedWorkloadDay = (WorkloadDay)_workloadDay.Clone();
            Assert.IsNotNull(clonedWorkloadDay);
            Assert.AreNotSame(clonedWorkloadDay, _workloadDay);
            Assert.AreEqual(clonedWorkloadDay.Id, _workloadDay.Id);
            Assert.AreEqual(_workloadDay.Annotation, clonedWorkloadDay.Annotation);
            Assert.AreEqual(_workloadDay.AverageAfterTaskTime, clonedWorkloadDay.AverageAfterTaskTime);
            Assert.AreEqual(_workloadDay.AverageTaskTime, clonedWorkloadDay.AverageTaskTime);
            Assert.AreEqual(_workloadDay.CampaignAfterTaskTime, clonedWorkloadDay.CampaignAfterTaskTime);

            Assert.AreEqual(_workloadDay.CampaignTasks, clonedWorkloadDay.CampaignTasks);
            Assert.AreEqual(_workloadDay.CampaignTaskTime, clonedWorkloadDay.CampaignTaskTime);
            Assert.AreEqual(_workloadDay.CurrentDate, clonedWorkloadDay.CurrentDate);
            Assert.AreEqual(_workloadDay.OpenForWork, clonedWorkloadDay.OpenForWork);
            Assert.AreEqual(_workloadDay.IsLocked, clonedWorkloadDay.IsLocked);
            Assert.AreEqual(_workloadDay.TaskPeriodListPeriod, clonedWorkloadDay.TaskPeriodListPeriod);
            Assert.AreEqual(_workloadDay.Tasks, clonedWorkloadDay.Tasks);
            Assert.AreEqual(_workloadDay.TemplateReference, clonedWorkloadDay.TemplateReference);
            Assert.AreEqual(_workloadDay.TotalAverageAfterTaskTime, clonedWorkloadDay.TotalAverageAfterTaskTime);
            Assert.AreEqual(_workloadDay.TotalAverageTaskTime, clonedWorkloadDay.TotalAverageTaskTime);
            Assert.AreEqual(_workloadDay.TotalStatisticAbandonedTasks, clonedWorkloadDay.TotalStatisticAbandonedTasks);
            Assert.AreEqual(_workloadDay.TotalStatisticAnsweredTasks, clonedWorkloadDay.TotalStatisticAnsweredTasks);
            Assert.AreEqual(_workloadDay.TotalStatisticAverageAfterTaskTime, clonedWorkloadDay.TotalStatisticAverageAfterTaskTime);
            Assert.AreEqual(_workloadDay.TotalStatisticAverageTaskTime, clonedWorkloadDay.TotalStatisticAverageTaskTime);
            Assert.AreEqual(_workloadDay.TotalStatisticCalculatedTasks, clonedWorkloadDay.TotalStatisticCalculatedTasks);
            Assert.AreEqual(_workloadDay.TotalTasks, clonedWorkloadDay.TotalTasks);
            Assert.AreEqual(_workloadDay.Workload, clonedWorkloadDay.Workload);
        }
    }
}
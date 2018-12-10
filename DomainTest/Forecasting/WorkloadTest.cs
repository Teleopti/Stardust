using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for forecast
    /// </summary>
    [TestFixture, SetUICulture("en-US")]
    public class WorkloadTest
    {
        private IWorkload _target;
        private ISkill _skill;

        /// <summary>
        /// Setup
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _skill = SkillFactory.CreateSkill("testSkill");
            _target = new Workload(_skill);
        }

        /// <summary>
        /// Constructor works.
        /// </summary>
        [Test]
        public void CanCreateForecastSourceObject()
        {
            _target = new Workload(_skill);
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyNameCannotBeTooLong()
        {
			Assert.Throws<ArgumentException>(() => _target.Name = string.Empty.PadRight(51));
        }

        /// <summary>
        /// Protected constructor works.
        /// </summary>
        [Test]
        public void ProtectedConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(), true));
        }

        [Test]
        public void VerifyDefaultQueueAdjustments()
        {
            QueueAdjustment expected = new QueueAdjustment
                                           {
                                               OfferedTasks = new Percent(1),
                                               OverflowIn = new Percent(1),
                                               OverflowOut = new Percent(-1),
                                               Abandoned=new Percent(-1),
                                               AbandonedShort = new Percent(0),
                                               AbandonedWithinServiceLevel = new Percent(1),
                                               AbandonedAfterServiceLevel = new Percent(1)
                                           };
            Assert.AreEqual(expected,_target.QueueAdjustments);
        }

        /// <summary>
        /// Verifies that properties are set correctly
        /// </summary>
        [Test]
        public void CanSetPropertiesAndAddSourceQueue()
        {
            const string name = "Workload Name";
            const string description = "Workload description";
            ISkill skill = SkillFactory.CreateSkill("Skill");
            IQueueSource qs = QueueSourceFactory.CreateQueueSource();

            _target.Name = name;
            _target.Description = description;
            _target.Skill = skill;
            _target.AddQueueSource(qs);

            Assert.AreEqual(_target.QueueSourceCollection[0], qs);
            Assert.AreEqual(_target.Name, name);
            Assert.AreEqual(_target.Description, description);
            Assert.AreEqual(_target.Skill, skill);
        }

        /// <summary>
        /// Determines whether this instance [can remove queue source].
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-11-19
        /// </remarks>
        [Test]
        public void CanRemoveQueueSource()
        {
            const string name = "Workload Name";
            const string description = "Workload description";
            ISkill skill = SkillFactory.CreateSkill("Skill");
            IQueueSource qs = QueueSourceFactory.CreateQueueSource();

            _target.Name = name;
            _target.Description = description;
            _target.Skill = skill;

            _target.AddQueueSource(qs);

            Assert.AreEqual(_target.QueueSourceCollection[0], qs);

            _target.RemoveQueueSource(qs);

            Assert.AreEqual(0, _target.QueueSourceCollection.Count);
        }

        /// <summary>
        /// Verifies that RemoveAllQueueSources works.
        /// </summary>
        [Test]
        public void CanRemoveAllQueueSources()
        {
            const string name = "Workload Name";
            const string description = "Workload description";
            ISkill skill = SkillFactory.CreateSkill("Skill");
            IQueueSource qs = QueueSourceFactory.CreateQueueSource();

            _target.Name = name;
            _target.Description = description;
            _target.Skill = skill;

            _target.AddQueueSource(qs);
            Assert.AreEqual(1, _target.QueueSourceCollection.Count);

            _target.RemoveAllQueueSources();
            Assert.AreEqual(0, _target.QueueSourceCollection.Count);
        }

        [Test]
        public void CanSetAndGetTemplateWorkloadDayTemplates()
        {
            IWorkloadDayTemplate workloadDay1 = new WorkloadDayTemplate();
            IWorkloadDayTemplate workloadDay2 = new WorkloadDayTemplate();
            workloadDay1.Create("<TEMPLATE1>", DateTime.UtcNow, _target, new List<TimePeriod>());
            workloadDay2.Create("<TEMPLATE2>", DateTime.UtcNow, _target, new List<TimePeriod>());

            _target.SetTemplateAt((int) DayOfWeek.Friday, workloadDay2);
            _target.SetTemplateAt((int) DayOfWeek.Monday, workloadDay1);

            Assert.AreEqual(workloadDay1, _target.GetTemplateAt(TemplateTarget.Workload, (int) DayOfWeek.Monday));
            Assert.AreEqual(workloadDay2, _target.GetTemplateAt(TemplateTarget.Workload, (int)DayOfWeek.Friday));
        }

        [Test]
        public void CannotSetTemplateFromOtherWorkload()
        {
            IWorkloadDayTemplate workloadDay2 = new WorkloadDayTemplate();
			Assert.Throws<ArgumentException>(() => _target.SetTemplate(DayOfWeek.Friday, workloadDay2));
        }

        [Test]
        public void CanSetAndGetTemplateWorkloadDayTemplatesWithDayOfWeek()
        {
            IWorkloadDayTemplate workloadDay1 = new WorkloadDayTemplate();
            IWorkloadDayTemplate workloadDay2 = new WorkloadDayTemplate();
            workloadDay1.Create("Template1",DateTime.UtcNow,_target, new List<TimePeriod>());
            workloadDay2.Create("Template2", DateTime.UtcNow, _target, new List<TimePeriod>());
            Assert.IsFalse(workloadDay1.DayOfWeek.HasValue);
            Assert.IsFalse(workloadDay2.DayOfWeek.HasValue);

            _target.SetTemplate(DayOfWeek.Friday, workloadDay2);
            _target.SetTemplate(DayOfWeek.Monday, workloadDay1);

            Assert.AreEqual(DayOfWeek.Monday, workloadDay1.DayOfWeek.Value);
            Assert.AreEqual(DayOfWeek.Friday, workloadDay2.DayOfWeek.Value);

            Assert.AreEqual(workloadDay1, _target.GetTemplate(TemplateTarget.Workload, DayOfWeek.Monday));
            Assert.AreEqual(workloadDay2, _target.GetTemplate(TemplateTarget.Workload, DayOfWeek.Friday));
        }

        [Test]
        public void CanAddAndGetWorkloadDayTemplate()
        {
            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            TimePeriod timePeriod = new TimePeriod("12-17");
            openHours.Add(timePeriod);
            
            workloadDayTemplate.Create("<TemplateName>", DateTime.UtcNow, _target, openHours);

            _target.SetTemplateAt((int)DayOfWeek.Monday,workloadDayTemplate);

            Assert.AreEqual(workloadDayTemplate, _target.GetTemplateAt(TemplateTarget.Workload, (int)DayOfWeek.Monday));
            //Assert.IsTrue(((IWorkloadDayTemplate) _target.GetTemplateAt(TemplateTarget.Workload, (int)DayOfWeek.Tuesday)).IsClosed);
            Assert.IsFalse(((IWorkloadDayTemplate)_target.GetTemplateAt(TemplateTarget.Workload, (int)DayOfWeek.Tuesday)).OpenForWork.IsOpen);
            Assert.IsNotNull(_target.GetTemplateAt(TemplateTarget.Workload, (int)DayOfWeek.Wednesday));
        }

        [Test]
        public void CanAddTemplate()
        {
            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            TimePeriod timePeriod = new TimePeriod("12-17");
            openHours.Add(timePeriod);

            workloadDayTemplate.Create("<JULAFTON>", DateTime.UtcNow, _target, openHours);

            int currentHeighestKey = _target.TemplateWeekCollection.Count-1;
            int currentKey = _target.AddTemplate(workloadDayTemplate);

            Assert.IsTrue(ContainsTemplate(workloadDayTemplate));
            Assert.IsFalse(ContainsTemplate(new WorkloadDayTemplate()));
            Assert.AreEqual(8,_target.TemplateWeekCollection.Count); //0-6 + 1
            Assert.AreEqual("<JULAFTON>", _target.GetTemplateAt(TemplateTarget.Workload, 7).Name);
            Assert.AreEqual(currentHeighestKey + 1, currentKey);
        }

        [Test]
        public void CanAddTemplateTwiceAndRemoveAndAddAgain()
        {
            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            TimePeriod timePeriod = new TimePeriod("12-17");
            openHours.Add(timePeriod);

            workloadDayTemplate.Create("<JULAFTON>", DateTime.UtcNow, _target, openHours);
            int currentKey = _target.AddTemplate(workloadDayTemplate);

            Assert.AreEqual(7,currentKey);

            IWorkloadDayTemplate secondTemplate = workloadDayTemplate.NoneEntityClone();
            secondTemplate.Name = "<JULDAGEN>";

            currentKey = _target.AddTemplate(secondTemplate);
            Assert.AreEqual(8,currentKey);

            _target.RemoveTemplate(TemplateTarget.Workload,"<JULAFTON>");

            currentKey = _target.AddTemplate(workloadDayTemplate);
            Assert.AreEqual(9, currentKey);
        }

        [Test]
        public void CanRemoveTemplate()
        {
            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            TimePeriod timePeriod = new TimePeriod("12-17");
            openHours.Add(timePeriod);

            workloadDayTemplate.Create("<JULAFTON>", DateTime.UtcNow, _target, openHours);
            _target.AddTemplate(workloadDayTemplate);

            Assert.AreEqual("<JULAFTON>", _target.GetTemplateAt(TemplateTarget.Workload, 7).Name);

            _target.RemoveTemplate(TemplateTarget.Workload, "<JULAFTON>");

            Assert.IsNull(_target.TryFindTemplateByName(TemplateTarget.Workload, "<JULAFTON>"));
        }

        private bool ContainsTemplate(IWorkloadDayTemplate dayTemplate)
        {
            return _target.TemplateWeekCollection.Any(tk => tk.Value == dayTemplate);
        }

        /// <summary>
        /// Verifies the empty name of the workload cannot have.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-14
        /// </remarks>
        [Test]
        public void VerifyWorkloadCannotHaveEmptyName()
        {
			Assert.Throws<ArgumentException>(() => _target.Name = string.Empty);
        }

        [Test]
        public void CanFindByName()
        {
            IWorkloadDayTemplate template = (IWorkloadDayTemplate)_target.TryFindTemplateByName(TemplateTarget.Workload, "<FRI>");
            Assert.AreEqual("<FRI>",template.Name);

            template = (IWorkloadDayTemplate)_target.TryFindTemplateByName(TemplateTarget.Workload, "NONAMEDAY");
            Assert.IsNull(template);
        }

        [Test]
        public void CanSetDefaultTemplates()
        {
            WorkloadDay workloadDay1 = new WorkloadDay();
            workloadDay1.Create(new DateOnly(2008,7,1), _target, new List<TimePeriod>());
            WorkloadDay workloadDay2 = new WorkloadDay();
            workloadDay2.Create(new DateOnly(2008, 7, 2), _target, new List<TimePeriod>());

            _target.SetDefaultTemplates(new List<WorkloadDay> {workloadDay1, workloadDay2});

            Assert.AreEqual(DayOfWeek.Tuesday, workloadDay1.TemplateReference.DayOfWeek.Value);
            Assert.AreEqual(DayOfWeek.Wednesday, workloadDay2.TemplateReference.DayOfWeek.Value);
        }

        [Test]
        public void ShouldSetLongtermTemplate()
        {
            WorkloadDay workloadDay1 = new WorkloadDay();
            workloadDay1.Create(new DateOnly(2008, 7, 1), _target, new List<TimePeriod>());
            WorkloadDay workloadDay2 = new WorkloadDay();
            workloadDay2.Create(new DateOnly(2008, 7, 2), _target, new List<TimePeriod>());

            var openHours = new TimePeriod(10, 0, 14, 0);
            _target.TemplateWeekCollection.ForEach(
                t => t.Value.ChangeOpenHours(new List<TimePeriod> { openHours }));
            _target.SetLongtermTemplate(new List<WorkloadDay> { workloadDay1, workloadDay2 });
            
            Assert.AreEqual(TemplateReference.LongtermTemplateKey, workloadDay1.TemplateReference.TemplateName);
            Assert.AreEqual(1,workloadDay1.TaskPeriodList.Count);
            Assert.AreEqual(TemplateReference.LongtermTemplateKey, workloadDay2.TemplateReference.TemplateName);
            Assert.AreEqual(1, workloadDay2.TaskPeriodList.Count);
        }

        [Test]
        public void CanGetTemplates()
        {
            var result = _target.GetTemplates(TemplateTarget.Workload);
            Assert.AreEqual(7, result.Count);
            Assert.IsInstanceOf<IWorkloadDayTemplate>(result[0]);
        }

        [Test]
        public void CanClone()
        {
            _target.SetId(Guid.NewGuid());
            IWorkload workloadClone = (IWorkload)_target.Clone();
            Assert.IsFalse(workloadClone.Id.HasValue);
            Assert.AreEqual(_target.TemplateWeekCollection.Count,workloadClone.TemplateWeekCollection.Count);
            Assert.AreSame(_target, _target.TemplateWeekCollection[0].Parent);
            Assert.AreSame(workloadClone, workloadClone.TemplateWeekCollection[0].Parent);
            Assert.AreEqual(_target.Description,workloadClone.Description);
            Assert.AreEqual(_target.BusinessUnit,workloadClone.BusinessUnit);
            Assert.AreEqual(_target.Name, workloadClone.Name);
            Assert.AreEqual(_target.QueueSourceCollection.Count, workloadClone.QueueSourceCollection.Count);
            Assert.AreEqual(_target.Skill, workloadClone.Skill);

            workloadClone = _target.NoneEntityClone();
            Assert.IsFalse(workloadClone.Id.HasValue);
            Assert.AreEqual(_target.TemplateWeekCollection.Count, workloadClone.TemplateWeekCollection.Count);
            Assert.AreSame(_target, _target.TemplateWeekCollection[0].Parent);
            Assert.AreSame(workloadClone, workloadClone.TemplateWeekCollection[0].Parent);
            Assert.AreEqual(_target.Description, workloadClone.Description);
            Assert.AreEqual(_target.BusinessUnit, workloadClone.BusinessUnit);
            Assert.AreEqual(_target.Name, workloadClone.Name);
            Assert.AreEqual(_target.QueueSourceCollection.Count, workloadClone.QueueSourceCollection.Count);
            Assert.AreEqual(_target.Skill, workloadClone.Skill);

            workloadClone = _target.EntityClone();
            Assert.AreEqual(_target.Id.GetValueOrDefault(), workloadClone.Id.GetValueOrDefault());
            Assert.AreEqual(_target.TemplateWeekCollection.Count, workloadClone.TemplateWeekCollection.Count);
            Assert.AreSame(_target, _target.TemplateWeekCollection[0].Parent);
            Assert.AreSame(workloadClone, workloadClone.TemplateWeekCollection[0].Parent); 
            Assert.AreEqual(_target.Description, workloadClone.Description);
            Assert.AreEqual(_target.BusinessUnit, workloadClone.BusinessUnit);
            Assert.AreEqual(_target.Name, workloadClone.Name);
            Assert.AreEqual(_target.QueueSourceCollection.Count, workloadClone.QueueSourceCollection.Count);
            Assert.AreEqual(_target.Skill, workloadClone.Skill);
        }

        [Test]
        public void CanApplyTemplateByName()
        {
            IWorkloadDay workloadDay1 = new WorkloadDay();
            workloadDay1.Create(new DateOnly(2008, 7, 1), _target, new List<TimePeriod>());
            IWorkloadDay workloadDay2 = new WorkloadDay();
            workloadDay2.Create(new DateOnly(2008, 7, 2), _target, new List<TimePeriod>());

            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();
            const string templateName = "<Extra>";
            workloadDayTemplate.Create(templateName, DateTime.UtcNow, _target, new List<TimePeriod>());
            workloadDayTemplate.SetId(Guid.NewGuid());
            _target.SetTemplateAt(7, workloadDayTemplate);
            _target.SetTemplatesByName(TemplateTarget.Workload, templateName, new List<ITemplateDay> { workloadDay1, workloadDay2 });

            Assert.AreEqual(templateName, workloadDay1.TemplateReference.TemplateName);
        }

        [Test]
        public void VerifyCanRefreshTemplatesFromNewlyLoadedInstance()
        {
            _target.SetId(Guid.NewGuid());

            IWorkloadDayTemplate workloadDayTemplate1 = new WorkloadDayTemplate();
            workloadDayTemplate1.Create("To be removed", DateTime.UtcNow, _target, new List<TimePeriod>());
            _target.AddTemplate(workloadDayTemplate1);

            IWorkload clonedWorkload = _target.EntityClone();
            IWorkloadDayTemplate workloadDayTemplate2 = new WorkloadDayTemplate();
            workloadDayTemplate2.Create("To be added", DateTime.UtcNow, _target, new List<TimePeriod>());
            clonedWorkload.RemoveTemplate(TemplateTarget.Workload, "To be removed");
            clonedWorkload.AddTemplate(workloadDayTemplate2);

            Assert.AreEqual(8, _target.TemplateWeekCollection.Count);
            Assert.IsTrue(_target.TemplateWeekCollection.Values.Contains(workloadDayTemplate1));
            Assert.IsFalse(_target.TemplateWeekCollection.Values.Contains(workloadDayTemplate2));
            _target.RefreshTemplates(clonedWorkload);
            Assert.AreEqual(8, _target.TemplateWeekCollection.Count);
            Assert.IsFalse(_target.TemplateWeekCollection.Values.Contains(workloadDayTemplate1));
            Assert.IsTrue(_target.TemplateWeekCollection.Values.Contains(workloadDayTemplate2));
        }

        [Test]
        public void VerifyCanRefreshTemplatesFromNewlyLoadedInstanceRequiresSkill()
        {
            _target.SetId(Guid.NewGuid());
			Assert.Throws<ArgumentException>(() => _target.RefreshTemplates(_skill));
        }

        [Test]
        public void VerifyCanRefreshTemplatesFromNewlyLoadedInstanceRequiresSameSkill()
        {
            _target.SetId(Guid.NewGuid());

            IWorkload clonedWorkload = _target.EntityClone();
            clonedWorkload.SetId(Guid.NewGuid());

			Assert.Throws<ArgumentException>(() => _target.RefreshTemplates(clonedWorkload));
        }

        //[Test]
        //public void VerifyThatStatisticsIsLoaded()
        //{
        //    DateTimePeriod datePeriod = new DateTimePeriod(new DateTime(2008, 1, 28), new DateTime(2008, 2, 24));
        //    IList<Task> tasks = new List<Task>();
        //    TimePeriod period1 = new TimePeriod("7-7:15");
        //    TimePeriod period2 = new TimePeriod("7:15-7:30");
        //    int addTasks = 0;

        //    foreach (DateTime date in datePeriod.DateCollection())
        //    {
        //        if (date.DayOfWeek == DayOfWeek.Monday)
        //        {
        //            tasks.Add(new Task(5 + addTasks, new TimeSpan(0, 0, 45), new TimeSpan(0, 0, 60)));
        //            addTasks = addTasks + 5;
        //        }
        //    }

        //    Task task1 = new Task(5, new TimeSpan(0, 0, 45), new TimeSpan(0, 0, 60));
        //    Task task2 = new Task(20, new TimeSpan(0, 0, 30), new TimeSpan(0, 0, 30));
        //    TemplateTaskPeriod taskPeriod1 = new TemplateTaskPeriod(task1, period1);
        //    TemplateTaskPeriod taskPeriod2 = new TemplateTaskPeriod(task2, period2);
        //    _target.LoadStatistics(datePeriod);
        //    Assert.AreEqual(task1.Tasks, _target.GetTemplateAt(DayOfWeek.Monday).SortedTaskPeriodList[0].Task.Tasks)
        //}
    }
}

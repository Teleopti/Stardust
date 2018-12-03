using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests for WorkloadRepository
    /// </summary>
    [TestFixture]
    [Category("BucketB")]
    public class WorkloadRepositoryTest : RepositoryTest<IWorkload>
    {
        private ISkillType _skillType;
        private IActivity _activity;
        private ISkill _skill;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            _skillType = SkillTypeFactory.CreateSkillType();
						_activity = new Activity("The test") { DisplayColor = Color.Honeydew };
            _skill = SkillFactory.CreateSkill("Skill - Name", _skillType, 15);
            _skill.MidnightBreakOffset = TimeSpan.FromHours(2);

            PersistAndRemoveFromUnitOfWork(_skillType);
            PersistAndRemoveFromUnitOfWork(_activity);
            _skill.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(_skill);
        }

        /// <summary>
        /// Creates an aggreagte using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IWorkload CreateAggregateWithCorrectBusinessUnit()
        {
            IWorkload wl = WorkloadFactory.CreateWorkload(_skill);

            TimeSpan tsStart1 = new TimeSpan(15, 30, 0);
            TimeSpan tsEnd1 = new TimeSpan(1, 2, 0, 0);

            WorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();
            DateTimePeriod tp1 = new DateTimePeriod(DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(tsStart1), DateTimeKind.Utc), DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(tsEnd1), DateTimeKind.Utc));
            ITemplateTaskPeriod templateTaskPeriod = new TemplateTaskPeriod(new Task(), tp1);
            IList<ITemplateTaskPeriod> timeTaskPeriods = new List<ITemplateTaskPeriod>();
            timeTaskPeriods.Add(templateTaskPeriod);

            IList<TimePeriod>  openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(tsStart1,tsEnd1));

            workloadDayTemplate.Create("<MONDAY>",DateTime.UtcNow, wl, openHours);

            wl.SetTemplateAt((int)DayOfWeek.Monday,workloadDayTemplate);

            wl.QueueAdjustments = new QueueAdjustment{OfferedTasks = new Percent(0.9)};

            return wl;
        }


        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IWorkload loadedAggregateFromDatabase)
        {
            IWorkload workload = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(workload.Name, loadedAggregateFromDatabase.Name);
            Assert.AreEqual(workload.Description, loadedAggregateFromDatabase.Description);
            Assert.AreEqual(workload.Skill.Name, loadedAggregateFromDatabase.Skill.Name);
            Assert.AreEqual(workload.TemplateWeekCollection.Count,loadedAggregateFromDatabase.TemplateWeekCollection.Count);
            Assert.AreEqual(workload.QueueAdjustments.OfferedTasks,loadedAggregateFromDatabase.QueueAdjustments.OfferedTasks);
        }

        /// <summary>
        /// Determines whether this instance can be created.
        /// </summary>
        [Test]
        public void CanCreate()
        {
            Assert.IsNotNull(new WorkloadRepository(UnitOfWork));
        }

        [Test]
        public void CanDeleteWorkloadDay()
        {

            IWorkload firstWorkload = WorkloadFactory.CreateWorkload("first", _skill);
            IWorkload secondWorkload = WorkloadFactory.CreateWorkload("second",_skill);
            DateOnly startDate = new DateOnly(2008, 2, 8);
            DateOnlyPeriod period = new DateOnlyPeriod(startDate, startDate);
            ISkillDay skillDay = SkillDayFactory.CreateSkillDay(_skill, DateTime.SpecifyKind(startDate.Date,DateTimeKind.Utc), firstWorkload, secondWorkload);
            SkillDayCalculator skillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay>{skillDay}, period);

            Assert.IsNotNull(skillDay.SkillDayCalculator);
            Assert.AreSame(skillDayCalculator, skillDay.SkillDayCalculator);
            PersistAndRemoveFromUnitOfWork(skillDay.Scenario);
            PersistAndRemoveFromUnitOfWork(firstWorkload);
            PersistAndRemoveFromUnitOfWork(secondWorkload);
            PersistAndRemoveFromUnitOfWork(skillDay);
            Assert.IsTrue(skillDay.WorkloadDayCollection.Count > 0);

            Assert.IsNotNull(firstWorkload.Id);            
            Assert.IsNotNull(secondWorkload.Id);

            double tasks = GetWorkloadTasks(skillDay);
            Assert.AreEqual(tasks, skillDay.TotalTasks);

            WorkloadRepository workloadRepository = new WorkloadRepository(UnitOfWork);

            workloadRepository.Remove(firstWorkload);
            PersistAndRemoveFromUnitOfWork(firstWorkload);

            SkillDayRepository skillDayRepository = new SkillDayRepository(UnitOfWork);
            IList<ISkillDay> skillDays = skillDayRepository.FindRange(period, _skill, skillDay.Scenario).ToList();
            
            Assert.AreEqual(1, skillDays.Count);
            ISkillDay loadedSkillDay = skillDays[0];
            Assert.AreNotEqual(loadedSkillDay.TotalTasks, skillDay.TotalTasks);
        }

        /// <summary>
        /// Gets the workload tasks.
        /// </summary>
        /// <param name="skillDay">The skill day.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2009-11-02
        /// </remarks>
        private static double GetWorkloadTasks(ISkillDay skillDay)
        {
            double tasks = 0;
            foreach (IWorkloadDay workloadDay in skillDay.WorkloadDayCollection)
            {
                tasks += workloadDay.TotalTasks;
            }
            return tasks;
        }

        [Test]
        public void VerifyCanDeleteTemplate()
        {
            IWorkload workload = CreateAggregateWithCorrectBusinessUnit();
            
            WorkloadDayTemplate template = new WorkloadDayTemplate();
            template.Create("<TEMPLATE>", DateTime.UtcNow, workload, new List<TimePeriod>());
            workload.AddTemplate(template);

            PersistAndRemoveFromUnitOfWork(workload);

            workload.RemoveTemplate(TemplateTarget.Workload,"<TEMPLATE>");

            PersistAndRemoveFromUnitOfWork(workload);

            Assert.AreEqual(7,workload.TemplateWeekCollection.Count);
        }

        [Test]
        public void VerifyCanCloneAndAddTemplate()
        {
            IWorkload originalWorkload = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(originalWorkload);

            IWorkload workload = originalWorkload.EntityClone();

            WorkloadDayTemplate template = new WorkloadDayTemplate();
            template.Create("<TEMPLATE>", DateTime.UtcNow, workload, new List<TimePeriod>());
            workload.AddTemplate(template);

            UnitOfWork.Reassociate(originalWorkload);
            Assert.IsFalse(UnitOfWork.IsDirty());
            originalWorkload = UnitOfWork.Merge(workload);

            Assert.IsTrue(UnitOfWork.IsDirty());
            Assert.AreEqual(8,originalWorkload.TemplateWeekCollection.Count);
        }

        [Test]
        public void VerifyCanChangeOpenHoursForTemplate()
        {
            IWorkload originalWorkload = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(originalWorkload);

            IWorkloadDayTemplate template = originalWorkload.TemplateWeekCollection[0];
            template.ChangeOpenHours(new List<TimePeriod>{new TimePeriod(8,0,17,0)});

            PersistAndRemoveFromUnitOfWork(originalWorkload);

            template = originalWorkload.TemplateWeekCollection[0];
            template.ChangeOpenHours(new List<TimePeriod> { new TimePeriod(8, 0, 17, 15) });

            PersistAndRemoveFromUnitOfWork(originalWorkload);
        }

        [Test]
        public void VerifyCanMergeTemplateAndSave()
        {
            IWorkload originalWorkload = CreateAggregateWithCorrectBusinessUnit();
            IWorkloadDayTemplate template = originalWorkload.TemplateWeekCollection[0];
            template.MakeOpen24Hours();
            PersistAndRemoveFromUnitOfWork(originalWorkload);

            template = originalWorkload.TemplateWeekCollection[0];
            template.SplitTemplateTaskPeriods(new List<ITemplateTaskPeriod>(template.TaskPeriodList));

            PersistAndRemoveFromUnitOfWork(originalWorkload);

            template = originalWorkload.TemplateWeekCollection[0];
            template.MergeTemplateTaskPeriods(new List<ITemplateTaskPeriod>(template.TaskPeriodList));

            PersistAndRemoveFromUnitOfWork(originalWorkload);

            Assert.AreEqual(1,template.TaskPeriodList.Count);
        }

        protected override Repository<IWorkload> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new WorkloadRepository(currentUnitOfWork);
        }
    }
}

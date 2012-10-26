using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests for PersonAssignmentRepository
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class SkillRepositoryTest : RepositoryTest<ISkill>
    {
        private ISkillType _skillType;
        private IActivity _activity;
        private StaffingThresholds _staffingThresholds;
        private TimeSpan _midnightBreakOffset;
        private IGroupingActivity _groupingActivity;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            _skillType = SkillTypeFactory.CreateSkillType();
            PersistAndRemoveFromUnitOfWork(_skillType);
            _activity = ActivityFactory.CreateActivity("The test", Color.Honeydew);
            _groupingActivity = GroupingActivityFactory.CreateSimpleGroupingActivity("the group");
            PersistAndRemoveFromUnitOfWork(_groupingActivity);
            _activity.GroupingActivity = _groupingActivity;
            PersistAndRemoveFromUnitOfWork(_activity);
            _staffingThresholds = new StaffingThresholds(new Percent(0.1), new Percent(0.2), new Percent(0.3));
            _midnightBreakOffset = new TimeSpan(3, 0, 0);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-10-18
        /// </remarks>
        protected override ISkill CreateAggregateWithCorrectBusinessUnit()
        {
            ISkill skill = SkillFactory.CreateSkill("Skill - Name", _skillType, 15);
            
            skill.Activity = _activity;
            skill.StaffingThresholds = _staffingThresholds;
            skill.MidnightBreakOffset = _midnightBreakOffset;

            return skill;
        }


        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(ISkill loadedAggregateFromDatabase)
        {
            ISkill skill = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(skill.Name, loadedAggregateFromDatabase.Name);
            Assert.AreEqual(skill.Description, loadedAggregateFromDatabase.Description);
            Assert.AreEqual(skill.DisplayColor, loadedAggregateFromDatabase.DisplayColor);
            Assert.AreEqual(skill.SkillType, loadedAggregateFromDatabase.SkillType);
            Assert.AreEqual(skill.Activity, loadedAggregateFromDatabase.Activity);
            Assert.AreEqual(skill.StaffingThresholds,loadedAggregateFromDatabase.StaffingThresholds);
            Assert.AreEqual(skill.MidnightBreakOffset, loadedAggregateFromDatabase.MidnightBreakOffset);
        }

        [Test]
        public void CanLoadSkillsWithSkillDaysOnly()
        {
            ISkill skill1 = SkillFactory.CreateSkill("sdfsdf", _skillType, 15);
            skill1.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(skill1);

            IMultisiteSkill multisiteSkill = SkillFactory.CreateMultisiteSkill("test", _skillType, 15);
            multisiteSkill.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(multisiteSkill);

            QueueSource q = new QueueSource("v�gar inte skriva s�nt", "peter", 1);
            QueueSource q2 = new QueueSource("ft", "peter", 1);
            PersistAndRemoveFromUnitOfWork(q);
            PersistAndRemoveFromUnitOfWork(q2);

            IWorkload wl = WorkloadFactory.CreateWorkload(skill1);
            wl.AddQueueSource(q);
            PersistAndRemoveFromUnitOfWork(wl);

            IWorkload wl2 = WorkloadFactory.CreateWorkload(skill1);
            wl2.AddQueueSource(q2);
            PersistAndRemoveFromUnitOfWork(wl2);

            DateTime date = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            ISkillDay skillDay = SkillDayFactory.CreateSkillDay(skill1,date,wl,wl2);


            PersistAndRemoveFromUnitOfWork(skillDay.Scenario);
            PersistAndRemoveFromUnitOfWork(skillDay);
            
            ISkill skill2 = SkillFactory.CreateSkill("tv�an", _skillType, 15);
            skill2.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(skill2);

            var period = new DateOnlyPeriod(new DateOnly(date.AddDays(-10)), new DateOnly(date.AddDays(10)));
            SkillRepository rep = new SkillRepository(UnitOfWork);
            IList<ISkill> skills = new List<ISkill>(rep.FindAllWithSkillDays(period));
            Assert.AreEqual(1, skills.Count);
            Assert.IsFalse(skills[0].GetType().FullName.Contains("SkillProxy"));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skills[0].WorkloadCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skills[0].WorkloadCollection.First()));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skills[0].WorkloadCollection.First().QueueSourceCollection));
        }

        [Test]
        public void CanLoadMultisiteSkillsWithSkillDaysOnly()
        {
            IMultisiteSkill multisiteSkill = SkillFactory.CreateMultisiteSkill("test", _skillType, 15);
            multisiteSkill.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(multisiteSkill);

            ISkill skill1 = SkillFactory.CreateChildSkill("sdfsdf", multisiteSkill);
            skill1.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(skill1);

            
            QueueSource q = new QueueSource("v�gar inte skriva s�nt", "peter", 1);
            QueueSource q2 = new QueueSource("ft", "peter", 1);
            PersistAndRemoveFromUnitOfWork(q);
            PersistAndRemoveFromUnitOfWork(q2);

            IWorkload wl = WorkloadFactory.CreateWorkload(multisiteSkill);
            wl.AddQueueSource(q);
            PersistAndRemoveFromUnitOfWork(wl);

            IWorkload wl2 = WorkloadFactory.CreateWorkload(multisiteSkill);
            wl2.AddQueueSource(q2);
            PersistAndRemoveFromUnitOfWork(wl2);

            DateTime date = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            ISkillDay skillDay = SkillDayFactory.CreateSkillDay(multisiteSkill, date, wl, wl2);
            
            PersistAndRemoveFromUnitOfWork(skillDay.Scenario);
            PersistAndRemoveFromUnitOfWork(skillDay);

            ISkill skill2 = SkillFactory.CreateSkill("tv�an", _skillType, 15);
            skill2.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(skill2);

            var period = new DateOnlyPeriod(new DateOnly(date.AddDays(-10)), new DateOnly(date.AddDays(10)));
            SkillRepository rep = new SkillRepository(UnitOfWork);
            IList<ISkill> skills = new List<ISkill>(rep.FindAllWithSkillDays(period));
            Assert.AreEqual(2, skills.Count);
            Assert.IsFalse(skills[0].GetType().FullName.Contains("SkillProxy"));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(((IMultisiteSkill)skills[0]).ChildSkills));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(((IMultisiteSkill)skills[0]).ChildSkills[0].WorkloadCollection));
        }

        [Test]
        public void CanFindAllIncludingWorkloadAndQueueSources()
        {

            ISkill skill1 = SkillFactory.CreateSkill("sdfsdf", _skillType, 15);
            skill1.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(skill1);

            QueueSource q = new QueueSource("v�gar inte skriva s�nt", "peter", 1);
            PersistAndRemoveFromUnitOfWork(q); 
            
            Workload wl = new Workload(skill1);
            wl.AddQueueSource(q);
            PersistAndRemoveFromUnitOfWork(wl);
            
            Workload wl2 = new Workload(skill1);
            
            PersistAndRemoveFromUnitOfWork(wl2);

            Workload wl3 = new Workload(skill1);
            Session.Save(wl3);
            new Repository(UnitOfWork).Remove(wl3);
            Session.Flush();

            SkillRepository rep = new SkillRepository(UnitOfWork);
            IList<ISkill> skills = new List<ISkill>(rep.FindAllWithWorkloadAndQueues());

            ISkill skillToTest = skills[0];
            Assert.AreEqual(1, skills.Count);
            Assert.AreEqual(skillToTest, skill1);
            Assert.AreEqual(2,skillToTest.WorkloadCollection.Count());

            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection));
            IWorkload wlLoaded = skillToTest.WorkloadCollection.First();
            if(wlLoaded.Id==wl.Id)
                Assert.IsTrue(LazyLoadingManager.IsInitialized(wlLoaded.QueueSourceCollection));
            else
                Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection.ElementAt(1).QueueSourceCollection));                
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.Activity));
        }

        /// <summary>
        /// Determines whether this instance [can find all using multisite skill].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        [Test]
        public void CanFindAllUsingMultisiteSkill()
        {
            ISkill skill1 = new MultisiteSkill("test","test",Color.Blue,15, _skillType);
            skill1.TimeZone = (TimeZoneInfo.Utc);
            skill1.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(skill1);

            IQueueSource q = new QueueSource("v�gar inte skriva s�nt", "peter", 1);
            PersistAndRemoveFromUnitOfWork(q);

            Workload wl = new Workload(skill1);
            wl.AddQueueSource(q);
            PersistAndRemoveFromUnitOfWork(wl);

            SkillRepository rep = new SkillRepository(UnitOfWork);
            IList<ISkill> skills = new List<ISkill>(rep.FindAllWithWorkloadAndQueues());

            ISkill skillToTest = skills[0];
            Assert.AreEqual(1, skills.Count);
            Assert.AreEqual(skillToTest, skill1);
            Assert.AreEqual(1, skillToTest.WorkloadCollection.Count());

            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection.First()));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection.First().QueueSourceCollection[0]));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.Activity));
        }

        /// <summary>
        /// Determines whether this instance [can find all using multisite skill].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        [Test]
        public void CanFindAllUsingMultisiteSkillWithChildSkills()
        {
            IMultisiteSkill skill1 = SkillFactory.CreateMultisiteSkill("testp1",_skillType,15);
            skill1.TimeZone = (TimeZoneInfo.Utc);
            skill1.Activity = _activity;

            PersistAndRemoveFromUnitOfWork(skill1);

            IChildSkill child1 = SkillFactory.CreateChildSkill("testc1", skill1);
            
            IList<ISkill> children = new List<ISkill> { child1 };

            skill1.SetChildSkills(children.OfType<IChildSkill>().ToList());
            PersistAndRemoveFromUnitOfWork(children);
            
            IQueueSource q = new QueueSource("v�gar inte skriva s�nt", "peter", 1);
            PersistAndRemoveFromUnitOfWork(q);

            IWorkload wl = new Workload(skill1);
            wl.AddQueueSource(q);
            PersistAndRemoveFromUnitOfWork(wl);

            SkillRepository rep = new SkillRepository(UnitOfWork);
            IList<ISkill> skills = new List<ISkill>(rep.FindAllWithWorkloadAndQueues());

            ISkill skillToTest = skills.OfType<IMultisiteSkill>().First();
            Assert.AreEqual(2, skills.Count);
            Assert.AreEqual(skillToTest, skill1);
            Assert.AreEqual(1, skillToTest.WorkloadCollection.Count());
            Assert.IsInstanceOf<IMultisiteSkill>(skillToTest);
            Assert.AreEqual(1, ((IMultisiteSkill)skillToTest).ChildSkills.Count);

            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection.First()));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection.First().QueueSourceCollection[0]));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.Activity));
        }

        /// <summary>
        /// Determines whether this instance [can find all without multisite skill using multisite skill with child skills].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-07
        /// </remarks>
        [Test]
        public void CanFindAllWithoutMultisiteSkillUsingMultisiteSkillWithChildSkills()
        {
            IMultisiteSkill skill1 = SkillFactory.CreateMultisiteSkill("testp1", _skillType, 15);
            skill1.TimeZone = (TimeZoneInfo.Utc);
            skill1.Activity = _activity;

            PersistAndRemoveFromUnitOfWork(skill1);

            ISkill skill2 = SkillFactory.CreateSkill("testp2", _skillType, 15);
            skill2.TimeZone = (TimeZoneInfo.Utc);
            skill2.Activity = _activity;

            PersistAndRemoveFromUnitOfWork(skill2);

            IChildSkill child1 = SkillFactory.CreateChildSkill("testc1", skill1);
            IChildSkill child2 = SkillFactory.CreateChildSkill("testc2", skill1);

            IList<ISkill> children = new List<ISkill> { child1, child2 };

            skill1.SetChildSkills(children.OfType<IChildSkill>().ToList());
            PersistAndRemoveFromUnitOfWork(children);

            SkillRepository rep = new SkillRepository(UnitOfWork);
            IList<ISkill> skills = new List<ISkill>(rep.FindAllWithoutMultisiteSkills());

            Assert.AreEqual(3, skills.Count);
            Assert.IsTrue(skills.Contains(child1));
            Assert.IsTrue(skills.Contains(child2));
            Assert.IsTrue(skills.Contains(skill2));
            Assert.IsFalse(skills.Contains(skill1));
        }

        /// <summary>
        /// Determines whether this instance [can delete skill with workload and queue sources].
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-01-24
        /// </remarks>
        [Test]
        public void CanDeleteSkillWithWorkloadAndQueueSources()
        {

            ISkill skill1 = SkillFactory.CreateSkill("sdfsdf", _skillType, 15);
            skill1.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(skill1);

            IQueueSource q = new QueueSource("Q1", "Queue1", 1);
            PersistAndRemoveFromUnitOfWork(q);

            IWorkload wl = new Workload(skill1);
            wl.AddQueueSource(q);
            PersistAndRemoveFromUnitOfWork(wl);

            IWorkload wl2 = new Workload(skill1);

            PersistAndRemoveFromUnitOfWork(wl2);

            SkillRepository rep = new SkillRepository(UnitOfWork);
            IList<ISkill> skills = new List<ISkill>(rep.FindAllWithWorkloadAndQueues());

            ISkill skillToTest = skills[0];
            Assert.AreEqual(1, skills.Count);
            Assert.AreEqual(skillToTest, skill1);
            Assert.AreEqual(2, skillToTest.WorkloadCollection.Count());
            
            rep.Remove(skillToTest);
            Session.Flush();
            
            Assert.IsTrue(((IDeleteTag)skills[0]).IsDeleted);

            skills = new List<ISkill>(rep.FindAllWithWorkloadAndQueues());
            Assert.AreEqual(0, skills.Count);
        }

        [Test]
        public void VerifyCanDeleteTemplate()
        {
            ISkill skill = CreateAggregateWithCorrectBusinessUnit();

            ISkillDayTemplate template = new SkillDayTemplate("<TEMPLATE>", new List<ITemplateSkillDataPeriod>());
            skill.AddTemplate(template);

            PersistAndRemoveFromUnitOfWork(skill);

            skill.RemoveTemplate(TemplateTarget.Skill, "<TEMPLATE>");

            PersistAndRemoveFromUnitOfWork(skill);

            Assert.AreEqual(7, skill.TemplateWeekCollection.Count);
        }

        [Test]
        public void VerifyIntervalsAreRemovedWhenSplittingAndMerging()
        {
            ISkill skill = CreateAggregateWithCorrectBusinessUnit();
            ISkillDayTemplate skillDay = skill.TemplateWeekCollection[0];
            skillDay.SetSkillDataPeriodCollection(new List<ITemplateSkillDataPeriod> { new TemplateSkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(), DateTimeFactory.CreateDateTimePeriod(DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc), 0)) });
            
            Assert.AreEqual(1, skillDay.TemplateSkillDataPeriodCollection.Count);

            skillDay.SplitTemplateSkillDataPeriods(new List<ITemplateSkillDataPeriod>(skillDay.TemplateSkillDataPeriodCollection));
            skillDay.MergeTemplateSkillDataPeriods(new List<ITemplateSkillDataPeriod>(skillDay.TemplateSkillDataPeriodCollection));
            skillDay.SplitTemplateSkillDataPeriods(new List<ITemplateSkillDataPeriod>(skillDay.TemplateSkillDataPeriodCollection));

            Assert.AreEqual(96, skillDay.TemplateSkillDataPeriodCollection.Count);

            PersistAndRemoveFromUnitOfWork(skill);

            IRepository<ISkill> skillRepository = TestRepository(UnitOfWork);
            skill = skillRepository.Get(skill.Id.Value);
            skillDay = skill.TemplateWeekCollection[0];

            Assert.AreEqual(96, skillDay.TemplateSkillDataPeriodCollection.Count);

            skillDay.MergeTemplateSkillDataPeriods(new List<ITemplateSkillDataPeriod>(skillDay.TemplateSkillDataPeriodCollection));
            PersistAndRemoveFromUnitOfWork(skill);

            skillRepository = TestRepository(UnitOfWork);
            skill = skillRepository.Get(skill.Id.Value);
            skillDay = skill.TemplateWeekCollection[0];

            Assert.AreEqual(1, skillDay.TemplateSkillDataPeriodCollection.Count);
        }


        [Test]
        public void VerifyGettingSkillsThatHaveCertainActivities()
        {
            IActivity blueActivity = ActivityFactory.CreateActivity("Blue Activity", Color.Blue);
            IActivity redActivity = ActivityFactory.CreateActivity("Red Activity", Color.Red);
            IActivity greenActivity = ActivityFactory.CreateActivity("Green Activity", Color.Green);

            blueActivity.GroupingActivity = _groupingActivity;
            redActivity.GroupingActivity = _groupingActivity;
            greenActivity.GroupingActivity = _groupingActivity;

            PersistAndRemoveFromUnitOfWork(blueActivity);
            PersistAndRemoveFromUnitOfWork(redActivity);
            PersistAndRemoveFromUnitOfWork(greenActivity);

            IList<IActivity> activities = new List<IActivity> { blueActivity, redActivity, greenActivity };

            CreateAndPersistSkillWithCertainActivity("Skill Blue 1", blueActivity);
            CreateAndPersistSkillWithCertainActivity("Skill Blue 2", blueActivity);
            CreateAndPersistSkillWithCertainActivity("Skill Red 1", redActivity);
            CreateAndPersistSkillWithCertainActivity("Skill Red 2", redActivity);
            CreateAndPersistSkillWithCertainActivity("Skill Red 3", redActivity);
            CreateAndPersistSkillWithCertainActivity("Skill Green 1", greenActivity);

            SkillRepository rep = new SkillRepository(UnitOfWork);
            IList<ISkill> skills = new List<ISkill>(rep.FindAllWithActivities(activities));
            Assert.AreEqual(6, skills.Count);

            activities = new List<IActivity> { redActivity, greenActivity };
            skills = new List<ISkill>(rep.FindAllWithActivities(activities));
            Assert.AreEqual(4, skills.Count);

            activities = new List<IActivity> { blueActivity };
            skills = new List<ISkill>(rep.FindAllWithActivities(activities));
            Assert.AreEqual(2, skills.Count);

        }

        private void CreateAndPersistSkillWithCertainActivity(string name, IActivity activity)
        {
            ISkill skill = SkillFactory.CreateSkill(name, _skillType, 15);
            skill.Activity = activity;
            skill.StaffingThresholds = _staffingThresholds;
            skill.MidnightBreakOffset = _midnightBreakOffset;
            PersistAndRemoveFromUnitOfWork(skill);
            return;
        }

        [Test]
        public void ShouldCreateRepositoryWithUnitOfWorkFactory()
        {
            ISkillRepository skillRepository = new SkillRepository(UnitOfWorkFactory.Current);
            Assert.IsNotNull(skillRepository);
        }

        [Test]
        public void ShouldLoadSkill()
        {
            var skill = SkillFactory.CreateSkill("TestSkill", _skillType, 15);
            skill.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(skill);

            var q = new QueueSource("Queue 1", "Phone queue 1", 1);
            PersistAndRemoveFromUnitOfWork(q);

            var wl = new Workload(skill);
            wl.AddQueueSource(q);
            PersistAndRemoveFromUnitOfWork(wl);

            var wl2 = new Workload(skill);

            PersistAndRemoveFromUnitOfWork(wl2);

            var rep = new SkillRepository(UnitOfWork);
            var skillToTest = rep.LoadSkill(skill);

            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.SkillType)); 
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection.First().Skill.SkillType));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection.First().TemplateWeekCollection.First().Value.OpenHourList));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection.First().TemplateWeekCollection.First().Value.TaskPeriodList));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection.First().QueueSourceCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.TemplateWeekCollection.First().Value.TemplateSkillDataPeriodCollection));
        }

        [Test]
        public void ShouldLoadMultisiteSkill()
        {
            var skill1 = SkillFactory.CreateMultisiteSkill("multisite skill", _skillType, 15);
            skill1.TimeZone = (TimeZoneInfo.Utc);
            skill1.Activity = _activity;

            PersistAndRemoveFromUnitOfWork(skill1);

            var child1 = SkillFactory.CreateChildSkill("child skill 1", skill1);

            IList<ISkill> children = new List<ISkill> { child1 };

            skill1.SetChildSkills(children.OfType<IChildSkill>().ToList());
            PersistAndRemoveFromUnitOfWork(children);

            IQueueSource q = new QueueSource("Queue 1", "Phone Queue 1", 1);
            PersistAndRemoveFromUnitOfWork(q);

            IWorkload wl = new Workload(skill1);
            wl.AddQueueSource(q);
            PersistAndRemoveFromUnitOfWork(wl);

            var rep = new SkillRepository(UnitOfWork);
            var skillToTest = rep.LoadMultisiteSkill(skill1);

            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.SkillType)); 
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection.First().Skill.SkillType));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection.First().TemplateWeekCollection.First().Value.OpenHourList));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection.First().TemplateWeekCollection.First().Value.TaskPeriodList));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.WorkloadCollection.First().QueueSourceCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.TemplateMultisiteWeekCollection.First().Value.TemplateMultisitePeriodCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.TemplateWeekCollection.First().Value.TemplateSkillDataPeriodCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.ChildSkills));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(skillToTest.ChildSkills.First().TemplateWeekCollection.First().Value.TemplateSkillDataPeriodCollection));
        }


        [Test]
        public void ShouldLoadInboundTelephonySkills()
        {
            Description desc = new Description("My Phone skill type");
            ISkillTypePhone skillTypePhone = new SkillTypePhone(desc, ForecastSource.InboundTelephony);
            ISkillTypeEmail skillTypeEmail = new SkillTypeEmail(desc, ForecastSource.Email);
            PersistAndRemoveFromUnitOfWork(skillTypePhone);
            PersistAndRemoveFromUnitOfWork(skillTypeEmail);

            var skill1 = SkillFactory.CreateSkill("1", skillTypePhone, 15);
            skill1.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(skill1);

            var skill2 = SkillFactory.CreateSkill("2", skillTypeEmail, 15);
            skill2.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(skill2);

            var skill3 = SkillFactory.CreateMultisiteSkill("3", skillTypePhone, 13);
            skill3.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(skill3);

            var skill4 = SkillFactory.CreateChildSkill("4", skill3);
            skill4.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(skill4);

            var skill5 = SkillFactory.CreateMultisiteSkill("5", skillTypePhone, 15);
            skill5.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(skill5);

            var rep = new SkillRepository(UnitOfWork);
            var skillToTest = rep.LoadInboundTelephonySkills(15);
          
            CollectionAssert.Contains(skillToTest,skill1);
            Assert.AreEqual(1,skillToTest.Count());
           
        }

        protected override Repository<ISkill> TestRepository(IUnitOfWork unitOfWork)
        {
            return new SkillRepository(unitOfWork);
        }
    }
}

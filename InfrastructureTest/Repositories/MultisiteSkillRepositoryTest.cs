using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests for MultisiteSkillRepository
    /// </summary>
    [TestFixture]
    [Category("BucketB")]
    public class MultisiteSkillRepositoryTest : RepositoryTest<IMultisiteSkill>
    {
        private ISkillType _skillType;
        private IActivity _activity;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            _skillType = SkillTypeFactory.CreateSkillTypePhone();
            PersistAndRemoveFromUnitOfWork(_skillType);
            _activity = new Activity("The test"){DisplayColor = Color.Honeydew};
            PersistAndRemoveFromUnitOfWork(_activity);
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
        protected override IMultisiteSkill CreateAggregateWithCorrectBusinessUnit()
        {
            IMultisiteSkill skill = SkillFactory.CreateMultisiteSkill("Skill - Name", _skillType, 15);
            
            skill.Activity = _activity;
           
            return skill;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IMultisiteSkill loadedAggregateFromDatabase)
        {
            IMultisiteSkill skill = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(skill.Name, loadedAggregateFromDatabase.Name);
            Assert.AreEqual(skill.Description, loadedAggregateFromDatabase.Description);
            Assert.AreEqual(skill.DisplayColor, loadedAggregateFromDatabase.DisplayColor);
            Assert.AreEqual(skill.SkillType, loadedAggregateFromDatabase.SkillType);
            Assert.AreEqual(skill.Activity, loadedAggregateFromDatabase.Activity);
        }

        protected override Repository<IMultisiteSkill> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new MultisiteSkillRepository(currentUnitOfWork);
        }


        /// <summary>
        /// Verifies the can save multisite period distribution.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-24
        /// </remarks>
        [Test]
        public void VerifyCanSaveMultisitePeriodDistribution()
        {
            CleanUpAfterTest();
            IMultisiteSkill skill = CreateAggregateWithCorrectBusinessUnit();
            IMultisiteDayTemplate dayTemplate = skill.TemplateMultisiteWeekCollection.Values.First();
            SkillRepository skillRep = new SkillRepository(UnitOfWork);
            skillRep.Add(skill);

            IChildSkill childSkill = SkillFactory.CreateChildSkill("Child1", skill);

            try
            {
                skillRep.Add(childSkill);

                UnitOfWork.PersistAll();

                IDictionary<IChildSkill, Percent> distribution = new Dictionary<IChildSkill, Percent>();
                distribution.Add(childSkill, new Percent(1d));

                TemplateMultisitePeriod multisitePeriod = new TemplateMultisitePeriod(
                    new DateTimePeriod(
                        DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(TimeSpan.FromHours(0)), DateTimeKind.Utc),
                        DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(TimeSpan.FromDays(1)), DateTimeKind.Utc)),
                        distribution);

                dayTemplate.SetMultisitePeriodCollection(new List<ITemplateMultisitePeriod>() { multisitePeriod });
                UnitOfWork.PersistAll();

                Assert.AreEqual(1, dayTemplate.TemplateMultisitePeriodCollection.Count);
                Assert.AreEqual(new Percent(1d), dayTemplate.TemplateMultisitePeriodCollection[0].Distribution.ElementAt(0).Value);
                Assert.AreEqual(childSkill.Id.Value, dayTemplate.TemplateMultisitePeriodCollection[0].Distribution.ElementAt(0).Key.Id.Value);

            }
            finally
            {

                Session.Delete("from Skill");
                Session.Delete("from SkillType");
                Session.Delete("from Activity");
                Session.Flush();
            }

        }


        [Test]
        public void VerifyCanDeleteTemplate()
        {
            IMultisiteSkill skill = CreateAggregateWithCorrectBusinessUnit();

            IMultisiteDayTemplate template = new MultisiteDayTemplate("<TEMPLATE>", new List<ITemplateMultisitePeriod>());
            skill.AddTemplate(template);

            PersistAndRemoveFromUnitOfWork(skill);

            skill.RemoveTemplate(TemplateTarget.Multisite, "<TEMPLATE>");

            PersistAndRemoveFromUnitOfWork(skill);

            Assert.AreEqual(7, skill.TemplateMultisiteWeekCollection.Count);
        }

    }
}
using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{   
    /// <summary>
    /// Tests for MultisiteDayRepository
    /// </summary>
    [TestFixture]
    [Category("BucketB")]
    public class MultisiteDayRepositoryTest : RepositoryTest<IMultisiteDay>
    {
        private IScenario _scenario;
        private ISkillType _skillType;
        private IActivity _activity;
        private IMultisiteSkill _skill;
        private DateOnly _date = new DateOnly(2008, 1, 8);
        private IChildSkill _childSkill;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            PersistAndRemoveFromUnitOfWork(_scenario);

            _skillType = SkillTypeFactory.CreateSkillType();
            _skill = SkillFactory.CreateMultisiteSkill("dummy", _skillType, 15);
            _activity = new Activity("dummyActivity");
            _skill.Activity = _activity;

            PersistAndRemoveFromUnitOfWork(_skillType);

            PersistAndRemoveFromUnitOfWork(_activity);
            PersistAndRemoveFromUnitOfWork(_skill);

            _childSkill = SkillFactory.CreateChildSkill("skill1", _skill);
            PersistAndRemoveFromUnitOfWork(_childSkill);
        }

        /// <summary>
        /// Creates an aggreagte using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IMultisiteDay CreateAggregateWithCorrectBusinessUnit()
        {
            IDictionary<IChildSkill, Percent> distributions = new Dictionary<IChildSkill, Percent>();
            distributions.Add(_childSkill,new Percent(1));

            IList<IMultisitePeriod> multisitePeriods = new List<IMultisitePeriod>();
            multisitePeriods.Add(
                new MultisitePeriod(
                    DateTimeFactory.CreateDateTimePeriod(DateTime.SpecifyKind(_date.Date,DateTimeKind.Utc), 1).ChangeEndTime(TimeSpan.FromHours(-12)),
                    distributions));
            multisitePeriods.Add(
                new MultisitePeriod(
                    multisitePeriods[0].Period.ChangeStartTime(TimeSpan.FromHours(12)),
                    distributions));

            IMultisiteDay multisiteDay = new MultisiteDay(_date, _skill, _scenario);
            multisiteDay.SetMultisitePeriodCollection(multisitePeriods);

            _date = _date.AddDays(1);

            return multisiteDay;
        }


        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IMultisiteDay loadedAggregateFromDatabase)
        {
            IMultisiteDay multisiteDay = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(multisiteDay.Scenario.Description, loadedAggregateFromDatabase.Scenario.Description);
            Assert.AreEqual(multisiteDay.Skill.Name, loadedAggregateFromDatabase.Skill.Name);
        }

        /// <summary>
        /// Determines whether this instance can be created.
        /// </summary>
        [Test]
        public void CanCreate()
        {
            Assert.IsNotNull(new MultisiteDayRepository(UnitOfWork));
        }

        /// <summary>
        /// Determines whether this instance [can find skill days].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-23
        /// </remarks>
        [Test]
        public void CanFindMultisiteDays()
        {
            IMultisiteDay multisiteDay = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(multisiteDay);

            MultisiteDayRepository multisiteDayRepository = new MultisiteDayRepository(UnitOfWork);
            ICollection<IMultisiteDay> multisiteDays = multisiteDayRepository.FindRange(new DateOnlyPeriod(multisiteDay.MultisiteDayDate,multisiteDay.MultisiteDayDate.AddDays(1)),
                multisiteDay.Skill, multisiteDay.Scenario);

            Assert.AreEqual(1, multisiteDays.Count);
            IMultisiteDay md = new List<IMultisiteDay>(multisiteDays)[0];
            Assert.IsTrue(LazyLoadingManager.IsInitialized(md));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(md.MultisitePeriodCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(md.MultisitePeriodCollection[0].Distribution));
            Assert.AreEqual(2, md.MultisitePeriodCollection.Count);
        }

        /// <summary>
        /// Verifies the get all multisite days work.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyGetAllMultisiteDaysWork()
        {
            var multisiteDayRepository = new MultisiteDayRepository(UnitOfWork);
            var multisiteDays = multisiteDayRepository.GetAllMultisiteDays(new DateOnlyPeriod(_date,_date.AddDays(1)), new List<IMultisiteDay>(), _skill, _scenario);
            Assert.AreEqual(2, multisiteDays.Count);
            Assert.IsNotNull(multisiteDays.ElementAt(0).Id);
            Assert.IsNotNull(multisiteDays.ElementAt(1).Id);
        }

        /// <summary>
        /// Verifies the get all multisite days work without adding to repository.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-15
        /// </remarks>
        [Test]
        public void VerifyGetAllMultisiteDaysWorkWithoutAddingToRepository()
        {
            var multisiteDayRepository = new MultisiteDayRepository(UnitOfWork);
            var multisiteDays = multisiteDayRepository.GetAllMultisiteDays(new DateOnlyPeriod(_date, _date.AddDays(1)),
                                                           new List<IMultisiteDay>(), _skill, _scenario, false);
            Assert.AreEqual(2, multisiteDays.Count);
            Assert.IsNull(multisiteDays.ElementAt(0).Id);
            Assert.IsNull(multisiteDays.ElementAt(1).Id);
        }

        /// <summary>
        /// Determines whether this instance [can delete skill days].
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-12-11
        /// </remarks>
        [Test]
        public void CanDeleteSkillDays()
        {
            CleanUpAfterTest();

            IMultisiteDay multisiteDay = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(multisiteDay);

            MultisiteDayRepository multisiteDayRepository = new MultisiteDayRepository(UnitOfWork);

            DateOnlyPeriod dateTimePeriod = new DateOnlyPeriod(multisiteDay.MultisiteDayDate,
                                                               multisiteDay.MultisiteDayDate.AddDays(1));
            ICollection<IMultisiteDay> multisiteDays = multisiteDayRepository.FindRange(dateTimePeriod, _skill, _scenario);
            Assert.AreEqual(1, multisiteDays.Count);

            multisiteDayRepository.Delete(dateTimePeriod, _skill, _scenario);

            multisiteDays = multisiteDayRepository.FindRange(dateTimePeriod, _skill, _scenario);
            Assert.AreEqual(0, multisiteDays.Count);

            //clean up. bara smörja jag gjort här. orkar inte fixa henrys. sen fredag.
            UnitOfWork.Clear();
            new SkillTypeRepository(UnitOfWork).Remove(_skillType);
	        new ScenarioRepository(UnitOfWork).Remove(_scenario);
	        new ActivityRepository(UnitOfWork).Remove(_skill.Activity);
            foreach (var childSkill in new SkillRepository(UnitOfWork).LoadAll())
            {
							Session.Delete(childSkill);
            }


            UnitOfWork.PersistAll();
        }

        /// <summary>
        /// Verifies the cannot save multiple multisite days.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-30
        /// </remarks>
        [Test]
        public void VerifyCannotSaveMultipleMultisiteDays()
        {
            IMultisiteDay day1 = CreateAggregateWithCorrectBusinessUnit();
            _date = day1.MultisiteDayDate;
            IMultisiteDay day2 = CreateAggregateWithCorrectBusinessUnit();

            PersistAndRemoveFromUnitOfWork(day1);
            Assert.Throws<ConstraintViolationException>(() => PersistAndRemoveFromUnitOfWork(day2));
        }

        [Test]
        public void VerifyCanSaveClonedMultisiteDayToNewScenario()
        {
            IMultisiteDay day1 = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(day1);

            IScenario scenario2 = ScenarioFactory.CreateScenarioAggregate("Scenario nr 2", false);
            PersistAndRemoveFromUnitOfWork(scenario2);

            IMultisiteDay day2 = day1.NoneEntityClone(scenario2);
            PersistAndRemoveFromUnitOfWork(day2);
        }

        protected override Repository<IMultisiteDay> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new MultisiteDayRepository(currentUnitOfWork);
        }
    }
}

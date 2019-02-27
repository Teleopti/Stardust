using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class RuleSetBagRepositoryTest : RepositoryTest<IRuleSetBag>
    {
	    private IWorkShiftRuleSet ruleSet;

	    protected override void ConcreteSetup()
		{
			IActivity act = new Activity("for test");
			IShiftCategory cat = new ShiftCategory("for test");
			PersistAndRemoveFromUnitOfWork(act);
			PersistAndRemoveFromUnitOfWork(cat);
			ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(act, new TimePeriodWithSegment(1, 2, 3, 4, 5), new TimePeriodWithSegment(1, 2, 3, 4, 5), cat));
			ruleSet.Description = new Description("test");
			ruleSet.AddAccessibilityDate(new DateOnly(2015, 1, 1));
			ruleSet.AddAccessibilityDate(new DateOnly(2015, 1, 2));
			ruleSet.AddAccessibilityDayOfWeek(DayOfWeek.Friday);
			ruleSet.AddAccessibilityDayOfWeek(DayOfWeek.Saturday);
			PersistAndRemoveFromUnitOfWork(ruleSet);
		}

        protected override IRuleSetBag CreateAggregateWithCorrectBusinessUnit()
        {
            RuleSetBag root = new RuleSetBag();
            root.Description = new Description("for test");
            root.AddRuleSet(ruleSet);
            return root;
        }

        protected override void VerifyAggregateGraphProperties(IRuleSetBag loadedAggregateFromDatabase)
        {
            Assert.AreEqual("for test", loadedAggregateFromDatabase.Description.Name);
            Assert.AreEqual(1, loadedAggregateFromDatabase.RuleSetCollection.Count);
        }

        protected override Repository<IRuleSetBag> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return RuleSetBagRepository.DONT_USE_CTOR(currentUnitOfWork);
        }

        [Test]
        public void ShouldLoadRuleSetsAtTheSameTime()
        {
            var bag = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(bag);
            var bags = RuleSetBagRepository.DONT_USE_CTOR(UnitOfWork).LoadAllWithRuleSets();
            Assert.That(bags.Count(),Is.EqualTo(1));
            Assert.That(bags.First().RuleSetCollection.Count,Is.EqualTo(1));
        }

		[Test]
		public void ShouldLoadRuleSetWhenFound()
		{
			var bag = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(bag);

			var bags = RuleSetBagRepository.DONT_USE_CTOR(UnitOfWork).FindWithRuleSetsAndAccessibility(bag.Id.GetValueOrDefault());
			Assert.That(bags.RuleSetCollection.Count, Is.EqualTo(1));
			Assert.That(LazyLoadingManager.IsInitialized(bags.RuleSetCollection[0].AccessibilityDates), Is.True);
			Assert.That(LazyLoadingManager.IsInitialized(bags.RuleSetCollection[0].AccessibilityDaysOfWeek), Is.True);
			Assert.That(bags.RuleSetCollection[0].AccessibilityDates.Count(), Is.EqualTo(2));
			Assert.That(bags.RuleSetCollection[0].AccessibilityDaysOfWeek.Count(), Is.EqualTo(2));
		}
    }
}

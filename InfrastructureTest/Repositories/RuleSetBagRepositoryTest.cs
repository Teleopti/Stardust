using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class RuleSetBagRepositoryTest : RepositoryTest<IRuleSetBag>
    {
        protected override void ConcreteSetup()
        {
        }

        protected override IRuleSetBag CreateAggregateWithCorrectBusinessUnit()
        {
            IActivity act = new Activity("for test");
            IShiftCategory cat = new ShiftCategory("for test");
            PersistAndRemoveFromUnitOfWork(act);
            PersistAndRemoveFromUnitOfWork(cat);
            WorkShiftRuleSet ruleSet =new WorkShiftRuleSet(new WorkShiftTemplateGenerator(act, new TimePeriodWithSegment(1, 2, 3, 4, 5), new TimePeriodWithSegment(1, 2, 3, 4, 5), cat));
            ruleSet.Description = new Description("test");
            PersistAndRemoveFromUnitOfWork(ruleSet);
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

        protected override Repository<IRuleSetBag> TestRepository(IUnitOfWork unitOfWork)
        {
            return new RuleSetBagRepository(unitOfWork);
        }

        [Test]
        public void ShouldLoadRuleSetsAtTheSameTime()
        {
            var bag = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(bag);
            var bags = new RuleSetBagRepository(UnitOfWork).LoadAllWithRuleSets();
            Assert.That(bags.Count(),Is.EqualTo(1));
            Assert.That(bags.First().RuleSetCollection.Count,Is.EqualTo(1));
        }
    }
}

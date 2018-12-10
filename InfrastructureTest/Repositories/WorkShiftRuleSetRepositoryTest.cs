using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class WorkShiftRuleSetRepositoryTest : RepositoryTest<IWorkShiftRuleSet>
    {
        private IShiftCategory shiftCat;
        private IActivity act;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            shiftCat = ShiftCategoryFactory.CreateShiftCategory("used in test");
            act = new Activity("used in test");

            PersistAndRemoveFromUnitOfWork(shiftCat);
            PersistAndRemoveFromUnitOfWork(act);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IWorkShiftRuleSet CreateAggregateWithCorrectBusinessUnit()
        {
            IWorkShiftTemplateGenerator generator =
                new WorkShiftTemplateGenerator(act, new TimePeriodWithSegment(8, 0, 10, 20, 15),
                                               new TimePeriodWithSegment(17, 0, 19, 0, 25), shiftCat);

            WorkShiftRuleSet root = new WorkShiftRuleSet(generator);
            root.AddExtender(
                new ActivityAbsoluteStartExtender(act, new TimePeriodWithSegment(1, 2, 3, 4, 5),
                                                  new TimePeriodWithSegment(2, 3, 4, 5, 6)));
            root.AddExtender(
                            new ActivityRelativeEndExtender(act, new TimePeriodWithSegment(1, 2, 3, 4, 5),
                                                              new TimePeriodWithSegment(1, 2, 3, 4, 5)));
            root.AddExtender(
                            new ActivityRelativeStartExtender(act, new TimePeriodWithSegment(1, 2, 3, 4, 5),
                                                              new TimePeriodWithSegment(1, 2, 3, 4, 5)));
            root.AddExtender(
                new AutoPositionedActivityExtender(act, new TimePeriodWithSegment(1, 2, 3, 4, 5),
                                                   new TimeSpan(22)));
            root.AddLimiter(new ContractTimeLimiter(new TimePeriod(1,2,3,4), new TimeSpan()));
            root.AddLimiter(new ActivityTimeLimiter(act, TimeSpan.FromHours(1), OperatorLimiter.LessThen));
            root.Description = new Description("test");
            root.DefaultAccessibility = DefaultAccessibility.Excluded;
            root.AddAccessibilityDayOfWeek(DayOfWeek.Sunday);
            root.AddAccessibilityDayOfWeek(DayOfWeek.Saturday);
            root.AddAccessibilityDate(new DateOnly(2007, 12, 24));
            root.AddAccessibilityDate(new DateOnly(2008, 12, 24));
            root.AddAccessibilityDate(new DateOnly(2009, 12, 24));
            return root;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IWorkShiftRuleSet loadedAggregateFromDatabase)
        {
            Assert.AreEqual("used in test", loadedAggregateFromDatabase.TemplateGenerator.Category.Description.Name);
            Assert.AreEqual("used in test", loadedAggregateFromDatabase.TemplateGenerator.BaseActivity.Description.Name);
            Assert.AreEqual(new TimePeriodWithSegment(8, 0, 10, 20, 15), loadedAggregateFromDatabase.TemplateGenerator.StartPeriod);
            Assert.AreEqual(new TimePeriodWithSegment(17, 0, 19, 0, 25), loadedAggregateFromDatabase.TemplateGenerator.EndPeriod);

            IList<IWorkShiftExtender> extenders = loadedAggregateFromDatabase.ExtenderCollection;
            Assert.AreEqual(4, extenders.Count);
            ActivityAbsoluteStartExtender extCheck = extenders[0] as ActivityAbsoluteStartExtender;
            Assert.IsNotNull(extCheck);
            Assert.AreEqual(new TimePeriodWithSegment(1, 2, 3, 4, 5), extCheck.ActivityLengthWithSegment);
            Assert.AreEqual("used in test", extCheck.ExtendWithActivity.Description.Name);
            Assert.AreEqual(new TimePeriodWithSegment(2, 3, 4, 5, 6), extCheck.ActivityPositionWithSegment);
            Assert.IsNotInstanceOf<ActivityRelativeEndExtender>(extenders[1].GetType());
            Assert.IsNotInstanceOf<ActivityRelativeStartExtender>(extenders[2].GetType());
            Assert.IsNotInstanceOf<AutoPositionedActivityExtender>(extenders[3].GetType());

            Assert.AreEqual(2, loadedAggregateFromDatabase.LimiterCollection.Count);

            Assert.AreEqual(DefaultAccessibility.Excluded, loadedAggregateFromDatabase.DefaultAccessibility);

            Assert.AreEqual(2,loadedAggregateFromDatabase.AccessibilityDaysOfWeek.Count());
            Assert.IsTrue(loadedAggregateFromDatabase.AccessibilityDaysOfWeek.Contains(DayOfWeek.Saturday));
            Assert.IsTrue(loadedAggregateFromDatabase.AccessibilityDaysOfWeek.Contains(DayOfWeek.Sunday));

            Assert.AreEqual(3, loadedAggregateFromDatabase.AccessibilityDates.Count());
            Assert.IsTrue(loadedAggregateFromDatabase.AccessibilityDates.Contains(new DateOnly(2007, 12, 24)));
            Assert.IsTrue(loadedAggregateFromDatabase.AccessibilityDates.Contains(new DateOnly(2008, 12, 24)));
            Assert.IsTrue(loadedAggregateFromDatabase.AccessibilityDates.Contains(new DateOnly(2009, 12, 24)));
        }


        protected override Repository<IWorkShiftRuleSet> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new WorkShiftRuleSetRepository(currentUnitOfWork);
        }

        [Test]
        public void VerifyFindAllWithLimitersAndExtenders()
        {
            IWorkShiftTemplateGenerator generator =
                new WorkShiftTemplateGenerator(act, new TimePeriodWithSegment(8, 0, 10, 20, 15),
                                               new TimePeriodWithSegment(17, 0, 19, 0, 25), shiftCat);
            WorkShiftRuleSet ruleSet = new WorkShiftRuleSet(generator);
            ruleSet.Description = new Description("first", "sdg");
            WorkShiftRuleSet ruleSet2 = new WorkShiftRuleSet(generator);
            ruleSet2.Description = new Description("second", "sdg");
            ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(10, 11, 12, 13), new TimeSpan()));
            Activity actForExtender = new Activity("sdf");
            PersistAndRemoveFromUnitOfWork(actForExtender);
            ruleSet.AddExtender(
                new ActivityRelativeEndExtender(act, new TimePeriodWithSegment(11, 12, 13, 14, 15),
                                           new TimePeriodWithSegment(10, 11, 12, 13, 14)));
            ruleSet.AddExtender(
                new ActivityRelativeEndExtender(actForExtender, new TimePeriodWithSegment(11, 12, 13, 14, 15),
                                           new TimePeriodWithSegment(10, 11, 12, 13, 14)));
            PersistAndRemoveFromUnitOfWork(ruleSet);
            PersistAndRemoveFromUnitOfWork(ruleSet2);

            ICollection<IWorkShiftRuleSet> res = new WorkShiftRuleSetRepository(UnitOfWork).FindAllWithLimitersAndExtenders();

            Assert.AreEqual(2, res.Count);
            foreach (WorkShiftRuleSet shiftRuleSet in res)
            {
                Assert.IsTrue(LazyLoadingManager.IsInitialized(shiftRuleSet.TemplateGenerator.BaseActivity));
                Assert.IsTrue(LazyLoadingManager.IsInitialized(shiftRuleSet.TemplateGenerator.Category));
                Assert.IsTrue(LazyLoadingManager.IsInitialized(shiftRuleSet.LimiterCollection));
                Assert.IsTrue(LazyLoadingManager.IsInitialized(shiftRuleSet.ExtenderCollection));
                if (shiftRuleSet.Description.Name == "first")
                {
                    Assert.AreEqual(1, shiftRuleSet.LimiterCollection.Count);
                    Assert.AreEqual(2, shiftRuleSet.ExtenderCollection.Count);
                    Assert.IsTrue(LazyLoadingManager.IsInitialized(shiftRuleSet.ExtenderCollection[0].ExtendWithActivity));
                }
                else
                {
                    Assert.AreEqual(0, shiftRuleSet.LimiterCollection.Count);
                    Assert.AreEqual(0, shiftRuleSet.ExtenderCollection.Count);
                }
            }
        }

        [Test]
        public void VerifyCanRemoveExtenders()
        {
            IWorkShiftRuleSet ruleSet = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(ruleSet);

            ruleSet = new WorkShiftRuleSetRepository(UnitOfWork).Get(ruleSet.Id.Value);
            ruleSet.DeleteExtender(ruleSet.ExtenderCollection[2]);
            PersistAndRemoveFromUnitOfWork(ruleSet);

            ruleSet = new WorkShiftRuleSetRepository(UnitOfWork).Get(ruleSet.Id.Value);
            Assert.AreEqual(3, ruleSet.ExtenderCollection.Count);
        }

        [Test]
        public void VerifyCanRemoveLimiters()
        {
            IWorkShiftRuleSet ruleSet = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(ruleSet);

            ruleSet = new WorkShiftRuleSetRepository(UnitOfWork).Get(ruleSet.Id.Value);
            ruleSet.DeleteLimiter(ruleSet.LimiterCollection[1]);
            PersistAndRemoveFromUnitOfWork(ruleSet);

            ruleSet = new WorkShiftRuleSetRepository(UnitOfWork).Get(ruleSet.Id.Value);
            Assert.AreEqual(1, ruleSet.LimiterCollection.Count);
        }
    }
}

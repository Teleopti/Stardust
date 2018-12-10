using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
    [TestFixture]
    public class BudgetDayGapFillerTest
    {
        private IScenario scenario;
        private IBudgetGroup budgetGroup;
        private DateOnlyPeriod period;
        private IBudgetDayGapFiller _target;
        private BudgetGroupMainModel budgetGroupMainModel;

        [SetUp]
        public void Setup()
        {
            scenario = ScenarioFactory.CreateScenarioAggregate();
            budgetGroup = new BudgetGroup { Name = "BG" };
            period = new DateOnlyPeriod(2010, 2, 1, 2010, 2, 5);
            budgetGroupMainModel = new BudgetGroupMainModel(null) { BudgetGroup = budgetGroup, Period = period, Scenario = scenario };
            _target = new BudgetDayGapFiller(budgetGroupMainModel);
        }

        [Test]
        public void ShouldAddConsecutiveDaysIfMissing()
        {
            var day = new DateOnly(2010, 2, 1);
            var budgetDay2 = new BudgetDay(budgetGroup, scenario, day.AddDays(1));
            var budgetDay3 = new BudgetDay(budgetGroup, scenario, day.AddDays(3));
            IList<IBudgetDay> budgetDays = new List<IBudgetDay> {budgetDay2, budgetDay3};

            budgetDays = _target.AddMissingDays(budgetDays, period);
            Assert.AreEqual(5, budgetDays.Count);

            Assert.AreEqual(new DateOnly(2010, 2, 1), budgetDays[0].Day);
            Assert.AreEqual(new DateOnly(2010, 2, 2), budgetDays[1].Day);
            Assert.AreEqual(new DateOnly(2010, 2, 3), budgetDays[2].Day);
            Assert.AreEqual(new DateOnly(2010, 2, 4), budgetDays[3].Day);
            Assert.AreEqual(new DateOnly(2010, 2, 5), budgetDays[4].Day);
        }

        [Test]
        public void ShouldReturnSortedListOfConsecutiveDays()
        {
            var day = new DateOnly(2010, 2, 1);
            var budgetDay2 = new BudgetDay(budgetGroup, scenario, day.AddDays(3));
            var budgetDay3 = new BudgetDay(budgetGroup, scenario, day.AddDays(1));
            IList<IBudgetDay> budgetDays = new List<IBudgetDay> { budgetDay2, budgetDay3 };

            budgetDays = _target.AddMissingDays(budgetDays, period);
            Assert.AreEqual(5, budgetDays.Count);

            Assert.AreEqual(new DateOnly(2010, 2, 1), budgetDays[0].Day);
            Assert.AreEqual(new DateOnly(2010, 2, 2), budgetDays[1].Day);
            Assert.AreEqual(new DateOnly(2010, 2, 3), budgetDays[2].Day);
            Assert.AreEqual(new DateOnly(2010, 2, 4), budgetDays[3].Day);
            Assert.AreEqual(new DateOnly(2010, 2, 5), budgetDays[4].Day);
        }

        [Test]
        public void ShouldSetDefaultValueIfIsMissing()
        {
            var day = new DateOnly(2010, 2, 1);
            var budgetDay2 = new BudgetDay(budgetGroup, scenario, day.AddDays(1));
            var budgetDay3 = new BudgetDay(budgetGroup, scenario, day.AddDays(3));
            IList<IBudgetDay> budgetDays = new List<IBudgetDay> { budgetDay2, budgetDay3 };

            budgetDays = _target.AddMissingDays(budgetDays, period);
            Assert.AreEqual(5, budgetDays.Count);

            Assert.AreEqual(new DateOnly(2010, 2, 1), budgetDays[0].Day);
            Assert.AreEqual(1.0d, budgetDays[0].AbsenceThreshold.Value);
            Assert.AreEqual(1.0d, budgetDays[2].AbsenceThreshold.Value);
            Assert.AreEqual(1.0d, budgetDays[4].AbsenceThreshold.Value);
        }
    }
}
using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for Contract class.
    /// </summary>
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class ContractTest
    {
        private IContract _testContract;

        /// <summary>
        /// Setups the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _testContract = new Contract("test");
        }

        /// <summary>
        /// Verifies that default properties are set
        /// </summary>
        [Test]
        public void VerifyDefaultPropertiesAreSet()
        {
            Assert.AreEqual("test", _testContract.Description.Name);
            Assert.AreEqual(BusinessUnitFactory.BusinessUnitUsedInTest, ((Contract)_testContract).BusinessUnit);
            Assert.AreEqual(EmploymentType.FixedStaffNormalWorkTime, _testContract.EmploymentType);
            Assert.IsNotNull(_testContract.WorkTime);
            Assert.AreEqual(8, _testContract.WorkTime.AvgWorkTimePerDay.TotalHours);
            Assert.IsNotNull(_testContract.WorkTimeDirective);
            Assert.AreEqual(48, _testContract.WorkTimeDirective.MaxTimePerWeek.TotalHours);
            Assert.AreEqual(11, _testContract.WorkTimeDirective.NightlyRest.TotalHours);
            Assert.AreEqual(36, _testContract.WorkTimeDirective.WeeklyRest.TotalHours);
            Assert.IsTrue(_testContract.IsChoosable);
            Assert.IsNotNull(_testContract.MultiplicatorDefinitionSetCollection);
            Assert.AreEqual(TimeSpan.Zero, _testContract.PositivePeriodWorkTimeTolerance);
            Assert.AreEqual(TimeSpan.Zero, _testContract.NegativePeriodWorkTimeTolerance);
            Assert.AreEqual(TimeSpan.Zero, _testContract.MinTimeSchedulePeriod);
            Assert.IsFalse(_testContract.AdjustTimeBankWithSeasonality);
            Assert.IsFalse(_testContract.AdjustTimeBankWithPartTimePercentage);
            Assert.AreEqual(0, _testContract.PositiveDayOffTolerance);
            Assert.AreEqual(0, _testContract.NegativeDayOffTolerance);
        }

        /// <summary>
        /// Verifies that name can be set
        /// </summary>
        [Test]
        public void VerifyNameCanBeSet()
        {
            _testContract.Description = new Description("new",null);
            Assert.AreEqual("new", _testContract.Description.Name);
        }

        /// <summary>
        /// Verifies that employment type for contract can be set
        /// </summary>
        [Test]
        public void VerifyEmploymentTypeCanBeSet()
        {
            _testContract.EmploymentType = EmploymentType.HourlyStaff;
            Assert.AreEqual(EmploymentType.HourlyStaff, _testContract.EmploymentType);
        }

        [Test]
        public void VerifyAdjustTimeBankWithSeasonality()
        {
            bool newValue = !_testContract.AdjustTimeBankWithSeasonality;
            _testContract.AdjustTimeBankWithSeasonality = newValue;

            Assert.AreEqual(newValue, _testContract.AdjustTimeBankWithSeasonality);
        }

        [Test]
        public void VerifyAdjustTimeBankWithPartTimePercentage()
        {
            bool newValue = !_testContract.AdjustTimeBankWithPartTimePercentage;
            _testContract.AdjustTimeBankWithPartTimePercentage = newValue;

            Assert.AreEqual(newValue, _testContract.AdjustTimeBankWithPartTimePercentage);
        }

        /// <summary>
        /// Verifies that work time can be set for contract
        /// </summary>
        [Test]
        public void VerifyWorkTimeCanBeSet()
        {
            var workTime = new WorkTime(TimeSpan.FromHours(8));
            _testContract.WorkTime = workTime;
            Assert.AreEqual(workTime, _testContract.WorkTime);
        }

        /// <summary>
        /// Verifies that work time directive can be set for contract
        /// </summary>
        [Test]
        public void VerifyWorkTimeDirectiveCanBeSet()
        {
            var workTimeDirective =
				new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(48), TimeSpan.FromHours(11),
                                      TimeSpan.FromHours(36));
            _testContract.WorkTimeDirective = workTimeDirective;
            Assert.AreEqual(workTimeDirective, _testContract.WorkTimeDirective);
        }

        [Test]
        public void VerifyMinTimeSchedulePeriodCanBeSet()
        {
            var ts = new TimeSpan(10);
            _testContract.MinTimeSchedulePeriod = ts;
            Assert.AreEqual(ts, _testContract.MinTimeSchedulePeriod);
        }


        [Test]
        public void VerifyAddAndRemoveMultiplicatorDefinitionSets()
        {
            IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("OverTime", MultiplicatorType.Overtime);
            _testContract.AddMultiplicatorDefinitionSetCollection(definitionSet);
            Assert.IsTrue(1 == _testContract.MultiplicatorDefinitionSetCollection.Count);
            _testContract.RemoveMultiplicatorDefinitionSetCollection(definitionSet);
            Assert.IsTrue(0 == _testContract.MultiplicatorDefinitionSetCollection.Count);
        }
        [Test]
        public void VerifyCannotAddDuplicateMultiplicatorDefinitionSets()
        {
            IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("OverTime", MultiplicatorType.Overtime);
            _testContract.AddMultiplicatorDefinitionSetCollection(definitionSet);
            Assert.IsTrue(1 == _testContract.MultiplicatorDefinitionSetCollection.Count);
            _testContract.AddMultiplicatorDefinitionSetCollection(definitionSet);
            Assert.IsTrue(1 == _testContract.MultiplicatorDefinitionSetCollection.Count);
        }
        [Test]
        public void VerifyNullCannotBeAddedMultiplicatorDefinitionSets()
        {
            Assert.Throws<ArgumentNullException>(() => _testContract.AddMultiplicatorDefinitionSetCollection(null));
        }

        [Test]
        public void ProtectedConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_testContract.GetType()));
        }

        [Test]
        public void VerifyPeriodWorkTimeTolerance()
        {
            _testContract.PositivePeriodWorkTimeTolerance = TimeSpan.FromMinutes(1);
            _testContract.NegativePeriodWorkTimeTolerance = TimeSpan.FromMinutes(2);
            Assert.AreEqual(TimeSpan.FromMinutes(1), _testContract.PositivePeriodWorkTimeTolerance);
            Assert.AreEqual(TimeSpan.FromMinutes(2), _testContract.NegativePeriodWorkTimeTolerance);
        }

        [Test]
        public void ShouldNotAllowPositiveValueOnPlanningTimeBankMin()
        {
            _testContract.PlanningTimeBankMin = TimeSpan.FromHours(-5);
            Assert.That(_testContract.PlanningTimeBankMin, Is.EqualTo(TimeSpan.FromHours(-5)));
            _testContract.PlanningTimeBankMin = TimeSpan.FromHours(1);
            Assert.That(_testContract.PlanningTimeBankMin, Is.EqualTo(TimeSpan.FromHours(-5)));
        }

        [Test]
        public void ShouldNotAllowTooLowValuesOnPlanningTimeBankMin()
        {
            var min = TimeSpan.FromHours(-99).Add(TimeSpan.FromMinutes(-59));
            _testContract.PlanningTimeBankMin = min;
            Assert.That(_testContract.PlanningTimeBankMin, Is.EqualTo(min));
            _testContract.PlanningTimeBankMin = TimeSpan.FromHours(-100);
            Assert.That(_testContract.PlanningTimeBankMin, Is.EqualTo(min));
        }

        [Test]
        public void ShouldNotAllowNegativeValueOnPlanningTimeBankMax()
        {
            _testContract.PlanningTimeBankMax = TimeSpan.FromHours(5);
            Assert.That(_testContract.PlanningTimeBankMax, Is.EqualTo(TimeSpan.FromHours(5)));
            _testContract.PlanningTimeBankMax = TimeSpan.FromHours(-1);
            Assert.That(_testContract.PlanningTimeBankMax, Is.EqualTo(TimeSpan.FromHours(5)));
        }

        [Test]
        public void ShouldNotAllowTooHighValueOnPlanningTimeBankMax()
        {
            var max = TimeSpan.FromHours(99).Add(TimeSpan.FromMinutes(59));
            _testContract.PlanningTimeBankMax = max;
            Assert.That(_testContract.PlanningTimeBankMax, Is.EqualTo(max));
            _testContract.PlanningTimeBankMax = TimeSpan.FromHours(100);
            Assert.That(_testContract.PlanningTimeBankMax, Is.EqualTo(max));
        }

        [Test]
        public void VerifyDayOffTolerance()
        {
            Assert.AreEqual(0, _testContract.PositiveDayOffTolerance);
            Assert.AreEqual(0, _testContract.NegativeDayOffTolerance);
            _testContract.PositiveDayOffTolerance = 2;
            _testContract.NegativeDayOffTolerance = 2;
            Assert.AreEqual(2, _testContract.PositiveDayOffTolerance);
            Assert.AreEqual(2, _testContract.NegativeDayOffTolerance);
        }

        [Test]
        public void VerifyIsWorkTimeFromContract()
        {
            Assert.AreEqual(WorkTimeSource.FromContract, _testContract.WorkTimeSource);
            _testContract.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;
            Assert.AreNotEqual(WorkTimeSource.FromContract, _testContract.WorkTimeSource);
        }

        [Test]
        public void VerifyIsWorkTimeFromSchedulePeriod()
        {
            Assert.AreEqual(WorkTimeSource.FromContract, _testContract.WorkTimeSource);
            _testContract.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;
            Assert.AreEqual(WorkTimeSource.FromSchedulePeriod, _testContract.WorkTimeSource);
        }
    }
}
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class EmptyPersonAccountBalanceCalculatorTest
    {
        private IPersonAccountBalanceCalculator _target;
        private IAbsence _absence;

        [SetUp]
        public void Setup()
        {
            _absence = AbsenceFactory.CreateAbsence("Semester");
            _target = new EmptyPersonAccountBalanceCalculator(_absence);
        }

        [Test]
        public void VerifyAbsenceMustNotHaveTracker()
        {
            _absence.Tracker = Tracker.CreateDayTracker();
            _target = new EmptyPersonAccountBalanceCalculator(_absence);
            Assert.IsFalse(_target.CheckBalance(null, new DateOnlyPeriod()));
        }

        [Test]
        public void VerifyCalculate()
        {
            Assert.IsTrue(_target.CheckBalance(null, new DateOnlyPeriod()));
        }
    }
}

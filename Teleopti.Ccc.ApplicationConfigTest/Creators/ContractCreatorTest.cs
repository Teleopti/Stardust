using System;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class ContractCreatorTest
    {
        private ContractCreator _target;

        [SetUp]
        public void Setup()
        {
            _target = new ContractCreator();
        }

        [Test]
        public void VerifyCanCreateAbsence()
        {
            WorkTimeDirective workTimeDirective = new WorkTimeDirective(new TimeSpan(0, 0, 0), new TimeSpan(40,0,0), new TimeSpan(8,0,0), new TimeSpan(50,0,0));
            WorkTime workTime = new WorkTime(new TimeSpan(8,0,0));
            IContract contract = _target.Create("name", new Description("name", "shortName"),EmploymentType.FixedStaffNormalWorkTime, workTime, workTimeDirective);
            Assert.AreEqual(new Description("name", "shortName"), contract.Description);
            Assert.AreEqual(EmploymentType.FixedStaffNormalWorkTime, contract.EmploymentType);
            Assert.AreEqual(workTime, contract.WorkTime);
            Assert.AreEqual(workTimeDirective, contract.WorkTimeDirective);
            Assert.AreEqual(true, contract.IsChoosable);
        }
    }
}

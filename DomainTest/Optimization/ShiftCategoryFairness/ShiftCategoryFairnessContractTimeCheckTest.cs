using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ShiftCategoryFairness
{
    [TestFixture]
    public class ShiftCategoryFairnessContractTimeCheckTest
    {
        private IShiftCategoryFairnessContractTimeChecker _target;
        private IPersonPeriod _personPeriod1, _personPeriod2;

        [SetUp]
        public void Setup()
        {
            _target = new ShiftCategoryFairnessContractTimeChecker();
            
            var contract1 = new Contract("contract1");
            var contract2 = new Contract("contract2");

            var partTimePercent1 = new PartTimePercentage("partTimePercentage1");
            var partTimePercent2 = new PartTimePercentage("partTimePercentage2");

            var contractSchedule1 = new ContractSchedule("contractSchedule1");
            var contractSchedule2 = new ContractSchedule("contractSchedule2");

            var personContract1 = new PersonContract(contract1, partTimePercent1, contractSchedule1);
            var personContract2 = new PersonContract(contract2, partTimePercent2, contractSchedule2);
            _personPeriod1 = new PersonPeriod(new DateOnly(2012, 10, 08), personContract1, new Team());
            _personPeriod2 = new PersonPeriod(new DateOnly(2012, 10, 08), personContract2, new Team());
        }

        [Test]
        public void SameShouldReturnTrue()
        {
            Assert.That(_target.Check(_personPeriod1, _personPeriod1), Is.True);
        }

        [Test]
        public void DifferentShouldReturnFalse()
        {
            Assert.That(_target.Check(_personPeriod1, _personPeriod2), Is.False);
        }

    }
}

using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
    [TestFixture]
    public class IsShiftTradeRequestNotNullSpecificationTest
    {
        private IsShiftTradeRequestNotNullSpecification _target;


        [SetUp]
        public void Setup()
        {
            _target = new IsShiftTradeRequestNotNullSpecification();
        }

        [Test]
        public void VerifyShiftTradeRequestMustNotBeNull()
        {
            Assert.IsFalse(_target.IsSatisfiedBy(null));
            Assert.IsTrue(_target.IsSatisfiedBy(new ShiftTradeRequest(new List<IShiftTradeSwapDetail>())));
        }
    }
}

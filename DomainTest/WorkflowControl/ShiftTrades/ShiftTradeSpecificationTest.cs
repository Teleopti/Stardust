using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
    [TestFixture]
    public class ShiftTradeSpecificationTest
    {

        private TestSpecification _spec = new TestSpecification(false, "hej");
        private TestSpecification _spec2 = new TestSpecification(true, "hej2");


        [SetUp]
        public void Setup()
        {
            _spec = new TestSpecification(false, "hej");
            _spec2 = new TestSpecification(true, "hej2");


        }

        [Test]
        public void VerifyThatValidateReturnsDenyReason()
        {

            ShiftTradeRequestValidationResult result = _spec.Validate(new List<IShiftTradeSwapDetail>());
            var result2 = _spec2.Validate(new List<IShiftTradeSwapDetail>());

            Assert.IsFalse(result.Value);
            Assert.AreEqual("hej", result.DenyReason);

            Assert.IsTrue(result2.Value);
            Assert.AreEqual(string.Empty, result2.DenyReason);
        }


        private class TestSpecification : ShiftTradeSpecification
        {
            private readonly bool _retValue;
            private readonly string _denyReason;

            public override string DenyReason
            {
                get { return _denyReason; }
            }

            public TestSpecification(bool retValue, string denyReason)
            {
                _retValue = retValue;
                _denyReason = denyReason;
            }

            public override bool IsSatisfiedBy(IList<IShiftTradeSwapDetail> obj)
            {
                return _retValue;
            }
        }
    }
}

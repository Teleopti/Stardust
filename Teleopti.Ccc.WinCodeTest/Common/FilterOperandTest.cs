using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class FilterOperandTest
    {
        private FilterAdvancedTupleItem _filterOperand;

        [SetUp]
        public void Setup()
        {
            _filterOperand = new FilterAdvancedTupleItem("=", "equals");
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual("=", _filterOperand.Text);
            Assert.AreEqual("equals", _filterOperand.Value);
        }
    }
}
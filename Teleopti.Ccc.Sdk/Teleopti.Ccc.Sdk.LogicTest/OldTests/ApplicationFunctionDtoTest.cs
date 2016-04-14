using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class ApplicationFunctionDtoTest
    {
        [Test]
        public void VerifyCanSetAndGetProperties()
        {
			var target = new ApplicationFunctionDto();
            target.ForeignId = "100";
            Assert.AreEqual("100", target.ForeignId);

            target.FunctionDescription = "Desc";
            Assert.AreEqual("Desc", target.FunctionDescription);

            target.ForeignSource = "1001";
            Assert.AreEqual("1001", target.ForeignSource);

			target.FunctionCode = "code707";
			Assert.AreEqual("code707", target.FunctionCode);

            target.FunctionPath = @"Raptor\MyTime";
            Assert.AreEqual(@"Raptor\MyTime", target.FunctionPath);

            target.FunctionCode = "code";
            Assert.AreEqual("code", target.FunctionCode);
        }

        [Test]
        public void VerifyCanCreateApplicationFunctionUsingFunctionCode()
        {
            var target = new ApplicationFunctionDto("/MyTime");

            Assert.IsNotNull(target);
            Assert.AreEqual("/MyTime", target.FunctionCode);
        }
    }
}
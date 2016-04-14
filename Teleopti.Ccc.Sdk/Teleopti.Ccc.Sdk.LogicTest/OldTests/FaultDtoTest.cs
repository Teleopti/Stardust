using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class FaultDtoTest
    {
        [Test]
        public void VerifyProperties()
        {
			const string message = "LicenseIsInvalidPerhapsForgedPleaseApplyANewOne";
			var target = new FaultDto(message);

			Assert.AreEqual(message, target.Message);
        }
    }
}
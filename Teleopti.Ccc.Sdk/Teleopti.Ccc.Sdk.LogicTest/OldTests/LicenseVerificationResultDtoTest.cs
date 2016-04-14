using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class LicenseVerificationResultDtoTest
    {
        [Test]
        public void VerifyProperties()
        {
			var isValidLicenseFound = false;
			var exception = new FaultDto("ProductActivationKeyIsInvalidPerhapsForgedPleaseApplyANewOne");
			var warning = new FaultDto("YourProductActivationKeyWillExpireDoNotForgetToRenewItInTime");

			var target = new LicenseVerificationResultDto(isValidLicenseFound);
			target.AddExceptionToCollection(exception);
			target.AddWarningToCollection(warning);

			Assert.AreEqual(isValidLicenseFound, target.IsValidLicenseFound);
            Assert.AreEqual(1, target.ExceptionCollection.Count);
            Assert.AreEqual(1, target.WarningCollection.Count);
        }

        [Test]
        public void VerifySetValidLicenseFound()
        {
			const bool isValidLicenseFound = false;
			var exception = new FaultDto("ProductActivationKeyIsInvalidPerhapsForgedPleaseApplyANewOne");
			var warning = new FaultDto("YourProductActivationKeyWillExpireDoNotForgetToRenewItInTime");

			var target = new LicenseVerificationResultDto(isValidLicenseFound);
			target.AddExceptionToCollection(exception);
			target.AddWarningToCollection(warning);

			target.SetValidLicenseFoundTrue();
            Assert.IsTrue(target.IsValidLicenseFound);
        }
    }
}
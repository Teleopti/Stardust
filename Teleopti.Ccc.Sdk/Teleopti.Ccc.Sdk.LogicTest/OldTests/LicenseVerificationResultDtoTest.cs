#region Imports

using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

#endregion

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class LicenseVerificationResultDtoTest
    {
        private LicenseVerificationResultDto _target;
        private bool _isValidLicenseFound;

        [SetUp]
        public void Setup()
        {
            _isValidLicenseFound = false;
			FaultDto exception = new FaultDto("ProductActivationKeyIsInvalidPerhapsForgedPleaseApplyANewOne");
			FaultDto warning = new FaultDto("YourProductActivationKeyWillExpireDoNotForgetToRenewItInTime");

            _target = new LicenseVerificationResultDto(_isValidLicenseFound);
            _target.AddExceptionToCollection(exception);
            _target.AddWarningToCollection(warning);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_isValidLicenseFound, _target.IsValidLicenseFound);
            Assert.AreEqual(1, _target.ExceptionCollection.Count);
            Assert.AreEqual(1, _target.WarningCollection.Count);
        }

        [Test]
        public void VerifySetValidLicenseFound()
        {
            _target.SetValidLicenseFoundTrue();
            Assert.IsTrue(_target.IsValidLicenseFound);
        }
    }
}
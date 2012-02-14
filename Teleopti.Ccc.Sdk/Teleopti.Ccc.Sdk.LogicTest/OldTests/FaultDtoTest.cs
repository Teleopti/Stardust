#region Imports

using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

#endregion

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class FaultDtoTest
    {
        private FaultDto _target;
        private string _message;

        [SetUp]
        public void Setup()
        {
            _message = "LicenseIsInvalidPerhapsForgedPleaseApplyANewOne";
            _target = new FaultDto(_message);
        }

        /// <summary>
        /// Verifies the properties.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/24/2008
        /// </remarks>
        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_message, _target.Message);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    [TestFixture]
    public class LicenseSchemaTest
    {
        private IEnumerable<LicenseOption> _licenseList;
        private LicenseSchema _target;

        [SetUp]
        public void Setup()
        {
            _target = LicenseDataFactory.CreateDefaultActiveLicenseSchemaForTest();
            _licenseList = _target.LicenseOptions;
        }

        [Test]
        public void TestDefinedLicenseOptionsProperty()
        {
            // every option holds an empty list
            foreach (LicenseOption licenseOption in _licenseList)
            {
                Assert.AreEqual(0, licenseOption.EnabledApplicationFunctions.Count);
            }
        }

        [Test]
        public void VerifyEnabledLicenseOptions()
        {
            // by default
            int expected = 1;
            int result = _target.EnabledLicenseOptions.Count();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void VerifyEnabledLicenseOptionPath()
        {
            // by default
            int expected = 1;
            int result = _target.EnabledLicenseOptionPaths.Count;

            Assert.AreEqual(expected, result);
        }
    }
}

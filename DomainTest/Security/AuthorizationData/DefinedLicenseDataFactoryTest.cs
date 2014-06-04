using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationData
{
    [TestFixture]
    public class DefinedLicenseDataFactoryTest
    {
        private MockRepository _mocks;
        private ILicenseActivator _licenseActivator;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _licenseActivator = _mocks.StrictMock<ILicenseActivator>();
        }

        [Test]
        public void VerifyLicenseActivator()
        {
            DefinedLicenseDataFactory.SetLicenseActivator("asdf", _licenseActivator);
            Assert.AreEqual(_licenseActivator, DefinedLicenseDataFactory.GetLicenseActivator("asdf"));
        }

        [Test]
        public void VerifyCreateActiveLicenseSchema()
        {
            const string enabledLicenseSchemaName = "SchemaName";
            const string enabledLicenseOptionPath = "root/name";
            using(_mocks.Record())
            {
                Expect.Call(_licenseActivator.EnabledLicenseSchemaName)
                    .Return(enabledLicenseSchemaName);
                Expect.Call(_licenseActivator.EnabledLicenseOptionPaths)
                    .Return(new List<string> {enabledLicenseOptionPath});
            }
            using (_mocks.Playback())
            {
                DefinedLicenseDataFactory.SetLicenseActivator("asdf", _licenseActivator);
                LicenseSchema licenseSchema = DefinedLicenseDataFactory.CreateActiveLicenseSchema("asdf");
                
                Assert.IsNotNull(licenseSchema);

            }
        }

    }
}

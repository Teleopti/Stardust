using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Secrets.Licensing;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationData
{
    [TestFixture]
    public class DefinedLicenseDataFactoryTest
    {
        [Test]
        public void VerifyLicenseActivator()
        {
			var licenseActivator = MockRepository.GenerateMock<ILicenseActivator>();
            DefinedLicenseDataFactory.SetLicenseActivator("asdf", licenseActivator);
            Assert.AreEqual(licenseActivator, DefinedLicenseDataFactory.GetLicenseActivator("asdf"));
        }

	    [Test]
	    public void VerifyCreateActiveLicenseSchema()
	    {
		    var licenseActivator = MockRepository.GenerateMock<ILicenseActivator>();
		    const string enabledLicenseSchemaName = "SchemaName";
		    const string enabledLicenseOptionPath = "root/name";

		    licenseActivator.Stub(x => x.EnabledLicenseSchemaName).Return(enabledLicenseSchemaName);
		    licenseActivator.Stub(x => x.EnabledLicenseOptionPaths).Return(new List<string> {enabledLicenseOptionPath});

		    DefinedLicenseDataFactory.SetLicenseActivator("asdf", licenseActivator);
		    LicenseSchema licenseSchema = DefinedLicenseDataFactory.CreateActiveLicenseSchema("asdf");

		    Assert.IsNotNull(licenseSchema);
	    }

	    [Test]
		public void VerifyCreateActiveLicenseSchemaWithNoLicenseAvailableShouldThrowLicenseMissingException()
		{
			DefinedLicenseDataFactory.SetLicenseActivator("asdf", null);
			Assert.Throws<LicenseMissingException>(()=> DefinedLicenseDataFactory.CreateActiveLicenseSchema("asdf"));
		}
    }
}

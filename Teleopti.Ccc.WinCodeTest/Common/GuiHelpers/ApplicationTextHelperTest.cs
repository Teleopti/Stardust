using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common.GuiHelpers
{
    [TestFixture, SetUICulture("en-US")]
    public class ApplicationTextHelperTest
    {
        private string _expectedLicenseText;
        private string _expectedLoggedOnUserText;

        [SetUp]
        public void Setup()
        {
            _expectedLicenseText = "Licensed to: Teleopti";
            _expectedLoggedOnUserText = "Logged on user: Kalle K";
        }

        [Test]
        public void VerifyReturnLicensedToString()
        {
            DefinedLicenseDataFactory.SetLicenseActivator(((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).DataSource.DataSourceName, new LicenseActivator("Teleopti", new DateTime(2009, 11, 1), 1000, 100, LicenseType.Agent, new Percent(0.1),
								XmlLicenseService.IsThisAlmostTooManyActiveAgents, LicenseActivator.IsThisTooManyActiveAgents));
            
            string returnText = ApplicationTextHelper.LicensedToCustomerText;
            Assert.AreEqual(_expectedLicenseText, returnText);
        }

        [Test]
        public void VerifyReturnLoggedOnUserString()
        {
            ((IUnsafePerson)TeleoptiPrincipal.Current).Person.Name = new Name("Kalle","K");

            string returnText = ApplicationTextHelper.LoggedOnUserText;
            Assert.AreEqual(_expectedLoggedOnUserText, returnText);
        }
    }

}

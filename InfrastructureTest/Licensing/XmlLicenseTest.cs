using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
    [TestFixture]
    [Category("LongRunning")]
    public class XmlLicenseTest
    {
        private const string folder = "Licensing\\";
        private const string teleoptiCccUnsignedLicenseFileName = folder + "TeleoptiCCCUnsignedLicense.xml";

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifySimpleExtraction()
        {
            Assert.IsTrue(File.Exists(XmlLicenseTestSetupFixture.PublicKeyFileName));
        }

        /// <summary>
        /// Verifies that public key extraction works. NOTE: This test does not work if you are not admin!
        /// </summary>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-11-24
        /// </remarks>
        [Test(
            Description =
                "Verifies that public key extraction works. NOTE: This test does not work if you are not admin, because it implicitly assumes that you can create a CSP container!"
            )]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void VerifyStandardExtraction()
        {
            string tempKeyName = Path.GetTempFileName();
            int exitCode = XmlLicense.ExtractPublicKey(tempKeyName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);
            Assert.IsTrue(File.Exists(tempKeyName));
            File.Delete(tempKeyName);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifySignErrorWhenFileExists()
        {
            int exitCode = XmlLicense.Sign(null, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
            Assert.AreEqual((int) ExitCode.FileNotFound, exitCode);
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void VerifySigning()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(teleoptiCccUnsignedLicenseFileName);

            int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
            Assert.AreEqual((int) ExitCode.Success, exitCode);
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void VerifySignature()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(teleoptiCccUnsignedLicenseFileName);

            int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            XDocument signedXml;
            using (var reader = new XmlNodeReader(doc))
            {
                signedXml = XDocument.Load(reader);
            }

            IXmlLicense xmlLicense = new XmlLicense(signedXml, XmlLicenseTestSetupFixture.PublicKeyXmlString);
            Assert.AreEqual("This license is stolen!", xmlLicense.CustomerName);
            Assert.AreEqual("TeleoptiCCC", xmlLicense.SchemaName);
        }

        /// <summary>
        /// Verifies the signature invalid when the singing with the wrong key.
        /// </summary>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-11-24
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test(
            Description =
                "Verifies the signature invalid when the singing with the wrong key. NOTE: This test does not work if you are not admin, because it implicitly assumes that you can create a CSP container!"
            )]
        public void VerifySignatureInvalidWhenWrongSingingKey()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(teleoptiCccUnsignedLicenseFileName);

            int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore("nonexistantcontainer"));
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            XDocument signedXml;
            using (var reader = new XmlNodeReader(doc))
            {
                signedXml = XDocument.Load(reader);
            }
            bool exceptionThrown = false;
            try
            {
                new XmlLicense(signedXml, XmlLicenseTestSetupFixture.PublicKeyXmlString);
            }
            catch (SignatureValidationException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifySignatureInvalidWhenModified()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(teleoptiCccUnsignedLicenseFileName);

            int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            XDocument signedXml;
            using (var reader = new XmlNodeReader(doc))
            {
                signedXml = XDocument.Load(reader);
            }

            XElement maxActiveAgentsElement = signedXml.XPathSelectElement("License/MaxActiveAgents");
            Assert.AreEqual("10000", maxActiveAgentsElement.Value);
            string fraudulentValue = "20000";
            maxActiveAgentsElement.ReplaceNodes(fraudulentValue);
            Assert.AreEqual(fraudulentValue, maxActiveAgentsElement.Value);

            bool exceptionThrown = false;
            try
            {
                new XmlLicense(signedXml, XmlLicenseTestSetupFixture.PublicKeyXmlString);
            }
            catch (SignatureValidationException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test, ExpectedException(typeof (SignatureValidationException))]
        public void VerifySignatureInvalidWhenMissing()
        {
            XDocument unsignedLicense = XDocument.Load(teleoptiCccUnsignedLicenseFileName);

            new XmlLicense(unsignedLicense, XmlLicenseTestSetupFixture.PublicKeyXmlString);
        }
    }
}
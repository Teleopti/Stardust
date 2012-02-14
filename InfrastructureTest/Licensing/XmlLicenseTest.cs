#region Imports

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Licensing;

#endregion

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
    [TestFixture]
    [Category("LongRunning")]
    public class XmlLicenseTest
    {
        private const string folder = "Licensing\\";
        private const string teleoptiCccUnsignedLicenseFileName = folder + "TeleoptiCCCUnsignedLicense.xml";
        private const string teleoptiCccLicenseFileName = "TeleoptiCCCLicense.xml";

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
            string nonExistentFile = Path.GetRandomFileName();
            int exitCode = XmlLicense.Sign(nonExistentFile, XmlLicenseTestSetupFixture.TestKeyContainterName,
                                           Path.GetRandomFileName());
            Assert.AreEqual((int) ExitCode.FileNotFound, exitCode);
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void VerifySigning()
        {
            int exitCode = XmlLicense.Sign(teleoptiCccUnsignedLicenseFileName,
                                           XmlLicenseTestSetupFixture.TestKeyContainterName,
                                           teleoptiCccLicenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);
            File.Delete(teleoptiCccLicenseFileName);
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void VerifySignature()
        {
            int exitCode = XmlLicense.Sign(teleoptiCccUnsignedLicenseFileName,
                                           XmlLicenseTestSetupFixture.TestKeyContainterName,
                                           teleoptiCccLicenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            XDocument signedXml = XDocument.Load(teleoptiCccLicenseFileName);
            IXmlLicense xmlLicense = new XmlLicense(signedXml, XmlLicenseTestSetupFixture.PublicKeyXmlString);
            Assert.AreEqual("This license is stolen!", xmlLicense.CustomerName);
            Assert.AreEqual("TeleoptiCCC", xmlLicense.SchemaName);

            File.Delete(teleoptiCccLicenseFileName);
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
            int exitCode = XmlLicense.Sign(teleoptiCccUnsignedLicenseFileName, "nonexistantcontainer",
                                           teleoptiCccLicenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            XDocument signedXml = XDocument.Load(teleoptiCccLicenseFileName);
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

            File.Delete(teleoptiCccLicenseFileName);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifySignatureInvalidWhenModified()
        {
            int exitCode = XmlLicense.Sign(teleoptiCccUnsignedLicenseFileName,
                                           XmlLicenseTestSetupFixture.TestKeyContainterName,
                                           teleoptiCccLicenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            XDocument signedXml = XDocument.Load(teleoptiCccLicenseFileName);

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

            File.Delete(teleoptiCccLicenseFileName);
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
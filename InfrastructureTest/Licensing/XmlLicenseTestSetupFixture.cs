#region Imports

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;

#endregion

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
    [SetUpFixture]
    public class XmlLicenseTestSetupFixture
    {
        internal const string TestKeyContainterName = "TeleoptiTestKey";
        private static readonly string _publicKeyFileName = Path.GetTempFileName();
        private static string _publicKeyXmlString;

        internal static string PublicKeyFileName
        {
            get { return _publicKeyFileName; }
        }

        internal static string PublicKeyXmlString
        {
            get { return _publicKeyXmlString; }
        }

        [SetUp]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void Setup()
        {
            File.Delete(_publicKeyFileName);
            FileStream fs = File.Create(_publicKeyFileName);
            fs.Close();
            int exitCode = XmlLicense.ExtractPublicKey(TestKeyContainterName, _publicKeyFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            // Get the XML content from the embedded XML public key.
            // Read-in the XML content.
            using(var reader = new StreamReader(_publicKeyFileName))
            {
                _publicKeyXmlString = reader.ReadToEnd();
                reader.Close();                
            }
        }

        [TearDown]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void Teardown()
        {
            File.Delete(_publicKeyFileName);

            DeleteKeyContainer(TestKeyContainterName);
        }

        private static void DeleteKeyContainer(string keyContainerName)
        {
            CspParameters parms = new CspParameters(1); // PROV_RSA_FULL
            parms.Flags = CspProviderFlags.UseMachineKeyStore; // Use Machine store
            parms.KeyContainerName = keyContainerName; // container
            parms.KeyNumber = 2; // AT_SIGNATURE

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(parms);
            csp.PersistKeyInCsp = false;
            csp.Clear();
        }
    }
}
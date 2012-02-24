using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
    public class LicenseStatusXml : ILicenseStatusXml
    {
        private XmlDocument _xdoc;

        public LicenseStatusXml(){}

        public LicenseStatusXml(XDocument licenseStatus)
        {
            CheckSignature(licenseStatus);
            Load(licenseStatus);
        }


        private void GetNewStatusDocument()
        {
            IFormatProvider invariant = CultureInfo.InvariantCulture;
            _xdoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = _xdoc.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlElement rootNode = _xdoc.CreateElement("LicenseStatus");
            _xdoc.InsertBefore(xmlDeclaration, _xdoc.DocumentElement);
            _xdoc.AppendChild(rootNode);
            rootNode.AppendChild(_xdoc.CreateElement("CheckDate")).AppendChild(_xdoc.CreateTextNode(CheckDate.ToString("s", invariant)));
            rootNode.AppendChild(_xdoc.CreateElement("StatusOk")).AppendChild(_xdoc.CreateTextNode(StatusOk.ToString(invariant)));
            rootNode.AppendChild(_xdoc.CreateElement("LastValidDate")).AppendChild(_xdoc.CreateTextNode(LastValidDate.AddDays(1).ToString("s", invariant)));
            SignStatus(_xdoc);
        }

        private void Load(XDocument license)
        {
            XElement root = license.Root;
            CheckDate = (DateTime)root.Element("CheckDate");
            LastValidDate = (DateTime)root.Element("LastValidDate");
            StatusOk = (bool)root.Element("StatusOk");
        }

        public DateTime CheckDate { get; set; }
        public bool StatusOk { get; set; }
        public DateTime LastValidDate { get; set; }

        public XmlDocument XmlDocument
        {
            get
            {
                GetNewStatusDocument();
                return _xdoc;
            }
        }

        private static void SignStatus(XmlDocument xdoc)
        {
            var parms = new CspParameters(1)
            {
                Flags = CspProviderFlags.UseMachineKeyStore,
                KeyContainerName = XmlLicense.ContainerName,
                KeyNumber = 2
            };

            using (var csp = new RSACryptoServiceProvider(parms))
            {
                var sxml = new SignedXml(xdoc) { SigningKey = csp };

                sxml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigCanonicalizationUrl;

                var r = new Reference("");
                r.AddTransform(new XmlDsigEnvelopedSignatureTransform(false));

                sxml.AddReference(r);

                sxml.ComputeSignature();

                XmlElement sig = sxml.GetXml();
                xdoc.DocumentElement.AppendChild(sig);
            }

        }

        private static void CheckSignature(XNode licenseStatus)
        {
            // Create an RSA crypto service provider from the embedded
            // XML document resource (the public key).
            var parms = new CspParameters(1)
            {
                Flags = CspProviderFlags.UseMachineKeyStore,
                KeyContainerName = XmlLicense.ContainerName,
                KeyNumber = 2
            };
            var cryptoServiceProvider = new RSACryptoServiceProvider(parms);
            //cryptoServiceProvider.FromXmlString(publicKeyXmlContent);

            // Load the signed XML license file.
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(licenseStatus.CreateReader());

            // Create the signed XML object.
            var signedXml = new SignedXml(xmlDocument);

            // Get the XML Signature node and load it into the signed XML object.
            XmlNode signature = xmlDocument.GetElementsByTagName("Signature",
                                                                 SignedXml.XmlDsigNamespaceUrl)[0];

            if (signature == null)
                throw new SignatureValidationException("Signature may be missing from license");

            signedXml.LoadXml((XmlElement)signature);

            // Verify the signature.
            if (!signedXml.CheckSignature(cryptoServiceProvider))
            {
                throw new SignatureValidationException();
            }
            cryptoServiceProvider.Clear();
        }
    }
}
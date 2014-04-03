using System;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Teleopti.Ccc.Obfuscated.Security;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
    public class LicenseStatusXml : ILicenseStatusXml
    {
        public LicenseStatusXml(){}

        public LicenseStatusXml(XDocument licenseStatus)
        {
            XmlLicense.CheckSignature(licenseStatus,LicenseStatusConstants.Public);
            Load(licenseStatus);
        }

        public XmlDocument GetNewStatusDocument()
        {
            IFormatProvider invariant = CultureInfo.InvariantCulture;
            var xdoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xdoc.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlElement rootNode = xdoc.CreateElement("LicenseStatus");
            xdoc.InsertBefore(xmlDeclaration, xdoc.DocumentElement);
            xdoc.AppendChild(rootNode);
            rootNode.AppendChild(xdoc.CreateElement("CheckDate")).AppendChild(xdoc.CreateTextNode(CheckDate.ToString("s", invariant)));
            rootNode.AppendChild(xdoc.CreateElement("StatusOk")).AppendChild(xdoc.CreateTextNode(StatusOk.ToString(invariant)));
            rootNode.AppendChild(xdoc.CreateElement("LastValidDate")).AppendChild(xdoc.CreateTextNode(LastValidDate.AddDays(1).ToString("s", invariant)));
            rootNode.AppendChild(xdoc.CreateElement("AlmostTooMany")).AppendChild(xdoc.CreateTextNode(AlmostTooMany.ToString(invariant)));
            rootNode.AppendChild(xdoc.CreateElement("NumberOfActiveAgents")).AppendChild(xdoc.CreateTextNode(NumberOfActiveAgents.ToString(invariant)));
            rootNode.AppendChild(xdoc.CreateElement("DaysLeft")).AppendChild(xdoc.CreateTextNode(DaysLeft.ToString(invariant)));
            
			XmlLicense.Sign(xdoc,new CryptoSettingsFromXml(LicenseStatusConstants.Private));

        	return xdoc;
        }

        private void Load(XDocument license)
        {
            XElement root = license.Root;
            CheckDate = (DateTime)root.Element("CheckDate");
            LastValidDate = (DateTime)root.Element("LastValidDate");
            StatusOk = (bool)root.Element("StatusOk");
            if(root.Element("AlmostTooMany") != null)
                AlmostTooMany = (bool)root.Element("AlmostTooMany");
            if (root.Element("NumberOfActiveAgents") != null)
                NumberOfActiveAgents = (int)root.Element("NumberOfActiveAgents");
            if (root.Element("DaysLeft") != null)
                DaysLeft = (int)root.Element("DaysLeft");
        }

        public DateTime CheckDate { get; set; }
        public bool StatusOk { get; set; }
        public DateTime LastValidDate { get; set; }
        public bool AlmostTooMany { get; set; }
        public int NumberOfActiveAgents { get; set; }
        public int DaysLeft { get; set; }
    }
}
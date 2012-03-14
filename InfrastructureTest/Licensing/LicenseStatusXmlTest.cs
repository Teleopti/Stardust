using System;
using System.Xml.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
	[TestFixture]
	public class LicenseStatusXmlTest
	{
		private ILicenseStatusXml _target;
	    private XDocument _signedXml;

		[SetUp]
		public void Setup()
		{
            _target = new LicenseStatusXml{CheckDate = new DateTime(2012,2,24), LastValidDate = new DateTime(2012,2,25),StatusOk = true};

		    var xml = _target.XmlDocument;
            _signedXml = XDocument.Parse(xml.InnerXml);
            _target = new LicenseStatusXml(_signedXml);
		}

		[Test]
		public void ShouldProvideNewSignedDocument()
		{
            Assert.That(_target.XmlDocument.GetElementsByTagName("LicenseStatus"), Is.Not.Empty);
            Assert.That(_target.XmlDocument.GetElementsByTagName("StatusOk")[0].InnerXml, Is.EqualTo("True"));
            Assert.That(_target.XmlDocument.GetElementsByTagName("Signature"), Is.Not.Empty);
            Assert.That(_target.XmlDocument.GetElementsByTagName("SignatureValue"), Is.Not.Empty);
		}

        [Test]
        public void ShouldBePossibleToResignDocument()
        {
            var signNode = _target.XmlDocument.GetElementsByTagName("SignatureValue");

            var newSignNode = _target.XmlDocument.GetElementsByTagName("SignatureValue");
            Assert.That(signNode[0].InnerXml, Is.EqualTo(newSignNode[0].InnerXml));
            _target.LastValidDate = new DateTime(2012,3,15);
            _target.StatusOk = false;

            newSignNode = _target.XmlDocument.GetElementsByTagName("SignatureValue");

            Assert.That(signNode[0].InnerXml, Is.Not.EqualTo(newSignNode[0].InnerXml));

        }

        [Test]
        public void ShouldBeAbleToReadTheDocument()
        {

            var status =  new LicenseStatusXml(XDocument.Parse(_target.XmlDocument.OuterXml));
            Assert.That(status.StatusOk, Is.True);
        }
	}

	
}
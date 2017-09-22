using System;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Licensing;

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

		    var xml = _target.GetNewStatusDocument();

			using (var reader = new XmlNodeReader(xml))
			{
				_signedXml = XDocument.Load(reader);
			}
            _target = new LicenseStatusXml(_signedXml);
		}

		[Test]
		public void ShouldProvideNewSignedDocument()
		{
			var document = _target.GetNewStatusDocument();
            Assert.That(document.GetElementsByTagName("LicenseStatus"), Is.Not.Empty);
            Assert.That(document.GetElementsByTagName("StatusOk")[0].InnerXml, Is.EqualTo("True"));
            Assert.That(document.GetElementsByTagName("Signature"), Is.Not.Empty);
            Assert.That(document.GetElementsByTagName("SignatureValue"), Is.Not.Empty);
		}

        [Test]
        public void ShouldBePossibleToResignDocument()
        {
            var signNode = _target.GetNewStatusDocument().GetElementsByTagName("SignatureValue");

            var newSignNode = _target.GetNewStatusDocument().GetElementsByTagName("SignatureValue");
            Assert.That(signNode[0].InnerXml, Is.EqualTo(newSignNode[0].InnerXml));
            _target.LastValidDate = new DateTime(2012,3,15);
            _target.StatusOk = false;

            newSignNode = _target.GetNewStatusDocument().GetElementsByTagName("SignatureValue");

            Assert.That(signNode[0].InnerXml, Is.Not.EqualTo(newSignNode[0].InnerXml));
        }

        [Test]
        public void ShouldBeAbleToReadTheDocument()
        {
			using (var reader = new XmlNodeReader(_target.GetNewStatusDocument()))
			{
				var status = new LicenseStatusXml(XDocument.Load(reader));
				Assert.That(status.StatusOk, Is.True);
        	}
        }
	}
}
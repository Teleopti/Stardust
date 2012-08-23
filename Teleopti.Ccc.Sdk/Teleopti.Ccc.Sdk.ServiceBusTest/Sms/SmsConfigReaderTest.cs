using System.Xml;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.ServiceBus.SMS;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Sms
{
	[TestFixture]
	public class SmsConfigReaderTest
	{
		private SmsConfigReader _target;
		private string _emptyDoc = "";
		

		[SetUp]
		public void Setup()
		{
			_target = new SmsConfigReader("SmsConfig.xml.notinuse");
			_emptyDoc = @"<?xml version='1.0' encoding='utf-8' ?>
					<Config>
					</Config>";
		}

		[Test]
		public void ShouldLoadTheFile()
		{
			Assert.That(_target.HasLoadedConfig , Is.True);
		}

		[Test]
		public void ShouldNotLoadTheFile()
		{
			_target = new SmsConfigReader("SmsConfig.xml");
			Assert.That(_target.HasLoadedConfig, Is.False);
			Assert.That(_target.XmlDocument, Is.Null);
			Assert.That(_target.User, Is.EqualTo(""));
			Assert.That(_target.Password, Is.EqualTo(""));
			Assert.That(_target.Url, Is.EqualTo(""));
			Assert.That(_target.Api, Is.EqualTo(""));
			Assert.That(_target.From, Is.EqualTo(""));
			Assert.That(_target.Data, Is.EqualTo(""));
		}

		[Test]
		public void ShouldHaveEmptyPropertiesIfConfigIsEmpty()
		{
			var doc = new XmlDocument();
			doc.LoadXml(_emptyDoc);

			_target = new SmsConfigReader(doc);
			Assert.That(_target.HasLoadedConfig, Is.True);
			Assert.That(_target.XmlDocument, Is.Not.Null);
			Assert.That(_target.User, Is.EqualTo(""));
			Assert.That(_target.Password, Is.EqualTo(""));
			Assert.That(_target.Url, Is.EqualTo(""));
		}

		[Test]
		public void ShouldContainTheXmlDocument()
		{
			Assert.That(_target.XmlDocument, Is.Not.Null);
		}

		[Test]
		public void ShouldHaveAnUrlProperty()
		{
			Assert.That(_target.Url, Is.EqualTo("http://api.clickatell.com/xml/xml?data="));
		}

		[Test]
		public void ShouldHaveAPasswordProperty()
		{
			Assert.That(_target.Password, Is.EqualTo("pwd"));
		}

		[Test]
		public void ShouldHaveAnUserProperty()
		{
			Assert.That(_target.User, Is.EqualTo("username"));
		}
		
		[Test]
		public void ShouldHaveAnApiProperty()
		{
			Assert.That(_target.Api, Is.EqualTo("3388480"));
		}

		[Test]
		public void ShouldHaveAClassProperty()
		{
			Assert.That(_target.Class, Is.EqualTo("Teleopti.Ccc.Sdk.ServiceBus.SMS.ClickatellSmsSender"));
		}

		[Test]
		public void ShouldHaveAFromProperty()
		{
			Assert.That(_target.From, Is.EqualTo("from"));
		}

		[Test]
		public void ShouldHaveADataProperty()
		{
			Assert.That(_target.Data, Is.Not.Empty);
		}
	}

	
}
using System;
using System.Xml;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Notification
{
	[TestFixture]
	public class NotificationConfigReaderTest
	{
		private NotificationConfigReader _target;
		private string _emptyDoc = "";
		

		[SetUp]
		public void Setup()
		{
			_target = new NotificationConfigReader("NotificationConfig.xml.notinuse");
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
			_target = new NotificationConfigReader();
			Assert.That(_target.HasLoadedConfig, Is.False);
			Assert.That(_target.XmlDocument, Is.Null);
			Assert.That(_target.User, Is.EqualTo(""));
			Assert.That(_target.Password, Is.EqualTo(""));
			Assert.That(_target.Url, Is.Null);
			Assert.That(_target.Api, Is.EqualTo(""));
			Assert.That(_target.From, Is.EqualTo(""));
			Assert.That(_target.Data, Is.EqualTo(""));
			Assert.That(_target.ClassName, Is.EqualTo(""));
		}

		[Test]
		public void ShouldHaveEmptyPropertiesIfConfigIsEmpty()
		{
			var doc = new XmlDocument();
			doc.LoadXml(_emptyDoc);

			_target = new NotificationConfigReader(doc);
			Assert.That(_target.HasLoadedConfig, Is.True);
			Assert.That(_target.XmlDocument, Is.Not.Null);
			Assert.That(_target.User, Is.EqualTo(""));
			Assert.That(_target.Password, Is.EqualTo(""));
			Assert.That(_target.Url, Is.Null);
			Assert.That(_target.Api, Is.EqualTo(""));
			Assert.That(_target.From, Is.EqualTo(""));
			Assert.That(_target.Data, Is.EqualTo(""));
			Assert.That(_target.ClassName, Is.EqualTo(""));
		}

		[Test]
		public void ShouldContainTheXmlDocument()
		{
			Assert.That(_target.XmlDocument, Is.Not.Null);
		}

		[Test]
		public void ShouldHaveAnUrlProperty()
		{
			Assert.That(_target.Url, Is.EqualTo(new Uri("http://api.clickatell.com/xml/xml?data=")));
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
			Assert.That(_target.Api, Is.EqualTo("3388822"));
		}

		[Test]
		public void ShouldHaveAnAssemblyProperty()
		{
			Assert.That(_target.Assembly, Is.EqualTo("Teleopti.Ccc.Sdk.Notification"));
		}

		[Test]
		public void ShouldHaveAClassProperty()
		{
			Assert.That(_target.ClassName, Is.EqualTo("Teleopti.Ccc.Sdk.Notification.ClickatellNotificationSender"));
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

		[Test]
		public void ShouldHaveAFindSuccessOrErrorProperty()
		{
			Assert.That(_target.FindSuccessOrError, Is.Not.Empty);
		}

		[Test]
		public void ShouldHaveAErrorCodeProperty()
		{
			Assert.That(_target.ErrorCode, Is.Not.Empty);
		}

		[Test]
		public void ShouldHaveASuccessCodeProperty()
		{
			Assert.That(_target.SuccessCode, Is.Not.Empty);
		}

		[Test]
		public void ShouldHaveASkipProperty()
		{
			Assert.That(_target.SkipSearch, Is.False);
		}
	}

	
}
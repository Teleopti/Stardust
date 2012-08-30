﻿using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Notification
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Clickatell"), TestFixture]
	public class ClickatellNotificationSenderTest
	{
		// the send will not work in thia test because of the password is wrong
		// we don't want to send a lot of messages all the time :-) 
		// if you want to try change to <password>cadadi01</password> (works as long as we have credits) <from>{3}</from>
		private MockRepository _mocks;
		private INotificationConfigReader _notificationConfigReader;
		private ClickatellNotificationSender _target;
		private INotificationMessage smsMessage = new NotificationMessage(){Subject = "Schedule has changed"};

		private const string xml = @"<?xml version='1.0' encoding='utf-8' ?>
<Config>
	<class>Teleopti.Ccc.Sdk.ServiceBus.SMS.ClickatellNotificationSender</class>
	<url>http://api.clickatell.com/xml/xml?data=</url>
	<user>ola.hakansson@teleopti.com</user>
	<password>cadadi02</password>
	<api_id>3388822</api_id>
	<from>TeleoptiCCC</from>
	<data>
		<![CDATA[ <clickAPI><sendMsg>
		<api_id>3388822</api_id>
		<user>{0}</user>
		<password>{1}</password>
		<to>{2}</to>
		<from>{3}</from>
		<text>{4}</text>
		</sendMsg></clickAPI>]]>
	</data>
</Config>";

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_notificationConfigReader = _mocks.StrictMock<INotificationConfigReader>();
			_target = new ClickatellNotificationSender();
			_target.SetConfigReader(_notificationConfigReader);
		}

		[Test]
		public void ShouldNotSendIfNoConfig()
		{
			Expect.Call(_notificationConfigReader.HasLoadedConfig).Return(false);
			_mocks.ReplayAll();
			_target.SendNotification(smsMessage,"" );
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldTryToSendIfConfig()
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);

			_notificationConfigReader = new NotificationConfigReader(doc);
			_target.SetConfigReader(_notificationConfigReader);
			_target.SendNotification(smsMessage, "46709218108");
		}

		[Test]
		public void ShouldLogIfUrlIsIncorrect()
		{
			const string incorrectXml = @"<?xml version='1.0' encoding='utf-8' ?><Config>
	<class>Teleopti.Ccc.Sdk.ServiceBus.SMS.ClickatellNotificationSender</class>
	<url>http://api.sticktohell.com/xml/xml?data=</url>
	<user>ola.hakansson@teleopti.com</user>
	<password>cadadi02</password>
	<api_id>3388480</api_id>
	<from>Teleopti CCC</from>
	<data>
		<![CDATA[ <clickAPI><sendMsg>
		<api_id>3388480</api_id>
		<user>{0}</user>
		<password>{1}</password>
		<to>{2}</to>
		<from>{3}</from>
		<text>{4}</text>
		</sendMsg></clickAPI>]]>
	</data>
</Config>";
			var doc = new XmlDocument();
			doc.LoadXml(incorrectXml);

			_notificationConfigReader = new NotificationConfigReader(doc);
			_target.SetConfigReader(_notificationConfigReader);
			_target.SendNotification(smsMessage, "46709218108");
		}
	}
}
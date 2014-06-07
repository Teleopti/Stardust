using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Notification;
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
		private INotificationMessage smsMessage = new NotificationMessage() { Subject = "Schedule has changed" };

		private const string xml = @"<?xml version='1.0' encoding='utf-8' ?>
<Config>
	<class>Teleopti.Ccc.Sdk.Notification.ClickatellNotificationSender</class>
	<url>http://api.clickatell.com/xml/xml?data=</url>
	<user>ola.hakansson@teleopti.com</user>
	<password>cadadi02</password>
	<api_id>3388822</api_id>
	<from>TeleoptiCCC</from>
	<FindSuccessOrError>Error</FindSuccessOrError>
	<ErrorCode>fault</ErrorCode>
	<SuccessCode>success</SuccessCode>
	<SkipSearch>false</SkipSearch>
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
			smsMessage.Messages.Add("On a day");
			Expect.Call(_notificationConfigReader.HasLoadedConfig).Return(false);
			_mocks.ReplayAll();
			_target.SendNotification(smsMessage, "");
			_mocks.VerifyAll();
		}

		[Test, ExpectedException(typeof(SendNotificationException))]
		public void ShouldTryToSendIfConfig()
		{
			smsMessage.Messages.Add("test1");
			var doc = new XmlDocument();
			doc.LoadXml(xml);

			_notificationConfigReader = new NotificationConfigReader(doc);
			_target.SetConfigReader(_notificationConfigReader);
			_target.SendNotification(smsMessage, "46709218108");
		}

	   [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms"), Test]
        public void ShouldSplitMessageIfGreaterThanMaxSmsLength()
		{
		    INotificationMessage msg = new NotificationMessage();
		    msg.Subject = "Your Working Hours have changed";
            msg.Messages.Add("Monday 2012-01-01 08:00-17:00");
            msg.Messages.Add("Tuesday 2012-01-02 08:00-16:00");
            msg.Messages.Add("Wedneday 2012-01-03 08:00-16:00");
            msg.Messages.Add("Thrusday 2012-01-04 08:00-16:00");
            msg.Messages.Add("Friday 2012-01-05 08:00-16:00");
            msg.Messages.Add("Monday 2012-01-08 Not Working");
            
            IList<string> messages = _target.GetSmsMessagesToSend(msg, false);
            Assert.That(messages.Count, Is.EqualTo(2));
		}

		[Test]
		public void ShouldSplitMessageIfGreaterThanMaxSmsLengthForUnicode()
		{
			INotificationMessage msg = new NotificationMessage();
			msg.Subject = "您的工作小时数已经改变：";
			msg.Messages.Add("星期一 2012-01-01 08:00-17:00");
			msg.Messages.Add("星期二 2012-01-02 08:00-16:00");
			msg.Messages.Add("星期三 2012-01-03 08:00-16:00");
			msg.Messages.Add("星期四 2012-01-04 08:00-16:00");
			msg.Messages.Add("星期五 2012-01-05 08:00-16:00");
			msg.Messages.Add("星期一 2012-01-08 不工作");

			IList<string> messages = _target.GetSmsMessagesToSend(msg, true);
			Assert.That(messages.Count, Is.EqualTo(3));
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

		[Test, Ignore("Fails randomly, bug #28329")]
		public void ShouldNotThrowIfSkipCheck()
		{
			const string xmlWithNoCheck = @"<?xml version='1.0' encoding='utf-8' ?>
<Config>
	<class>Teleopti.Ccc.Sdk.Notification.ClickatellNotificationSender</class>
	<url>http://api.clickatell.com/xml/xml?data=</url>
	<user>ola.hakansson@teleopti.com</user>
	<password>cadadi02</password>
	<api_id>3388822</api_id>
	<from>TeleoptiCCC</from>
	<FindSuccessOrError>Error</FindSuccessOrError>
	<ErrorCode>fault</ErrorCode>
	<SuccessCode>success</SuccessCode>
	<SkipSearch>true</SkipSearch>
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
			var doc = new XmlDocument();
			doc.LoadXml(xmlWithNoCheck);

			_notificationConfigReader = new NotificationConfigReader(doc);
			_target.SetConfigReader(_notificationConfigReader);
			_target.SendNotification(smsMessage, "46709218108");
		}
	

		[Test, ExpectedException(typeof(SendNotificationException))]
		public void ShouldSearchForSuccess()
		{
			const string xmlWithSuccessCheck = @"<?xml version='1.0' encoding='utf-8' ?>
<Config>
	<class>Teleopti.Ccc.Sdk.Notification.ClickatellNotificationSender</class>
	<url>http://api.clickatell.com/xml/xml?data=</url>
	<user>ola.hakansson@teleopti.com</user>
	<password>cadadi02</password>
	<api_id>3388822</api_id>
	<from>TeleoptiCCC</from>
	<FindSuccessOrError>Success</FindSuccessOrError>
	<ErrorCode>fault</ErrorCode>
	<SuccessCode>detgickbra</SuccessCode>
	<SkipSearch>false</SkipSearch>
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
			smsMessage.Messages.Add("test1");
			var doc = new XmlDocument();
			doc.LoadXml(xmlWithSuccessCheck);

			_notificationConfigReader = new NotificationConfigReader(doc);
			_target.SetConfigReader(_notificationConfigReader);
			_target.SendNotification(smsMessage, "46709218108");
		}
	}
}


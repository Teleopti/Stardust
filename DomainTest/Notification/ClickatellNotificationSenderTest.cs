using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Notification
{

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Clickatell"),
		TestFixture,
		DomainTest]
	public class ClickatellNotificationSenderTest : ISetup
	{
		// the send will not work in thia test because of the password is wrong
		// we don't want to send a lot of messages all the time :-) 
		// if you want to try change to <password>cadadi01</password> (works as long as we have credits) <from>{3}</from>
		public FakeNotificationConfigReader _notificationConfigReader;
		private ClickatellNotificationSender _target;
		private readonly INotificationMessage smsMessage = new NotificationMessage { Subject = "Schedule has changed" };
		private LogSpy _log;

		private const string xml = @"<?xml version='1.0' encoding='utf-8' ?>
		<Config>
			<class>Teleopti.Ccc.Domain.Notification.ClickatellNotificationSender</class>
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
				<user>{0}</user>
				<password>{1}</password>
				<api_id>{2}</api_id>
				<to>{3}</to>
				<from>{4}</from>
				<text>{5}</text>
				<unicode>{6}</unicode>
				</sendMsg></clickAPI>]]>
			</data>
		</Config>";

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			_log = new LogSpy();
			_target = new ClickatellNotificationSender(new FakeLogManager(_log));

			system.UseTestDouble<FakeNotificationConfigReader>().For<INotificationConfigReader>();
			system.UseTestDouble<FakeNofiticationWebClient>().For<INotificationClient>();
		}

		[Test]
		public void ShouldNotSendIfNoConfig()
		{
			_target.SetConfigReader(_notificationConfigReader);
			smsMessage.Messages.Add("On a day");
			_target.SendNotification(smsMessage, new NotificationHeader());
			_notificationConfigReader.Client.SentMessages.Count.Should().Be(0);
		}

		[Test]
		public void ShouldTryToSendIfConfig()
		{
			_target.SetConfigReader(_notificationConfigReader);
			_notificationConfigReader.LoadConfig(xml);
			smsMessage.Messages.Add("test send");
			_target.SendNotification(smsMessage, new NotificationHeader { MobileNumber = "46709218108" });
			_notificationConfigReader.Client.SentMessages.Count.Should().Be(1);
		}

		[Test]
		public void ShouldNotSendIfNoMobileNumber()
		{
			_target.SetConfigReader(_notificationConfigReader);
			_notificationConfigReader.LoadConfig(xml);
			smsMessage.Messages.Add("test send");
			_target.SendNotification(smsMessage, new NotificationHeader { MobileNumber = "" });
			_notificationConfigReader.Client.SentMessages.Count.Should().Be(0);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms"), Test]
		public void ShouldSplitMessageIfGreaterThanMaxSmsLength()
		{
			INotificationMessage msg = new NotificationMessage();
			msg.Subject = "Your Working Hours have changed";
			msg.Messages.Add("Monday 2012-01-01 08:00-17:00");
			msg.Messages.Add("Tuesday 2012-01-02 08:00-16:00");
			msg.Messages.Add("Wedneday 2012-01-03 08:00-16:00");
			msg.Messages.Add("Thursday 2012-01-04 08:00-16:00");
			msg.Messages.Add("Friday 2012-01-05 08:00-16:00");
			msg.Messages.Add("Monday 2012-01-08 Not Working");

			IList<string> messages = _target.GetSmsMessagesToSend(msg, false);
			messages.Count.Should().Be(2);
		}

		[Test]
		public void ShouldAddCustomerNameIfSetInMessage()
		{
			INotificationMessage msg = new NotificationMessage();
			msg.CustomerName = "Teleopti Test";
			msg.Subject = "Your Working Hours have changed";
			msg.Messages.Add("Monday 2012-01-01 08:00-17:00");

			string message = _target.GetSmsMessagesToSend(msg, false).FirstOrDefault();
			message.Should().Not.Be.Null();
			message.Should().EndWith("[Teleopti Test]");
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
			messages.Count.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldLogErrorIfSentFailed()
		{
			const string incorrectXml = @"<?xml version='1.0' encoding='utf-8' ?>
			<Config>
			<class>Teleopti.Ccc.Domain.Notification.ClickatellNotificationSender</class>
			<url>http://api.clickatell.com/xml/xml?data=</url>
			<user>ola.hakansson@teleopti.com</user>
			<password>cadadi02</password>
			<api_id>3388822</api_id>
			<from>TeleoptiCCC</from>
			<FindSuccessOrError>Error</FindSuccessOrError>
			<ErrorCode>Error</ErrorCode>
			<SuccessCode>Success</SuccessCode>
			<SkipSearch>false</SkipSearch>
			<data>
				<![CDATA[ <clickAPI><sendMsg>
				<user>{0}</user>
				<password>{1}</password>
				<api_id>{2}</api_id>
				<to>{3}</to>
				<from>{4}</from>
				<text>{5}</text>
				<unicode>{6}</unicode>
				</sendMsg></clickAPI>]]>
			</data>
		</Config>";

			_target.SetConfigReader(_notificationConfigReader);
			_notificationConfigReader.LoadConfig(incorrectXml);

			_notificationConfigReader.Client.MakeRequestFaild();
			smsMessage.Messages.Add("test send");
			_target.SendNotification(smsMessage, new NotificationHeader { MobileNumber = "46709218108" });

			_log.ErrorMessages.Count.Should().Be.EqualTo(1);

		}

		[Test]
		public void ShouldNotLogErrorIfSkipCheck()
		{
			const string xmlWithNoCheck = @"<?xml version='1.0' encoding='utf-8' ?>
					<Config>
						<class>Teleopti.Ccc.Domain.Notification.ClickatellNotificationSender</class>
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
							<user>{0}</user>
							<password>{1}</password>
							<api_id>{2}</api_id>
							<to>{3}</to>
							<from>{4}</from>
							<text>{5}</text>
							<unicode>{6}</unicode>
							</sendMsg></clickAPI>]]>
						</data>
					</Config>";
			_target.SetConfigReader(_notificationConfigReader);
			_notificationConfigReader.LoadConfig(xmlWithNoCheck);

			_notificationConfigReader.Client.MakeRequestFaild();
			smsMessage.Messages.Add("test send");
			_target.SendNotification(smsMessage, new NotificationHeader { MobileNumber = "46709218108" });

			_notificationConfigReader.Client.SentMessages.Count.Should().Be.EqualTo(1);
			_log.ErrorMessages.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSendSMSSuccess()
		{
			smsMessage.Messages.Add("Test success");
			var xmlConfig = @"<?xml version='1.0' encoding='utf-8' ?>
					<Config>
						<class>Teleopti.Ccc.Domain.Notification.ClickatellNotificationSender</class>
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
							<user>{0}</user>
							<password>{1}</password>
							<api_id>{2}</api_id>
							<to>{3}</to>
							<from>{4}</from>
							<text>{5}</text>
							<unicode>{6}</unicode>
							</sendMsg></clickAPI>]]>
						</data>
					</Config>";

			_target.SetConfigReader(_notificationConfigReader);
			_notificationConfigReader.LoadConfig(xmlConfig);

			smsMessage.Messages.Add("test send");
			_target.SendNotification(smsMessage, new NotificationHeader { MobileNumber = "46709218108" });
			_notificationConfigReader.Client.SentMessages.Count.Should().Be.EqualTo(1);
			_log.ErrorMessages.Count.Should().Be.EqualTo(0);
		}

	
	}
}


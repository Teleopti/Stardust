using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.ServiceBus.SMS;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Sms
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Clickatell"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms"), TestFixture]
	public class ClickatellSmsSenderTest
	{
		// the send will not work in thia test because of the password is wrong
		// we don't want to send a lot of messages all the time :-) 
		// if you want to try change to <password>cadadi01</password> (works as long as we have credits) <from>{3}</from>
		private MockRepository _mocks;
		private ISmsConfigReader _smsConfigReader;
		private ClickatellSmsSender _target;
		private const string smsMessage = "Schedule has changed";

		private const string xml = @"<?xml version='1.0' encoding='utf-8' ?>
<Config>
	<class>Teleopti.Ccc.Sdk.ServiceBus.SMS.ClickatellSmsSender</class>
	<url>http://api.clickatell.com/xml/xml?data=</url>
	<user>ola.hakansson@teleopti.com</user>
	<password>cadadi01</password>
	<api_id>3388822</api_id>
	<from>TeleoptiCCC</from>
	<data>
		<![CDATA[ <clickAPI><sendMsg>
		<api_id>3388822</api_id>
		<user>{0}</user>
		<password>{1}</password>
		<to>{2}</to>
		
		<text>{3}</text>
		</sendMsg></clickAPI>]]>
	</data>
</Config>";

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_smsConfigReader = _mocks.StrictMock<ISmsConfigReader>();
			_target = new ClickatellSmsSender();
			_target.SetConfigReader(_smsConfigReader);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Sdk.ServiceBus.SMS.ClickatellSmsSender.SendSms(System.String,System.String)"), Test]
		public void ShouldNotSendIfNoConfig()
		{
			Expect.Call(_smsConfigReader.HasLoadedConfig).Return(false);
			_mocks.ReplayAll();
			_target.SendSms(smsMessage,"" );
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Sdk.ServiceBus.SMS.ClickatellSmsSender.SendSms(System.String,System.String)"), Test]
		public void ShouldTryToSendIfConfig()
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);

			_smsConfigReader = new SmsConfigReader(doc);
			_target.SetConfigReader(_smsConfigReader);
			_target.SendSms(smsMessage, "46709218108");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Sdk.ServiceBus.SMS.ClickatellSmsSender.SendSms(System.String,System.String)"), Test]
		public void ShouldLogIfUrlIsIncorrect()
		{
			const string incorrectXml = @"<?xml version='1.0' encoding='utf-8' ?><Config>
	<class>Teleopti.Ccc.Sdk.ServiceBus.SMS.ClickatellSmsSender</class>
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

			_smsConfigReader = new SmsConfigReader(doc);
			_target.SetConfigReader(_smsConfigReader);
			_target.SendSms(smsMessage, "46709218108");
		}
	}
}
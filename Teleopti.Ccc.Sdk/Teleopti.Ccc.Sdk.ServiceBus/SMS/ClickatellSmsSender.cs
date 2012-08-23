using System;
using System.IO;
using System.Net;
using System.Xml;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.SMS
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Clickatell"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class ClickatellSmsSender : ISmsSender
	{
		private readonly ISmsConfigReader _smsConfigReader;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ClickatellSmsSender));

//        private const string smsString = @"<clickAPI>
//				<sendMsg><api_id>3388480</api_id><user>{0}</user>
//				<password>{1}</password><to>{2}</to><from>Teleopti CCC</from>
//				<text>{3}</text>
//				</sendMsg>
//			</clickAPI>";

		public ClickatellSmsSender(ISmsConfigReader smsConfigReader)
		{
			_smsConfigReader = smsConfigReader;
		}

		public void SendSms(DateOnlyPeriod dateOnlyPeriod, string mobileNumber)
		{
			if(!_smsConfigReader.HasLoadedConfig)
				return;
			//if (args == null || args.Length == 0)
			//{
			//    throw new ApplicationException("Specify the URI of the resource to retrieve.");
			//}

			var client = new WebClient();

			// Add a user agent header in case the 
			// requested URI contains a query.
			var smsString = _smsConfigReader.Data;

			client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
			var msgData = string.Format(smsString, _smsConfigReader.User, _smsConfigReader.Password, mobileNumber, _smsConfigReader.From,
			                         "ditt schema har ändrats");
			try
			{
				var data = client.OpenRead(_smsConfigReader.Url + msgData);
				if (data != null)
				{
					var reader = new StreamReader(data);
					var s = reader.ReadToEnd();
					var doc = new XmlDocument();
					doc.LoadXml(s);
					data.Close();
					reader.Close();
					if(doc.GetElementsByTagName("fault").Count > 0)
					{
						Logger.Error("Error occurred sending SMS: " + s);
					}
				}
			}
			catch (Exception exception)
			{
				Logger.Error("Error occurred trying to access: " + _smsConfigReader.Url + msgData, exception);
			}
			
			//logga om fel
		}
	}
}
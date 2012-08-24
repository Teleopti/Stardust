using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.SMS
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Clickatell"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class ClickatellSmsSender : ISmsSender
	{
		private ISmsConfigReader _smsConfigReader;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ClickatellSmsSender));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void SendSms(string message, string mobileNumber)
		{
			if(!_smsConfigReader.HasLoadedConfig)
				return;

			using (var client = new WebClient())
			{
				// Add a user agent header in case the 
				// requested URI contains a query.
				var smsString = _smsConfigReader.Data;

				client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
				var msgData = string.Format(CultureInfo.InvariantCulture, smsString, _smsConfigReader.User, _smsConfigReader.Password, mobileNumber, _smsConfigReader.From,
										 message);
				try
				{
					var data = client.OpenRead(_smsConfigReader.Url + msgData);
					if (data != null)
					{
						var reader = new StreamReader(data);
						var s = reader.ReadToEnd();
						data.Close();				
						reader.Close();
						var doc = new XmlDocument();
						doc.LoadXml(s);
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
			}
			
		}

		public void SetConfigReader(ISmsConfigReader smsConfigReader)
		{
			_smsConfigReader = smsConfigReader;
		}
	}
}
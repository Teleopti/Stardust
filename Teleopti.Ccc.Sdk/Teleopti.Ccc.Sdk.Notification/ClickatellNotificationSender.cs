using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Notification
{
	// this class is a dll where all the Notification Senders could be 
	// Then more could be added without changes in the Service Bus
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Clickatell"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class ClickatellNotificationSender : INotificationSender
	{
		private INotificationConfigReader _notificationConfigReader;
		//private static readonly ILog Logger = LogManager.GetLogger(typeof(ClickatellNotificationSender));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SendNotification(INotificationMessage message, string to)
		{
			if (!_notificationConfigReader.HasLoadedConfig)
				return;
			//TODO check if empty concat and split into several if too long
			// we handle this here because here we know it is a sms
			//var smsMessage = message.Subject;

			// list for messages to send
            IList<string> messagesToSendList = GetSmsMessagesToSend(message);
		    
            foreach(var msg in messagesToSendList)
            {
                SendSmsNotifications(msg, to);
            }

        }

        public IList<string> GetSmsMessagesToSend(INotificationMessage message)
        {
			IList<string> messagesToSendList = new List<string>();
            const int maxSmsLength = 160;
			var temp = message.Subject + " ";

			for (var i = 0; i < message.Messages.Count; )
			{
                if (temp.Length + message.Messages[i].Length < maxSmsLength)
				{
					temp = temp + message.Messages[i] + ",";
					i++;
					if (i == message.Messages.Count)
					{
                        string replace = temp.Substring(0, temp.Length - 1);
						messagesToSendList.Add(replace + ".");
					}
				}
				else
				{
					var replace = temp.Substring(0, temp.Length - 1);
					messagesToSendList.Add(replace + ".");
                    temp = message.Subject + " ";
				}
			}

            return messagesToSendList;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		private void SendSmsNotifications(string smsMessage, string mobileNumber)
		{
			using (var client = new WebClient())
			{
				// Add a user agent header in case the 
				// requested URI contains a query.
				var smsString = _notificationConfigReader.Data;

				client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
				var msgData = string.Format(CultureInfo.InvariantCulture, smsString, _notificationConfigReader.User, _notificationConfigReader.Password, mobileNumber, _notificationConfigReader.From,
										 smsMessage);
				try
				{
					var data = client.OpenRead(_notificationConfigReader.Url + msgData);
					if (data != null)
					{
						var reader = new StreamReader(data);
						var s = reader.ReadToEnd();
						data.Close();
						reader.Close();
						var doc = new XmlDocument();
						doc.LoadXml(s);
						if (doc.GetElementsByTagName("fault").Count > 0)
						{
							throw (new SendNotificationException("Error occurred sending SMS: " + s));
							//Logger.Error("Error occurred sending SMS: " + s);
						}
					}
				}
				catch (Exception exception)
				{
					throw (new SendNotificationException(
						"Error occurred trying to access: " + _notificationConfigReader.Url + msgData, exception));
					//Logger.Error("Error occurred trying to access: " + _notificationConfigReader.Url + msgData, exception);
				}
			}
		}

		public void SetConfigReader(INotificationConfigReader notificationConfigReader)
		{
			_notificationConfigReader = notificationConfigReader;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Interfaces.Infrastructure;
using log4net;

namespace Teleopti.Ccc.Sdk.Notification
{
	// this class is a dll where all the Notification Senders could be 
	// Then more could be added without changes in the Service Bus
    [IsNotDeadCode("This is instantiated via reflection when to send a SMS."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Clickatell"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class ClickatellNotificationSender : INotificationSender
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ClickatellNotificationSender));
		private INotificationConfigReader _notificationConfigReader;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SendNotification(INotificationMessage message, string receiver)
		{
			if (!_notificationConfigReader.HasLoadedConfig)
				return;
			//TODO check if empty concat and split into several if too long
			// we handle this here because here we know it is a sms
			//var smsMessage = message.Subject;

			// list for messages receiver send
			var containUnicode = message.Subject.Any(t => char.GetUnicodeCategory(t) == UnicodeCategory.OtherLetter);
			IList<string> messagesToSendList = GetSmsMessagesToSend(message, containUnicode);
		    
            foreach(var msg in messagesToSendList)
            {
                sendSmsNotifications(msg, receiver, containUnicode);
            }

        }

        public IList<string> GetSmsMessagesToSend(INotificationMessage message, bool containUnicode)
        {
			IList<string> messagesToSendList = new List<string>();
			var temp = message.Subject + " ";
			
			var maxSmsLength = 160;
	        if (containUnicode)
		        maxSmsLength = 70;

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
		private void sendSmsNotifications(string smsMessage, string mobileNumber, bool containUnicode)
		{
			using (var client = _notificationConfigReader.CreateClient())
			{
				// Add a user agent header in case the 
				// requested URI contains a query.
				var smsString = _notificationConfigReader.Data;

				if (containUnicode)
					smsMessage = smsMessage.Aggregate(@"",
					                                  (current, c) =>
					                                  current +
					                                  string.Format("{0:x4}",
					                                                Convert.ToUInt32(((int) c).ToString(CultureInfo.InvariantCulture))))
					                       .ToUpper();
				
				var msgData = string.Format(CultureInfo.InvariantCulture, smsString, _notificationConfigReader.User,
				                            _notificationConfigReader.Password, mobileNumber, _notificationConfigReader.From,
				                            smsMessage, containUnicode ? 1 : 0);

				Logger.Info("Sending SMS on: " + _notificationConfigReader.Url + msgData);
				try
				{
					var data = client.MakeRequest(msgData);
					if (data != null)
					{
						var reader = new StreamReader(data);
						var s = reader.ReadToEnd();
						data.Close();
						reader.Close();
						if(_notificationConfigReader.SkipSearch) return;
						if (_notificationConfigReader.FindSuccessOrError.Equals("Error"))
						{
							if (s.Contains(_notificationConfigReader.ErrorCode))
							{
								Logger.Error("Error occurred sending SMS: " + s);
								throw new SendNotificationException("Error occurred sending SMS: " + s);
							}
						}
						else
						{
							if (!s.Contains(_notificationConfigReader.SuccessCode))
							{
								Logger.Error("Error occurred sending SMS: " + s);
								throw new SendNotificationException("Error occurred sending SMS: " + s);
							}
						}
					}
				}
				catch (Exception exception)
				{
                    Logger.Error("Error occurred trying receiver access: " + _notificationConfigReader.Url + msgData, exception);
					throw new SendNotificationException(
						"Error occurred trying receiver access: " + _notificationConfigReader.Url + msgData, exception);
				}
			}
		}

		public void SetConfigReader(INotificationConfigReader notificationConfigReader)
		{
			_notificationConfigReader = notificationConfigReader;
		}
	}
}
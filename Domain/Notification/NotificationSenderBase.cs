﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	// this class is a dll where all the Notification Senders could be 
	// Then more could be added without changes in the Service Bus
	public class NotificationSenderBase : INotificationSender
	{
		private static ILog _logger;
		private INotificationConfigReader _notificationConfigReader;
		private readonly INotificationClient _notificationClient;

		public NotificationSenderBase(ILog log, INotificationClient notificationClient)
		{
			_notificationClient = notificationClient;
			_logger = log;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SendNotification(INotificationMessage message, NotificationHeader notificationHeader)
		{
			if (!_notificationConfigReader.HasLoadedConfig)
				return;

			if (string.IsNullOrEmpty(notificationHeader.MobileNumber))
			{
				_logger.Info($"Did not find a Mobile Number on {notificationHeader.PersonName}");
				return;
			}

			//TODO check if empty concat and split into several if too long
			// we handle this here because here we know it is a sms
			//var smsMessage = message.Subject;

			// list for messages receiver send
			var containUnicode = containsUnicode(message.Subject) || message.Messages.Any(containsUnicode);
			IList<string> messagesToSendList = GetSmsMessagesToSend(message, containUnicode);

			foreach (var msg in messagesToSendList)
			{
				sendSmsNotifications(msg, notificationHeader.MobileNumber, containUnicode);
			}

		}

		public IList<string> GetSmsMessagesToSend(INotificationMessage message, bool containUnicode)
		{
			var messagesToSendList = new List<string>();
			var maxSmsLength = 160;
			if (containUnicode)
				maxSmsLength = 70;
			var customerString = "";
			if (!string.IsNullOrWhiteSpace(message.CustomerName))
				customerString = $" from [{message.CustomerName}]";
			var formatString = $"{message.Subject} {{0}}{customerString}";

			var tmp = "";
			foreach (var msg in message.Messages)
			{
				// If adding next message is still less than limit, add it and move on
				if (string.Format(formatString, tmp + msg).Length < maxSmsLength)
				{
					tmp = string.IsNullOrWhiteSpace(tmp) ? $"{tmp}{msg}" : $"{tmp},{msg}";
					continue;
				}
				// Otherwise we don't want to send empty message so add it anyways 
				if (string.IsNullOrWhiteSpace(tmp))
				{
					messagesToSendList.Add(string.Format(formatString, msg));
				}
				else
				{
					// send and reset
					messagesToSendList.Add(string.Format(formatString, tmp));
					tmp = msg;
				}
			}
			if (!string.IsNullOrWhiteSpace(tmp))
				messagesToSendList.Add(string.Format(formatString, tmp));
			return messagesToSendList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		private void sendSmsNotifications(string smsMessage, string mobileNumber, bool containUnicode)
		{
			var smsString = _notificationConfigReader.Data;

			if (containUnicode)
				smsMessage = smsMessage.Aggregate(@"", (current, c) => $"{current}{Convert.ToUInt32(((int)c).ToString(CultureInfo.InvariantCulture)):x4}")
					.ToUpper();

			var msgData = string.Format(CultureInfo.InvariantCulture, smsString, _notificationConfigReader.User,
																	_notificationConfigReader.Password, _notificationConfigReader.Api, mobileNumber, _notificationConfigReader.From,
																	smsMessage, containUnicode ? 1 : 0);

			_logger.Info($"Sending SMS on: {_notificationConfigReader.Url}{msgData}");
			try
			{
				var data = _notificationClient.MakeRequest(_notificationConfigReader.Url, msgData);
				if (data != null)
				{
					if (_notificationConfigReader.SkipSearch) return;
					var hasError = (_notificationConfigReader.FindSuccessOrError.Equals("Error") && data.Contains(_notificationConfigReader.ErrorCode))
						|| !data.Contains(_notificationConfigReader.SuccessCode);

					if (hasError)
						_logger.Error($"Error occurred sending SMS: {data}");

				}
			}
			catch (Exception exception)
			{
				_logger.Error($"Error occurred trying receiver access: {_notificationConfigReader.Url}{msgData}", exception);
			}
		}

		public void SetConfigReader(INotificationConfigReader notificationConfigReader)
		{
			_notificationConfigReader = notificationConfigReader;
		}

		private static bool containsUnicode(string s)
		{
			return s.Any(c => c > 255);
		}
	}
}
using System;
using System.Net;
using System.Net.Mail;
using log4net;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class EmailSender : INotificationSender
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(EmailSender));
		private readonly IEmailConfiguration _emailConfiguration;

		public EmailSender(IEmailConfiguration emailConfiguration)
		{
			_emailConfiguration = emailConfiguration;
		}

		public void SendNotification(INotificationMessage message, NotificationHeader notificationHeader)
		{
			if (!validateArguments(notificationHeader))
				return;

			using (var client = new SmtpClient())
			{
				client.Host = _emailConfiguration.SmtpHost;
				client.Port = _emailConfiguration.SmtpPort;
				client.EnableSsl = _emailConfiguration.SmtpUseSsl;
				client.DeliveryMethod = SmtpDeliveryMethod.Network;
				client.UseDefaultCredentials = false;
				if (!_emailConfiguration.SmtpUseRelay)
					client.Credentials = new NetworkCredential(_emailConfiguration.SmtpUser, _emailConfiguration.SmtpPassword);
				client.Timeout = 10000;

				var from = new MailAddress(notificationHeader.EmailSender);
				var to = new MailAddress(notificationHeader.EmailReceiver, notificationHeader.PersonName);

				using (var mailMessage = new MailMessage(from, to))
				{
					mailMessage.Subject = message.Subject;
					mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
					mailMessage.Body = getBodyMessage(message);
					mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
					mailMessage.ReplyToList.Add(from);

					try
					{
						Logger.Debug(string.Format("Sending E-mail from sender '{0}' to receiver '{1}'.", notificationHeader.EmailSender, notificationHeader.EmailReceiver));
						client.Send(mailMessage);
					}
					catch (SmtpException exception)
					{
						Logger.Error(string.Format("Failed to send E-mail from sender '{0}' to receiver '{1}'.", notificationHeader.EmailSender, notificationHeader.EmailReceiver), exception);
					}
					catch (Exception exception)
					{
						Logger.Error(string.Format("Failed to send E-mail from sender '{0}' to receiver '{1}'.", notificationHeader.EmailSender, notificationHeader.EmailReceiver), exception);
					}

				}
			}
		}

		private string getBodyMessage(INotificationMessage notificationMessage)
		{
			string returnMessage = string.Empty;

			foreach (var message in notificationMessage.Messages)
			{
				returnMessage += message + Environment.NewLine;
			}

			if (!string.IsNullOrEmpty(returnMessage))
				returnMessage = returnMessage.Substring(0, returnMessage.Length - 2);

			return returnMessage;
		}

		private bool hasSmtpHost()
		{
			if (string.IsNullOrEmpty(_emailConfiguration.SmtpHost))
			{
				Logger.Info("E-mail not sent due to missing SMTP Host.");
				return false;
			}
			return true;
		}

		private bool hasSmtpPort()
		{
			if (_emailConfiguration.SmtpPort == 0)
			{
				Logger.Info("E-mail not sent due to missing SMTP Port.");
				return false;
			}
			return true;
		}

		private bool hasSmtpCredentials()
		{
			if (_emailConfiguration.SmtpUseRelay)
				return true;
			if (string.IsNullOrEmpty(_emailConfiguration.SmtpUser) || string.IsNullOrEmpty(_emailConfiguration.SmtpPassword))
			{
				Logger.Info("E-mail not sent due to missing SMTP user and/or password.");
				return false;
			}
			return true;
		}

		private bool hasEmailSender(NotificationHeader notificationHeader)
		{
			if (string.IsNullOrEmpty(notificationHeader.EmailSender))
			{
				Logger.Info("E-mail not sent due to missing sender.");
				return false;
			}
			return true;
		}

		private bool hasEmailReceiver(NotificationHeader notificationHeader)
		{
			if (string.IsNullOrEmpty(notificationHeader.EmailReceiver))
			{
				Logger.Info("E-mail not sent due to missing receiver.");
				return false;
			}
			return true;
		}

		private bool validateArguments(NotificationHeader notificationHeader)
		{
			if (!hasSmtpHost())
				return false;

			if (!hasSmtpPort())
				return false;

			if (!hasSmtpCredentials())
				return false;

			if (!hasEmailSender(notificationHeader))
				return false;

			if (!hasEmailReceiver(notificationHeader))
				return false;

			return true;
		}

		public void SetConfigReader(INotificationConfigReader notificationConfigReader)
		{
		}
	}
}
using System;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class EmailSender : IEmailSender
	{
		private readonly IEmailConfiguration _emailConfiguration;

		public EmailSender(IEmailConfiguration emailConfiguration)
		{
			_emailConfiguration = emailConfiguration;
		}

		public void Send(EmailMessage emailMessage)
		{
			throw new NotImplementedException();
		}
	}
}
using System;

namespace Teleopti.Ccc.Domain.Auditing
{
	public class LoginAttemptModel
	{
		public string Result { get; set; }
		public string UserCredentials { get; set; }
		public string Provider { get; set; }
		public string Client { get; set; }
		public string ClientIp { get; set; }
		public Guid? PersonId { get; set; }
	}
}
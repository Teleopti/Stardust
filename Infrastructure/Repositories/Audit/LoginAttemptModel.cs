using System;

namespace Teleopti.Ccc.Infrastructure.Repositories.Audit
{
	public class LoginAttemptModel
	{
		public string Result { get; set; }
		public string UserCredentials { get; set; }
		public string Provider { get; set; }
		public string ClientIp { get; set; }
		public Guid? PersonId { get; set; }
	}
}
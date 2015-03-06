using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class AuthenticationResult
	{
		public bool Successful { get; set; }
		public string Message { get; set; }
		public bool HasMessage { get; set; }
		public IPerson Person { get; set; }
		public bool PasswordExpired { get; set; }
		public string PasswordPolicy { get; set; }
	}
}
namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class PasswordPolicyResult
	{
		public bool Successful { get; set; }
		public string Message { get; set; }
		public bool HasMessage { get; set; }
		public bool PasswordExpired { get; set; }
	}
}
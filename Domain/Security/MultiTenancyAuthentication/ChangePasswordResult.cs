namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public class ChangePasswordResult
	{
		public bool Success { get; set; }
	}

	public class SavePersonInfoResult
	{
		public bool Success { get; set; }
		public string FailReason { get; set; }
		public string UserName { get; set; }
	}

	public class PersistPersonInfoResult
	{
		public bool PasswordStrengthIsValid { get; set; }
	}
}
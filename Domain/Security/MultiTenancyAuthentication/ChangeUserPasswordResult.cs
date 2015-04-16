namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public class ChangeUserPasswordResult
	{
		public bool Success { get; set; }
		public string FailReason { get; set; }
	}
}
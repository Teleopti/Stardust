namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class ApplicationUserQueryResult
	{
		public PersonInfo PersonInfo { get; set; }
		public PasswordPolicyForUser PasswordPolicy { get; set; }
	}
}
namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class ApplicationUserQueryResult
	{
		public ApplicationUserQueryResult()
		{
			PasswordPolicy = new PasswordPolicyForUser();
		}

		public PersonInfo PersonInfo { get; set; }
		public PasswordPolicyForUser PasswordPolicy { get; set; }
	}
}
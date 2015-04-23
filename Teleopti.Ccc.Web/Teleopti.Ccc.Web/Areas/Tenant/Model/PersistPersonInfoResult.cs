namespace Teleopti.Ccc.Web.Areas.Tenant.Model
{
	public class PersistPersonInfoResult
	{
		public PersistPersonInfoResult()
		{
			PasswordStrengthIsValid = true;
			ApplicationLogonNameIsValid = true;
			IdentityIsValid = true;
		}
		public bool PasswordStrengthIsValid { get; set; }
		public bool ApplicationLogonNameIsValid { get; set; }
		public bool IdentityIsValid { get; set; }
	}
}
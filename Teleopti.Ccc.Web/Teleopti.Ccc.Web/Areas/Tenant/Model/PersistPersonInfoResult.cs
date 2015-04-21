namespace Teleopti.Ccc.Web.Areas.Tenant.Model
{
	public class PersistPersonInfoResult
	{
		public PersistPersonInfoResult()
		{
			PasswordStrengthIsValid = true;
		}
		public bool PasswordStrengthIsValid { get; set; }
	}
}
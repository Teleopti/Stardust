namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class PasswordPolicyConfigurable
	{
		public int MaxNumberOfAttempts { get; set; }
		public int InvalidAttemptWindow { get; set; }
		public int PasswordValidForDayCount { get; set; }
		public int PasswordExpireWarningDayCount { get; set; }
		public string Rule1 { get; set; }
	}
}
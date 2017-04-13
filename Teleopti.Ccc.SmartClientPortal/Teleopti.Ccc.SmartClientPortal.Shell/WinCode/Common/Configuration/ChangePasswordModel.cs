namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public class ChangePasswordModel
	{
		public ChangePasswordModel()
		{
			NewPassword = string.Empty;
			ConfirmPassword = string.Empty;
		}

		public string NewPassword { get; set; }
		public string ConfirmPassword { get; set; }
		public string OldPassword { get; set; }

		public bool ConfirmPasswordValid
		{
			get
			{
				return NewPassword.Length > 0 && ConfirmPassword.Equals(NewPassword);
			}
		}

		public bool NewPasswordIsNew
		{
			get
			{
				if (OldPassword == null)
				{
					return NewPassword != null;
				}
				return !OldPassword.Equals(NewPassword);
			}
		}

		public bool OldPasswordValid
		{
			get
			{
				return !string.IsNullOrEmpty(OldPassword);
			}
		}

		public bool IsValid()
		{
			return NewPasswordIsNew &&
					 ConfirmPasswordValid &&
					 OldPasswordValid;
		}
	}
}
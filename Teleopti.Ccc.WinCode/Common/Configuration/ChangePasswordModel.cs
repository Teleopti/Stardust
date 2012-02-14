using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public class ChangePasswordModel
    {
        public ChangePasswordModel()
        {
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }

        public string OldEncryptedPassword { get; set; }
        public string OldEnteredEncryptedPassword { get; set; }
        public string OldEnteredPassword { get; set; }
        public bool OldEnteredPasswordValid
        {
            get
            {
                if (OldEncryptedPassword == null)
                {
                    return OldEnteredEncryptedPassword == null;
                }
                return OldEncryptedPassword.Equals(OldEnteredEncryptedPassword);
            }
        }

        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

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
                if (OldEnteredPassword == null)
                {
                    return NewPassword != null;
                }
                return !OldEnteredPassword.Equals(NewPassword);
            }
        }

        public bool IsValid(IPasswordPolicy passwordPolicy)
        {
            return NewPasswordIsNew &&
                   ConfirmPasswordValid &&
                   OldEnteredPasswordValid &&
                   passwordPolicy.CheckPasswordStrength(NewPassword);
        }
    }
}
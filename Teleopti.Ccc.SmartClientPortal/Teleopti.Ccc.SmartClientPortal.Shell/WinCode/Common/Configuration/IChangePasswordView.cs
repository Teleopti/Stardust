namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
    public interface IChangePasswordView
    {
        void SetInputFocus();
        void SetOldPasswordValid(bool valid);
        void SetConfirmPasswordValid(bool valid);
        void SetNewPasswordValid(bool valid);
        void Close();
        void ShowValidationError();
    }
}
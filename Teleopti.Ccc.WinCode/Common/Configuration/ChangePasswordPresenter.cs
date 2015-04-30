using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public class ChangePasswordPresenter
	{
		private readonly IChangePasswordView _view;
		private readonly IChangePassword _changePassword;

		public ChangePasswordPresenter(IChangePasswordView view, IChangePassword changePassword)
		{
			_view = view;
			_changePassword = changePassword;
		}

		public ChangePasswordModel Model { get; private set; }

		public void Initialize()
		{
				
			Model = new ChangePasswordModel();
			_view.SetInputFocus();
		}

		public void Save()
		{
			if (!Model.IsValid())
			{
				_view.ShowValidationError();
				return;
			}
			
			var res =
				_changePassword.SetNewPassword(new ChangePasswordInput
				{
					NewPassword = Model.NewPassword,
					OldPassword = Model.OldPassword,
					UserName = StateHolderReader.Instance.StateReader.SessionScopeData.UserName
				});
			if (!res.Success)
			{
				_view.ShowValidationError();
				return;
			}
			((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person.ApplicationAuthenticationInfo.Password = Model.NewPassword;
			_view.Close();
		}

		public void SetOldPassword(string password)
		{
			Model.OldPassword = password;
			_view.SetOldPasswordValid(Model.OldPasswordValid);
		}

		public void SetNewPassword(string newPassword)
		{
			Model.NewPassword = newPassword;
			_view.SetNewPasswordValid(Model.NewPasswordIsNew);
		}

		public void SetConfirmNewPassword(string confirmNewPassword)
		{
			Model.ConfirmPassword = confirmNewPassword;
			_view.SetConfirmPasswordValid(Model.ConfirmPasswordValid);
		}
	}
}
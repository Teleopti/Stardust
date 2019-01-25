using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public class ChangePasswordPresenter
	{
		private readonly IChangePasswordView _view;
		private readonly IChangePasswordTenantClient _changePassword;
		private readonly IPerson _loggedOnPerson;

		public ChangePasswordPresenter(IChangePasswordView view, IChangePasswordTenantClient changePassword, IPerson loggedOnPerson)
		{
			_view = view;
			_changePassword = changePassword;
			_loggedOnPerson = loggedOnPerson;
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
					PersonId = _loggedOnPerson.Id.Value
				});
			if (!res.Success)
			{
				_view.ShowValidationError();
				return;
			}
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
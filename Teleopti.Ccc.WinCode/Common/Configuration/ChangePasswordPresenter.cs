using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public class ChangePasswordPresenter
	{
		private readonly IChangePasswordView _view;
		private readonly IPasswordPolicy _passwordPolicy;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IOneWayEncryption _encryption;
		private readonly IChangePassword _changePassword;

		public ChangePasswordPresenter(IChangePasswordView view, IPasswordPolicy passwordPolicy,
			IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IOneWayEncryption encryption,
			IChangePassword changePassword)
		{
			_view = view;
			_passwordPolicy = passwordPolicy;
			_unitOfWorkFactory = unitOfWorkFactory;
			_repositoryFactory = repositoryFactory;
			_encryption = encryption;
			_changePassword = changePassword;
		}

		public ChangePasswordModel Model { get; private set; }

		public void Initialize()
		{
			using (IUnitOfWork unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				IPersonRepository personRepository = _repositoryFactory.CreatePersonRepository(unitOfWork);
				IPerson person = TeleoptiPrincipal.CurrentPrincipal.GetPerson(personRepository);
				Model = new ChangePasswordModel { OldEncryptedPassword = person.ApplicationAuthenticationInfo.Password };
			}
			_view.SetInputFocus();
		}

		public void Save()
		{
			if (!Model.IsValid(_passwordPolicy))
			{
				_view.ShowValidationError();
				return;
			}
			using (IUnitOfWork unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				IPersonRepository personRepository = _repositoryFactory.CreatePersonRepository(unitOfWork);
				IPerson person = TeleoptiPrincipal.CurrentPrincipal.GetPerson(personRepository);
				IUserDetailRepository userDetailRepository = _repositoryFactory.CreateUserDetailRepository(unitOfWork);
				IUserDetail userDetail = userDetailRepository.FindByUser(person);
				bool result = person.ChangePassword(Model.NewPassword,
																StateHolderReader.Instance.StateReader.ApplicationScopeData.
																	 LoadPasswordPolicyService, userDetail);
				if (!result)
				{
					_view.ShowValidationError();
					return;
				}
				((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person.ApplicationAuthenticationInfo.Password = Model.NewPassword;
				unitOfWork.PersistAll();
			}
			// todo : tenant remove code above and just save to tenant
			var res =
				_changePassword.SetNewPassword(new ChangePasswordInput
				{
					NewPassword = Model.NewPassword,
					OldPassword = Model.OldEnteredPassword,
					UserName = StateHolderReader.Instance.StateReader.SessionScopeData.UserName
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
			string encryptedOldPassword = _encryption.EncryptString(password);
			Model.OldEnteredPassword = password;
			Model.OldEnteredEncryptedPassword = encryptedOldPassword;
			_view.SetOldPasswordValid(Model.OldEnteredPasswordValid);
		}

		public void SetNewPassword(string newPassword)
		{
			Model.NewPassword = newPassword;
			_view.SetNewPasswordValid(_passwordPolicy.CheckPasswordStrength(newPassword) &&
											  Model.NewPasswordIsNew);
		}

		public void SetConfirmNewPassword(string confirmNewPassword)
		{
			Model.ConfirmPassword = confirmNewPassword;
			_view.SetConfirmPasswordValid(Model.ConfirmPasswordValid);
		}
	}
}
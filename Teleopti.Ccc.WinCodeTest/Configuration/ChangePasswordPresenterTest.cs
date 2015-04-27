using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[TestFixture]
	public class ChangePasswordPresenterTest
	{
		private ChangePasswordPresenter target;
		private MockRepository mocks;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IRepositoryFactory repositoryFactory;
		private IUnitOfWork unitOfWork;
		private IPerson person;
		private Guid personId;
		private IChangePasswordView view;
		private IOneWayEncryption oneWayEncryption;
		private IApplicationAuthenticationInfo applicationAuthenticationInfo;
		private IPasswordPolicy passwordPolicy;
		private IPersonRepository personRepository;
		private IChangePassword changePw;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			person = MockRepository.GenerateMock<IPerson>();
			personId = Guid.Empty;
			view = MockRepository.GenerateMock<IChangePasswordView>();
			passwordPolicy = MockRepository.GenerateMock<IPasswordPolicy>();
			oneWayEncryption = MockRepository.GenerateMock<IOneWayEncryption>();
			applicationAuthenticationInfo = MockRepository.GenerateMock<IApplicationAuthenticationInfo>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			changePw = MockRepository.GenerateMock<IChangePassword>();
			target = new ChangePasswordPresenter(view, passwordPolicy, unitOfWorkFactory, repositoryFactory, oneWayEncryption, changePw);
		}

		private void initializeExpectation()
		{
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			repositoryFactory.Stub(x => x.CreatePersonRepository(unitOfWork)).Return(personRepository);
			personRepository.Stub(x => x.Get(personId)).Return(person);
			person.Stub(x => x.ApplicationAuthenticationInfo).Return(applicationAuthenticationInfo);
			applicationAuthenticationInfo.Stub(x => x.Password).Return("currentEncryptedPassword");
			unitOfWork.Stub(x => x.Dispose());
			view.Stub(x => x.SetInputFocus());
		}

		[Test]
		public void VerifyOldPasswordMustMatch()
		{

			initializeExpectation();
			view.Stub(x => x.SetOldPasswordValid(false));
			oneWayEncryption.Stub(x => x.EncryptString("currentNotEncryptedPassword2")).Return(
				"currentEncryptedPassword2");

			target.Initialize();
			target.SetOldPassword("currentNotEncryptedPassword2");
		}

		[Test]
		public void VerifyNewPasswordMustPassPolicyCheck()
		{
			initializeExpectation();
			view.Stub(x => x.SetNewPasswordValid(false));
			passwordPolicy.Stub(x => x.CheckPasswordStrength("newNotEncryptedPassword2")).Return(false);

			target.Initialize();
			target.SetNewPassword("newNotEncryptedPassword2");
		}

		[Test]
		public void VerifyNewPasswordsWhenMatch()
		{
			initializeExpectation();
			view.Stub(x => x.SetNewPasswordValid(true));
			passwordPolicy.Stub(x => x.CheckPasswordStrength("newNotEncryptedPassword2")).Return(true);
			view.Stub(x => x.SetConfirmPasswordValid(true));

			target.Initialize();
			target.SetNewPassword("newNotEncryptedPassword2");
			target.SetConfirmNewPassword("newNotEncryptedPassword2");
		}

		[Test]
		public void VerifySaveNewPasswordsWhenMatch()
		{
			var userDetailRepository = mocks.StrictMock<IUserDetailRepository>();
			var userDetail = mocks.StrictMock<IUserDetail>();

			initializeExpectation();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			repositoryFactory.Stub(x => x.CreatePersonRepository(unitOfWork)).Return(personRepository);
			repositoryFactory.Stub(x => x.CreateUserDetailRepository(unitOfWork)).Return(userDetailRepository);
			personRepository.Stub(x => x.Get(personId)).Return(person);
			userDetailRepository.Stub(x => x.FindByUser(person)).Return(userDetail);
			person.Stub(x => x.ChangePassword("newNotEncryptedPassword2",
														StateHolderReader.Instance.StateReader.ApplicationScopeData.
															LoadPasswordPolicyService, userDetail)).Return(true).IgnoreArguments();
			passwordPolicy.Stub(x => x.CheckPasswordStrength("newNotEncryptedPassword2")).Return(true);
			unitOfWork.Stub(x => x.PersistAll()).Return(new List<IRootChangeInfo>(0));
			unitOfWork.Stub(x => x.Dispose());
			view.Stub(x => x.Close());
			changePw.Stub(x => x.SetNewPassword(null))
				.IgnoreArguments()
				.Return(new ChangePasswordResult { Success = true });


			target.Initialize();
			target.Model.OldEnteredEncryptedPassword = target.Model.OldEncryptedPassword;
			target.Model.NewPassword = "newNotEncryptedPassword2";
			target.Model.ConfirmPassword = "newNotEncryptedPassword2";
			target.Save();
			Assert.AreEqual("newNotEncryptedPassword2",
								((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person.ApplicationAuthenticationInfo.Password);
		}

		[Test]
		public void VerifyWhenPasswordCouldNotBeChanged()
		{
			var userDetailRepository = mocks.StrictMock<IUserDetailRepository>();
			var userDetail = mocks.StrictMock<IUserDetail>();

			initializeExpectation();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			repositoryFactory.Stub(x => x.CreatePersonRepository(unitOfWork)).Return(personRepository);
			repositoryFactory.Stub(x => x.CreateUserDetailRepository(unitOfWork)).Return(userDetailRepository);
			personRepository.Stub(x => x.Get(personId)).Return(person);
			userDetailRepository.Stub(x => x.FindByUser(person)).Return(userDetail);
			person.Stub(x => x.ChangePassword("newNotEncryptedPassword2",
														StateHolderReader.Instance.StateReader.ApplicationScopeData.
															LoadPasswordPolicyService, userDetail)).Return(false);
			passwordPolicy.Stub(x => x.CheckPasswordStrength("newNotEncryptedPassword2")).Return(true);
			unitOfWork.Stub(x => x.Dispose());
			view.Stub(x => x.ShowValidationError());


			target.Initialize();
			target.Model.OldEnteredEncryptedPassword = target.Model.OldEncryptedPassword;
			target.Model.NewPassword = "newNotEncryptedPassword2";
			target.Model.ConfirmPassword = "newNotEncryptedPassword2";
			target.Save();
		}

		[Test]
		public void VerifySaveWithErrorInModel()
		{
			initializeExpectation();
			view.Stub(x => x.ShowValidationError());

			target.Initialize();
			target.Model.NewPassword = "newNotEncryptedPassword2";
			target.Model.ConfirmPassword = "newNotEncryptedPassword3";
			target.Save();
		}

		[Test]
		public void VerifySaveWithErrorWhenNoNewPassword()
		{
			initializeExpectation();

			target.Initialize();
			target.Model.OldEncryptedPassword = "encrypted!";
			target.Model.OldEnteredEncryptedPassword = "encrypted!";
			target.Model.NewPassword = "";
			target.Model.ConfirmPassword = "";
			target.Save();
		}
	}
}

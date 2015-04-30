using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common.Configuration;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[TestFixture]
	public class ChangePasswordPresenterTest
	{
		private ChangePasswordPresenter target;
		private IChangePasswordView view;
		private IChangePassword changePw;

		[SetUp]
		public void Setup()
		{
			view = MockRepository.GenerateMock<IChangePasswordView>();
			changePw = MockRepository.GenerateMock<IChangePassword>();
			target = new ChangePasswordPresenter(view, changePw);
		}

		private void initializeExpectation()
		{
			view.Stub(x => x.SetInputFocus());
		}

		[Test]
		public void OldPasswordCannotBeEmpty()
		{
			initializeExpectation();
			view.Stub(x => x.SetOldPasswordValid(false));

			target.Initialize();
			target.SetOldPassword("");
		}


		[Test]
		public void NewPasswordsShouldMatch()
		{
			initializeExpectation();
			view.Stub(x => x.SetNewPasswordValid(true));
			
			view.Stub(x => x.SetConfirmPasswordValid(false));

			target.Initialize();
			target.SetNewPassword("newPassword");
			target.SetConfirmNewPassword("newPassword2");
		}

		[Test]
		public void NewPasswordsShouldNotBeSameAsOld()
		{
			initializeExpectation();
			view.Stub(x => x.SetNewPasswordValid(false));

			target.Initialize();
			target.SetOldPassword("newPassword");
			target.SetNewPassword("newPassword");
		}

		[Test]
		public void VerifySaveNewPasswordsWhenMatch()
		{
			initializeExpectation();
			
			view.Stub(x => x.Close());
			changePw.Stub(x => x.SetNewPassword(null))
				.IgnoreArguments()
				.Return(new ChangePasswordResult { Success = true });


			target.Initialize();
			target.Model.OldPassword = "oldOne";
			target.Model.NewPassword = "newOne";
			target.Model.ConfirmPassword = "newOne";
			target.Save();
			Assert.AreEqual("newOne",
								((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person.ApplicationAuthenticationInfo.Password);
		}

		[Test]
		public void VerifyWhenPasswordCouldNotBeChanged()
		{
			initializeExpectation();
			view.Stub(x => x.ShowValidationError());
			target.Initialize();
			target.Model.OldPassword = "oldOne";
			target.Model.NewPassword = "oldOne";
			target.Model.ConfirmPassword = "oldOne";
			target.Save();
		}

		[Test]
		public void VerifySaveWithErrorInModel()
		{
			initializeExpectation();
			view.Stub(x => x.ShowValidationError());

			target.Initialize();
			target.Model.NewPassword = "oldOne";
			target.Model.ConfirmPassword = "oldOne";
			target.Save();
		}

		[Test]
		public void VerifySaveWithErrorWhenNoNewPassword()
		{
			initializeExpectation();

			target.Initialize();
			target.Model.OldPassword = "ngt";
			target.Model.NewPassword = "";
			target.Model.ConfirmPassword = "";
			target.Save();
		}
	}
}

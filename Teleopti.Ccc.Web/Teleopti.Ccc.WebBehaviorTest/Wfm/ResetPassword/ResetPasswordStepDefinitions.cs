using System;
using System.Linq;
using Autofac;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.ResetPassword
{
	[Binding]
	public sealed class ResetPasswordStepDefinitions
	{
		[When(@"I choose to click reset password")]
		public void WhenIChooseToClickResetPassword()
		{
			Browser.Interactions.Click(".signin_forgot_password a");
		}

		[When(@"I choose to enter username in reset password form with '(.*)'")]
		public void ChooseToEnterUsernameInForm(string name)
		{
			Browser.Interactions.FillWith("#user-name", name);
		}

		[When(@"I click send password button")]
		public void ClickSendPasswordButton()
		{
			Browser.Interactions.Click("#send-forgot-password-mail-button");
		}

		[Then(@"I should see a notification about an email about further instructions")]
		public void SeeNotitificationAboutTheEmailBeeingSent()
		{
			Browser.Interactions.AssertJavascriptResultContains("return document.querySelector('#Password-change-success').style.display !== 'none'", "True");
		}


		[When(@"I view the reset password form as '(.*)' with password '(.*)'")]
		public void ViewTheResetFormWithValidToken(string name, string password)
		{
			using (LocalSystem.TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var repository = new PersonRepository(LocalSystem.UnitOfWork);
				var users = repository.FindAllSortByName();
	
				var tokenGenerator = IntegrationIoCTest.Container.Resolve<ITokenGenerator>();
				var token = tokenGenerator.CreateSecurityToken(users.ElementAt(0).Id ?? Guid.NewGuid(), new OneWayEncryption().CreateHash(password));
				Navigation.GoToResetPassword(token);
			}
		}

		[When(@"I fill the reset password form with mismatch of passwords")]
		public void FillTheResetFormWithIncorrectData()
		{
			Browser.Interactions.FillWith("[formcontrolname='password']", "Wrong");
			Browser.Interactions.FillWith("[formcontrolname='confirmPassword']", "Password");
		}

		[When(@"I fill the reset password form with shorter password than the policy")]
		public void FillTheResetFormWithShortPasswords()
		{
			Browser.Interactions.FillWith("[formcontrolname='password']", "s");
			Browser.Interactions.FillWith("[formcontrolname='confirmPassword']", "s");
		}

		[When(@"I fill the reset password form with password '(.*)'")]
		public void FillTheResetFormWithCorrectData(string password)
		{
			Browser.Interactions.FillWith("[formcontrolname='password']", password);
			Browser.Interactions.FillWith("[formcontrolname='confirmPassword']", password);
		}

		[When(@"press the reset password submit form button")]
		public void PressTheSubmitButton()
		{
			Browser.Interactions.Click("button[type='submit']");
		}

		[When(@"I press the reset password form logon button")]
		public void PressTheLogonButton()
		{
			Browser.Interactions.Click("[nz-button]");
		}

		[When(@"I click the cancel button at the user submit form")]
		public void PressTheCancelButtonAtTheUserSubmitForm()
		{
			Browser.Interactions.Click("#back-button");
		}

		[When(@"press the reset password cancel button")]
		public void PressTheCancelButton()
		{
			Browser.Interactions.Click("button[type='button']");
		}

		[When(@"press the reset password logon button")]
		public void PressTheResetPasswordButton()
		{
			Browser.Interactions.Click("button[type='button']");
		}

		[Then(@"I should see a error message for the reset password form")]
		public void ViewErrorMessageForForm()
		{
			Browser.Interactions.AssertJavascriptResultContains("return document.querySelector('nz-form-explain') !== null", "True");
		}

		[Then(@"I should see a policy error message for the reset password form")]
		public void ViewPolicyErrorMessageForForm()
		{
			Browser.Interactions.AssertJavascriptResultContains("return document.querySelector('nz-alert[data-policy-error]') !== null", "True");
		}

		[Then(@"I should see a success message for the reset password form")]
		public void FillInformationAndBuffton()
		{
			Browser.Interactions.AssertJavascriptResultContains("return document.querySelector('success-message') !== null", "True");
		}

		[Given(@"I am viewing the reset password form with invalid token")]
		public void ViewTheResetFormWithInValidToken()
		{
			Navigation.GoToResetPassword("thisisaninvalidtoken");
		}


		[Then(@"I should see an error message about an invalid token")]
		public void SeeErrorMessageWithInvalidToken()
		{
			Browser.Interactions.AssertJavascriptResultContains("return document.querySelector('invalid-link-message') !== null", "True");
		}
	}
}

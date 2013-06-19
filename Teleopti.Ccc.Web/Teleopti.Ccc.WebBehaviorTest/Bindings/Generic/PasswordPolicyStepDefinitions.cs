using System;
using System.IO;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PasswordPolicyStepDefinitions
	{
		private IDisposable _timeoutScope;

		[BeforeScenario("PasswordPolicy")]
		public void BeforePasswordPolicyScenario()
		{
			_timeoutScope = Browser.TimeoutScope(TimeSpan.FromSeconds(20));
		}

		[AfterScenario("PasswordPolicy")]
		public void AfterPasswordPolicyScenario()
		{
			var targetTestPasswordPolicyFile = Path.Combine(Path.Combine(IniFileInfo.SitePath, "bin"), "PasswordPolicy.xml");
			if (File.Exists(targetTestPasswordPolicyFile))
				File.Delete(targetTestPasswordPolicyFile);
			_timeoutScope.Dispose();
			_timeoutScope = null;
		}

		[Given(@"There is a password policy with")]
		public void GivenThereIsAPasswordPolicyWith(Table table)
		{
			var targetTestPasswordPolicyFile = Path.Combine(Path.Combine(IniFileInfo.SitePath, "bin"), "PasswordPolicy.xml");
			if (File.Exists(targetTestPasswordPolicyFile))
				return;
			var contents = File.ReadAllText("Data\\PasswordPolicy.xml");
			var passwordPolicy = table.CreateInstance<PasswordPolicyConfigurable>();

			contents = contents.Replace("_MaxNumberOfAttempts_", passwordPolicy.MaxNumberOfAttempts.ToString());
			contents = contents.Replace("_InvalidAttemptWindow_", passwordPolicy.InvalidAttemptWindow.ToString());
			contents = contents.Replace("_PasswordValidForDayCount_", passwordPolicy.PasswordValidForDayCount.ToString());
			contents = contents.Replace("_PasswordExpireWarningDayCount_", passwordPolicy.PasswordExpireWarningDayCount.ToString());

			if (passwordPolicy.Rule1.Equals("PasswordLengthMin8"))
			{
			}

			File.WriteAllText(targetTestPasswordPolicyFile, contents);
		}

		[Given(@"I have user logon details with")]
		public void GivenIHaveUserLogonDetailsWith(Table table)
		{
			var userLogonDetai = table.CreateInstance<UserLogonDetailConfigurable>();
			UserFactory.User().Setup(userLogonDetai);
		}

		[When(@"I click skip button")]
		public void WhenIClickSkipButton()
		{
			Pages.Pages.SignInPage.SkipButton.EventualClick();
		}

		[When(@"I change my password with")]
		public void WhenIChangeMyPasswordWith(Table table)
		{
			var password = table.CreateInstance<PasswordConfigurable>();
			Pages.Pages.SignInPage.ChangePassword(password.Password, password.ConfirmedPassword, password.OldPassword);
		}

		[Then(@"I should see change password page with warning '(.*)'")]
		public void ThenIShouldSeeChangePasswordPageWithWarning(string resourceText)
		{
			EventualAssert.That(() => Pages.Pages.SignInPage.PasswordExpireSoonError.DisplayVisible(), Is.True);
			EventualAssert.That(() => Pages.Pages.SignInPage.PasswordExpireSoonError.InnerHtml, new StringContainsAnyLanguageResourceConstraint(resourceText));
		}

		[Then(@"I should see must change password page with warning '(.*)'")]
		public void ThenIShouldSeeMustChangePasswordPageWithWarning(string resourceText)
		{
			EventualAssert.That(() => Pages.Pages.SignInPage.PasswordAlreadyExpiredError.DisplayVisible(), Is.True);
			EventualAssert.That(() => Pages.Pages.SignInPage.PasswordAlreadyExpiredError.InnerHtml, new StringContainsAnyLanguageResourceConstraint(resourceText));
		}

		[Then(@"I should not see skip button")]
		public void ThenIShouldNotSeeSkipButton()
		{
			EventualAssert.That(() => Pages.Pages.SignInPage.SkipButton.DisplayVisible(), Is.False);
		}
		
		[Then(@"I should see an error '(.*)'")]
		public void ThenIShouldSeeAnError(string resourceText)
		{
			EventualAssert.That(() => Pages.Pages.SignInPage.ChangePasswordErrorMessage.Text, new StringContainsAnyLanguageResourceConstraint(resourceText));
		}

	}
}
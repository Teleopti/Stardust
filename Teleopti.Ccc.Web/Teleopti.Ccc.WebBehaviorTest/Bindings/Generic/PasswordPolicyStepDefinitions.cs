using System.IO;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PasswordPolicyStepDefinitions
	{
		[Given(@"There is a password policy with")]
		public void GivenThereIsAPasswordPolicyWith(Table table)
		{
			var targetTestPasswordPolicyFile = Path.Combine(Path.Combine(IniFileInfo.SitePath, "bin"), "PasswordPolicy.xml");
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

		[Then(@"I should see change password page with warning '(.*)'")]
		public void ThenIShouldSeeChangePasswordPageWithWarning(string resourceText)
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.PasswordExpireSoonError.DisplayVisible(), Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.PasswordExpireSoonError.InnerHtml, new StringContainsAnyLanguageResourceContraint(resourceText));
		}

		[Then(@"I should see must change password page with warning '(.*)'")]
		public void ThenIShouldSeeMustChangePasswordPageWithWarning(string resourceText)
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.PasswordAlreadyExpiredError.DisplayVisible(), Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.PasswordAlreadyExpiredError.InnerHtml, new StringContainsAnyLanguageResourceContraint(resourceText));
		}

		[Then(@"I should not see skip button")]
		public void ThenIShouldNotSeeSkipButton()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.SkipButton.DisplayVisible(), Is.False);
		}

		[Then(@"I click skip button")]
		public void ThenIClickSkipButton()
		{
			Pages.Pages.CurrentSignInPage.SkipButton.EventualClick();
		}


	}
}
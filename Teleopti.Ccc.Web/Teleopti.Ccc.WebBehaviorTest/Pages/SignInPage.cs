using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;
using List = WatiN.Core.List;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class SignInPage : Page
	{

	    [FindBy(Id = "Signin-error")]
		public Element ValidationSummary { get; set; }

		[FindBy(Id = "PasswordExpireSoon")]
		public Div PasswordExpireSoonError { get; set; }

		[FindBy(Id = "DataSources")]
		public List DataSources { get; set; }

		[FindBy(Id = "PasswordAlreadyExpired")]
		public Div PasswordAlreadyExpiredError { get; set; }

		[FindBy(Id = "Login-button")]
		public Button LoginButton;

		[FindBy(Id = "Skip-button")]
		public Button SkipButton { get; set; }

		[FindBy(Id = "New-password")]
		public TextField NewPassword { get; set; }
		[FindBy(Id = "Confirm-new-password")]
		public TextField ConfirmNewPassword { get; set; }
		[FindBy(Id = "Old-password")]
		public TextField OldPassword { get; set; }
		[FindBy(Id = "Change-password-button")]
		public Button ChangePasswordButton { get; set; }
		[FindBy(Id = "Password-change-error")]
		public Div ChangePasswordErrorMessage { get; set; }

		public void ChangePassword(string newPassword, string confirmedNewPassword, string oldPassword)
		{
			NewPassword.ChangeValue(newPassword);
			ConfirmNewPassword.ChangeValue(newPassword);
			OldPassword.ChangeValue(oldPassword);
			ChangePasswordButton.EventualClick();
		}

		public void SignInWindows()
		{
		    LoginButton.EventualClick();
		}

		public void SelectFirstBusinessUnit()
		{
			throw new System.NotImplementedException();
		}

		public void SelectBusinessUnitByName(string name)
		{
			Document.Element(Find.BySelector("li a:contains('" + name + "')")).EventualClick();
		}

		public void ClickBusinessUnitOkButton()
		{
			throw new System.NotImplementedException();
		}
	}
}
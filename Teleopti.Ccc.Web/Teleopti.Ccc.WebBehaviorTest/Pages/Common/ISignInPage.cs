using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public interface ISignInPage
	{
		TextField UserNameTextField { get; }

		Element ValidationSummary { get; }
		Div PasswordExpireSoonError { get; }
		Div PasswordAlreadyExpiredError { get; }
		Button SkipButton { get; }
		Div ChangePasswordErrorMessage { get; }

		void SelectApplicationTestDataSource();
		void SelectWindowsTestDataSource();

		void SignInApplication(string username, string password);
		void SignInWindows();
		
		void SelectFirstBusinessUnit();
		void SelectBusinessUnitByName(string name);
		void ClickBusinessUnitOkButton();

		void ChangePassword(string newPassword, string confirmedNewPassword, string oldPassword);
	}
}
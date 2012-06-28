using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public interface ISignInPage
	{
		TextField UserNameTextField { get; }

		Div ValidationSummary { get; }

		void SelectApplicationTestDataSource();
		void SignInApplication(string username, string password);
		void SignInWindows();
		void ClickApplicationOkButton();
		void SelectFirstBusinessUnit();
		void ClickBusinessUnitOkButton();
	}
}
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public interface ISignInPage
	{
		TextField UserNameTextField { get; }

		Element ValidationSummary { get; }

		void SelectApplicationTestDataSource();
		void SignInApplication(string username, string password);
		void TrySignInApplication(string username, string password);
		void SignInWindows();
		void SelectFirstBusinessUnit();
		void ClickBusinessUnitOkButton();
	}
}
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public interface ISignInPage
	{
		TextField UserNameTextField { get; }

		Element ValidationSummary { get; }

		void SelectApplicationTestDataSource();
		void SelectWindowsTestDataSource();

		void SignInApplication(string username, string password);
		void SignInWindows();
		
		void SelectFirstBusinessUnit();
		void SelectBusinessUnitByName(string name);
		void ClickBusinessUnitOkButton();
	}
}
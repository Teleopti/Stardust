using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class SignInNewPage : Page
	{

		[FindBy(Id = "Username-input")]
		public TextField UserNameTextField { get; set; }
		[FindBy(Id = "Password-input")]
		public TextField PasswordTextField { get; set; }

		[FindBy(Id = "Login-button")]
		public Button LoginButton;

		[FindBy(Id = "DataSources")]
		public List DataSourcesList;

		[FindBy(Id = "BusinessUnits")]
		public List BusinessUnitsList;

		public void SelectTestDataApplicationLogon()
		{
			DataSourcesList.WaitUntilDisplayed();
			DataSourcesList.Link(Find.ByText("TestData")).EventualClick();
		}

		public void SelectBusinessUnitByName(string businessUnit)
		{
			BusinessUnitsList.WaitUntilDisplayed();
			BusinessUnitsList.Link(Find.ByText(businessUnit)).EventualClick();
		}

		public void SignInApplication(string userName, string password)
		{
			TrySignInApplication(userName, password);
		}

		public void TrySignInApplication(string username, string password)
		{
			UserNameTextField.ChangeValue(username);
			PasswordTextField.ChangeValue(password);
			LoginButton.EventualClick();
		}

		
	}
}
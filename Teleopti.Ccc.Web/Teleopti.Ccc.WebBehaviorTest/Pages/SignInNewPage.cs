using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class SignInNewPage : Page, ISignInNewPage
	{

		[FindBy(Id = "Username-input")]
		public TextField UserNameTextField { get; set; }
		[FindBy(Id = "Password-input")]
		public TextField PasswordTextField { get; set; }

		[FindBy(Id = "Login-button")]
		public Button LoginButton;

		[FindBy(Id = "DataSources")]
		public List DataSourcesList;

		public void SelectTestDataApplicationLogon()
		{
			DataSourcesList.WaitUntilExists();
			DataSourcesList.ListItem(Find.ByText("TestData")).EventualClick();
		}

		public void SignInApplication(string userName, string password)
		{
			TrySignInApplication(userName, password);
		}

		public void TrySignInApplication(string username, string password)
		{
			UserNameTextField.Value = username;
			PasswordTextField.Value = password;
			LoginButton.EventualClick();
		}
	}
}
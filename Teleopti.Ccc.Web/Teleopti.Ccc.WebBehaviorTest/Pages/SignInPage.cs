using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class SignInPage : Page, ISignInPage
	{
		[FindBy(Id = "Username-input")]
		public TextField UserNameTextField { get; set; }
		[FindBy(Id = "Password-input")]
		public TextField PasswordTextField { get; set; }

		[FindBy(Id = "Signin-error")]
		public Element ValidationSummary { get; private set; }

		[FindBy(Id = "Login-button")]
		public Button LoginButton;

		public void SelectApplicationTestDataSource()
		{
		    Document.Element(Find.BySelector("li.application a:contains(TestData)")).EventualClick();
		}

		public void SelectWindowsTestDataSource()
		{
		    Document.Element(Find.BySelector("li.windows a:contains(TestData)")).EventualClick();
		}

		public void SignInApplication(string username, string password)
		{
		    UserNameTextField.ChangeValue(username);
		    PasswordTextField.ChangeValue(password);
		    LoginButton.EventualClick();
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
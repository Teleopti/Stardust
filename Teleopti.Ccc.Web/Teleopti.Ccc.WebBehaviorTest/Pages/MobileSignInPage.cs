using System;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class MobileSignInPage : Page, ISignInPage
	{
		[FindBy(Id = "ApplicationSignIn_SignIn_Password")] public TextField PasswordTextField;

		[FindBy(Id = "application-signin-button")] public Button SignInButton;
		[FindBy(Id = "ApplicationSignIn_SignIn_UserName")] public TextField UserNameTextField { get; set; }

		public Element ValidationSummary { get { return Document.Span(Find.ByClass("error")); } }

		[FindBy(Id = "businessunit-ok-button")]public Button SignInBusinessInitsOkButton;

		[FindBy(Id = "application-signin")]
		public Div ApplicationSignIn { get; set; }

		public RadioButtonCollection SigninDataSources
		{
			get { return Document.RadioButtons.Filter(Find.ByName("signin-sel-datasource")); }
		}

		public RadioButtonCollection SignInBusinessUnits
		{
			get { return Document.RadioButtons.Filter(Find.ByName("signin-sel-businessunit")); }
		}

		[FindBy(Id = "signout-button")]
		public Link SignoutButton { get; set; }

		public void SelectApplicationTestDataSource()
		{
			SigninDataSources.Filter(Find.ByValue("TestData")).First().Click();
		}

		public void SignInWindows()
		{
			throw new NotImplementedException();
		}

		public void ClickApplicationOkButton()
		{
			throw new NotImplementedException();
		}

		public void SignInApplication(string userName, string password)
		{
			SigninDataSources.Filter(Find.ByValue("TestData")).First().Click();
			UserNameTextField.Value = userName;
			PasswordTextField.Value = password;
			SignInButton.EventualClick();

			WaitForSigninResult();
		}

		private void WaitForSigninResult()
		{
			Func<bool> signedInOrBusinessUnitListExists = () =>
			                                              	{
																return SignoutButton.IESafeExists() ||
																	ErrorDisplayed() ||
																	BusinessUnitsDisplayed()
			                                              			;
			                                              	};
			var found = signedInOrBusinessUnitListExists.WaitUntil(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(5));
			if (!found)
				throw new ApplicationException("Waiting for signin result failed!");
		}

		private bool BusinessUnitsDisplayed()
		{
			return Document.RadioButton(Find.ByName("signin-sel-businessunit")).IESafeExists();
		}

		private bool ErrorDisplayed()
		{
			var span = Document.Span(Find.ByClass("error", false));
			if (span.IESafeExists())
			{
				if (span.Text == null)
					return false;
				return span.Text.Trim().Length > 0;
			}
			return false;
		}

		public void SelectFirstBusinessUnit()
		{
			SignInBusinessUnits.First().Click();
		}

		public void ClickBusinessUnitOkButton()
		{
			SignInBusinessInitsOkButton.Click();
		}
	}
}
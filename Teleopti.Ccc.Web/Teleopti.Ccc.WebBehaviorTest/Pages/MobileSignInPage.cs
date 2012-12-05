using System;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class MobileSignInPage : Page, ISignInPage
	{
		[FindBy(Id = "ApplicationSignIn_SignIn_Password")] public TextField PasswordTextField;

		[FindBy(Id = "application-signin-button")] public Button SignInButton;
		[FindBy(Id = "ApplicationSignIn_SignIn_UserName")] public TextField UserNameTextField { get; set; }

		public Element ValidationSummary { get { return Document.Span(QuicklyFind.ByClass("error")); } }
		public Div WarningMessage { get; private set; }

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
			SigninDataSources.Filter(Find.ByValue("TestData")).First().EventualClick();
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
			TrySignInApplication(userName,password);

			WaitForSigninResult();
		}

		public void TrySignInApplication(string userName, string password)
		{
			SigninDataSources.Filter(Find.ByValue("TestData")).First().EventualClick();
			UserNameTextField.Value = userName;
			PasswordTextField.Value = password;
			SignInButton.EventualClick();
		}

		private void WaitForSigninResult()
		{
			EventualAssert.That(() =>
			                    	{
			                    		if (SignoutButton.SafeExists())
			                    			return true;
			                    		if (Document.RadioButton(Find.ByName("signin-sel-businessunit")).SafeExists())
			                    			return true;
										var span = Document.Span(QuicklyFind.ByClass("error"));
			                    		if (span.SafeExists())
			                    		{
			                    			if (span.Text == null)
			                    				return false;
			                    			return span.Text.Trim().Length > 0;
			                    		}
			                    		return false;
			                    	}, Is.True);
		}

		public void SelectFirstBusinessUnit()
		{
			SignInBusinessUnits.First().EventualClick();
		}

		public void ClickBusinessUnitOkButton()
		{
			SignInBusinessInitsOkButton.EventualClick();
		}
	}
}
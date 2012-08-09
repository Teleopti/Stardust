using System;
using System.Linq;
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

		[FindBy(Id = "signout-button")] // Belongs to MobileReports
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
			SignInButton.Click();

			WaitUntilSignInOrBusinessUnitListOrErrorAppears();
		}

		private void WaitUntilSignInOrBusinessUnitListOrErrorAppears()
		{
			Exception exception = null;
			Func<bool> signedInOrBusinessUnitListExists =
				() =>
					{
						try
						{
							exception = null;
							return SignoutButton.Exists || SignInBusinessUnits.Any(e => e.Style.Display != "none");
							/*|| ErrorSpans.Any(e => e.Style != null && e.Style.Display != "none");*/
						}
						catch (UnauthorizedAccessException ex)
						{
							exception = ex;
							return false;
						}
					};
			if (!signedInOrBusinessUnitListExists.WaitUntil(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(5)))
			{
				if (exception != null)
				{
					throw exception;
				}
			}
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
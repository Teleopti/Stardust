using System;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class MobileSignInPage : SignInPageBase

	{
		[FindBy(Id = "ApplicationSignIn_SignIn_Password")] public TextField PasswordTextField;

		[FindBy(Id = "application-signin-button")] public Button SignInButton;
		[FindBy(Id = "ApplicationSignIn_SignIn_UserName")] public TextField UserNameTextField;
		[FindBy(Id = "businessunit-ok-button")]public Button SignInBusinessInitsOkButton;

		[FindBy(Id = "application-signin")]
		public Div ApplicationSignIn { get; set; }

		public RadioButtonCollection SigninDataSources
		{
			get { return Document.RadioButtons.Filter(Find.ByName("signin-sel-datasource")); }
		}
		public RadioButtonCollection SignInBusinessInits
		{
			get { return Document.RadioButtons.Filter(Find.ByName("signin-sel-businessunit")); }
		}

		[FindBy(Id = "signout-button")] // Belongs to MobileReports
			public Link SignoutButton { get; set; }


		public SpanCollection ErrorSpans
		{
			get { return Document.Spans.Filter(Find.ByClass("error")); }
		}

		public override string ErrorMessage
		{
			get { return string.Empty; }
		}

		public override void SelectFirstApplicationDataSource()
		{
			SigninDataSources.First().Click();
		}

		public override void ClickApplicationOkButton()
		{
			throw new NotImplementedException();
		}

		public override void SignInApplication(string userName, string password)
		{
			SigninDataSources.First().Click();
			UserNameTextField.Value = userName;
			PasswordTextField.Value = password;
			SignInButton.Click();

			WaitUntilSignInOrBusinessUnitListOrErrorAppears();
		}

		protected override void WaitUntilSignInOrBusinessUnitListOrErrorAppears()
		{
			Func<bool> signedInOrBusinessUnitListExists =
				() => SignoutButton.Exists || SignInBusinessInits.Any(e => e.Style.Display != "none");
					  /*|| ErrorSpans.Any(e => e.Style != null && e.Style.Display != "none");*/
			signedInOrBusinessUnitListExists.WaitUntil(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(5));
		}

		protected override void WaitUntilSignInOrErrorAppears()
		{
			Func<bool> SignedInExists = () => SignoutButton.Exists;
			SignedInExists.WaitUntil(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(10));
		}

		public override void SelectFirstBusinessUnit()
		{
			SignInBusinessInits.First().Click();
		}

		public override void ClickBusinessUnitOkButton()
		{
			
			SignInBusinessInitsOkButton.Click();
		}
	}
}
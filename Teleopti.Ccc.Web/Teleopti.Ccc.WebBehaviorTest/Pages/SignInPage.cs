using System;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class SignInPage : SignInPageBase
	{
		[FindBy(Id = "WindowsTabLink")] public Link WindowsTabLink;
		[FindBy(Id = "ApplicationTabLink")] public Link ApplicationTabLink;

		[FindBy(Id = "ApplicationDataSourceList")] public List ApplicationDataSourceList;
		[FindBy(Id = "WindowsDataSourceList")] public List WindowsDataSourceList;

		[FindBy(Id = "ApplicationOkButton")] public Button ApplicationOkButton;
		[FindBy(Id = "WindowsOkButton")] public Button WindowsOkButton;
		[FindBy(Id = "BusinessUnitOkButton")] public Button BusinessUnitOkButton;

		[FindBy(Id = "SignIn_UserName")] public TextField UserNameTextField;
		[FindBy(Id = "SignIn_Password")] public TextField PasswordTextField;

		[FindBy(Id = "BusinessUnitList")] public List BusinessUnitList;

		[FindBy(Id = "signout")]
		public Link SignOutLink;

		public override string ErrorMessage
		{
			get { return string.Empty; }
		}

		public override void SelectFirstApplicationDataSource()
		{
			ApplicationDataSourceList.ListItems.First().Click();
		}

		public override void ClickApplicationOkButton()
		{
			ApplicationOkButton.EventualClick();
		}

		public void SelectFirstWindowsDataSource()
		{
			WindowsDataSourceList.ListItems.First().Click();
		}

		public override void SelectFirstBusinessUnit()
		{
			BusinessUnitList.WaitUntilExists(5);
			BusinessUnitList.ListItems.First().Click();
		}

		public override void ClickBusinessUnitOkButton()
		{
			BusinessUnitOkButton.EventualClick();
		}

		public override void SignInApplication(string username, string password)
		{
			ApplicationTabLink.Click();
			SelectFirstApplicationDataSource();
			UserNameTextField.Value = username;
			PasswordTextField.Value = password;
			ApplicationOkButton.Click();

			WaitUntilSignInOrBusinessUnitListOrErrorAppears();
		}

		public void SignInWindows()
		{
			WindowsTabLink.Click();
			SelectFirstWindowsDataSource();
			WindowsOkButton.Click();
		}

		protected override void WaitUntilSignInOrBusinessUnitListOrErrorAppears()
		{
			Func<bool> SignedInOrBusinessUnitListExists = () => SignOutLink.Exists || BusinessUnitList.Exists;
			SignedInOrBusinessUnitListExists.WaitUntil(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(10));
		}

	}
}
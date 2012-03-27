using System;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class SignInPage : Page, ISignInPage
	{
		[FindBy(Id = "WindowsTabLink")] public Link WindowsTabLink;
		[FindBy(Id = "ApplicationTabLink")] public Link ApplicationTabLink;

		[FindBy(Id = "ApplicationDataSourceList")] public List ApplicationDataSourceList;
		[FindBy(Id = "WindowsDataSourceList")] public List WindowsDataSourceList;

		[FindBy(Id = "ApplicationOkButton")] public Button ApplicationOkButton;
		[FindBy(Id = "WindowsOkButton")] public Button WindowsOkButton;
		[FindBy(Id = "BusinessUnitOkButton")] public Button BusinessUnitOkButton;

		[FindBy(Id = "SignIn_UserName")] public TextField UserNameTextField { get; set; }
		[FindBy(Id = "SignIn_Password")] public TextField PasswordTextField;

		[FindBy(Id = "BusinessUnitList")] public List BusinessUnitList;

		[FindBy(Id = "signout")]
		public Link SignOutLink;


		public void SelectFirstApplicationDataSource()
		{
			ApplicationDataSourceList.ListItem(Find.First()).EventualClick();
		}

		public void ClickApplicationOkButton()
		{
			ApplicationOkButton.EventualClick();
		}

		public void SelectFirstWindowsDataSource()
		{
			WindowsDataSourceList.ListItem(Find.First()).EventualClick();
		}

		public void SelectFirstBusinessUnit()
		{
			BusinessUnitList.WaitUntilExists();
			BusinessUnitList.ListItem(Find.First()).EventualClick();
		}

		public void ClickBusinessUnitOkButton()
		{
			BusinessUnitOkButton.EventualClick();
			Func<bool> SignedInExists = () => SignOutLink.Exists;
			SignedInExists.WaitUntil(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(10));
		}

		public void SignInApplication(string username, string password)
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
			WindowsTabLink.EventualClick();
			SelectFirstWindowsDataSource();
			WindowsOkButton.EventualClick();
		}

		private void WaitUntilSignInOrBusinessUnitListOrErrorAppears()
		{
			Func<bool> SignedInOrBusinessUnitListExists = () => SignOutLink.Exists || BusinessUnitList.Exists;
			SignedInOrBusinessUnitListExists.WaitUntil(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(10));
		}
	}
}
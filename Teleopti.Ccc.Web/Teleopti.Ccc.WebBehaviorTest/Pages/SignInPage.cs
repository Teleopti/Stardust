using System;
using System.Threading;
using Teleopti.Ccc.Domain.Helper;
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

		public Div ValidationSummary
		{
			get
			{
				using (PerformanceOutput.ForOperation("ValidationSummary!!!!"))
				{
					return Document.Div(Find.ByClass("validation-summary-errors", false));
				}
			}
		}

		[FindBy(Id = "SignIn_Password")] public TextField PasswordTextField;

		[FindBy(Id = "BusinessUnitList")] public List BusinessUnitList;

		[FindBy(Id = "global-menu-list")] public List GlobalMenuList;

		[FindBy(Id = "signout")]
		public Link SignOutLink;


		public void SelectApplicationTestDataSource()
		{
			ApplicationDataSourceList.ListItem(Find.ByText("TestData")).EventualClick();
		}

		public void ClickApplicationOkButton()
		{
			ApplicationOkButton.EventualClick();
		}

		public void SelectWindowsTestDataSource()
		{
			WindowsDataSourceList.ListItem(Find.ByText("TestData")).EventualClick();
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
			SelectApplicationTestDataSource();
			UserNameTextField.Value = username;
			PasswordTextField.Value = password;
			ApplicationOkButton.Click();

			WaitForSigninResult();
		}

		public void SignInWindows()
		{
			WindowsTabLink.EventualClick();
			SelectWindowsTestDataSource();
			WindowsOkButton.EventualClick();
		}

		private void WaitForSigninResult()
		{
			// move this to the actual navigation, which is the one that actually acts too early?
			Func<bool> SignedInOrBusinessUnitListExists = () =>
			                                              	{
																return SignOutLink.IESafeExists() ||
																	   BusinessUnitList.IESafeExists() ||
																	   GlobalMenuList.IESafeExists() ||
																	   ValidationSummary.IESafeExists();
			                                              	};
			SignedInOrBusinessUnitListExists.WaitUntil(EventualTimeouts.Poll, EventualTimeouts.Timeout);
		}
	}
}
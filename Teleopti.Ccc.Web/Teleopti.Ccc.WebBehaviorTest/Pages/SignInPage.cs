﻿using System;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;
using List = WatiN.Core.List;

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

		public Element ValidationSummary { get { return Document.Div(QuicklyFind.ByClass("validation-summary-errors")); } }

		[FindBy(Id = "SignIn_Password")] public TextField PasswordTextField;

		[FindBy(Id = "BusinessUnitList")] public List BusinessUnitList;

		[FindBy(Id = "global-menu-list")] public List GlobalMenuList;

		[FindBy(Id = "signout")]
		public Link SignOutLink;

		[FindBy(Id = "signout-button")]
		public Link SignoutButton { get; set; }

		[FindBy(Id = "warning")]
		public Div WarningMessage { get; set; }

		public void SelectApplicationTestDataSource()
		{
			ApplicationDataSourceList.ListItem(Find.ByText("TestData")).EventualClick();
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
			TrySignInApplication(username, password);
			WaitForSigninResult();
		}

		public void TrySignInApplication(string username, string password)
		{
			ApplicationTabLink.EventualClick();
			SelectApplicationTestDataSource();
			UserNameTextField.Value = username;
			PasswordTextField.Value = password;
			ApplicationOkButton.EventualClick();
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
			EventualAssert.That(() => SignoutButton.SafeExists() ||
			                          SignOutLink.SafeExists() ||
			                          BusinessUnitList.SafeExists() ||
			                          GlobalMenuList.SafeExists() ||
			                          ValidationSummary.SafeExists(), Is.True);
		}
	}
}
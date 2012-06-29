using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class SharedSignInStepDefinitions
	{
		private bool _hasPermission = true;
		private bool _singleBusinessUnit = false;

		[Given(@"I dont have permission to sign in")]
		public void GivenIDontHavePermissionToSignIn()
		{
			_hasPermission = false;
		}

		[When(@"I sign in by user name")]
		public void WhenISignInByApplicationAuthentication()
		{
			Navigation.GotoGlobalSignInPage();
			string userName;
			if (_hasPermission)
			{
				if (_singleBusinessUnit)
				{
					userName = UserTestData.PersonApplicationUserSingleBusinessUnitUserName;
			}
			else
			{
					userName = UserTestData.PersonApplicationUserName;
				}
			}
			else
			{
				userName = UserTestData.PersonWithNoPermissionUserName;
			}
			Pages.Pages.CurrentSignInPage.SignInApplication(userName, TestData.CommonPassword);
		}

		[Given(@"I am a (?:mobileuser|user) with multiple business units")]
		public void GivenIAmAUserWithMultipleBusinessUnits()
		{
			_singleBusinessUnit = false;
			_hasPermission = true;
		}

		[Given(@"I am a mobileuser with a single business unit")]
		[Given(@"I am a user with a single business unit")]
		public void GivenIAmAUserWithASingleBusinessUnit()
		{
			_singleBusinessUnit = true;
			_hasPermission = true;
		}

		[When(@"I select a business unit")]
		public void WhenISelectABusinessUnit()
		{
			Pages.Pages.CurrentSignInPage.SelectFirstBusinessUnit();
			Pages.Pages.CurrentSignInPage.ClickBusinessUnitOkButton();
		}

		[Then(@"I should be signed in")]
		public void ThenIShouldBeSignedIn()
		{
			EventualAssert.That(() => Browser.Current.Link("signout").Exists || Browser.Current.Link("signout-button").Exists, Is.True);
		}

		[When(@"I sign in by user name and wrong password")]
		public void WhenISignInByUserNameAndWrongPassword()
		{
			Navigation.GotoGlobalSignInPage();
			Pages.Pages.CurrentSignInPage.SignInApplication(UserTestData.PersonApplicationUserSingleBusinessUnitUserName, "wrong password");
		}

		[Then(@"I should see an log on error")]
		public void ThenIShouldSeeAnLogOnError()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.ValidationSummary.Text, new StringContainsAnyLanguageResourceContraint("LogOnFailedInvalidUserNameOrPassword"));
		}

		[Then(@"I should not be signed in")]
		public void ThenIAmNotSignedIn()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.UserNameTextField.Exists, Is.True);
		}

		[When(@"I sign in by Windows credentials")]
		public void WhenISignInByWindowsAuthentication()
		{
			Navigation.GotoGlobalSignInPage();
			if (_singleBusinessUnit)
			{
				ScenarioContext.Current.Pending();
				return;
			}
			Pages.Pages.CurrentSignInPage.SignInWindows();
		}

	}


	public class StringContainsAnyLanguageResourceContraint : Constraint
	{
		private readonly List<string> _texts = new List<string>();

		public StringContainsAnyLanguageResourceContraint(string resource)
		{
			// add any browser language in which tests need to run on here
			_texts.Add(Resources.ResourceManager.GetString(resource, new CultureInfo("en-US")));
			_texts.Add(Resources.ResourceManager.GetString(resource, new CultureInfo("sv-SE")));
		}

		public override bool Matches(object actual)
		{
			this.actual = actual;
			var actualString = (string)actual;
			return _texts.Any(actualString.Contains);
		}

		public override void WriteDescriptionTo(MessageWriter writer)
		{
			writer.WriteMessageLine("Should contain a string in any language: ");
			_texts.ForEach(s =>
			               	{
								writer.WriteMessageLine("");
			               		writer.WriteExpectedValue(s);
			               	});
		}
	}
}
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
using Teleopti.Ccc.WebBehaviorTest.Data.User;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class SharedSignInStepDefinitions
	{
		[When(@"I open the sign in page")]
		[Given(@"I am viewing the sign in page")]
		public void GivenIAmAtTheSignInPage()
		{
			Navigation.GotoGlobalSignInPage();
		}

		[Given(@"I am viewing the mobile sign in page")]
		public void GivenIAmAtTheMobileSignInPage()
		{
			Navigation.GotoMobileReportsSignInPage(string.Empty);
		}

		[Given(@"I dont have permission to sign in")]
		public void GivenIDontHavePermissionToSignIn()
		{
			UserFactory.User().Setup(new UserNoPermission());
		}

		[Given(@"I am a (.*)user with multiple business units")]
		public void GivenIAmAUserWithMultipleBusinessUnits(string mobile)
		{
			if (mobile.Contains("mobile"))
			{
				UserFactory.User().Setup(new Supervisor());
				UserFactory.User().Setup(new SupervisorSecondBusinessUnit());
			}
			else
			{
				UserFactory.User().Setup(new Agent());
				UserFactory.User().Setup(new AgentSecondBusinessUnit());
			}
		}

		[Given(@"I am a (.*)user with a single business unit")]
		public void GivenIAmAUserWithASingleBusinessUnit(string mobile)
		{
			if (mobile.Contains("mobile"))
				UserFactory.User().Setup(new Supervisor());
			else
				UserFactory.User().Setup(new Agent());
		}

		[When(@"I sign in by windows credentials")]
		public void WhenISignInByWindowsAuthentication()
		{
			if (!UserFactory.User().HasSetup<AgentSecondBusinessUnit>())
			{
				ScenarioContext.Current.Pending();
				return;
			}
			Pages.Pages.CurrentSignInPage.SignInWindows();
		}

		[When(@"I sign in by user name and wrong password")]
		public void WhenISignInByUserNameAndWrongPassword()
		{
			var userName = UserFactory.User().MakeUser();
			Pages.Pages.CurrentSignInPage.SignInApplication(userName, "wrong password");
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

		[Then(@"I should see an log on error")]
		public void ThenIShouldSeeAnLogOnError()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.ValidationSummary.Text, new StringContainsAnyLanguageResourceContraint("LogOnFailedInvalidUserNameOrPassword"));
		}

		[Then(@"I should not be signed in")]
		[Then(@"I should be signed out")]
		[Then(@"I should see the login page")]
		public void ThenIAmNotSignedIn()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.UserNameTextField.Exists, Is.True);
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
			if (actual == null)
				return false;
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
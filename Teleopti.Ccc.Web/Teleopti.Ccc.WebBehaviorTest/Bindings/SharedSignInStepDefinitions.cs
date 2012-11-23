using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TechTalk.SpecFlow;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class SharedSignInStepDefinitions
	{


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



		[Then(@"I should see an error message ""(.*)""")]
		public void ThenIShouldSeeAnErrorMessage(string msg)
		{
		}


		[Then(@"I should not be signed in")]
		[Then(@"I should be signed out")]
		[Then(@"I should see the login page")]
		public void ThenIAmNotSignedIn()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.UserNameTextField.Exists, Is.True);
		}

		[Then(@"I should be signed out from MobileReports")]
		public void ThenIShouldBeSignedOutFromMobileReports()
		{
			// when test on desktop browser, cannot detect it's a mobile browser
			// so when signout, it goes to common signin page, not mobile signin page
			EventualAssert.That(
				() => Pages.Pages.MobileSignInPage.UserNameTextField.Exists || Pages.Pages.SignInPage.UserNameTextField.Exists,
				Is.True);
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TechTalk.SpecFlow;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class SharedSignInStepDefinitions
	{
		[Given(@"I am a user with multiple business units")]
		public void GivenIAmAUserWithMultipleBusinessUnits()
		{
			UserFactory.User().Setup(new Agent());
			UserFactory.User().Setup(new AgentSecondBusinessUnit());
		}


		[Then(@"I should see an error message ""(.*)""")]
		public void ThenIShouldSeeAnErrorMessage(string msg)
		{
		}

		[Then(@"I should see the sign in page")]
		[Then(@"I should not be signed in")]
		[Then(@"I should be signed out")]
		[Then(@"I should be signed out from MobileReports")]
		public void ThenIAmNotSignedIn()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.UserNameTextField.Exists, Is.True);
		}
	}


	public class StringContainsAnyLanguageResourceContraint : Constraint
	{
		private readonly List<string> _texts = new List<string>();

		public StringContainsAnyLanguageResourceContraint(string resourceOrText)
		{
			if (resourceOrText.Contains(" "))
				resourceOrText = CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(resourceOrText).Replace(" ", "");
			// add any browser language in which tests need to run on here
			_texts.Add(Resources.ResourceManager.GetString(resourceOrText, new CultureInfo("en-US")));
			_texts.Add(Resources.ResourceManager.GetString(resourceOrText, new CultureInfo("sv-SE")));
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
using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Toggle
{
	[Binding]
	public class ToggleStepDefinition
	{
		private bool reply;

		[When(@"I query toggle service for '(.*)'")]
		public void WhenIQueryOutofprocessToggleServiceFor(string flag)
		{
			var toggleQuerier = new ToggleQuerier(TestSiteConfigurationSetup.URL.ToString());
			reply = toggleQuerier.IsEnabled((Toggles)Enum.Parse(typeof(Toggles), flag));
		}

		[When(@"I query toggle service for '(.*)' by loading them all")]
		public void WhenIQueryOutofprocessToggleServiceForByLoadingThemAll(string flag)
		{
			var toggleQuerier = new ToggleQuerier(TestSiteConfigurationSetup.URL.ToString());
			toggleQuerier.FillAllToggles();
			reply = toggleQuerier.IsEnabled((Toggles)Enum.Parse(typeof(Toggles), flag));
		}

		[Then(@"I should get '(.*)' back")]
		public void ThenIShouldGetBack(bool theReply)
		{
			theReply.Should().Be.EqualTo(reply);
		}

		public static void CheckIfRunTestDueToToggleFlags()
		{
			const string ignoreMessage = "Ignore toggle {0} because it is {1}.";

			var toggleQuerier = new ToggleQuerier(TestSiteConfigurationSetup.URL.ToString());
			var matchingEnkelsnuffs = new Regex(@"\'(.*)\'");
			var tags = ScenarioContext.Current.ScenarioInfo.Tags.Union(FeatureContext.Current.FeatureInfo.Tags).ToArray();

			var allOnlyRunIfEnabled = tags.Where(s => s.StartsWith("OnlyRunIfEnabled"))
				.Select(onlyRunIfEnabled => (Toggles)Enum.Parse(typeof(Toggles), matchingEnkelsnuffs.Match(onlyRunIfEnabled).Groups[1].ToString()));
			var allOnlyRunIfDisabled = tags.Where(s => s.StartsWith("OnlyRunIfDisabled"))
				.Select(onlyRunIfDisabled => (Toggles)Enum.Parse(typeof(Toggles), matchingEnkelsnuffs.Match(onlyRunIfDisabled).Groups[1].ToString()));

			foreach (var toggleOnlyRunIfDisabled in allOnlyRunIfDisabled.Where(toggleQuerier.IsEnabled))
			{
				Assert.Ignore(ignoreMessage, toggleOnlyRunIfDisabled, "enabled");
			}

			foreach (var toggleOnlyRunIfEnabled in allOnlyRunIfEnabled.Where(toggleOnlyRunIfEnabled => !toggleQuerier.IsEnabled(toggleOnlyRunIfEnabled)))
			{
				Assert.Ignore(ignoreMessage, toggleOnlyRunIfEnabled, "disabled");
			}
		}
	}
}
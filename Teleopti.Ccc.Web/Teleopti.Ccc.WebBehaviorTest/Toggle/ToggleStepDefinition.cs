using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
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

		public static void IgnoreScenarioIfDisabledByToggle()
		{
			var matchingEnkelsnuffs = new Regex(@"\'(.*)\'");

			var tags = ScenarioContext.Current.ScenarioInfo.Tags.Union(FeatureContext.Current.FeatureInfo.Tags).ToArray();
			var runIfEnabled = tags.Where(s => s.StartsWith("OnlyRunIfEnabled"))
				.Select(onlyRunIfEnabled => (Toggles)Enum.Parse(typeof(Toggles), matchingEnkelsnuffs.Match(onlyRunIfEnabled).Groups[1].ToString()));
			var runIfDisabled = tags.Where(s => s.StartsWith("OnlyRunIfDisabled"))
				.Select(onlyRunIfDisabled => (Toggles)Enum.Parse(typeof(Toggles), matchingEnkelsnuffs.Match(onlyRunIfDisabled).Groups[1].ToString()));

			runIfEnabled.ForEach(t =>
			{
				if (!LocalSystem.Toggles.IsEnabled(t))
					Assert.Ignore("Ignoring scenario {0} because toggle {1} is disabled", ScenarioContext.Current.ScenarioInfo.Title, t);
			});

			runIfDisabled.ForEach(t =>
			{
				if (LocalSystem.Toggles.IsEnabled(t))
					Assert.Ignore("Ignoring scenario {0} because toggle {1} is enabled", ScenarioContext.Current.ScenarioInfo.Title, t);
			});
			
		}

		public static bool CheckToggleEnabled(Toggles toggle)
		{
			return LocalSystem.Toggles.IsEnabled(toggle);
		}
	}
}
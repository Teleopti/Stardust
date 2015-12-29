﻿using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

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

		private static readonly Lazy<ToggleQuerier> _toggleQueryInstance = new Lazy<ToggleQuerier>(() =>
		{
			var toggles = new ToggleQuerier(TestSiteConfigurationSetup.URL.ToString());
			toggles.FillAllToggles();
			return toggles;
		});

		public static void CheckIfRunTestDueToToggleFlags()
		{
			const string ignoreMessage = "Ignore toggle {0} because it is {1}.";

			var matchingEnkelsnuffs = new Regex(@"\'(.*)\'");
			var tags = ScenarioContext.Current.ScenarioInfo.Tags.Union(FeatureContext.Current.FeatureInfo.Tags).ToArray();

			var allOnlyRunIfEnabled = tags.Where(s => s.StartsWith("OnlyRunIfEnabled"))
				.Select(onlyRunIfEnabled => (Toggles)Enum.Parse(typeof(Toggles), matchingEnkelsnuffs.Match(onlyRunIfEnabled).Groups[1].ToString()));
			var allOnlyRunIfDisabled = tags.Where(s => s.StartsWith("OnlyRunIfDisabled"))
				.Select(onlyRunIfDisabled => (Toggles)Enum.Parse(typeof(Toggles), matchingEnkelsnuffs.Match(onlyRunIfDisabled).Groups[1].ToString()));

			foreach (var toggleOnlyRunIfDisabled in allOnlyRunIfDisabled.Where(_toggleQueryInstance.Value.IsEnabled))
			{
				Assert.Ignore(ignoreMessage, toggleOnlyRunIfDisabled, "enabled");
			}

			foreach (var toggleOnlyRunIfEnabled in allOnlyRunIfEnabled.Where(toggleOnlyRunIfEnabled => !_toggleQueryInstance.Value.IsEnabled(toggleOnlyRunIfEnabled)))
			{
				Assert.Ignore(ignoreMessage, toggleOnlyRunIfEnabled, "disabled");
			}
		}

		public static bool CheckToggleEnabled(Toggles toggle)
		{
			return _toggleQueryInstance.Value.IsEnabled(toggle);
		}
	}
}
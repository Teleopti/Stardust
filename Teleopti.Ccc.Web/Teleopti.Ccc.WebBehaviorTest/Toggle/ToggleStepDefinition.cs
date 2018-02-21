﻿using NUnit.Framework;
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
			reply = toggleQuerier.IsEnabled((Toggles) Enum.Parse(typeof(Toggles), flag));
		}

		[When(@"I query toggle service for '(.*)' by loading them all")]
		public void WhenIQueryOutofprocessToggleServiceForByLoadingThemAll(string flag)
		{
			var toggleQuerier = new ToggleQuerier(TestSiteConfigurationSetup.URL.ToString());
			toggleQuerier.FillAllToggles();
			reply = toggleQuerier.IsEnabled((Toggles) Enum.Parse(typeof(Toggles), flag));
		}

		[Then(@"I should get '(.*)' back")]
		public void ThenIShouldGetBack(bool theReply)
		{
			theReply.Should().Be.EqualTo(reply);
		}
	}
}
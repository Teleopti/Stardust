using System;
using System.IO;
using System.Net;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Toggle
{
	[Binding]
	public class ToggleStepDefinition
	{
		private bool reply;

		[When(@"I query outofprocess toggle service for '(.*)'")]
		public void WhenIQueryOutofprocessToggleServiceFor(string flag)
		{
			var toggleQuerier = new ToggleQuerier(new CurrentDataSource(new CurrentIdentity()), new Uri(TestSiteConfigurationSetup.Url, "ToggleHandler/IsEnabled").ToString());
			reply = toggleQuerier.IsEnabled((Toggles)Enum.Parse(typeof(Toggles), flag));
		}

		[Then(@"I should get '(.*)' back")]
		public void ThenIShouldGetBack(bool theReply)
		{
			theReply.Should().Be.EqualTo(reply);
		}
	}
}
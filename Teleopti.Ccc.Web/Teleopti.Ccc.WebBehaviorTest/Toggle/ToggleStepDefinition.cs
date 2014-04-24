using System;
using System.IO;
using System.Net;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Toggle
{
	[Binding]
	public class ToggleStepDefinition
	{
		private bool reply;

		[When(@"I query inprocess toggle service for '(.*)'")]
		public void WhenIQueryInprocessToggleServiceFor(string flag)
		{
			var uri = new Uri(TestSiteConfigurationSetup.Url, "ToggleHandler/IsEnabled?toggle=" + flag);
			var request = WebRequest.Create(uri);
			using (var reader = new StreamReader(request.GetResponse().GetResponseStream()))
			{
				reply = Convert.ToBoolean(reader.ReadToEnd());
			}
		}

		[When(@"I query outofprocess toggle service for '(.*)'")]
		public void WhenIQueryOutofprocessToggleServiceFor(string flag)
		{
			var toggleQuerier = new ToggleQuerier(new Uri(TestSiteConfigurationSetup.Url, "ToggleHandler/IsEnabled").ToString());
			reply = toggleQuerier.IsEnabled(flag);
		}

		[Then(@"I should get '(.*)' back")]
		public void ThenIShouldGetBack(bool theReply)
		{
			theReply.Should().Be.EqualTo(reply);
		}
	}
}
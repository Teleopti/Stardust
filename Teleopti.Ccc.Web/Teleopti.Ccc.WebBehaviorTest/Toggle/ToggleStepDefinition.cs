using System;
using System.IO;
using System.Net;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Toggle
{
	[Binding]
	public class ToggleStepDefinition
	{
		private bool reply;

		[When(@"I query toggle service for '(.*)'")]
		public void WhenIQueryToggleServiceFor(string flag)
		{
			var uri = new Uri(TestSiteConfigurationSetup.Url, "ToggleHandler/IsEnabled?toggle=" + flag);
			var request = WebRequest.Create(uri);
			using(var reader = new StreamReader(request.GetResponse().GetResponseStream()))
			{
				reply = Convert.ToBoolean(reader.ReadToEnd());
			}
		}

		[Then(@"I should get '(.*)' back")]
		public void ThenIShouldGetBack(bool theReply)
		{
			theReply.Should().Be.EqualTo(reply);
		}
	}
}
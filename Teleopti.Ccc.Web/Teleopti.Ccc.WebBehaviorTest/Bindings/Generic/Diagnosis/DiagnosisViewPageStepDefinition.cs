using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Diagnosis
{
	[Binding]
	public class DiagnosisViewPageStepDefinition
	{
		[When(@"I input a numberOfPings of (.*)")]
		public void WhenIInputANumberOfPingsOf(int value)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("input[name='numberOfPings']", value.ToString());
		}

		[When(@"I input a numberOfMessagesPerSecond of (.*)")]
		public void WhenIInputANumberOfMessagesPerSecondOf(int value)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("input[name=numberOfMessagesPerSecond]", value.ToString());
		}

		[Then(@"I should see a count of (.*) messages sent")]
		public void ThenIShouldSeeACountOfMessagesSent(int sent)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".sent-pings:contains('{0}')", sent));
		}

		[Then(@"I should see a count of (.*) messages left")]
		public void ThenIShouldSeeACountOfMessagesLeft(int left)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".left-pongs:contains('{0}')", left));
		}

	}
}

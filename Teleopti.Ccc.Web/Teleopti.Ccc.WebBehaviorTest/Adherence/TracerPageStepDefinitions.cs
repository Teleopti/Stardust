using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Adherence
{
	[Binding]
	public class TracerPageStepDefinitions
	{
		[Then(@"I should see process tracing '(.*)'")]
		public void ThenIShouldSeeProcessTracing(string userCode)
		{
			Browser.Interactions.AssertAnyContains(".process", userCode);
		}

		[Then(@"I should see process received data at '(.*)'")]
		public void ThenIShouldSeeProcessReceivedDataAt(string time)
		{
			Browser.Interactions.AssertAnyContains(".process", time);
		}

		[Then(@"I should see trace of state '(.*)' being '(.*)'")]
		public void ThenIShouldSeeTraceOfStateBeing(string stateCode, string text)
		{
			Browser.Interactions.AssertAnyContains(".trace", stateCode);
			Browser.Interactions.AssertAnyContains(".trace", text);
		}

	}
}
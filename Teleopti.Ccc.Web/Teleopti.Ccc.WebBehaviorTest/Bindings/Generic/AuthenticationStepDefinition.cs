using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AuthenticationStepDefinition
	{
		[When(@"I select the first data source")]
		public void WhenISelectTheFirstDataSource()
		{
			Browser.Interactions.ClickUsingJQuery("#DataSources a:first");
		}
	}
}

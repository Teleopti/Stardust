using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

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

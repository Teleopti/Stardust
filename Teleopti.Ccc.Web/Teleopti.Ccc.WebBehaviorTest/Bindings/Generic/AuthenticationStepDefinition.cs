using System.IO;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AuthenticationStepDefinition
	{
		[Given(@"I have access to two data sources")]
		public void GivenIHaveAccessToTwoDataSources()
		{
		}

		[When(@"I select the first data source")]
		public void WhenISelectTheFirstDataSource()
		{
			Browser.Interactions.ClickUsingJQuery("#DataSources a:first");
		}
	}
}

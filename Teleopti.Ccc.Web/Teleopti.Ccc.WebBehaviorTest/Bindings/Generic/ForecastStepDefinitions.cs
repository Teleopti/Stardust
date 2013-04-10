using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	[Binding]
	public class ForecastStepDefinitions
	{
		[Given(@"there is a forecast with")]
		public void GivenThereIsAForecastWith(Table table)
		{
			var forecast = table.CreateInstance<ForecastConfigurable>();
			UserFactory.User().Setup(forecast);
		}
	}
}
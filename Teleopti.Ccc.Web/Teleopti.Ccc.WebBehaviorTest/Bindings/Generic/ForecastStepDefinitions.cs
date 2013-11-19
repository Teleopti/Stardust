using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ForecastStepDefinitions
	{
		[Given(@"there is a forecast with")]
		public void GivenThereIsAForecastWith(Table table)
		{
			var forecast = table.CreateInstance<ForecastConfigurable>();
			DataMaker.Data().Apply(forecast);
		}
	}
}
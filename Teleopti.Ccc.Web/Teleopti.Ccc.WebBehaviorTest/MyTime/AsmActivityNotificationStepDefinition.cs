using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class AsmActivityNotificationStepDefinition
	{
		[Given(@"Alert Time setting is '(.*)' seconds")]
		public void GivenAlertTimeSettingIs(int alertTime)
		{
			DataMaker.Data().Apply(new AlertTimeSettingConfigurable(alertTime));
		}
	}
}
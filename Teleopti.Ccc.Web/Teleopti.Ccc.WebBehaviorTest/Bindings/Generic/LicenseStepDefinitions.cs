using TechTalk.SpecFlow;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class LicenseStepDefinitions
	{
		[Given(@"I am a user that does not have license to web mytime")]
		public void GivenIamAUserThatDoesNotHaveLicenseToWebMytime()
		{
			//ta bort license
			ScenarioContext.Current.Pending();
		}

	}
}
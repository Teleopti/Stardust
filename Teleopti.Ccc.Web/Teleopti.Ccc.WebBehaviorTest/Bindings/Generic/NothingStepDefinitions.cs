using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class NothingStepDefinitions
	{
		[Given(@"(I) have no .*")]
		[Given(@"'(.*)' (?:has|have) no .*")]
		public void GivenHasNoThing(string personName)
		{
			// create the person, nothing more
			DataMaker.Person(personName);
		}

	}
}
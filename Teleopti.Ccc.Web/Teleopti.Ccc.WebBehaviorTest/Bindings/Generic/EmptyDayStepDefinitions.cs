using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class EmptyDayStepDefinitions
	{
		[Given(@"(I) have an empty day with")]
		[Given(@"'(.*)' has an empty day with")]
		public void GivenHaveADayOffWith(string person, Table table)
		{
			var personEmptyDay = table.CreateInstance<PersonEmptyDayConfigurable>();
			DataMaker.Person(person).Apply(personEmptyDay);
		}
	}
}
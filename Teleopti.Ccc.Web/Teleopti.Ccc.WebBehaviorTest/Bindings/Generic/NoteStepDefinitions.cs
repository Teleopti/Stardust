using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class NoteStepDefinitions
	{
		[Given(@"'(.*)' have the note '(.*)'")]
		public void GivenHaveTheNote(string person, string note)
		{
			DataMaker.Person(person).Apply(new NoteConfigurable {Note = note});
		}

	}
}
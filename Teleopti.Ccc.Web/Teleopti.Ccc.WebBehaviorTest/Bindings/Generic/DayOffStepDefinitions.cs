using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class DayOffStepDefinitions
	{
		[Given(@"there is a dayoff with")]
		public void GivenThereIsAnActivity(Table table)
		{
			var dayOff = table.CreateInstance<DayOffTemplateConfigurable>();
			DataMaker.Data().Setup(dayOff);
		}

		[Given(@"there is a dayoff named '(.*)'")]
		public void GivenThereIsADayOffNamed(string name)
		{
			DataMaker.Data().Setup(new DayOffTemplateConfigurable() {Name = name});
		}

		[Given(@"'(.*)' have a day off with")]
		public void GivenHaveADayOffWith(string username, Table table)
		{
			var personDayOff = table.CreateInstance<PersonDayOffConfigurable>();
			DataMaker.Person(username).Setup(personDayOff);
		}
	}
}
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
			UserFactory.User().Setup(dayOff);
		}

		[Given(@"there is a dayoff named '(.*)'")]
		public void GivenThereIsADayOffNamed(string name)
		{
			UserFactory.User().Setup(new DayOffTemplateConfigurable() {Name = name});
		}

		[Given(@"'(.*)' have a day off with")]
		public void GivenHaveADayOffWith(string username, Table table)
		{
			var personDayOff = table.CreateInstance<PersonDayOffConfigurable>();
			UserFactory.User(username).Setup(personDayOff);
		}
	}
}
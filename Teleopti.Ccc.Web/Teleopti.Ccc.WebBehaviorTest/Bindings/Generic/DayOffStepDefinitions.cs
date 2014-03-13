using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using PersonDayOffConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.PersonDayOffConfigurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class DayOffStepDefinitions
	{
		[Given(@"there is a dayoff with")]
		public void GivenThereIsAnActivity(Table table)
		{
			var dayOff = table.CreateInstance<DayOffTemplateConfigurable>();
			DataMaker.Data().Apply(dayOff);
		}

		[Given(@"there is a dayoff named '(.*)'")]
		[Given(@"there is a day off named '(.*)'")]
		public void GivenThereIsADayOffNamed(string name)
		{
			DataMaker.Data().Apply(new DayOffTemplateConfigurable() {Name = name});
		}

		[Given(@"(I) have a day off named '(.*)' on '(.*)'")]
		[Given(@"'(.*)' has a day off named '(.*)' on '(.*)'")]
		public void GivenHaveADayOffNamedOn(string person, string name, DateTime date)
		{
			DataMaker.Person(person).Apply(new PersonDayOffConfigurable
				{
					Name = name,
					Date = date
				});
		}

		[Given(@"(I) have a day off with")]
		[Given(@"'(.*)' has a day off with")]
		[Given(@"'(.*)' have a day off with")] //wrong!
		public void GivenHaveADayOffWith(string person, Table table)
		{
			var personDayOff = table.CreateInstance<PersonDayOffConfigurable>();
			DataMaker.Person(person).Apply(personDayOff);
		}
	}
}
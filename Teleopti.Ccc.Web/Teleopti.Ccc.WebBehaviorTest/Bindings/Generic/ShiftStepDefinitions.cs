using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftStepDefinitions
	{
		[Given(@"(I) have a shift with")]
		[Given(@"'?(.*)'? has a shift with")]
		public void GivenIHaveAShiftWith(string person, Table table)
		{
			DataMaker.ApplyFromTable<ShiftConfigurable>(person, table);
		}

		[Given(@"(I) have a '(.*)' shift on '(.*)'")]
		[Given(@"'(.*)' has a '(.*)' shift on '(.*)'")]
		public void GivenPersonHasAShiftWithOn(string person, string shiftCategory, DateTime date)
		{
			DataMaker.Person(person).Apply(new ShiftConfigurable
				{
					ShiftCategory = shiftCategory,
					StartTime = date.AddHours(8),
					EndTime = date.AddHours(16),
					LunchStartTime = date.AddHours(12),
					LunchEndTime = date.AddHours(13),
				});
		}

	}
}
using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftStepDefinitions
	{
		[Given(@"(.*) have a shift with")]
		public void GivenIHaveAShiftWith(string person, Table table)
		{
			DataMaker.ApplyFromTable<ShiftConfigurable>(person, table);
		}

		[Given(@"(I) have a shift on '(.*)'")]
		[Given(@"'(.*)' has a shift on '(.*)'")]
		public void GivenJohnSmithHaveAShiftWithOn(string person, DateTime date)
		{
			DataMaker.Person(person).Apply(new ShiftConfigurable
				{
					StartTime = date.AddHours(8),
					EndTime = date.AddHours(16)
				});
		}

	}
}
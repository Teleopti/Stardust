using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using PersonAbsenceConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.PersonAbsenceConfigurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AbsenceStepDefinitions
	{
		[Given(@"there is an absence with")]
		public void GivenThereIsAnActivity(Table table)
		{
			DataMaker.ApplyFromTable<AbsenceConfigurable>(table);
		}

		[Given(@"there are absences")]
		public void GivenThereAreAbsences(Table table)
		{
			var absences = table.CreateSet<AbsenceConfigurable>();
			absences.ForEach(x => DataMaker.Data().Apply(x));
		}

		[Given(@"'?(I)'? have a full day absence named '(.*)' on '(.*)'")]
		[Given(@"'?(.*)'? has a full day absence named '(.*)' on '(.*)'")]
		public void GivenHaveAAbsenceWith(string person, string name, DateTime date)
		{
			DataMaker.Person(person).Apply(new FullDayAbsenceConfigurable
				{
					Scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario.Description.Name,
					Name = name,
					Date = date
				});
		}

		[Given(@"'?(I)'? have an absence with")]
		[Given(@"'?(.*)'? has an absence with")]
		public void GivenHaveAAbsenceWith(string userName, Table table)
		{
			DataMaker.ApplyFromTable<PersonAbsenceConfigurable>(userName, table);
		}

		[When(@"'(.*)' adds an absence for '(.*)' with")]
		public void WhenAddsAnAbsenceForWith(string adder, string person, Table table)
		{
			DataMaker.ApplyFromTable<PersonAbsenceConfigurable>(person, table);
		}

	}
}
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;
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
		
		[Given(@"there is an absence named '(.*)'")]
		public void GivenThereIsAnAbsenceNamed(string name)
		{
			DataMaker.Data().Apply(new AbsenceConfigurable{Name = name});
		}

		[Given(@"'?(I)'? have a absence with")]
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
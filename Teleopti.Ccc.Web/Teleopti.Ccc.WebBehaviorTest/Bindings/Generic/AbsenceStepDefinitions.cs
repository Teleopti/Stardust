using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AbsenceStepDefinitions
	{
		[Given(@"there is an absence with")]
		public void GivenThereIsAnActivity(Table table)
		{
			var absence = table.CreateInstance<AbsenceConfigurable>();
			UserFactory.User().Setup(absence);
		}

		[Given(@"'?(I)'? have a absence with")]
		[Given(@"'?(.*)'? has an absence with")]
		public void GivenHaveAAbsenceWith(string userName, Table table)
		{
			var personAbsence = table.CreateInstance<PersonAbsenceConfigurable>();
			UserFactory.User(userName).Setup(personAbsence);
		}
	}
}
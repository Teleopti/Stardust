using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class GroupPageStepDefinitions
	{
		[Given(@"there is a group page with")]
		public void GivenThereIsAGroupPageWith(Table table)
		{
			DataMaker.ApplyFromTable<GroupPageConfigurable>(table);
		}

		[Given(@"'(.*)' is on '(.*)' of group page '(.*)'")]
		public void GivenInOnOfGroupPage(string person, string @group, string page)
		{
			DataMaker.Person(person).Apply(new PersonGroupConfigurable
				{
					Page = page,
					Group = @group
				});
		}

	}
}
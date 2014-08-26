using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class BudgetStepDefinitions
	{
		[Given(@"there is a budgetgroup with")]
		public void GivenThereIsABudgetgroup(Table table)
		{
			var budgetConfigurable = table.CreateInstance<BudgetGroupConfigurable>();
			DataMaker.Data().Apply(budgetConfigurable);
		}

		[Given(@"there is a budgetday")]
		public void GivenThereIsAnBudgetday(Table table)
		{
			var budgetday = table.CreateInstance<BudgetdayConfigurable>();
			DataMaker.Data().Apply(budgetday);
		}

		[Given(@"(I) have absence time for")]
		[Given(@"'?(.*)'? has absence time for")]
		public void GivenHasAbsenceTimeFor(string userName, Table table)
		{
			DataMaker.ApplyFromTable<AbsenceTimeConfigurable>(userName, table);
		}
	}
}
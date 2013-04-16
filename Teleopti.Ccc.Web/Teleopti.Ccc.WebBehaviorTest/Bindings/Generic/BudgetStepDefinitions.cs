using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class BudgetStepDefinitions
	{
	

		[Given(@"there is a budgetgroup with")]
		public void GivenThereIsABudgetgroup(Table table)
		{
			var budgetConfigurable = table.CreateInstance<BudgetGroupConfigurable>();
			UserFactory.User().Setup(budgetConfigurable);
		}

		[Given(@"there is a budgetday")]
		public void GivenThereIsAnBudgetday(Table table)
		{
			var budgetday = table.CreateInstance<BudgetdayConfigurable>();
			UserFactory.User().Setup(budgetday);
		}

		//henke hitta på ett bättre namn
		[Given(@"there is a \(readonly\) PersonScheduleDayModel")]
		public void GivenThereIsAReadonlyPersonScheduleDayModel(Table table)
		{
			var scheduleReadOnlyProjection = table.CreateInstance<ReadModelScheduleProjectionConfigurable>();
			UserFactory.User().Setup(scheduleReadOnlyProjection);
		}

		[Given(@"there is absence time for")]
		public void GivenThereIsAbsenceTimeForTo(Table table)
		{
			var absenceTimeConfigurable = table.CreateInstance<AbsenceTimeConfigurable>();
			UserFactory.User().Setup(absenceTimeConfigurable);
		}
	}
}
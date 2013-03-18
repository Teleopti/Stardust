using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
    [Binding]
    public class BudgetStepDefinitions
    {
        [Given(@"there is  a budgetgroup '(.*)'")]
        public void GivenThereIsABudgetgroup(string name)
        {
            var budgetConfigurable = new BudgetGroupConfigurable(name);
            UserFactory.User().Setup(budgetConfigurable);
        }

        [Given(@"'(.*)' belong to budgetgroup '(.*)'")]
        public void GivenBelongToBudgetgroup(string userName, string budgetGroupName)
        {
            //Henke: keine ahnung.....
            //ScenarioContext.Current.Pending();
        }

		[Given(@"there is a budgetday")]
		public void GivenThereIsAnBudgetday(Table table)
		{
			var budgetday = table.CreateInstance<BudgetdayConfigurable>();
			UserFactory.User().Setup(budgetday);
		}

    }
}
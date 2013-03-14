using System;
using TechTalk.SpecFlow;
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

        [Given(@"there is an allowance '(.*)' for '(.*)' on '(.*)'")]
        public void GivenThereIsAnAllowanceForOn(int allowance, string budgetGroup, DateTime date)
        {
            //todo: allowance

            var budgetdayConfigurable = new BudgetdayConfigurable(budgetGroup,date);
            UserFactory.User().Setup(budgetdayConfigurable);
        }

    }
}
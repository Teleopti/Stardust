using System;
using TechTalk.SpecFlow;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
    [Binding]
    public class ReportsSteps
    {
		 [When(@"I click reports menu")]
		 public void WhenIClickReportsMenu()
		 {
			 ScenarioContext.Current.Pending();  
		 }
    }
}

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm
{
	[Binding]
	public class GlobalStepDefinitions
	{
		[Then(@"I should have available business units with")]
		public void ThenIShouldHaveAvailableBusinessUnitsWith(Table table)
		{
			var buNames = table.CreateSet<BusinessUnitData>();
			buNames.ForEach(bu => Browser.Interactions.AssertExistsUsingJQuery(string.Format(".navbar-left select option:contains({0})", bu.Name)));
		}


		[When(@"I pick business unit '(.*)'")]
		public void WhenIPickBusinessUnit(string businessUnit)
		{
			Browser.Interactions.SelectOptionByTextUsingJQuery(".navbar-left select", businessUnit);
		}

		public class BusinessUnitData
		{
			public string Name { get; set; }
		}
	}
}
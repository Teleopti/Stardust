using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.Web.StartWeb.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

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

			//need to manually trigger the change event
			Browser.Interactions.Javascript("angular.element(jQuery('.navbar-left select')).triggerHandler('change');");
		}

		public class BusinessUnitData
		{
			public string Name { get; set; }
		}
	}
}
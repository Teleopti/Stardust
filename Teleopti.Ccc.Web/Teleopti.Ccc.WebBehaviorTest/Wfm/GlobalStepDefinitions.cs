using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
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
			buNames.ForEach(bu => Browser.Interactions.AssertExistsUsingJQuery($"#business-unit-select li:contains({bu.Name})"));
		}


		[When(@"I pick business unit '(.*)'")]
		public void WhenIPickBusinessUnit(string businessUnit)
		{
			Browser.Interactions.Click("#business-unit-select");
			Browser.Interactions.ClickContaining("#business-unit-select li", businessUnit);
		}

		public class BusinessUnitData
		{
			public string Name { get; set; }
		}
	}
}
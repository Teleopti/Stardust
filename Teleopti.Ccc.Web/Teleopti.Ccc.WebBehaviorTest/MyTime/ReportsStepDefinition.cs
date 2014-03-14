using System.Globalization;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class ReportsSteps
	{
		[When(@"I click reports menu")]
		public void WhenIClickReportsMenu()
		{
			Browser.Interactions.Click("#reports");
		}

		[Then(@"I should only see report '(.*)'")]
		public void ThenIShouldOnlySeeReport(string name)
		{
			Browser.Interactions.AssertFirstContains(".report-list", name);
			Browser.Interactions.AssertJavascriptResultContains(string.Format("return $('.report-list ').length === 1;"), "True");
		}
	}
}

using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class RoutingStepDefinition
	{

		[Then(@"I should see MyTime")]
		public void ThenIShouldSeeMyTime()
		{
			EventualAssert.That(() => Pages.Pages.WeekSchedulePage.DatePicker.Exists, Is.True);
		}

		[Then(@"I should see Mobile Reports")]
		public void ThenIShouldSeeMobileReports()
		{
			// Settings is Now preferred "home"
			EventualAssert.That(() => Pages.Pages.MobileReportsPage.ReportsSettingsViewPageContainer.DisplayVisible(), Is.True);
		}

		[Then(@"I should see Anywhere")]
		public void ThenIShouldSeeAnywhere()
		{
			EventualAssert.That(() => Pages.Pages.AnywherePage.Placeholder.DisplayVisible(), Is.True);
		}
	}
}

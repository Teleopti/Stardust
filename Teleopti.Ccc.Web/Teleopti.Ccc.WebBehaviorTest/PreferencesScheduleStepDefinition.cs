using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class PreferencesScheduleStepDefinition
	{
		[Then(@"I should see my shift")]
		public void ThenIShouldSeeMyShift()
		{
			ScenarioContext.Current.Pending();
			var data = UserFactory.User().UserData<ShiftToday>();
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(data.Date);
			var from = TimeHelper.GetLongHourMinuteTimeString(data.StartTime, UserFactory.User().Culture);
			var to = TimeHelper.GetLongHourMinuteTimeString(data.StartTime, UserFactory.User().Culture);
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(from));
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(to));
		}
	}
}
using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class JalaaliCalendarStepDefinitions
	{
		[Given (@"I am editing an existing Text Request")]
		public void GivenIAmEditingAnExistingTextRequest()
		{
			DataMaker.Data().Apply(new ExistingTextRequestCreatedOnTestDate());

			TestControllerMethods.Logon();
			Navigation.GotoRequests();

			Browser.Interactions.Click(".request-body");
			
		}

		[When (@"I open the date picker")]
		public void WhenIOpenTheDatePicker()
		{
			Browser.Interactions.Click("#dateFromButton");
		}

		[Then(@"I should see a jalaali date picker with date '(.*)', '(.*)'")]
		public void ThenIShouldSeeAJalaaliDatePickerWithDate (DateTime date, String monthName)
		{
			Browser.Interactions.AssertAnyContains(".datepicker-days .table-condensed .switch", date.Year.ToString());
			Browser.Interactions.AssertAnyContains(".datepicker-days .table-condensed .switch", monthName );
			Browser.Interactions.AssertInputValue (".request-edit-datefrom", date.ToString("yyyy\\/MM\\/dd"));
			
		}


		[Then(@"I should see a jalaali date picker with (.*) days")]
		public void ThenIShouldSeeAJalaaliDatePickerWithDays(int days)
		{
			const string javascript = 
							@"var lastDayInCurrentMonth = $('.day').not('.new').not('.old').last()[0];" +
							@"return lastDayInCurrentMonth.textContent;";

			Browser.Interactions.AssertJavascriptResultContains(javascript, days.ToString());
		}



		[When(@"I open the time picker")]
		public void WhenIOpenTheTimePicker()
		{
			Browser.Interactions.Click("#timeFromButton");
		}


		[Then(@"I should see a jalaali time picker with '(.*)','(.*)'")]
		public void ThenIShouldSeeAJalaaliTimePickerWith(string hours, string minutes)
		{
			Browser.Interactions.AssertInputValue(".bootstrap-timepicker-hour", hours);
			Browser.Interactions.AssertInputValue (".bootstrap-timepicker-minute", minutes);
		}



	}
}
			
				
$(document).ready(function() {
	module('Teleopti.MyTimeWeb.StudentAvailability.DayViewModel');

	test('should read bankholidaycalendar when load availability', function() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_Availability_Indicate_BankHoliday_81656');
		var data = {
			"Date": "2019-02-28",
			"AvailableTimeSpan": "10:00 - 11:00",
			"BankHolidayCalendar": {
			  "CalendarId": "7dc8a920-24fd-4524-8556-aa0000247a6a",
			  "CalendarName": "China BankHolidayCalendar",
			  "DateDescription": "National Day"
			}
		};
		var viewModelDay = new Teleopti.MyTimeWeb.StudentAvailability.DayViewModel();
		viewModelDay.ReadStudentAvailability(data);

		equal(viewModelDay.HasBankHolidayCalendar(), true);
		equal(viewModelDay.DateDescription(), 'National Day');
	});
});

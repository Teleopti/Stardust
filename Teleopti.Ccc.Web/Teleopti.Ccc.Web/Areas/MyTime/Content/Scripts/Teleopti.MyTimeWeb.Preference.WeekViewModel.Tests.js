

$(document).ready(function () {

	 module("Teleopti.MyTimeWeb.Preference.WeekViewModel");

	test("should summarize possible weekly contract time", function () {
		var viewModelDay1 = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var viewModelDay2 = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var weekViewModel = new Teleopti.MyTimeWeb.Preference.WeekViewModel();

		viewModelDay1.PossibleContractTimeMinutesLower(6 * 60);
		viewModelDay1.PossibleContractTimeMinutesUpper(10 * 60);
		viewModelDay1.EditableIsInOpenPeriod(true);
		viewModelDay2.PossibleContractTimeMinutesLower(6 * 60);
		viewModelDay2.PossibleContractTimeMinutesUpper(10 * 60);
		viewModelDay2.EditableIsInOpenPeriod(true);

		weekViewModel.DayViewModels.push(viewModelDay1);
		weekViewModel.DayViewModels.push(viewModelDay2);

		expect(2);
		equal(weekViewModel.PossibleResultWeeklyContractTimeMinutesLower(), 12 * 60);
		equal(weekViewModel.PossibleResultWeeklyContractTimeMinutesUpper(), 20 * 60);
	});

	test("should format possible weekly contract time", function () {
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var weekViewModel = new Teleopti.MyTimeWeb.Preference.WeekViewModel();
		viewModelDay.PossibleContractTimeMinutesLower(100 * 60 + 30);
		viewModelDay.PossibleContractTimeMinutesUpper(160 * 60 + 5);
		viewModelDay.EditableIsInOpenPeriod(true);
		weekViewModel.DayViewModels.push(viewModelDay);

		expect(2);
		equal(weekViewModel.PossibleResultWeeklyContractTimeLower(), "100:30");
		equal(weekViewModel.PossibleResultWeeklyContractTimeUpper(), "160:05");
	});

	test("should read weekly work time setting", function () {
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var weekViewModel = new Teleopti.MyTimeWeb.Preference.WeekViewModel();
		viewModelDay.Date = "2014-05-28";
		weekViewModel.DayViewModels.push(viewModelDay);
		weekViewModel.readWeeklyWorkTimeSettings({
			 MaxWorkTimePerWeekMinutes: 360,
			 MinWorkTimePerWeekMinutes:120,
		});

		expect(2);
		equal(weekViewModel.MaxTimePerWeekMinutesSetting(), 360);
		equal(weekViewModel.MinTimePerWeekMinutesSetting(), 120);
	});

});

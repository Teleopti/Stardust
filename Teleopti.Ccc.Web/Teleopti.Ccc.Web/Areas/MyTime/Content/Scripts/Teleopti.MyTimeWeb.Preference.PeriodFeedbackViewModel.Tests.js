
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Preference period feedback view model");

	test("should summarize possible contract time", function () {
		var viewModelDay1 = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var viewModelDay2 = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var viewModel = new Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel(null, [viewModelDay1, viewModelDay2], null, []);
		viewModelDay1.PossibleContractTimeMinutesLower(6 * 60);
		viewModelDay1.PossibleContractTimeMinutesUpper(10 * 60);
		viewModelDay2.PossibleContractTimeMinutesLower(6 * 60);
		viewModelDay2.PossibleContractTimeMinutesUpper(10 * 60);

		expect(2);
		equal(viewModel.PossibleResultContractTimeMinutesLower(), 12 * 60);
		equal(viewModel.PossibleResultContractTimeMinutesUpper(), 20 * 60);
	});

	test("should format possible contract time", function () {
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var viewModel = new Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel(null, [viewModelDay], null, []);
		viewModelDay.PossibleContractTimeMinutesLower(100 * 60 + 30);
		viewModelDay.PossibleContractTimeMinutesUpper(160 * 60 + 5);

		expect(2);
		equal(viewModel.PossibleResultContractTimeLower(), "100:30");
		equal(viewModel.PossibleResultContractTimeUpper(), "160:05");
	});

	test("should load feedback", function () {

		var ajax = {
			Ajax: function (options) {
				equal(options.data.Date, "2012-06-11");
				options.success({
					TargetDaysOff: {
						Lower: 2,
						Upper: 4
					},
					PossibleResultDaysOff: 1,
					TargetContractTime: {
						LowerMinutes: 2100,
						UpperMinutes: 2700
					}
				});
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel(ajax, [], "2012-06-11", []);

		viewModel.LoadFeedback();

		equal(viewModel.TargetDaysOffLower(), 2);
		equal(viewModel.TargetDaysOffUpper(), 4);
		equal(viewModel.PossibleResultDaysOff(), 1);
		equal(viewModel.TargetContractTimeLower(), "35:00");
		equal(viewModel.TargetContractTimeUpper(), "45:00");
	});

	//test("should make possible night rest violation array", function () {

	//	Teleopti.MyTimeWeb.Common.SetupCalendar({
	//		UseJalaaliCalendar: false,
	//		DateFormat: 'YYYY-MM-DD',
	//		TimeFormat: 'HH:mm tt',
	//		AMDesignator: 'AM',
	//		PMDesignator: 'PM'
	//	});

	//	var viewModelDay1 = new Teleopti.MyTimeWeb.Preference.DayViewModel(null);
	//	viewModelDay1.RawDate("2016-02-04");
	//	viewModelDay1.ExpectedNightRest("11");
	//	viewModelDay1.RestTimeToPreviousDay("10");
	//	viewModelDay1.RestTimeToNextDay("10");
	//	viewModelDay1.HasNightRestViolationToNextDay(true);
	//	var viewModelDay2 = new Teleopti.MyTimeWeb.Preference.DayViewModel(null);
	//	viewModelDay2.RawDate("2016-01-26");
	//	viewModelDay2.ExpectedNightRest("11");
	//	viewModelDay2.RestTimeToPreviousDay("10");
	//	viewModelDay2.RestTimeToNextDay("10");
	//	viewModelDay2.HasNightRestViolationToNextDay(true);
	//	viewModelDay2.HasNightRestViolationToPreviousDay(true);



	//	var viewModel = new Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel(null, [viewModelDay1, viewModelDay2], null, []);
	//	console.log(viewModel);

	//	equal(viewModel.PossibleNightRestViolations()()[0].firstDay, "2016-02-04");
	//	equal(viewModel.PossibleNightRestViolations()()[0].sencondDay, "2016-02-05");
	//	equal(viewModel.PossibleNightRestViolations()()[0].hoursBetweenTwoDays, 10);
	//	equal(viewModel.PossibleNightRestViolations()()[0].nightRestTimes, 11);

	//	equal(viewModel.PossibleNightRestViolations()()[1].firstDay, "2016-01-25");
	//	equal(viewModel.PossibleNightRestViolations()()[1].sencondDay, "2016-01-26");
	//	equal(viewModel.PossibleNightRestViolations()()[1].hoursBetweenTwoDays, 10);
	//	equal(viewModel.PossibleNightRestViolations()()[1].nightRestTimes, 11);

	//	equal(viewModel.PossibleNightRestViolations()()[2].firstDay, "2016-01-26");
	//	equal(viewModel.PossibleNightRestViolations()()[2].sencondDay, "2016-01-27");
	//	equal(viewModel.PossibleNightRestViolations()()[2].hoursBetweenTwoDays, 10);
	//	equal(viewModel.PossibleNightRestViolations()()[2].nightRestTimes, 11);

	
	//});

});

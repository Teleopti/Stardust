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

	test("should count warnings", function () {
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var viewModel = new Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel(null, [viewModelDay], null, []);
		viewModelDay.PossibleContractTimeMinutesLower(100 * 60 + 30);
		viewModelDay.PossibleContractTimeMinutesUpper(160 * 60 + 5);

		viewModel.TargetDaysOffLower(1);
		viewModel.TargetDaysOffUpper(2);

		equal(viewModel.WarningCount(), 3);
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

	test("should make possible night rest violation array", function () {

		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var date1 = "\/Date(1454515200000)\/";//2016-02-04
		var currentDay1 = moment(date1);
		var plusOneDay1 = moment(date1).add(1, 'days');

		var date2 = "\/Date(1453737600000)\/";//2016-01-26
		var currentDay2 = moment(date2);
		var minusOneDay2 = moment(date2).subtract(1, 'days');
		var plusOneDay2 = moment(date2).add(1, 'days');

		var ajax1 = function(model, options) {
			options.success({
				RestTimeToNextDayTimeSpan: "10:00",
				RestTimeToPreviousDayTimeSpan: "10:00",
				ExpectedNightRestTimeSpan: "11:00",
				HasNightRestViolationToPreviousDay: false,
				HasNightRestViolationToNextDay: true,
				DateInternal: "\/Date(1454515200000)\/" //2016-02-04
			});
		};

		var viewModelDay1 = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax1);
		viewModelDay1.Feedback(true);
		viewModelDay1.LoadFeedback();

		var ajax2 = function(model, options) {
			options.success({
				RestTimeToNextDayTimeSpan: "10:00",
				RestTimeToPreviousDayTimeSpan: "10:00",
				ExpectedNightRestTimeSpan: "11:00",
				HasNightRestViolationToPreviousDay: true,
				HasNightRestViolationToNextDay: true,
				DateInternal: "\/Date(1453737600000)\/" //2016-01-26
			});
		};
		
		var viewModelDay2 = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax2);
		viewModelDay2.Feedback(true);
		viewModelDay2.LoadFeedback();

		var viewModel = new Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel(null, [viewModelDay1, viewModelDay2], null, []);

		equal(viewModel.PossibleNightRestViolations()()[0].firstDay, Teleopti.MyTimeWeb.Common.FormatDate(currentDay1));// 2016-02-04
		equal(viewModel.PossibleNightRestViolations()()[0].sencondDay, Teleopti.MyTimeWeb.Common.FormatDate(plusOneDay1));// 2016-02-03
		equal(viewModel.PossibleNightRestViolations()()[0].hoursBetweenTwoDays, "10:00");
		equal(viewModel.PossibleNightRestViolations()()[0].nightRestTimes, "11:00");

		equal(viewModel.PossibleNightRestViolations()()[1].firstDay, Teleopti.MyTimeWeb.Common.FormatDate(minusOneDay2));//2016-01-25
		equal(viewModel.PossibleNightRestViolations()()[1].sencondDay, Teleopti.MyTimeWeb.Common.FormatDate(currentDay2));//2016-01-26
		equal(viewModel.PossibleNightRestViolations()()[1].hoursBetweenTwoDays, "10:00");
		equal(viewModel.PossibleNightRestViolations()()[1].nightRestTimes, "11:00");

		equal(viewModel.PossibleNightRestViolations()()[2].firstDay, Teleopti.MyTimeWeb.Common.FormatDate(currentDay2));//2016-01-26
		equal(viewModel.PossibleNightRestViolations()()[2].sencondDay, Teleopti.MyTimeWeb.Common.FormatDate(plusOneDay2));//2016-01-27
		equal(viewModel.PossibleNightRestViolations()()[2].hoursBetweenTwoDays, "10:00");
		equal(viewModel.PossibleNightRestViolations()()[2].nightRestTimes, "11:00");
	});
});
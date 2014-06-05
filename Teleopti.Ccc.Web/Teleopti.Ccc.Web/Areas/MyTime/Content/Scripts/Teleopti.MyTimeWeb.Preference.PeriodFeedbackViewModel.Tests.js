
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
						Lower: "35:00",
						Upper: "45:00"
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

});

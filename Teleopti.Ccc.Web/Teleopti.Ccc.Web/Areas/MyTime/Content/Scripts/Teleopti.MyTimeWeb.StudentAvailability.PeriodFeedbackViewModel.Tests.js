﻿$(document).ready(function () {
	module("Teleopti.MyTimeWeb.StudentAvailability period feedback view model");

	test("should only summarize possible contract time for days in opened period", function () {
		var viewModelDay1 = new Teleopti.MyTimeWeb.StudentAvailability.DayViewModel();
		var viewModelDay2 = new Teleopti.MyTimeWeb.StudentAvailability.DayViewModel();
		var viewModel = new Teleopti.MyTimeWeb.StudentAvailability.PeriodFeedbackViewModel(
			null, [viewModelDay1, viewModelDay2], null);

		viewModelDay1.EditableIsInOpenPeriod(true);
		viewModelDay1.HasAvailability(true);
		viewModelDay1.PossibleContractTimeMinutesLower(7 * 60);
		viewModelDay1.PossibleContractTimeMinutesUpper(10 * 60);

		viewModelDay2.EditableIsInOpenPeriod(false);
		viewModelDay2.HasAvailability(true);
		viewModelDay2.PossibleContractTimeMinutesLower(5 * 60);
		viewModelDay2.PossibleContractTimeMinutesUpper(9 * 60);

		expect(2);
		equal(viewModel.PossibleResultContractTimeMinutesLower(), 7 * 60);
		equal(viewModel.PossibleResultContractTimeMinutesUpper(), 10 * 60);
	});

	test("should summarize possible contract time", function () {
		var viewModelDay1 = new Teleopti.MyTimeWeb.StudentAvailability.DayViewModel();
		var viewModelDay2 = new Teleopti.MyTimeWeb.StudentAvailability.DayViewModel();
		var viewModel = new Teleopti.MyTimeWeb.StudentAvailability.PeriodFeedbackViewModel(
			null, [viewModelDay1, viewModelDay2], null);

		viewModelDay1.EditableIsInOpenPeriod(true);
		viewModelDay1.HasAvailability(true);
		viewModelDay1.PossibleContractTimeMinutesLower(6 * 60);
		viewModelDay1.PossibleContractTimeMinutesUpper(10 * 60);

		viewModelDay2.EditableIsInOpenPeriod(true);
		viewModelDay2.HasAvailability(true);
		viewModelDay2.PossibleContractTimeMinutesLower(6 * 60);
		viewModelDay2.PossibleContractTimeMinutesUpper(10 * 60);

		expect(2);
		equal(viewModel.PossibleResultContractTimeMinutesLower(), 12 * 60);
		equal(viewModel.PossibleResultContractTimeMinutesUpper(), 20 * 60);
	});

	test("should format possible contract time", function () {
		var viewModelDay = new Teleopti.MyTimeWeb.StudentAvailability.DayViewModel();
		viewModelDay.HasAvailability(true);
		var viewModel = new Teleopti.MyTimeWeb.StudentAvailability.PeriodFeedbackViewModel(null, [viewModelDay], null);
		viewModelDay.EditableIsInOpenPeriod(true);
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
					TargetContractTime: {
						LowerMinutes: 2100,
						UpperMinutes: 2700
					}
				});
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.StudentAvailability.PeriodFeedbackViewModel(ajax, [], "2012-06-11");

		viewModel.LoadFeedback();

		equal(viewModel.TargetContractTimeLower(), "35:00");
		equal(viewModel.TargetContractTimeUpper(), "45:00");
	});
});

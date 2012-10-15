
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Preference period feedback view model");


	test("should summarize must have", function () {
		var viewModelDay1 = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var viewModelDay2 = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var mustHaveViewModel = new Teleopti.MyTimeWeb.Preference.MustHaveCountViewModel([viewModelDay1, viewModelDay2]);
		var viewModel = new Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel(null, [viewModelDay1, viewModelDay2], mustHaveViewModel);
		viewModelDay1.MustHave(true);
		viewModelDay2.MustHave(true);
		expect(1);
		equal(mustHaveViewModel.CurrentMustHave(), 2);
	});
});

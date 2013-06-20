
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Preference selection view model");

	test("should summarize must have", function () {
		var viewModelDay1 = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var viewModelDay2 = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var mustHaveViewModel = new Teleopti.MyTimeWeb.Preference.SelectionViewModel([viewModelDay1, viewModelDay2], 5);
		viewModelDay1.MustHave(true);
		viewModelDay2.MustHave(true);
	    equal(mustHaveViewModel.maxMustHave(), 5);
	    equal(mustHaveViewModel.currentMustHaves(), 2);
	});
});

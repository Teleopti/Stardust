
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Preference must have view model");


	test("should summarize must have", function () {
		var viewModelDay1 = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var viewModelDay2 = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		var mustHaveViewModel = new Teleopti.MyTimeWeb.Preference.MustHaveCountViewModel();
	    mustHaveViewModel.SetData([viewModelDay1, viewModelDay2], 5);
		viewModelDay1.MustHave(true);
		viewModelDay2.MustHave(true);
		expect(1);
		equal(mustHaveViewModel.MustHaveText(), '2(5)');
	});
});

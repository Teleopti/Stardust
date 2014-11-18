
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel");


	test("should set date picker range", function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
		var now = moment("Dec 25, 1995");

		viewModel.setDatePickerRange(now, 1, 2);

		equal(viewModel.openPeriodStartDate().format("YYYY-MM-DD"), "1995-12-26");
		equal(viewModel.openPeriodEndDate().format("YYYY-MM-DD"), "1995-12-27");
	});
	
});

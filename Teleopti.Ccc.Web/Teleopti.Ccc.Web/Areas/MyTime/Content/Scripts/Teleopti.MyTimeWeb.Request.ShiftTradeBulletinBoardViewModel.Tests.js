
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel");


	test("should set date picker range", function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
		var now = moment("Dec 25, 1995");

		viewModel.setDatePickerRange(now, 1, 2);

		equal(viewModel.openPeriodStartDate().format("YYYY-MM-DD"), "1995-12-26");
		equal(viewModel.openPeriodEndDate().format("YYYY-MM-DD"), "1995-12-27");
	});
	
	test("should go to next date", function () {
		var ajax = {
			Ajax: function (options) {
				if (options.url == "Team/TeamsForShiftTrade") {
					options.success(
						""
					);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel(ajax);
		var date = moment("12-25-1995", "MM-DD-YYYY");
		viewModel.requestedDate(date);
		viewModel.isRequestedDateValid = function (date) {
			return true;
		};

		viewModel.nextDate();

		equal(viewModel.requestedDateInternal().format("MM-DD-YYYY"), "12-26-1995");
	});

	test("should go to previous date", function () {
		var ajax = {
			Ajax: function (options) {
				if (options.url == "Team/TeamsForShiftTrade") {
					options.success(
						""
					);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel(ajax);
		var date = moment("12-25-1995", "MM-DD-YYYY");
		viewModel.requestedDate(date);
		viewModel.isRequestedDateValid = function (date) {
			return true;
		};

		viewModel.previousDate();

		equal(viewModel.requestedDateInternal().format("MM-DD-YYYY"), "12-24-1995");
	});
});

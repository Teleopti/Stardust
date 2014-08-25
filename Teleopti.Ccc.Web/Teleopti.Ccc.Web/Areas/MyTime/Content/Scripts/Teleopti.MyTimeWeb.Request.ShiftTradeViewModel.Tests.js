
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Request.ShiftTradeViewModel");

	test("should filter checked start time", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		var isChecked = true;
		viewModel.filterStartTimeList.push(new Teleopti.MyTimeWeb.Request.FilterStartTimeView('8:00-10:00', 8, 10, isChecked, false));

		viewModel.filterTime();

		equal(viewModel.isDayoffFiltered(), false);
		equal(viewModel.filteredStartTimesText().length, 1);
	});

	test("should filter checked end time", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		var isChecked = true;
		viewModel.filterEndTimeList.push(new Teleopti.MyTimeWeb.Request.FilterEndTimeView('8:00-10:00', 8, 10, isChecked, false));

		viewModel.filterTime();

		equal(viewModel.filteredEndTimesText().length, 1);
	});

	test("should load filter hours text and dayoff names", function() {
		var ajax = {
			Ajax: function (options) {
				if (options.url == "RequestsShiftTradeScheduleFilter/Get") {
					
					var hourTexts = [];
					var dayOffNames = ["DO"];
					for (var i = 0; i <= 24; ++i) {
						hourTexts[i] = i + ":00";
					}
					
					options.success(
						{
							HourTexts: hourTexts,
							DayOffShortNames: dayOffNames
						}
					);
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel(ajax);
		viewModel.loadFilterTimes();

		equal(viewModel.filterStartTimeList().length, 13);
		equal(viewModel.filterStartTimeList()[0].text, "0:00 - 2:00");
		equal(viewModel.filterStartTimeList()[12].text, "DO");
	});

	test("should load toggles", function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url == "../ToggleHandler/IsEnabled?toggle=Request_ShiftTradeRequestForMoreDays_20918") {
					options.success({
						IsEnabled: true
					});
				}

				if (options.url == "../ToggleHandler/IsEnabled?toggle=Request_SeePossibleShiftTradesFromAllTeams_28770") {
					options.success({
						IsEnabled: true
					});
				}

				if (options.url == "../ToggleHandler/IsEnabled?toggle=Request_FilterPossibleShiftTradeByTime_24560") {
					options.success({
						IsEnabled: true
					});
				}
			}
		}

		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel(ajax);
		viewModel.featureCheck();

		equal(viewModel.isTradeForMultiDaysEnabled(), true);
		equal(viewModel.isPossibleSchedulesForAllEnabled(), true);
		equal(viewModel.isFilterByTimeEnabled(), true);
	});

});

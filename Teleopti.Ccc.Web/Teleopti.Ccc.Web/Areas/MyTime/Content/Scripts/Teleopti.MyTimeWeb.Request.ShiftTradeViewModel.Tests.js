
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Request.ShiftTradeViewModel");

	test("should hide page view selector when no data", function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.pageCount(0);

		viewModel.setPageVisiblity();

		equal(viewModel.isPageVisible(), false);
	});

	test("should set select page", function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.selectedPageIndex(1);

		viewModel.setSelectPage(2);

		equal(viewModel.selectedPageIndex(), 2);
	});

	test("should go to next pages", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		var pageCount = 8;
		viewModel.pageCount(pageCount);
		viewModel.initSelectablePages(pageCount);

		viewModel.goNextPages();

		equal(viewModel.selectedPageIndex(), 6);
		equal(viewModel.isMore(), false);
		equal(viewModel.isPreviousMore(), true);
		equal(viewModel.selectablePages().length, pageCount - 5);
	});

	test("should go to previous pages", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		var pageCount = 8;
		viewModel.pageCount(pageCount);
		viewModel.initSelectablePages(pageCount);
		viewModel.goToLastPage();

		viewModel.goPreviousPages();

		equal(viewModel.selectedPageIndex(), 1);
		equal(viewModel.isMore(), true);
		equal(viewModel.isPreviousMore(), false);
		equal(viewModel.selectablePages().length, 5);
	});

	test("should update selected page", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.selectedPageIndex(1);
		viewModel.initSelectablePages(4);

		viewModel.selectedPageIndex(2);
		viewModel.updateSelectedPage();

		equal(viewModel.selectablePages().length, 4);
		equal(viewModel.selectablePages()[1].isSelected(), true);
	});

	test("should go to last page without previous more", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.pageCount(3);

		viewModel.goToLastPage();

		equal(viewModel.selectedPageIndex(), 3);
		equal(viewModel.isPreviousMore(), false); // only valid when 5 pages or more
		equal(viewModel.isMore(), false);
	});

	test("should go to last page with previous more", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.pageCount(6);
		viewModel.goToLastPage();

		equal(viewModel.selectedPageIndex(), 6);
		equal(viewModel.isPreviousMore(), true); // only valid when 5 pages or more
		equal(viewModel.isMore(), false);
		equal(viewModel.selectablePages().length, 1);
	});

	test("should go to first page without more", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.selectedPageIndex(3);

		viewModel.goToFirstPage();

		equal(viewModel.selectedPageIndex(), 1);
		equal(viewModel.isPreviousMore(), false);
		equal(viewModel.isMore(), false);
	});

	test("should go to first page with more", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.pageCount(6);
		viewModel.goToFirstPage();

		equal(viewModel.selectedPageIndex(), 1);
		equal(viewModel.isPreviousMore(), false);
		equal(viewModel.isMore(), true);
		equal(viewModel.selectablePages().length, 5);
	});

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

	test("should load filter hours text and dayoff names", function () {
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

	test("should load toggles", function () {
		var ajax = {
			Ajax: function (options) {
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

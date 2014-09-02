
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Request.ShiftTradeViewModel");

	test("should set date picker range", function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		var now = moment("Dec 25, 1995");

		viewModel.setDatePickerRange(now, 1, 2);

		equal(viewModel.openPeriodStartDate().format("YYYY-MM-DD"), "1995-12-26");
		equal(viewModel.openPeriodEndDate().format("YYYY-MM-DD"), "1995-12-27");
	});

	test("should get date with format", function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.requestedDateInternal(moment("Dec 25, 1995"));

		var result = viewModel.getDateWithFormat();

		equal(result, "1995-12-25");
	});

	test("shift trade date should be invalid when it is not in an open period", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		var periodStart = moment("2014-08-01", "YYYY-MM-DD");
		var periodEnd = moment("2014-08-31", "YYYY-MM-DD");
		viewModel.openPeriodStartDate(periodStart);
		viewModel.openPeriodEndDate(periodEnd);

		var startRangeResult = viewModel.isRequestedDateValid(moment("2014-07-31", "YYYY-MM-DD"));
		var endRangeResult = viewModel.isRequestedDateValid(moment("2014-09-01", "YYYY-MM-DD"));

		equal(startRangeResult, false);
		equal(endRangeResult, false);
	});

	test("shift trade date should be valid when it is in open period", function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		var periodStart = moment("2014-08-01", "YYYY-MM-DD");
		var periodEnd = moment("2014-08-31", "YYYY-MM-DD");
		viewModel.openPeriodStartDate(periodStart);
		viewModel.openPeriodEndDate(periodEnd);

		var result = viewModel.isRequestedDateValid(moment("2014-08-15", "YYYY-MM-DD"));

		equal(result, true);
	});

	test("should go to the requeted date and load team id", function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url == "Requests/ShiftTradeRequestMyTeam") {
					options.success(
						"myTeamId"
					);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel(ajax);
		var date = moment("12-25-1995", "MM-DD-YYYY");

		viewModel.requestedDate(date);

		equal(viewModel.requestedDateInternal().format("MM-DD-YYYY"), "12-25-1995");
		equal(viewModel.myTeamId(), "myTeamId");
	});

	test("should go to next date", function () {
		var ajax = {
			Ajax: function(options) {
				if (options.url == "Requests/ShiftTradeRequestMyTeam") {
					options.success(
						""
					);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel(ajax);
		var date = moment("12-25-1995", "MM-DD-YYYY");
		viewModel.requestedDate(date);
		viewModel.isRequestedDateValid = function(date) {
			return true;
		};

		viewModel.nextDate();

		equal(viewModel.requestedDateInternal().format("MM-DD-YYYY"), "12-26-1995");
	});

	test("should go to previous date", function () {
		var ajax = {
			Ajax: function(options) {
				if (options.url == "Requests/ShiftTradeRequestMyTeam") {
					options.success(
						""
					);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel(ajax);
		var date = moment("12-25-1995", "MM-DD-YYYY");
		viewModel.requestedDate(date);
		viewModel.isRequestedDateValid = function(date) {
			return true;
		};

		viewModel.previousDate();

		equal(viewModel.requestedDateInternal().format("MM-DD-YYYY"), "12-24-1995");
	});

	test("should clean data when prepare load without Multi shifts trade Enabled", function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.isTradeForMultiDaysEnabled(false);
		viewModel.isDetailVisible = function () {
			return false;
		}

		viewModel.prepareLoad();

		equal(viewModel.possibleTradeSchedulesRaw.length, 0);
		equal(viewModel.selectablePages().length, 0);
		equal(viewModel.selectedPageIndex(), 1);
		equal(viewModel.isPreviousMore(), false);
		equal(viewModel.isMore(), false);
		equal(viewModel.selectedInternal(), false);
		equal(viewModel.IsLoading(), false);
	});

	test("should get all team ids except 'Team All'", function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.availableTeams.push({ id: "A", text: "Team A" });
		viewModel.availableTeams.push({ id: "B", text: "Team B" });
		viewModel.availableTeams.push({ id: "allTeams", text: "Team All" });

		var target = viewModel.getAllTeamIds();

		equal(target.length, 2);
		equal(target[0], "A");
		equal(target[1], "B");
	});

	test("should load Team All", function() {
		var ajax = {
			Ajax: function (options) {
				if (options.url == "Team/TeamsForShiftTrade") {
					options.success(
							[
								{ id: "A", text: "Team A" },
								{ id: "B", text: "Team B" },
								{ id: "C", text: "Team C" }
							]
					);
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel(ajax);
		viewModel.isPossibleSchedulesForAllEnabled(true);

		viewModel.loadTeams();

		equal(viewModel.availableTeams().length, 4);
		var teamAll = viewModel.availableTeams()[3];
		equal(teamAll.id, "allTeams");
		equal(teamAll.text, "Team All");
	});

	test("should load no teams when no teams returned from server", function () {
		var ajax = {
			Ajax: function (options) {
				if (options.url == "Team/TeamsForShiftTrade") {
					options.success([]);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel(ajax);

		viewModel.loadTeams();

		equal(viewModel.availableTeams().length, 0);
	});

	test("should load teams", function() {
		var ajax = {
			Ajax: function (options) {
				if (options.url == "Team/TeamsForShiftTrade") {
					options.success(
							[
								{ id: "A", text: "Team A"},
								{ id: "B", text: "Team B" },
								{ id: "C", text: "Team C" }
							]
					);
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel(ajax);
		viewModel.myTeamId("A");

		viewModel.loadTeams();

		equal(viewModel.selectedTeam(), "A");
		equal(viewModel.availableTeams().length, 3);
		var teamTwo = viewModel.availableTeams()[1];
		equal(teamTwo.id, "B");
		equal(teamTwo.text, "Team B");
	});

	test("should set default team ID when get nothing", function () {
		var myTeamId = "";
		var ajax = {
			Ajax: function (options) {
				if (options.url == "Requests/ShiftTradeRequestMyTeam") {
					options.success(
							myTeamId
					);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel(ajax);

		viewModel.loadMyTeamId();

		equal(viewModel.myTeamId(), undefined);
		equal(viewModel.missingMyTeam(), true);

	});

	test("should load my team ID", function () {
		var myTeamId = "myTeam";
		var ajax = {
			Ajax: function (options) {
				if (options.url == "Requests/ShiftTradeRequestMyTeam") {
					options.success(
							myTeamId
					);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel(ajax);

		viewModel.loadMyTeamId();

		equal(viewModel.myTeamId(), myTeamId);
		equal(viewModel.missingMyTeam(), false);
	});

	test("should hide page view when no data", function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();

		viewModel.setPagingInfo(0);

		equal(viewModel.isPageVisible(), false);
	});

	test("should show page view when there is data", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();

		viewModel.setPagingInfo(1);

		equal(viewModel.isPageVisible(), true);
	});

	test("should not init selectable pages when it has data", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(1));

		viewModel.setPagingInfo(2);

		equal(viewModel.selectablePages().length, 1);
	});

	test("should set page count when set paging infos", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();

		viewModel.setPagingInfo(2);

		equal(viewModel.pageCount(), 2);
	});

	test("should set paging info", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.selectedPageIndex(1);

		viewModel.selectedPageIndex(2);
		viewModel.setPagingInfo(2);
		
		equal(viewModel.pageCount(), 2);
		equal(viewModel.selectablePages().length, 2);
		equal(viewModel.selectablePages()[1].isSelected(), true);
	});

	test("should can select page", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.selectedPageIndex(1);
		var page = new Teleopti.MyTimeWeb.Request.PageView(2);

		viewModel.selectPage(page);

		equal(viewModel.selectedPageIndex(), 2);
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

	test("should recognize time filter with start time", function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.filteredStartTimesText.push("6:00 - 8:00");

		var result = viewModel.isFiltered();

		equal(result, true);
	});

	test("should recognize time filter with end time", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.filteredEndTimesText.push("6:00 - 8:00");

		var result = viewModel.isFiltered();

		equal(result, true);
	});

	test("should recognize time filter with dayoff", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();
		viewModel.isDayoffFiltered(true);

		var result = viewModel.isFiltered();

		equal(result, true);
	});

	test("should recognize is not filtered by time filter", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel();

		var result = viewModel.isFiltered();

		equal(result, false);
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

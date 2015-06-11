
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel");

	test("should clean data when prepare load", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
		viewModel.isDetailVisible = function () {
			return false;
		}

		viewModel.prepareLoad();

		equal(viewModel.selectablePages().length, 0);
		equal(viewModel.selectedPageIndex(), 1);
		equal(viewModel.isPreviousMore(), false);
		equal(viewModel.isMore(), false);
		equal(viewModel.IsLoading(), false);
		equal(viewModel.chooseAgent(), null);
	});

	test("should get date with format", function () {

		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) { return true; };
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
		viewModel.requestedDateInternal(moment("Dec 25, 1995"));

		var result = viewModel.getFormattedDateForDisplay();

		equal(result, "1995-12-25");
	});

	test("should hide page view when no data", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();

		viewModel.setPagingInfo(0);

		equal(viewModel.isPageVisible(), false);
	});

	test("should show page view when there is data", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();

		viewModel.setPagingInfo(1);

		equal(viewModel.isPageVisible(), true);
	});

	test("should not init selectable pages when it has data", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
		viewModel.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(1));

		viewModel.setPagingInfo(2);

		equal(viewModel.selectablePages().length, 1);
	});

	test("should set page count when set paging infos", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();

		viewModel.setPagingInfo(2);

		equal(viewModel.pageCount(), 2);
	});

	test("should set paging info", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
		viewModel.selectedPageIndex(1);

		viewModel.selectedPageIndex(2);
		viewModel.setPagingInfo(2);

		equal(viewModel.pageCount(), 2);
		equal(viewModel.selectablePages().length, 2);
		equal(viewModel.selectablePages()[1].isSelected(), true);
	});

	test("should can select page", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
		viewModel.selectedPageIndex(1);
		var page = new Teleopti.MyTimeWeb.Request.PageView(2);

		viewModel.selectPage(page);

		equal(viewModel.selectedPageIndex(), 2);
	});

	test("should set select page", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
		viewModel.selectedPageIndex(1);

		viewModel.setSelectPage(2);

		equal(viewModel.selectedPageIndex(), 2);
	});

	test("should go to next pages", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
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
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
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
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
		viewModel.pageCount(3);

		viewModel.goToLastPage();

		equal(viewModel.selectedPageIndex(), 3);
		equal(viewModel.isPreviousMore(), false); // only valid when 5 pages or more
		equal(viewModel.isMore(), false);
	});

	test("should go to last page with previous more", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
		viewModel.pageCount(6);
		viewModel.goToLastPage();

		equal(viewModel.selectedPageIndex(), 6);
		equal(viewModel.isPreviousMore(), true); // only valid when 5 pages or more
		equal(viewModel.isMore(), false);
		equal(viewModel.selectablePages().length, 1);
	});

	test("should go to first page without more", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
		viewModel.selectedPageIndex(3);

		viewModel.goToFirstPage();

		equal(viewModel.selectedPageIndex(), 1);
		equal(viewModel.isPreviousMore(), false);
		equal(viewModel.isMore(), false);
	});

	test("should go to first page with more", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel();
		viewModel.pageCount(6);
		viewModel.goToFirstPage();

		equal(viewModel.selectedPageIndex(), 1);
		equal(viewModel.isPreviousMore(), false);
		equal(viewModel.isMore(), true);
		equal(viewModel.selectablePages().length, 5);
	});

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

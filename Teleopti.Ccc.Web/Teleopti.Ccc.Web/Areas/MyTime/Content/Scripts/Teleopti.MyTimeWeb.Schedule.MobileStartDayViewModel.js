﻿/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.LayerViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel.js" />


Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel = function (weekStart, parent) { 
	var self = this;

	var constants = Teleopti.MyTimeWeb.Common.Constants;

	self.displayDate = ko.observable();
	self.selectedDate = ko.observable(moment().startOf("day"));
	self.selectedDateSubscription = null;

	self.summaryColor = ko.observable();
	self.summaryName = ko.observable();
	self.summaryTime = ko.observable();
	self.dayOfWeek = ko.observable();
	self.isDayOff = ko.observable(false);
	self.summaryVisible = ko.observable(false);
	self.hasOvertime = ko.observable(false);
	self.timeLines = ko.observableArray();
	self.layers = ko.observableArray();
	self.scheduleHeight = ko.observable();
	self.unreadMessageCount = ko.observable(0);
	self.probabilities = ko.observableArray([]);
	self.requestCount = ko.observable(0);
	self.requestViewModel = ko.observable();

	self.textRequestPermission = ko.observable();
	self.absenceRequestPermission = ko.observable();
	self.absenceReportPermission = ko.observable();
	self.overtimeAvailabilityPermission = ko.observable();
	self.shiftTradeRequestPermission = ko.observable();
	self.shiftExchangePermission = ko.observable();
	self.personAccountPermission = ko.observable();
	self.requestPermission = ko.observable();
	self.showAbsenceReportingCommandItem = ko.observable();

	self.menuIsVisible = ko.observable(false);
	self.menuIconIsVisible = ko.observable(true);
	self.isCommandEnable = Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_DayScheduleForStartPage_Command_44209");

	self.selectedProbabilityType = constants.probabilityType.none;

	var initializeProbabilityType = Teleopti.MyTimeWeb.Portal.ParseHash().probability;
	self.selectedProbabilityOptionValue = ko.observable(initializeProbabilityType);

	self.navigateToMessages = function () {
		Teleopti.MyTimeWeb.Portal.NavigateTo("MessageTab");
	};

	self.navigateToRequests = function () {
		Teleopti.MyTimeWeb.Portal.NavigateTo("Requests/Index");
	};

	self.readData = function (data) {
		self.displayDate(moment(data.Date).format(Teleopti.MyTimeWeb.Common.DateFormat));
		self.summaryColor(data.Schedule.Summary.Color);
		self.summaryName(data.Schedule.Summary.Title);
		self.summaryVisible(true);
		self.summaryTime(data.Schedule.Summary.TimeSpan);
		self.isDayOff(data.Schedule.IsDayOff);
		self.unreadMessageCount(data.UnReadMessageCount);

		self.hasOvertime(data.Schedule.HasOvertime);
		self.requestCount(data.Schedule.TextRequestCount);

		if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
			var dayDate = moment(data.Schedule.FixedDate, Teleopti.MyTimeWeb.Common.ServiceDateFormat);
			self.dayOfWeek(dayDate.format("dddd"));
		} else {
			self.dayOfWeek(data.Schedule.Header.Title);
		}

		var rawPeriods = data.Schedule.Periods;
		var scheduleHeight = Teleopti.MyTimeWeb.Schedule.GetScheduleHeight();
		self.scheduleHeight(scheduleHeight + "px");

		var timelines = ko.utils.arrayMap(data.TimeLine, function (item) {
			// 5 is half of timeline label height (10px)
			return new Teleopti.MyTimeWeb.Schedule.TimelineViewModel(item, scheduleHeight, -5);
		});
		self.timeLines(timelines);

		var layers = ko.utils.arrayMap(rawPeriods, function (item) {
			return new Teleopti.MyTimeWeb.Schedule.LayerViewModel(item, self);
		});
		self.layers(layers);

		if (data.RequestPermission) {
			self.overtimeAvailabilityPermission(data.RequestPermission.OvertimeAvailabilityPermission);
			self.absenceReportPermission(data.RequestPermission.AbsenceReportPermission);
			self.textRequestPermission(data.RequestPermission.TextRequestPermission);
			self.absenceRequestPermission(data.RequestPermission.AbsenceRequestPermission);
			self.shiftTradeRequestPermission(data.RequestPermission.ShiftTradeRequestPermission);
			self.shiftExchangePermission(data.RequestPermission.ShiftExchangePermission);
			self.personAccountPermission(data.RequestPermission.PersonAccountPermission);
		}

		self.selectedDate(moment(data.Date));

		var dateDiff = self.selectedDate().startOf('day').diff(moment().startOf('day'), 'days');
		var isTodayOrTomorrow = dateDiff === 0 || dateDiff === 1;

		self.showAbsenceReportingCommandItem(self.absenceReportPermission() && isTodayOrTomorrow);
	};

	self.setSelectedDateSubscription = function () {
		if (self.selectedDateSubscription)
			self.selectedDateSubscription.dispose();

		self.selectedDateSubscription = self.selectedDate.subscribe(function (date) {
			Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/MobileDay" + getUrlPartForDate(date) + getUrlPartForProbability());
		});
	};

	function getUrlPartForDate(date) {
		return Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date.format("YYYY-MM-DD"));
	}

	function getUrlPartForProbability() {
		return (self.selectedProbabilityType !== constants.probabilityType.none && self.selectedProbabilityType)
			? "/Probability/" + self.selectedProbabilityType
			: "";
	}

	self.setSelectedDateSubscription();

	self.today = function () {
		self.currentUserDate = ko.observable(moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime()).startOf("day"));
		self.selectedDate(self.currentUserDate());
	};

	self.nextDay = function () {
		var nextDate = moment(self.selectedDate()).add(1, 'days');
		self.selectedDate(nextDate);
	};

	self.previousDay = function () {
		var previousDate = moment(self.selectedDate()).add(-1, 'days');
		self.selectedDate(previousDate);
	};

	self.enableMenu = function () {
		self.menuIsVisible(true);
		self.menuIconIsVisible(false);
	};

	self.disableMenu = function () {
		self.menuIsVisible(false);
		self.menuIconIsVisible(true);
	};

	function resetRequestViewModel() {
		self.requestViewModel(undefined);
		self.menuIconIsVisible(true);
	}

	function setupRequestViewModel(requestViewModel, cancelAddingNewRequest) {
		self.requestViewModel({
			model: requestViewModel,
			CancelAddingNewRequest: cancelAddingNewRequest
		});
		self.menuIsVisible(false);
		self.menuIconIsVisible(false);
	}

	self.showOvertimeAvailabilityForm = function () {
		setupRequestViewModel(null, null);
	};

	self.showAbsenceReportingForm = function () {
		var requestViewModel = new Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel(parent.Ajax(), function (data) {
			parent.ReloadSchedule(data);
			resetRequestViewModel();
		});
		setupRequestViewModel(requestViewModel, resetRequestViewModel);
	};

	self.showAddTextRequestForm = function () {
		var requestViewModel = new Teleopti.MyTimeWeb.Request
			.RequestViewModel(Teleopti.MyTimeWeb.Request.RequestDetail.AddTextOrAbsenceRequest,
			weekStart,
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues);

		requestViewModel.AddRequestCallback = function (data) {
			var count = self.requestCount() + 1;
			self.requestCount(count);
			resetRequestViewModel();
		};

		requestViewModel.AddTextRequest(false);
		setupRequestViewModel(requestViewModel, resetRequestViewModel);
	};

	self.showAddAbsenceRequestForm = function () {
		var requestViewModel = new Teleopti.MyTimeWeb.Request
			.RequestViewModel(Teleopti.MyTimeWeb.Request.RequestDetail.AddTextOrAbsenceRequest,
			weekStart,
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues);

		requestViewModel.AddRequestCallback = function (data) {
			var count = self.requestCount() + 1;
			self.requestCount(count);
			resetRequestViewModel();
		};

		requestViewModel.readPersonalAccountPermission(self.personAccountPermission());
		requestViewModel.AddAbsenceRequest(false);
		setupRequestViewModel(requestViewModel, resetRequestViewModel);
	}

	self.redirectToTeamSchduleForShiftTradeRequest = function () {
		//go to TeamSchedule view
	};

	self.showPostShiftForTradeForm = function () {
		//please fill the request view model
		setupRequestViewModel(null, null);
	};
};

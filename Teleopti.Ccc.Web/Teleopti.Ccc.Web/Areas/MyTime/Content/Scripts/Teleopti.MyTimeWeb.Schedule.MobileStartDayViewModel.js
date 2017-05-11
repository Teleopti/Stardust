/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.LayerViewModel.js" />

Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel = function (weekStart) {
	var self = this;
	var constants = Teleopti.MyTimeWeb.Common.Constants;

	self.isCommandEnable = Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_DayScheduleForStartPage_Command_44209");

	self.displayDate = ko.observable();
	self.selectedDate = ko.observable(moment().startOf("day"));
	self.selectedDateSubscription = null;

	self.summaryColor = ko.observable();
	self.summaryName = ko.observable();
	self.summaryTime = ko.observable();
	self.dayOfWeek = ko.observable();
	self.isDayOff = ko.observable(false);
	self.hasShift = ko.observable(false);
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
	self.shiftExchangePermission = ko.observable();
	self.personAccountPermission = ko.observable();
	self.requestPermission = ko.observable();
	self.menuIsVisible = ko.observable(false);

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

		self.hasShift(data.Schedule.HasMainShift || data.Schedule.HasOvertime);
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
			self.textRequestPermission(data.RequestPermission.TextRequestPermission);
			self.absenceRequestPermission(data.RequestPermission.AbsenceRequestPermission);
			self.absenceReportPermission(data.RequestPermission.AbsenceReportPermission);
			self.overtimeAvailabilityPermission(data.RequestPermission.OvertimeAvailabilityPermission);
			self.shiftExchangePermission(data.RequestPermission.ShiftExchangePermission);
			self.personAccountPermission(data.RequestPermission.PersonAccountPermission);
			self.requestPermission(data.RequestPermission.TextRequestPermission || data.RequestPermission.AbsenceRequestPermission);
		}
	};

	self.setSelectedDateSubscription = function() {
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

	var cancelAddingNewRequest = function () {
		self.requestViewModel(undefined);
	};

	self.showAddTextRequestForm = function () {
		var requestViewModel = new Teleopti.MyTimeWeb.Request
			.RequestViewModel(Teleopti.MyTimeWeb.Request.RequestDetail.AddTextOrAbsenceRequest,
			weekStart,
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues);

		requestViewModel.AddRequestCallback = function (data) {
			var count = self.requestCount() + 1;
			self.requestCount(count); 
			cancelAddingNewRequest();
		};

		self.requestViewModel({
			model: requestViewModel,
			CancelAddingNewRequest: cancelAddingNewRequest
		});
	};

	self.enableMenu = function(){
		self.menuIsVisible(true);
	};

	self.disableMenu = function(){
		self.menuIsVisible(false);
	};
};

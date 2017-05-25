/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.LayerViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel.js" />


Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel = function (weekStart, parent, dataService) {
	var self = this;

	var constants = Teleopti.MyTimeWeb.Common.Constants;

	self.displayDate = ko.observable();
	self.selectedDate = ko.observable(moment().startOf("day"));
	self.currentUserDate = ko.observable();
	self.isToday = ko.observable(false);
	self.selectedDateSubscription = null;

	self.summaryColor = ko.observable();
	self.textColor = ko.observable();
	self.summaryName = ko.observable();
	self.summaryTime = ko.observable();
	self.dayOfWeek = ko.observable();
	self.isFullDayAbsence = false;
	self.isDayOff = ko.observable(false);
	self.summaryVisible = ko.observable(false);
	self.hasOvertime = ko.observable(false);
	self.hasScheduled = ko.observable(true);
	self.timeLines = ko.observableArray();
	self.periods = [];
	self.layers = ko.observableArray();
	self.scheduleHeight = ko.observable();
	self.unreadMessageCount = ko.observable(0);
	self.requestCount = ko.observable(0);
	self.requestViewModel = ko.observable();

	self.textRequestPermission = ko.observable();
	self.absenceRequestPermission = ko.observable();
	self.absenceReportPermission = ko.observable();
	self.overtimeAvailabilityPermission = ko.observable();
	self.shiftTradeRequestPermission = ko.observable();
	self.personAccountPermission = ko.observable();
	self.requestPermission = ko.observable();
	self.showAbsenceReportingCommandItem = ko.observable();
	self.showPostShiftTradeMenu = ko.observable(false);
	self.baseUtcOffsetInMinutes = 0;

	self.overtimeAvailabililty = null;

	self.menuIsVisible = ko.observable(false);
	self.menuIconIsVisible = ko.observable(true);
	self.focusingRequestForm = ko.observable(false);
	self.isCommandEnable = Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_DayScheduleForStartPage_Command_44209");

	self.datePickerFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);
	self.requestDay = null;

	var initializeProbabilityType = Teleopti.MyTimeWeb.Portal.ParseHash().probability;
	self.selectedProbabilityOptionValue = ko.observable(initializeProbabilityType != undefined ? initializeProbabilityType : 0);
	self.showingAbsenceProbability = ko.observable(initializeProbabilityType === constants.probabilityType.absence);
	self.showingOvertimeProbability = ko.observable(initializeProbabilityType === constants.probabilityType.overtime);
	self.absenceProbabilityEnabled = ko.observable(false);
	self.showProbabilityOptionsToggleIcon = ko.observable(false);
	self.mergeIdenticalProbabilityIntervals = true;
	self.hideProbabilityEarlierThanNow = false;
	self.staffingProbabilityOnMobileEnabled = ko.observable(false);
	self.loadingProbabilityData = ko.observable(false);
	self.probabilities = ko.observableArray();
	self.userNowInMinute = ko.observable(0);
	self.userTexts = Teleopti.MyTimeWeb.Common.GetUserTexts();
	self.openHourPeriod = null;
	self.isLoading = ko.observable(false);

	self.navigateToMessages = function () {
		Teleopti.MyTimeWeb.Portal.NavigateTo("MessageTab");
	};

	self.navigateToRequests = function () {
		Teleopti.MyTimeWeb.Portal.NavigateTo("Requests/Index");
	};

	self.readData = function (data) {
		self.requestDay = moment(data.Date);

		self.overtimeAvailabililty = data.Schedule.OvertimeAvailabililty;

		self.displayDate(moment(data.Date).format(Teleopti.MyTimeWeb.Common.DateFormat));
		self.summaryColor(data.Schedule.Summary.Color);
		self.textColor(Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(self.summaryColor()));
        self.summaryName(data.Schedule.Summary.Title);
		self.hasScheduled(!data.Schedule.HasNotScheduled);
		self.summaryVisible(true);
		self.summaryTime(data.Schedule.Summary.TimeSpan);
		self.isDayOff(data.Schedule.IsDayOff);
		self.isFullDayAbsence = data.Schedule.IsFullDayAbsence;
		self.periods = data.Schedule.Periods;
		self.unreadMessageCount(data.UnReadMessageCount);
		self.openHourPeriod = data.Schedule.OpenHourPeriod;

		self.hasOvertime(data.Schedule.HasOvertime);
		self.requestCount(data.Schedule.TextRequestCount);
		self.baseUtcOffsetInMinutes = data.BaseUtcOffsetInMinutes;

		if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
			var dayDate = moment(data.Schedule.FixedDate, Teleopti.MyTimeWeb.Common.ServiceDateFormat);
			self.dayOfWeek(dayDate.format("dddd"));
		} else {
			self.dayOfWeek(data.Schedule.Header.Title);
		}

		var scheduleHeight = Teleopti.MyTimeWeb.Schedule.GetScheduleHeight();
		self.scheduleHeight(scheduleHeight + "px");

		var timelines = ko.utils.arrayMap(data.TimeLine, function (item) {
			// 5 is half of timeline label height (10px)
			return new Teleopti.MyTimeWeb.Schedule.TimelineViewModel(item, scheduleHeight, -5);
		});
		self.timeLines(timelines);

		var rawPeriods = data.Schedule.Periods || [];
		var layers = rawPeriods.map(function (item, index) {
			var layer = new Teleopti.MyTimeWeb.Schedule.LayerViewModel(item, self, true);
			layer.isLastLayer = index == rawPeriods.length - 1;
			return layer;
		});
		self.layers(layers);


		if (data.RequestPermission) {
			self.overtimeAvailabilityPermission(data.RequestPermission.OvertimeAvailabilityPermission);
			self.absenceReportPermission(data.RequestPermission.AbsenceReportPermission);
			self.textRequestPermission(data.RequestPermission.TextRequestPermission);
			self.absenceRequestPermission(data.RequestPermission.AbsenceRequestPermission);
			self.shiftTradeRequestPermission(data.RequestPermission.ShiftTradeRequestPermission);
			self.personAccountPermission(data.RequestPermission.PersonAccountPermission);
		}

		setSelectedDateSubscription(data.Date);

		var dateDiff = self.selectedDate().startOf('day').diff(moment().startOf('day'), 'days');
		var isTodayOrTomorrow = dateDiff === 0 || dateDiff === 1;

		self.showAbsenceReportingCommandItem(self.absenceReportPermission() && isTodayOrTomorrow);
		setStaffingProbabilityToggleStates(data);

		if (!self.showProbabilityOptionsToggleIcon()) {
			self.probabilities([]);
		}

		if (self.showProbabilityOptionsToggleIcon() && (self.selectedProbabilityOptionValue() === constants.probabilityType.absence || self.selectedProbabilityOptionValue() == constants.probabilityType.overtime))
			self.reloadProbabilityData();

		setPostShiftTradeMenuVisibility(data);
		self.currentUserDate(getCurrentUserDate());
		self.isLoading(false);
	};

	function setPostShiftTradeMenuVisibility(data) {
		if (!data.RequestPermission) {
			return;
		}
		if (!data.RequestPermission.ShiftExchangePermission) {
			self.showPostShiftTradeMenu(false);
			return;
		}
		var shiftTradeRequestSetting = data.ShiftTradeRequestSetting;
		var shiftExchangeOfferViewModel = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel();
		shiftExchangeOfferViewModel.DateTo(self.requestDay);
		shiftExchangeOfferViewModel.OpenPeriodRelativeStart(shiftTradeRequestSetting.OpenPeriodRelativeStart);
		shiftExchangeOfferViewModel.OpenPeriodRelativeEnd(shiftTradeRequestSetting.OpenPeriodRelativeEnd);
		self.showPostShiftTradeMenu(shiftExchangeOfferViewModel.IsSelectedDateInShiftTradePeriod());
	}

	function setSelectedDateSubscription(date) {
		if (self.selectedDateSubscription)
			self.selectedDateSubscription.dispose();

		self.selectedDate(moment(date));

		self.selectedDateSubscription = self.selectedDate.subscribe(function (date) {
			parent.ReloadSchedule(date);
		});
	};

	function fillRequestFormData(requestViewModel) {
		if (requestViewModel.DateFrom) {
			requestViewModel.DateFrom(self.requestDay);
		}
		if (requestViewModel.DateTo) {
			requestViewModel.DateTo(self.requestDay);
		}
	}

	function getCurrentUserDate() {
		var day = Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(self.baseUtcOffsetInMinutes).format(constants.dateOnlyFormat);
		return moment(day);
	}

	self.today = function () {
		self.currentUserDate = ko.observable(getCurrentUserDate());
		parent.ReloadSchedule(self.currentUserDate());
	};

	self.nextDay = function () {
		var nextDate = moment(self.selectedDate()).add(1, 'days');
		parent.ReloadSchedule(nextDate);
	};

	self.previousDay = function () {
		var previousDate = moment(self.selectedDate()).add(-1, 'days');
		parent.ReloadSchedule(previousDate);
	};

	function setStaffingProbabilityToggleStates(data) {
		self.staffingProbabilityOnMobileEnabled(data.ViewPossibilityPermission
			&& Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913"));

		self.absenceProbabilityEnabled(self.staffingProbabilityOnMobileEnabled() && data.CheckStaffingByIntraday);

		if (!self.absenceProbabilityEnabled() && self.selectedProbabilityOptionValue() === constants.probabilityType.absence) {
			self.selectedProbabilityOptionValue(constants.probabilityType.none);
			self.showingAbsenceProbability(false);
			self.showingOvertimeProbability(false);
		}

		var withinProbabilityDisplayPeriod = self.selectedDate() >= getCurrentUserDate() && self.selectedDate() < getCurrentUserDate().add('day', constants.maximumDaysDisplayingProbability).startOf('day');
		self.showProbabilityOptionsToggleIcon(self.staffingProbabilityOnMobileEnabled() && withinProbabilityDisplayPeriod && self.hasScheduled());
	}

	var probabilityOptionModel = {
		model: new Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel(self.selectedProbabilityOptionValue(), self),
		type: function () { return "probabilityOptions" },
		OnProbabilityOptionSelectCallback: function (selectedOptionValue) { self.onProbabilityOptionSelectCallback(selectedOptionValue); }
	};

	self.toggleProbabilityOptionsPanel = function () {
		probabilityOptionModel.model = new Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel(self.selectedProbabilityOptionValue(), self);

		if (self.requestViewModel() && self.requestViewModel().type() === probabilityOptionModel.type()) {
			self.requestViewModel(undefined);
			self.focusingRequestForm(false);
		} else {
			self.requestViewModel(probabilityOptionModel);
			self.focusingRequestForm(true);
		}
	};

	self.onProbabilityOptionSelectCallback = function (selectedOptionValue) {
		self.focusingRequestForm(false);

		resetRequestViewModel();
		if (selectedOptionValue === self.selectedProbabilityOptionValue()) {
			return;
		}
		self.selectedProbabilityOptionValue(selectedOptionValue);
		self.showingAbsenceProbability(self.selectedProbabilityOptionValue() === constants.probabilityType.absence);
		self.showingOvertimeProbability(self.selectedProbabilityOptionValue() === constants.probabilityType.overtime);

		if (self.selectedProbabilityOptionValue() === constants.probabilityType.none) {
			self.probabilities([]);
		}

		parent.ReloadSchedule();
	};

	self.reloadProbabilityData = function () {
		self.loadingProbabilityData(true);
		dataService.fetchProbabilityData(self.selectedDate().format('YYYY-MM-DD'), self.selectedProbabilityOptionValue(), self.updateProbabilityData);
	};

	self.updateProbabilityData = function (rawProbabilities) {
		if (!self.staffingProbabilityOnMobileEnabled()) return;
		var options = {
			probabilityType: self.selectedProbabilityOptionValue(),
			layoutDirection: constants.layoutDirection.vertical,
			timelines: self.timeLines(),
			mergeSameIntervals: self.mergeIdenticalProbabilityIntervals,
			hideProbabilityEarlierThanNow: self.hideProbabilityEarlierThanNow,
			userTexts: self.userTexts
		};
		self.fixedDate = self.selectedDate;

		self.probabilities(Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(rawProbabilities, self, options));

		self.loadingProbabilityData(false);
	};

	self.showMenu = function () {
		self.menuIsVisible(true);
		self.menuIconIsVisible(false);
	};

	self.hideMenuAndRequestForm = function () {
		self.menuIsVisible(false);
		self.focusingRequestForm(false);
		self.menuIconIsVisible(true);
		resetRequestViewModel();
	};

	function resetRequestViewModel() {
		self.requestViewModel(undefined);
		self.menuIconIsVisible(true);
		self.focusingRequestForm(false);
	}

	function setupRequestViewModel(requestViewModel, cancelAddingNewRequest) {
		self.requestViewModel({
			model: requestViewModel,
			CancelAddingNewRequest: cancelAddingNewRequest
		});
		if (requestViewModel.DateFormat) {
			requestViewModel.DateFormat(self.datePickerFormat());
		}
		self.menuIsVisible(false);
		self.menuIconIsVisible(false);
		self.focusingRequestForm(true);
	}

	function addRequestCallBack(data) {
		if (data) {
			var count = self.requestCount();
			var date = moment(new Date(data.DateFromYear, data.DateFromMonth - 1, data.DateFromDayOfMonth));
			var formattedDate = date.format(constants.dateOnlyFormat);
			if (self.requestDay.format(constants.dateOnlyFormat) === formattedDate) {
				count++;
			}
			self.requestCount(count);
		}
		resetRequestViewModel();
	}

	self.showOvertimeAvailabilityForm = function () {
		var requestViewModel = new Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel(parent.Ajax(), function (data) {
			parent.ReloadSchedule();
			resetRequestViewModel();
		});
		setupRequestViewModel(requestViewModel, resetRequestViewModel);

		fillRequestFormData(requestViewModel);
		requestViewModel.LoadRequestData(self.overtimeAvailabililty);
	};

	self.showAbsenceReportingForm = function () {
		var requestViewModel = new Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel(parent.Ajax(), function () {
			resetRequestViewModel();
		});
		setupRequestViewModel(requestViewModel, resetRequestViewModel);
		fillRequestFormData(requestViewModel);
	};

	self.showAddTextRequestForm = function () {
		var requestViewModel = new Teleopti.MyTimeWeb.Request
			.RequestViewModel(Teleopti.MyTimeWeb.Request.RequestDetail.AddTextOrAbsenceRequest,
			weekStart,
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues);

		requestViewModel.AddRequestCallback = function (data) {
			addRequestCallBack(data);
		};

		requestViewModel.AddTextRequest(false);
		setupRequestViewModel(requestViewModel, resetRequestViewModel);
		fillRequestFormData(requestViewModel);
	};

	self.showAddAbsenceRequestForm = function () {
		var requestViewModel = new Teleopti.MyTimeWeb.Request
			.RequestViewModel(Teleopti.MyTimeWeb.Request.RequestDetail.AddTextOrAbsenceRequest,
			weekStart,
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues);

		requestViewModel.AddRequestCallback = function (data) {
			addRequestCallBack(data);
		};

		requestViewModel.readPersonalAccountPermission(self.personAccountPermission());
		requestViewModel.AddAbsenceRequest(false);
		setupRequestViewModel(requestViewModel, resetRequestViewModel);
		fillRequestFormData(requestViewModel);
	}

	self.redirectToShiftTradeRequest = function () {
		Teleopti.MyTimeWeb.Portal.NavigateTo("Requests/Index/ShiftTrade/" + self.requestDay.format("YYYYMMDD"));
	};

	self.showPostShiftForTradeForm = function () {
		var requestViewModel = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModelFactory(parent.Ajax(), addRequestCallBack)
			.Create(Teleopti.MyTimeWeb.Common.DateTimeDefaultValues);

		setupRequestViewModel(requestViewModel, resetRequestViewModel);
		fillRequestFormData(requestViewModel);
	};
};

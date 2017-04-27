/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Request.ShiftTradeViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ProbabilityModels.js" />

if (typeof Teleopti === "undefined") {
	Teleopti = {};
}
if (typeof Teleopti.MyTimeWeb === "undefined") {
	Teleopti.MyTimeWeb = {};
}
if (typeof Teleopti.MyTimeWeb.Schedule === "undefined") {
	Teleopti.MyTimeWeb.Schedule = {};
}

Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel = function (userTexts, ajax, reloadData) {
	var self = this;

	var constants = Teleopti.MyTimeWeb.Common.Constants;
	var probabilityType = constants.probabilityType;

	self.userTexts = userTexts;
	self.dayViewModels = ko.observableArray();
	self.displayDate = ko.observable();
	self.nextWeekDate = ko.observable(moment());
	self.previousWeekDate = ko.observable(moment());
	self.selectedDate = ko.observable(moment().startOf("day"));
	self.currentUserDate = ko.observable();
	self.formatedCurrentUserDate = ko.computed(function () {
		return moment(self.currentUserDate()).format("YYYY-MM-DD");
	});

	self.baseUtcOffsetInMinutes = ko.observable();
	self.intradayOpenPeriod = null;

	var initializeProbabilityType = Teleopti.MyTimeWeb.Portal.ParseHash().probability;
	self.selectedProbabilityOptionValue = ko.observable(initializeProbabilityType);
	self.showingAbsenceProbability = ko.observable(initializeProbabilityType === probabilityType.absence);
	self.showingOvertimeProbability = ko.observable(initializeProbabilityType === probabilityType.overtime);

	self.absenceProbabilityEnabled = ko.observable(false);

	self.selectedDateSubscription = null;
	self.initialRequestDay = ko.observable();
	self.formattedRequestDate = ko.computed(function () {
		return moment(self.initialRequestDay()).format("YYYY-MM-DD");
	});
	self.requestViewModel = ko.observable();
	self.datePickerFormat = ko.observable();
	self.absenceReportPermission = ko.observable();
	self.overtimeAvailabilityPermission = ko.observable();

	self.staffingProbabilityOnMobileEnabled = ko.observable(false);
	self.staffingProbabilityForMultipleDaysEnabled = ko.observable(false);
	self.showProbabilityOptionsToggleIcon = ko.observable(false);
	self.mergeIdenticalProbabilityIntervals = true;
	self.hideProbabilityEarlierThanNow = false;
	self.loadingProbabilityData = ko.observable(false);

	self.maxDate = ko.observable();
	self.minDate = ko.observable();

	self.setCurrentDate = function(dateMoment){
		self.selectedDate(dateMoment);
	};

	self.desktop = function () {
		var date = self.selectedDate();
		Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" +
			Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date.format("YYYY-MM-DD")));
		//Hide AgentScheduleMessenger on mobile and show it on desktop #40179
		$("#autocollapse.bdd-mytime-top-menu ul.show-outside-toolbar li:nth-child(3)").show();
		$("#autocollapse.bdd-mytime-top-menu ul.show-outside-toolbar li:nth-child(4)").show();
	};

	self.nextWeek = function () {
		self.selectedDate(self.nextWeekDate());
		Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/MobileWeek" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(self.selectedDate().format("YYYY-MM-DD")) + getProbabilityUrl());
	};

	self.previousWeek = function () {
		self.selectedDate(self.previousWeekDate());
		Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/MobileWeek" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(self.selectedDate().format("YYYY-MM-DD")) + getProbabilityUrl());
	};

	function getProbabilityUrl(){
		if(self.selectedProbabilityOptionValue() !== probabilityType.none && self.selectedProbabilityOptionValue() && self.showProbabilityOptionsToggleIcon())
			return "/Probability/" + self.selectedProbabilityOptionValue();
		else 
			return '';
	}

	self.showAddRequestToolbar = ko.computed(function () {
		return (self.requestViewModel() || "") !== "";
	});

	self.showAddAbsenceReportFormWithData = function (data) {
		if (!self.absenceReportPermission())
			return;

		self.initialRequestDay(data.fixedDate());
		self.requestViewModel(addAbsenceReportModel);
		_fillFormData();
	};

	self.isWithinSelected = function (startDate, endDate) {
		return (startDate <= self.maxDate() && endDate >= self.minDate());
	};

	function _fillFormData(data) {
		var requestViewModel = self.requestViewModel().model;
		requestViewModel.DateFormat(self.datePickerFormat());
		var requestDay = moment(self.initialRequestDay(), Teleopti.MyTimeWeb.Common.ServiceDateFormat);
		requestViewModel.DateFrom(requestDay);
		requestViewModel.DateTo(requestDay);

		if (requestViewModel.LoadRequestData) {
			if (data && data.StartTime) {
				requestViewModel.LoadRequestData(data);
			} else {
				var day = ko.utils.arrayFirst(self.dayViewModels(), function (item) {
					return item.fixedDate() === self.initialRequestDay();
				});
				var oaData = day.overtimeAvailability();
				requestViewModel.LoadRequestData(oaData);
			}
		}
	}

	self.reloadSchedule = function () {
		self.CancelAddingNewRequest();
		reloadData && reloadData();
	};

	var addOvertimeModel = {
		model: new Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel(ajax, self.reloadSchedule),
		type: function () { return "overtime"; },
		CancelAddingNewRequest: function () { self.CancelAddingNewRequest(); }
	};

	var addAbsenceReportModel = {
		model: new Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel(ajax, self.reloadSchedule),
		type: function () { return "absenceReport"; },
		CancelAddingNewRequest: function () { self.CancelAddingNewRequest(); }
	};

	self.showAddOvertimeAvailabilityForm = function (data) {
		if (self.overtimeAvailabilityPermission() !== true) {
			return;
		}
		self.initialRequestDay(data.fixedDate());
		self.requestViewModel(addOvertimeModel);
		_fillFormData(data);
	};

	self.CancelAddingNewRequest = function () {
		self.requestViewModel(undefined);
	};

	var probabilityOptionModel = {
		model: new Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel(self.selectedProbabilityOptionValue(), self),
		type: function () { return "probabilityOptions" },
		OnProbabilityOptionSelectCallback: function (selectedOptionValue) { self.OnProbabilityOptionSelectCallback(selectedOptionValue); }
	};

	self.toggleProbabilityOptionsPanel = function (data) {
		probabilityOptionModel.model = new Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel(self.selectedProbabilityOptionValue(), self);
		if(data && data.fixedDate)
			self.initialRequestDay(data.fixedDate());
		else
			self.initialRequestDay();

		if (self.requestViewModel() && self.requestViewModel().type() === probabilityOptionModel.type()) {
			self.requestViewModel(undefined);
		} else {
			self.requestViewModel(probabilityOptionModel);
		}
	};

	self.showProbabilityOptionsForm = ko.computed(function() {
		return self.showProbabilityOptionsToggleIcon() 
			&& self.requestViewModel() != undefined
			&& self.requestViewModel() != undefined
			&& self.requestViewModel().type() === probabilityOptionModel.type();
	});

	self.OnProbabilityOptionSelectCallback = function (selectedOptionValue) {
		if (selectedOptionValue === self.selectedProbabilityOptionValue()) {
			self.requestViewModel(undefined);
			return;
		}

		self.selectedProbabilityOptionValue(selectedOptionValue);
		self.showingAbsenceProbability(self.selectedProbabilityOptionValue() === probabilityType.absence);
		self.showingOvertimeProbability(self.selectedProbabilityOptionValue() === probabilityType.overtime);

		self.requestViewModel(undefined);
		if(self.selectedProbabilityOptionValue() == constants.probabilityType.none){
			self.dayViewModels().forEach(function(d){
				d.probabilities([]);
			});
			self.loadingProbabilityData(false);
			return;
		}
		self.fetchProbabilityData();
	};

	self.fetchProbabilityData = function() {
		self.loadingProbabilityData(true);
		ajax.Ajax({
			url: "../api/ScheduleStaffingPossibility",
			dataType: "json",
			type: "GET",
			data: {
				date: self.selectedDate().format('YYYY-MM-DD'),
				staffingPossiblityType: self.selectedProbabilityOptionValue()
			},
			success: function (data) {
				self.updateProbabilityData(data);
			}
		});
	};

	self.updateProbabilityData = function(data){
		if(!self.staffingProbabilityOnMobileEnabled()) return;

		var options = {
			probabilityType: self.selectedProbabilityOptionValue(),
			layoutDirection: constants.layoutDirection.horizontal,
			timelines: self.timeLines(),
			intradayOpenPeriod: self.intradayOpenPeriod,
			mergeSameIntervals: self.mergeIdenticalProbabilityIntervals,
			hideProbabilityEarlierThanNow: self.hideProbabilityEarlierThanNow,
			userTexts: self.userTexts
		};
		if(self.staffingProbabilityForMultipleDaysEnabled()) {
			self.dayViewModels().forEach(function(day){
				var rawProbabilities = data.filter(function(d){return d.Date == day.fixedDate();});
				day.probabilities(Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(rawProbabilities, day, options));
			});
		}else{
			self.dayViewModels().forEach(function(day){
				if(day.fixedDate() == self.formatedCurrentUserDate()){
					var rawProbabilities = data.filter(function(d){return d.Date == day.fixedDate();});
					day.probabilities(Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(rawProbabilities, day, options));
				}
			});
		}
		self.loadingProbabilityData(false);
	};

	function setStaffingProbabilityToggleStates(data){
		self.staffingProbabilityOnMobileEnabled(data.ViewPossibilityPermission
			&& Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913"));

		self.staffingProbabilityForMultipleDaysEnabled(data.ViewPossibilityPermission
			&& Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880"));

		self.absenceProbabilityEnabled = ko.observable((self.staffingProbabilityOnMobileEnabled() || self.staffingProbabilityForMultipleDaysEnabled()) && data.CheckStaffingByIntraday);
		
		if (!self.absenceProbabilityEnabled() && self.selectedProbabilityOptionValue() === probabilityType.absence) {
			self.selectedProbabilityOptionValue(probabilityType.none);
			self.showingAbsenceProbability(false);
			self.showingOvertimeProbability(false);
		}
		if(self.staffingProbabilityOnMobileEnabled() && self.staffingProbabilityForMultipleDaysEnabled()){
			var interceptWith14Days = (moment(data.Days[data.Days.length - 1].FixedDate) >= moment(self.formatedCurrentUserDate())) && (moment(data.Days[0].FixedDate) < moment(self.formatedCurrentUserDate()).add('day', constants.maximumDaysDisplayingProbability));
			self.showProbabilityOptionsToggleIcon(interceptWith14Days);
		}
	}

	self.readData = function(data) {
		if (data.DatePickerFormat != undefined && data.DatePickerFormat != null) {
			self.datePickerFormat(data.DatePickerFormat.toUpperCase());
		} else {
			self.datePickerFormat("");
		}

		setStaffingProbabilityToggleStates(data);

		self.baseUtcOffsetInMinutes(data.BaseUtcOffsetInMinutes);
		self.currentUserDate(moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(self.baseUtcOffsetInMinutes)));

		var hasAbsenceReportPermission = false;
		var hasOvertimeAvailabilityPermission = false;
		if (data.RequestPermission != null) {
			hasAbsenceReportPermission = data.RequestPermission.AbsenceReportPermission;
			hasOvertimeAvailabilityPermission = data.RequestPermission.OvertimeAvailabilityPermission;
		}

		var timelines = ko.utils.arrayMap(data.TimeLine, function(rawTimeline) {
			var hourMinuteSecond = rawTimeline.Time.split(":");
			return {
				minutes: hourMinuteSecond[0] * 60 + parseInt(hourMinuteSecond[1])
			};
		});
		self.timeLines = ko.observableArray(timelines);

		self.intradayOpenPeriod = data.SiteOpenHourIntradayPeriod &&
			{
				"startTime": data.SiteOpenHourIntradayPeriod.StartTime,
				"endTime": data.SiteOpenHourIntradayPeriod.EndTime
			};

		self.absenceReportPermission(hasAbsenceReportPermission);
		self.overtimeAvailabilityPermission(hasOvertimeAvailabilityPermission);

		var dayViewModels = [];
		if(Array.isArray(data.Days) && data.Days.length > 0) {
			dayViewModels = data.Days.map(function(scheduleDay) {
				return new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, hasAbsenceReportPermission, hasOvertimeAvailabilityPermission, self);
			});
		}

		self.dayViewModels(dayViewModels);
		self.minDate(moment(data.Days[0].FixedDate).add("day", -1));
		self.maxDate(moment(data.Days[data.Days.length - 1].FixedDate).add("day", 1));
		self.displayDate(data.PeriodSelection.Display);
		if(self.showProbabilityOptionsToggleIcon() && (self.selectedProbabilityOptionValue() == constants.probabilityType.absence || self.selectedProbabilityOptionValue() == constants.probabilityType.overtime))
			self.fetchProbabilityData();
	};
};
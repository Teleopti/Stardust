/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Content/moment/moment.js" />

if (typeof (Teleopti) === 'undefined') {
    Teleopti = {};
}
if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
    Teleopti.MyTimeWeb = {};
}
if (typeof (Teleopti.MyTimeWeb.Schedule) === 'undefined') {
    Teleopti.MyTimeWeb.Schedule = {};
}

Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel = function (ajax, reloadData) {
	var self = this;

	self.dayViewModels = ko.observableArray();
	self.displayDate = ko.observable();
	self.nextWeekDate = ko.observable(moment());
	self.previousWeekDate = ko.observable(moment());
	self.selectedDate = ko.observable(moment().startOf('day'));
	
	self.selectedDateSubscription = null;
	self.initialRequestDay = ko.observable();
	self.formattedRequestDate = ko.computed(function () {
		return moment(self.initialRequestDay()).format("l");
	});
	self.requestViewModel = ko.observable();
	self.datePickerFormat = ko.observable();
	self.absenceReportPermission = ko.observable();

	self.setCurrentDate = function (date) {
		if (self.selectedDateSubscription)
			self.selectedDateSubscription.dispose();
		self.selectedDate(date);
		self.selectedDateSubscription = self.selectedDate.subscribe(function (d) {
			Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/MobileWeek" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')));
		});
	};

	self.desktop = function() {
		var date = self.selectedDate();
		Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date.format('YYYY-MM-DD')));
	};

	self.nextWeek = function () {
		self.selectedDate(self.nextWeekDate());
	};

	self.previousWeek = function () {
		self.selectedDate(self.previousWeekDate());
	};

	self.showAddRequestToolbar = ko.computed(function () {
		return (self.requestViewModel() || '') != '';
	});

	self.showAddRequestForm = function (day) {
		self.showAddRequestFormWithData(day.fixedDate());
	};
	var defaultRequestFunction = function () {
		if (self.absenceReportPermission())
			return self.showAddAbsenceReportForm;
	}
	self.showAddRequestFormWithData = function (date) {
		self.initialRequestDay(date);

		if ((self.requestViewModel() != undefined) && (self.requestViewModel().type() == 'absenceReport')) {
			self.requestViewModel(null);
		}

		if ((self.requestViewModel() || '') != '') {
			_fillFormData();
			return;
		}

		defaultRequestFunction()();
	};


	function _fillFormData() {
		var requestViewModel = self.requestViewModel().model;
		requestViewModel.DateFormat(self.datePickerFormat());
		var requestDay = moment(self.initialRequestDay());
		requestViewModel.DateFrom(requestDay);
		requestViewModel.DateTo(requestDay);
	}

	var addAbsenceReportModel = {
		model: new Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel(ajax, reloadSchedule),
		type: function () { return 'absenceReport'; },
		CancelAddingNewRequest: function() { self.CancelAddingNewRequest(); }
	};

	self.showAddAbsenceReportForm = function (data) {
		if (self.absenceReportPermission() !== true) {
			return;
		}
		self.requestViewModel(addAbsenceReportModel);
		_fillFormData(data);
	};

	self.CancelAddingNewRequest = function () {
		self.requestViewModel(undefined);
	};

	function reloadSchedule() {
		self.CancelAddingNewRequest();
		self.dayViewModels([]);
		reloadData();
	}
	self.readData = function (data) {
		if (data.DatePickerFormat != undefined && data.DatePickerFormat != null) {
			self.datePickerFormat(data.DatePickerFormat.toUpperCase());
		} else {
			self.datePickerFormat("");
		}
		var hasAbsenceReportPermission = data.RequestPermission != null ? data.RequestPermission.AbsenceReportPermission : false;
		self.absenceReportPermission(hasAbsenceReportPermission);
		ko.utils.arrayForEach(data.Days, function(scheduleDay) {
			var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, hasAbsenceReportPermission);
			self.dayViewModels.push(vm);
		});
		self.displayDate(data.PeriodSelection.Display);
	};
};

Teleopti.MyTimeWeb.Schedule.MobileDayViewModel = function(scheduleDay, absenceReportPermission) {
	var self = this;
	self.summaryName = ko.observable(scheduleDay.Summary ? scheduleDay.Summary.Title : null);
	self.summaryTimeSpan = ko.observable(scheduleDay.Summary ? scheduleDay.Summary.TimeSpan : null);
	self.summaryColor = ko.observable(scheduleDay.Summary ? scheduleDay.Summary.Color : null);
	self.fixedDate = ko.observable(scheduleDay.FixedDate);
	self.formattedFixedDate = ko.computed(function () {
		return moment(self.fixedDate()).format("l");
	});
	self.weekDayHeaderTitle = ko.observable(scheduleDay.Header ? scheduleDay.Header.Title : null);
	self.summaryStyleClassName = ko.observable(scheduleDay.Summary ? scheduleDay.Summary.StyleClassName : null);
	self.isDayoff = function() {
		if (self.summaryStyleClassName() != undefined && self.summaryStyleClassName() != null) {
			return self.summaryStyleClassName() == "dayoff striped";
		}
		return false;
	};
	self.hasOvertime = scheduleDay.HasOvertime && !scheduleDay.IsFullDayAbsence;
	if (self.summaryColor() == null && self.hasOvertime) self.summaryTimeSpan = scheduleDay.Periods[0].TimeSpan;
	self.hasShift = self.summaryColor() != null ? true : false;

    self.backgroundColor = scheduleDay.Summary ? scheduleDay.Summary.Color : null;
    self.summaryTextColor = ko.observable(self.backgroundColor ? Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(self.backgroundColor) : 'black');

    self.absenceReportPermission = ko.observable(absenceReportPermission != undefined ? absenceReportPermission : false);
	self.isPermittedToReportAbsence = ko.computed(function () {
		var momentToday = (new Date().getTeleoptiTime == undefined)
			? moment().startOf('day')
			: moment(new Date(new Date().getTeleoptiTime())).startOf('day');
		var momentCurrentDate = moment(self.fixedDate());

		var dateDiff = momentCurrentDate.diff(momentToday, 'days');

		//Absence report is available only for today and tomorrow.
		var isPermittedDate = (dateDiff == 0 || dateDiff == 1);
		var result = self.absenceReportPermission() && isPermittedDate;
		return result;
	});
};


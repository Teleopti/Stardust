﻿/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
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
	self.overtimeAvailabilityPermission = ko.observable();

	self.maxDate = ko.observable();
	self.minDate = ko.observable();

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

	self.showAddRequestFormWithData = function (date) {
		self.initialRequestDay(date);
		if ((self.requestViewModel() != undefined) && (self.requestViewModel().type() == 'absenceReport')) {
			self.requestViewModel(null);
		}

		if ((self.requestViewModel() || '') != '') {
			_fillFormData();
			return;
		}

		if (self.absenceReportPermission())
			self.showAddAbsenceReportForm();	
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
					return item.fixedDate() == self.initialRequestDay();
				});
				var oaData = day.overtimeAvailability();
				requestViewModel.LoadRequestData(oaData);
			}
		}
	}

	var addOvertimeModel = {
		model: new Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel(ajax, reloadSchedule),
		type: function () { return 'overtime'; },
		CancelAddingNewRequest: function () { self.CancelAddingNewRequest(); }
	};


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

	self.showAddOvertimeAvailabilityForm = function (data) {		
		if (self.overtimeAvailabilityPermission() !== true) {
			return;
		}
		self.initialRequestDay(data.fixedDate());
		self.requestViewModel(addOvertimeModel);
		_fillFormData(data);
	}


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
		var hasOvertimeAvailabilityPermission = data.RequestPermission != null ? data.RequestPermission.OvertimeAvailabilityPermission : false;

		self.absenceReportPermission(hasAbsenceReportPermission);
		self.overtimeAvailabilityPermission(hasOvertimeAvailabilityPermission);


		var dayViewModels = data.Days.map(function(scheduleDay) {
			return new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, hasAbsenceReportPermission, hasOvertimeAvailabilityPermission);
		});

		self.dayViewModels(dayViewModels);
	
		self.minDate(moment(data.Days[0].FixedDate).add('day', -1));
		self.maxDate(moment(data.Days[ data.Days.length -1  ].FixedDate).add('day', 1));
		
		self.displayDate(data.PeriodSelection.Display);
	};
};

Teleopti.MyTimeWeb.Schedule.MobileDayViewModel = function (scheduleDay, absenceReportPermission, overtimeAvailabilityPermission) {
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

	if (self.summaryColor() == null && self.hasOvertime) {
		var timespan = [];
		var count = scheduleDay.Periods.length;
		for (var i = 0; i < count; i++) {
			timespan.push(scheduleDay.Periods[i].TimeSpan);
		}
		timespan.sort();
		self.summaryTimeSpan(timespan[0].slice(0,-8) + timespan[count-1].slice(-8));
	}

	self.hasShift = self.summaryColor() != null ? true : false;

    self.backgroundColor = scheduleDay.Summary ? scheduleDay.Summary.Color : null;
    self.summaryTextColor = ko.observable(self.backgroundColor ? Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(self.backgroundColor) : 'black');

    self.absenceReportPermission = ko.observable(absenceReportPermission != undefined ? absenceReportPermission : false);
    self.overtimeAvailabilityPermission = ko.observable(overtimeAvailabilityPermission != undefined ? overtimeAvailabilityPermission : false);
    self.overtimeAvailability = ko.observable(scheduleDay.OvertimeAvailabililty);

    self.hasOvertimeAvailability = ko.observable(scheduleDay.OvertimeAvailabililty? scheduleDay.OvertimeAvailabililty.HasOvertimeAvailability : false);
    self.overtimeAvailabilityStart = ko.observable(scheduleDay.OvertimeAvailabililty? scheduleDay.OvertimeAvailabililty.StartTime : null);
    self.overtimeAvailabilityEnd = ko.observable(scheduleDay.OvertimeAvailabililty? scheduleDay.OvertimeAvailabililty.EndTime : null);

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

	self.layers = ko.utils.arrayMap(scheduleDay.Periods, function (item) {
		return new MobileWeekLayerViewModel(item, parent);
	});
};

var MobileWeekLayerViewModel = function (layer, parent) {
	var scheduleHeight = 668;
	var pixelToDisplayAll = 38;
	var pixelToDisplayTitle = 16;
	var self = this;

	self.title = ko.observable(layer.Title);
	self.hasMeeting = ko.computed(function () {
		return layer.Meeting != null;
	});
	self.meetingTitle = ko.computed(function () {
		if (self.hasMeeting()) {
			return layer.Meeting.Title;
		}
		return null;
	});
	self.meetingLocation = ko.computed(function () {
		if (self.hasMeeting()) {
			return layer.Meeting.Location;
		}
		return null;
	});
	self.meetingDescription = ko.computed(function () {
		if (self.hasMeeting()) {
			if (layer.Meeting.Description.length > 300) {
				return layer.Meeting.Description.substring(0, 300) + '...';
			}
			return layer.Meeting.Description;
		}
		return null;
	});

	self.timeSpan = ko.computed(function () {
		var originalTimespan = layer.TimeSpan;
		// Remove extra space for extreme long timespan (For example: "10:00 PM - 12:00 AM +1")
		var realTimespan = originalTimespan.length >= 22 ? originalTimespan.replace(" - ", "-").replace(" +1", "+1") : originalTimespan;
		return realTimespan;
	});

	self.tooltipText = ko.computed(function () {
		//not nice! rewrite tooltips in the future!
		var text = '';
		if (self.hasMeeting()) {
			text = ('<div>{0}</div><div style="text-align: left">' +
				'<div class="tooltip-wordwrap" style="overflow: hidden"><i>{1}</i> {2}</div>' +
				'<div class="tooltip-wordwrap" style="overflow: hidden"><i>{3}</i> {4}</div>' +
				'<div class="tooltip-wordwrap" style="white-space: normal"><i>{5}</i> {6}</div>' +
				'</div>')
				.format(self.timeSpan(),
						parent.userTexts.subjectColon,
						$('<div/>').text(self.meetingTitle()).html(),
						parent.userTexts.locationColon,
						$('<div/>').text(self.meetingLocation()).html(),
						parent.userTexts.descriptionColon,
						$('<div/>').text(self.meetingDescription()).html());
		} else {
			text = self.timeSpan();
		}

		return '<div>{0}</div>{1}'.format(self.title(), text);
	});

	self.backgroundColor = ko.observable('rgb(' + layer.Color + ')');
	self.textColor = ko.computed(function () {
		if (layer.Color != null && layer.Color != 'undefined') {
			var backgroundColor = 'rgb(' + layer.Color + ')';
			return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
		}
		return 'black';
	});

	self.startPositionPercentage = ko.observable(layer.StartPositionPercentage);
	self.endPositionPercentage = ko.observable(layer.EndPositionPercentage);
	self.overtimeAvailabilityYesterday = layer.OvertimeAvailabilityYesterday;
	self.isOvertimeAvailability = ko.observable(layer.IsOvertimeAvailability);
	self.isOvertime = layer.IsOvertime;
	self.left = ko.computed(function () {
		return self.startPositionPercentage();
	});
	self.widthPer = ko.computed(function () {
		return 100 * (self.endPositionPercentage() - self.startPositionPercentage()) + '%';
	});
	self.leftPer = ko.computed(function () {
		return self.left() * 100 + '%';
	});
	self.overTimeLighterBackgroundStyle = ko.computed(function () {
		var rgbTohex = function (rgb) {
			if (rgb.charAt(0) === '#')
				return rgb;
			var ds = rgb.split(/\D+/);
			var decimal = Number(ds[1]) * 65536 + Number(ds[2]) * 256 + Number(ds[3]);
			var digits = 6;
			var hexString = decimal.toString(16);
			while (hexString.length < digits)
				hexString += "0";

			return "#" + hexString;
		}

		var getLumi = function (cstring) {
			var matched = /#([\w\d]{2})([\w\d]{2})([\w\d]{2})/.exec(cstring);
			if (!matched) return null;
			return (299 * parseInt(matched[1], 16) + 587 * parseInt(matched[2], 16) + 114 * parseInt(matched[3], 16)) / 1000;
		}

		var lightColor = "#00ffff";
		var darkColor = "#795548";
		var backgroundColor = rgbTohex(self.backgroundColor());
		var useLighterStyle = Math.abs(getLumi(backgroundColor) - getLumi(lightColor)) > Math.abs(getLumi(backgroundColor) - getLumi(darkColor));

		return useLighterStyle;
	});

	self.overTimeDarkerBackgroundStyle = ko.computed(function () { return !self.overTimeLighterBackgroundStyle(); });

	self.styleJson = ko.computed(function () {
		return {
			'left': self.leftPer,
			'width': self.widthPer,
			'color': self.textColor,
			'background-size': self.isOvertime ? '11px 11px' : 'initial',
			'background-color': self.backgroundColor
		};
	});
};
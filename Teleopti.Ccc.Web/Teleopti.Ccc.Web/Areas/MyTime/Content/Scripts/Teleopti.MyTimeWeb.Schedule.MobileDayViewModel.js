/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />

if (typeof Teleopti === "undefined") {
	Teleopti = {};
}
if (typeof Teleopti.MyTimeWeb === "undefined") {
	Teleopti.MyTimeWeb = {};
}
if (typeof Teleopti.MyTimeWeb.Schedule === "undefined") {
	Teleopti.MyTimeWeb.Schedule = {};
}

Teleopti.MyTimeWeb.Schedule.MobileDayViewModel = function (scheduleDay, absenceReportPermission, overtimeAvailabilityPermission, parent) {
	var self = this;
	var constants = Teleopti.MyTimeWeb.Common.Constants;

	self.summaryName = ko.observable(scheduleDay.Summary ? scheduleDay.Summary.Title : null);
	self.summaryTimeSpan = ko.observable(scheduleDay.Summary ? scheduleDay.Summary.TimeSpan : null);
	self.summaryColor = ko.observable(scheduleDay.Summary ? scheduleDay.Summary.Color : null);
	self.fixedDate = ko.observable(scheduleDay.FixedDate);
	self.formattedFixedDate = ko.computed(function () {
		return moment(self.fixedDate()).format("YYYY-MM-DD");
	});
	self.isToday = ko.observable(self.formattedFixedDate() === parent.formatedCurrentUserDate());

	self.userNowInMinute = ko.observable(0);
	self.mergeIdenticalProbabilityIntervals = true;
	self.hideProbabilityEarlierThanNow = false;

	self.weekDayHeaderTitle = ko.observable(scheduleDay.Header ? scheduleDay.Header.Title : null);
	self.summaryStyleClassName = ko.observable(scheduleDay.Summary ? scheduleDay.Summary.StyleClassName : null);
	self.isFullDayAbsence = scheduleDay.IsFullDayAbsence;
	self.isDayOff = scheduleDay.IsDayOff;
	self.periods = scheduleDay.Periods;
	self.siteOpenHourPeriod = scheduleDay.SiteOpenHourPeriod;
	self.probabilities = ko.observableArray();
	self.isDayoff = function () {
		return scheduleDay.IsDayOff;
	};

	self.hasOvertime = scheduleDay.HasOvertime && !scheduleDay.IsFullDayAbsence;

	if (self.summaryColor() == null && self.hasOvertime) {
		var timespan = [];
		var count = scheduleDay.Periods.length;
		for (var i = 0; i < count; i++) {
			var period = scheduleDay.Periods[i];
			if (!period.IsOvertimeAvailability)
				timespan.push(scheduleDay.Periods[i].TimeSpan);
		}
		self.summaryTimeSpan(timespan[0].slice(0, -8) + timespan[timespan.length - 1].slice(-8));
	}

	self.hasShift = self.summaryColor() != null ? true : false;

	self.backgroundColor = scheduleDay.Summary ? scheduleDay.Summary.Color : null;
	self.summaryTextColor = ko.observable(self.backgroundColor
		? Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(self.backgroundColor)
		: "black");

	self.absenceReportPermission = ko.observable(absenceReportPermission !== undefined ? absenceReportPermission : false);
	self.overtimeAvailabilityPermission = ko.observable(overtimeAvailabilityPermission !== undefined ? overtimeAvailabilityPermission : false);
	self.overtimeAvailability = ko.observable(scheduleDay.OvertimeAvailabililty);

	self.hasOvertimeAvailability = ko.observable(scheduleDay.OvertimeAvailabililty ? scheduleDay.OvertimeAvailabililty.HasOvertimeAvailability : false);
	self.overtimeAvailabilityStart = ko.observable(scheduleDay.OvertimeAvailabililty ? scheduleDay.OvertimeAvailabililty.StartTime : null);
	self.overtimeAvailabilityEnd = ko.observable(scheduleDay.OvertimeAvailabililty ? scheduleDay.OvertimeAvailabililty.EndTime : null);

	self.isPermittedToReportAbsence = ko.computed(function () {
		var momentToday = (new Date().getTeleoptiTime === undefined)
			? moment().startOf("day")
			: moment(new Date(new Date().getTeleoptiTime())).startOf("day");
		var momentCurrentDate = moment(self.fixedDate());

		var dateDiff = momentCurrentDate.diff(momentToday, "days");

		//Absence report is available only for today and tomorrow.
		var isPermittedDate = (dateDiff === 0 || dateDiff === 1);
		var result = self.absenceReportPermission() && isPermittedDate;
		return result;
	});

	self.staffingProbabilityOnMobileEnabled = ko.observable(parent.staffingProbabilityOnMobileEnabled());

	self.showStaffingProbabilityBar = ko.computed(function() {
		if(parent.staffingProbabilityForMultipleDaysEnabled())
			return  (moment(self.fixedDate()) >= moment(parent.formatedCurrentUserDate())) && (moment(self.fixedDate()) < moment(parent.formatedCurrentUserDate()).add('day', constants.maximumDaysDisplayingProbability));

		return self.formattedFixedDate() == parent.formatedCurrentUserDate();
	});

	self.showProbabilityToggleIcon = ko.computed(function () {
		//use a public toggle when staffingProbabilityForMultipleDays is enabled
		if(parent.staffingProbabilityForMultipleDaysEnabled())
			return false;
		//show probability toggle of today
		return self.isToday();
	});

	self.isModelVisible = ko.computed(function () {
		if (parent.requestViewModel() && parent.requestViewModel().type() === "probabilityOptions")
			return self.isToday() && !parent.staffingProbabilityForMultipleDaysEnabled();
		
		return self.formattedFixedDate() === parent.formattedRequestDate();
	});

	self.layers = ko.utils.arrayMap(scheduleDay.Periods, function (item) {
		return new MobileWeekLayerViewModel(item, parent.userTexts);
	});
};

var MobileWeekLayerViewModel = function (layer, userTexts) {
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
				return layer.Meeting.Description.substring(0, 300) + "...";
			}
			return layer.Meeting.Description;
		}
		return null;
	});

	self.timeSpan = ko.computed(function () {
		var originalTimespan = layer.TimeSpan;
		// Remove extra space for extreme long timespan (For example: "10:00 PM - 12:00 AM +1")
		var realTimespan = originalTimespan.length >= 22
			? originalTimespan.replace(" - ", "-").replace(" +1", "+1")
			: originalTimespan;
		return realTimespan;
	});

	self.tooltipText = ko.computed(function () {
		//not nice! rewrite tooltips in the future!
		var text = !self.hasMeeting()
			? self.timeSpan()
			: ("<div>{0}</div><div style='text-align: left'>" +
				"<div class='tooltip-wordwrap' style='overflow: hidden'><i>{1}</i> {2}</div>" +
				"<div class='tooltip-wordwrap' style='overflow: hidden'><i>{3}</i> {4}</div>" +
				"<div class='tooltip-wordwrap' style='white-space: normal'><i>{5}</i> {6}</div>" +
				"</div>")
			.format(self.timeSpan(),
				userTexts.subjectColon,
				$("<div/>").text(self.meetingTitle()).html(),
				userTexts.locationColon,
				$("<div/>").text(self.meetingLocation()).html(),
				userTexts.descriptionColon,
				$("<div/>").text(self.meetingDescription()).html());

		return "<div>{0}</div>{1}".format(self.title(), text);
	});

	self.backgroundColor = ko.observable("rgb(" + layer.Color + ")");
	self.textColor = ko.computed(function () {
		if (layer.Color != null && layer.Color !== "undefined") {
			var backgroundColor = "rgb(" + layer.Color + ")";
			return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
		}
		return "black";
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
		return 100 * (self.endPositionPercentage() - self.startPositionPercentage()) + "%";
	});
	self.leftPer = ko.computed(function () {
		return self.left() * 100 + "%";
	});
	self.overTimeLighterBackgroundStyle = ko.computed(function () {
		var rgbTohex = function (rgb) {
			if (rgb.charAt(0) === "#") {
				return rgb;
			}

			var ds = rgb.split(/\D+/);
			var decimal = Number(ds[1]) * 65536 + Number(ds[2]) * 256 + Number(ds[3]);
			var digits = 6;
			var hexString = decimal.toString(16);
			while (hexString.length < digits) {
				hexString += "0";
			}

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
		var useLighterStyle = Math.abs(getLumi(backgroundColor) - getLumi(lightColor)) >
			Math.abs(getLumi(backgroundColor) - getLumi(darkColor));

		return useLighterStyle;
	});

	self.overTimeDarkerBackgroundStyle = ko.computed(function () {
		return !self.overTimeLighterBackgroundStyle();
	});

	self.styleJson = ko.computed(function () {
		return {
			"left": self.leftPer,
			"width": self.widthPer,
			"color": self.textColor,
			"background-size": self.isOvertime ? "11px 11px" : "initial",
			"background-color": self.backgroundColor
		};
	});
};
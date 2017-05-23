/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Portal.js" />

Teleopti.MyTimeWeb.Schedule.LayerViewModel = function (layer, parent, layersOnMobile) {
	var self = this;
	var userTexts = Teleopti.MyTimeWeb.Common.GetUserTexts();
	var constants = Teleopti.MyTimeWeb.Common.Constants;

	self.title = ko.observable(layer.Title);
	self.hasMeeting = ko.computed(function () {
		return layer.Meeting !== null;
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
		var tooltipContent = "<div>{0}</div>".format(self.title());
		if (!self.hasMeeting()) {
			return tooltipContent + self.timeSpan();
		}

		var meetingDescription = layer.Meeting.Description;
		if (meetingDescription.length > 200) {
			meetingDescription = meetingDescription.substring(0, 200) + "...";
		}

		var text = ("<div>{0}</div><div style='text-align: left'>" +
			"<div class='tooltip-wordwrap' style='overflow: hidden'><i>{1}</i> {2}</div>" +
			"<div class='tooltip-wordwrap' style='overflow: hidden'><i>{3}</i> {4}</div>" +
			"<div class='tooltip-wordwrap' style='white-space: normal'><i>{5}</i> {6}</div>" +
			"</div>")
			.format(self.timeSpan(),
			userTexts.SubjectColon, $("<div/>").text(layer.Meeting.Title).html(),
			userTexts.LocationColon, $("<div/>").text(layer.Meeting.Location).html(),
			userTexts.DescriptionColon, $("<div/>").text(meetingDescription).html());

		return tooltipContent + text;
	});

	self.backgroundColor = ko.observable("rgb(" + layer.Color + ")");
	self.textColor = ko.computed(function () {
		if (layer.Color !== null && layer.Color !== undefined) {
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

	var scheduleHeight = Teleopti.MyTimeWeb.Schedule.GetScheduleHeight();
	self.top = ko.computed(function () {
		return Math.round(scheduleHeight * self.startPositionPercentage());
	});
	self.height = ko.computed(function () {
		var bottom = Math.round(scheduleHeight * self.endPositionPercentage()) + 1;
		var top = self.top();
		return bottom > top ? bottom - top : 0;
	});
	self.topPx = ko.computed(function () {
		return self.top() + "px";
	});
	self.widthPx = ko.computed(function () {
		return getWidth(parent.probabilities(), layersOnMobile);
	});
	self.heightPx = ko.computed(function () {
		return self.height() + "px";
	});
	self.overTimeLighterBackgroundStyle = ko.computed(function () {
		var rgbTohex = function (rgb) {
			if (rgb.charAt(0) === "#")
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
			"top": self.topPx,
			"width": self.widthPx,
			"height": self.heightPx,
			"color": self.textColor,
			"background-size": self.isOvertime ? "11px 11px" : "initial",
			"background-color": self.backgroundColor
		};
	});

	self.heightDouble = ko.computed(function () {
		return scheduleHeight * (self.endPositionPercentage() - self.startPositionPercentage());
	});
	self.showTitle = ko.computed(function () {
		return self.heightDouble() > constants.pixelToDisplayTitle;
	});
	self.showDetail = ko.computed(function () {
		return self.heightDouble() > constants.pixelToDisplayAll;
	});
};

function getWidth(probabilities, layersOnMobile) {
	var width;
	if (probabilities && probabilities.length > 0 && layersOnMobile) {
		width = "calc(100% - 28px)";//MobileDayView.css .mobile-start-day .probability-vertical-bar{width: 28px;}
	} else {
		width = "calc(" + 100 + "%" + " - 2px)";
	}
	return width;
};
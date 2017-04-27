/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />

Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel = function() {
    var self = this;
    var constants = Teleopti.MyTimeWeb.Common.Constants;
    var probabilityType = constants.probabilityType;

    self.displayDate = ko.observable();
    self.selectedDate = ko.observable(moment().startOf("day"));
    self.selectedDateSubscription = null;

    self.summaryColor = ko.observable();
    self.summaryName = ko.observable();
    self.summaryTime = ko.observable();
    self.dayOfWeek = ko.observable(moment().format('DDDD'));
    self.isDayOff = ko.observable(false);
    self.timeLines = ko.observableArray();
    self.layers = ko.observableArray();

    var initializeProbabilityType = Teleopti.MyTimeWeb.Portal.ParseHash().probability;
    self.selectedProbabilityOptionValue = ko.observable(initializeProbabilityType);

    self.readData = function (data) {
        self.displayDate(data.DisplayDate);
        self.summaryColor(data.Schedule.Summary.Color);
        self.summaryName(data.Schedule.Summary.Title);
        self.summaryTime(data.Schedule.Summary.TimeSpan);
        self.isDayOff(data.Schedule.IsDayOff);
        
        if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
            var dayDate = moment(data.Schedule.FixedDate, Teleopti.MyTimeWeb.Common.ServiceDateFormat);
            self.dayOfWeek(dayDate.format("dddd"));
        } else {
            self.dayOfWeek(data.Schedule.Header.Title);
        }

        var timelines = ko.utils.arrayMap(data.TimeLine, function (item) {
            return new TimelineViewModel(item);
        });
        self.timeLines(timelines);

        var layers = ko.utils.arrayMap(data.Schedule.Periods, function (item) {
            return new LayerViewModel(item, parent.userTexts, self);
        });
        self.layers(layers);
    };

    self.setCurrentDate = function (date) {
        if (self.selectedDateSubscription)
            self.selectedDateSubscription.dispose();
        self.selectedDate(date);
        var probabilityUrlPart = self.selectedProbabilityOptionValue() !== probabilityType.none && self.selectedProbabilityOptionValue()
            ? "/Probability/" + self.selectedProbabilityOptionValue()
            : "";
        self.selectedDateSubscription = self.selectedDate.subscribe(function (d) {
            Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/MobileDay" +
                Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format("YYYY-MM-DD")) + probabilityUrlPart);
        });
    };

    self.today = function(){
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
}

var TimelineViewModel = function (timeline) {
    var self = this;
    self.positionPercentage = ko.observable(timeline.PositionPercentage);
    var hourMinuteSecond = timeline.Time.split(":");
    self.minutes = hourMinuteSecond[0] * 60 + parseInt(hourMinuteSecond[1]);
    var timeFromMinutes = moment().startOf("day").add("minutes", self.minutes);

    self.timeText = timeline.TimeLineDisplay;

    self.topPosition = ko.computed(function () {
        return Math.round(Teleopti.MyTimeWeb.Common.Constants.scheduleHeight * self.positionPercentage())  + "px";
    });
    self.evenHour = ko.computed(function () {
        return timeFromMinutes.minute() === 0;
    });
};

var LayerViewModel = function (layer, userTexts, parent) {
    var self = this;

    var constants = Teleopti.MyTimeWeb.Common.Constants;

    self.title = ko.observable(layer.Title);
    self.hasMeeting = ko.computed(function () {
        return layer.Meeting !== null;
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
        var tooltipContent = "<div>{0}</div>".format(self.title());
        if (!self.hasMeeting()) {
            return tooltipContent + self.timeSpan();
        }

        var text = ("<div>{0}</div><div style='text-align: left'>" +
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

    self.top = ko.computed(function () {
        return Math.round(constants.scheduleHeight * self.startPositionPercentage());
    });
    self.height = ko.computed(function () {
        var bottom = Math.round(constants.scheduleHeight * self.endPositionPercentage()) + 1;
        var top = self.top();
        return bottom > top ? bottom - top : 0;
    });
    self.topPx = ko.computed(function () {
        return self.top() + "px";
    });
    self.widthPx = ko.computed(function () {
        var width;
        if (layer.IsOvertimeAvailability) {
            width = 20 + "%";
        } else if (parent.probabilities && parent.probabilities.length > 0) {
            width = 80 + "%";
        } else {
            width = 100 + "%";
        }
        return width;
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
        return constants.scheduleHeight * (self.endPositionPercentage() - self.startPositionPercentage());
    });
    self.showTitle = ko.computed(function () {
        return self.heightDouble() > constants.pixelToDisplayTitle;
    });
    self.showDetail = ko.computed(function () {
        return self.heightDouble() > constants.pixelToDisplayAll;
    });
};
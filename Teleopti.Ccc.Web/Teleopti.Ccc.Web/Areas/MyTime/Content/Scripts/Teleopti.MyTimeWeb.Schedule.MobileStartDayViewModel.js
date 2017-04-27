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
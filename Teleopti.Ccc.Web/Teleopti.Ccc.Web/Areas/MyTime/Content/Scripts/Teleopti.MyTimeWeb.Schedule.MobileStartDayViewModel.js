/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.LayerViewModel.js" />

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
    self.hasShift = ko.observable(false);
    self.timeLines = ko.observableArray();
    self.layers = ko.observableArray();
    self.scheduleHeight = ko.observable();
    self.unreadMessageCount = ko.observable();

    var initializeProbabilityType = Teleopti.MyTimeWeb.Portal.ParseHash().probability;
    self.selectedProbabilityOptionValue = ko.observable(initializeProbabilityType);

    var setTimelineHeight = function (lastLayer) {
    var scheduleHeightPercentage = lastLayer == null ? 0 : lastLayer.EndPositionPercentage;

        var timelinePoints = self.timeLines();
        var timelineEndPositionPercentage = timelinePoints && timelinePoints.length > 0
            ? timelinePoints[timelinePoints.length - 1].positionPercentage()
            : 1;
        if (timelineEndPositionPercentage > scheduleHeightPercentage) {
            scheduleHeightPercentage = timelineEndPositionPercentage;
        }

        var height = Math.round(constants.scheduleHeight * scheduleHeightPercentage) + 1 + 'px';
        self.scheduleHeight(height);
    };

    self.navigateToMessages = function() {
	      Teleopti.MyTimeWeb.Portal.NavigateTo("MessageTab");
    };

    self.readData = function (data) {
        self.displayDate(data.DisplayDate);
        self.summaryColor(data.Schedule.Summary.Color);
        self.summaryName(data.Schedule.Summary.Title);
        self.summaryTime(data.Schedule.Summary.TimeSpan);
        self.isDayOff(data.Schedule.IsDayOff);
        self.unreadMessageCount(data.UnReadMessageCount);
        self.hasShift(data.Schedule.HasMainShift || data.Schedule.HasOvertime);

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

        var rawPeriods = data.Schedule.Periods;
        var layers = ko.utils.arrayMap(rawPeriods, function (item) {
            return new Teleopti.MyTimeWeb.Schedule.LayerViewModel(item, self);
        });
        self.layers(layers);

        setTimelineHeight(rawPeriods && rawPeriods.length > 0 ? rawPeriods[rawPeriods.length - 1] : undefined);
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
};

var TimelineViewModel = function (timeline) {
    var self = this;
    self.positionPercentage = ko.observable(timeline.PositionPercentage);
    var hourMinuteSecond = timeline.Time.split(":");
    self.minutes = hourMinuteSecond[0] * 60 + parseInt(hourMinuteSecond[1]);
    var timeFromMinutes = moment().startOf("day").add("minutes", self.minutes);

    self.timeText = timeline.TimeLineDisplay;

    self.topPosition = ko.computed(function () {
        return Math.round(Teleopti.MyTimeWeb.Common.Constants.scheduleHeight * self.positionPercentage() - 5)  + "px";
    });
    self.evenHour = ko.computed(function () {
        return timeFromMinutes.minute() === 0;
    });
};

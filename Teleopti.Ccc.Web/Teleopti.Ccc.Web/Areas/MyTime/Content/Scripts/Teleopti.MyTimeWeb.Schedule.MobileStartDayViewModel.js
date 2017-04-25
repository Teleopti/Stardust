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
    self.nextDate = ko.observable(moment());
    self.previousDate = ko.observable(moment());
    self.selectedDate = ko.observable(moment().startOf("day"));
    self.selectedDateSubscription = null;

    var initializeProbabilityType = Teleopti.MyTimeWeb.Portal.ParseHash().probability;
    self.selectedProbabilityOptionValue = ko.observable(initializeProbabilityType);

    self.readData = function (data) {
        self.displayDate(data.DisplayDate);
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
        self.selectedDate(self.nextDate());
    };

    self.previousDay = function () {
        self.selectedDate(self.previousDate());
    };
}
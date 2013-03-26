/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.DayViewModel.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
		if (typeof (Teleopti.MyTimeWeb.Preference) === 'undefined') {
			Teleopti.MyTimeWeb.Preference = {};
		}
	}
}

Teleopti.MyTimeWeb.Preference.SelectionViewModel = function (dayViewModels, maxMustHave, setMustHaveMethod) {
    var self = this;

    self.minDate = ko.observable(moment());
    self.maxDate = ko.observable(moment());

    self.displayDate = ko.observable();
    self.nextPeriodDate = ko.observable(moment());
    self.previousPeriodDate = ko.observable(moment());

    self.selectedDate = ko.observable(moment().startOf('day'));
    
    self.setCurrentDate = function (date) {
        self.selectedDate(date);
        self.selectedDate.subscribe(function (d) {
            Teleopti.MyTimeWeb.Portal.NavigateTo("Preference/Index" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')));
        });
    };

    self.nextPeriod = function () {
        self.selectedDate(self.nextPeriodDate());
    };

    self.previousPeriod = function () {
        self.selectedDate(self.previousPeriodDate());
    };

    self.addMustHave = function() {
        setMustHaveMethod(true);
    };

    self.removeMustHave = function () {
        setMustHaveMethod(false);
    };
    
    self.maxMustHave = ko.observable(maxMustHave);
    self.currentMustHaves = ko.computed(function() {
        var total = 0;
        $.each(dayViewModels, function (index, day) {
            if (day.MustHave())
                total += 1;
        });
        return total;
    });

    self.addMustHaveEnabled = ko.computed(function() {
        return self.currentMustHaves() < maxMustHave;
    });
};

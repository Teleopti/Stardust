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

Teleopti.MyTimeWeb.Preference.SelectionViewModel = function (dayViewModels, maxMustHave, setMustHaveMethod, setPreferenceMethod, deletePreferenceMethod) {
    var self = this;
    this.MaxMustHaves = ko.observable(0);
    self.minDate = ko.observable(moment());
    self.maxDate = ko.observable(moment());
    
    self.displayDate = ko.observable();
    self.nextPeriodDate = ko.observable(moment());
    self.previousPeriodDate = ko.observable(moment());

    self.selectedDate = ko.observable(moment().startOf('day'));

    self.setCurrentDate = function (date) {
        self.selectedDate(date);
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

	self.removeMustHave = function() {
		setMustHaveMethod(false);
	}

    self.mustHaveEnabled = function() {
        return self.maxMustHave() > 0;
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

	self.selectedPreference = ko.observable();
    self.availablePreferences = ko.observableArray();

    self.selectAndapplyPreference = function (item) {
    	self.selectedPreference(item);
    	setPreferenceMethod(self.selectedPreference().Value);
	};
	
    self.applyPreference = function() {
        setPreferenceMethod(self.selectedPreference().Value);
    };

    self.addMustHaveEnabled = ko.computed(function() {
        return self.currentMustHaves() < maxMustHave;
    });

    self.deletePreference = deletePreferenceMethod;
    
    self.selectedPreferenceText = ko.computed(function () {
        if (self.selectedPreference()) {
            return self.selectedPreference().Text;
        }
        return '';
    });

    self.enableDateSelection = function() {
        self.subscription = self.selectedDate.subscribe(function(d) {
            Teleopti.MyTimeWeb.Portal.NavigateTo("Preference/Index" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')));
        });
    };
};

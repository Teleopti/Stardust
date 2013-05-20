/// <reference path="~/Content/Scripts/jquery-1.8.3-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.3.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.9.1.custom.js" />
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

Teleopti.MyTimeWeb.Preference.MustHaveCountViewModel = function() {
    var self = this;
    this.MaxMustHaves = ko.observable(0);
    this.DayViewModels = ko.observableArray();
    
    this.CurrentMustHaves = ko.computed(function() {
        var total = 0;
        $.each(self.DayViewModels(), function(index, day) {
            if (day.MustHave())
                total += 1;
        });
        return total;
    });
    
    this.MustHaveText = ko.computed(function () {
        return self.CurrentMustHaves() + "(" + self.MaxMustHaves() + ")";
    });

    this.MustHaveClass = ko.computed(function() {
        if (self.CurrentMustHaves() == self.MaxMustHaves())
            return "grey-out";

        return undefined;
    });
    
    this.SetData = function (inDayViewModels, inMaxMustHave) {

        self.DayViewModels([]);
        self.DayViewModels.push.apply(self.DayViewModels, $.map(inDayViewModels, function (value, key) { return value; }));

        self.MaxMustHaves(inMaxMustHave);
    };
};

/// <reference path="~/Content/Scripts/jquery-1.6.4-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.DayViewModel.js" />

Teleopti.MyTimeWeb.Preference.MustHaveCountViewModel = function (dayViewModels) {
	var self = this;

	this.MaxMustHave = ko.observable(0);

	this.CurrentMustHave = ko.computed(function () {
		var total = 0;
		$.each(dayViewModels, function (index, day) {
			if (day.MustHave())
				total += 1;
		});
		return total;
	});

	this.MustHaveText = ko.computed(function () {
		return self.CurrentMustHave() + "(" + self.MaxMustHave() + ")";
	});
};

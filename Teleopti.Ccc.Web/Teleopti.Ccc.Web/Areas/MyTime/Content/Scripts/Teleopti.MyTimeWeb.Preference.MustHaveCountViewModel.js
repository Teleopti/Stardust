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

Teleopti.MyTimeWeb.Preference.MustHaveCountViewModel = function (dayViewModels, maxMustHave) {

	this.MustHaveText = ko.computed(function () {
		var total = 0;
		$.each(dayViewModels, function (index, day) {
			if (day.MustHave())
				total += 1;
		});
		if (total == maxMustHave) {
			$("#Preference-must-have-button").addClass("grey-out");
		}else {
			$("#Preference-must-have-button").removeClass("grey-out");
		}
		
		return total + "(" + maxMustHave + ")";
	});
};

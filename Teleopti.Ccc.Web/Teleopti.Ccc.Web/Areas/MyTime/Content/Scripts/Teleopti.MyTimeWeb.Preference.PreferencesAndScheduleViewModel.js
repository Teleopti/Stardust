/// <reference path="~/Content/Scripts/jquery-1.6.4-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.DayViewModel.js" />

Teleopti.MyTimeWeb.Preference.PreferencesAndSchedulesViewModel = function (ajax, dayViewModels) {

	this.DayViewModels = dayViewModels;

	this.LoadPreferencesAndSchedules = function (from, to) {
		var deferred = $.Deferred();
		ajax.Ajax({
			url: "Preference/PreferencesAndSchedules",
			dataType: "json",
			data: {
				From: from,
				To: to
			},
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				data = data || [];
				$.each(data, function (index, element) {
					var dayViewModel = dayViewModels[element.Date];
					if (element.Preference)
						dayViewModel.ReadPreference(element.Preference);
					if (element.DayOff)
						dayViewModel.ReadDayOff(element.DayOff);
					if (element.Absence)
						dayViewModel.ReadAbsence(element.Absence);
					if (element.PersonAssignment)
						dayViewModel.ReadPersonAssignment(element.PersonAssignment);
					if (element.Fulfilled != null) {
						dayViewModel.Fulfilled(element.Fulfilled);
					}
					dayViewModel.Feedback(element.Feedback);
					if (element.StyleClassName)
						dayViewModel.StyleClassName(element.StyleClassName);
					if (element.BorderColor)
						dayViewModel.Color(element.BorderColor);
				});
				deferred.resolve();
			}
		});
		return deferred.promise();
	};
};





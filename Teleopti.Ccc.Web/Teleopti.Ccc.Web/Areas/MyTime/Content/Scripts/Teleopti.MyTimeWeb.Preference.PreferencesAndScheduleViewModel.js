/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
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

Teleopti.MyTimeWeb.Preference.PreferencesAndSchedulesViewModel = function (ajax, dayViewModels) {
    var self = this;
    
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
			beforeSend: function (jqXHR) {
			    $.each(self.DayViewModels, function (index, day) {
					day.IsLoading(true);
				});
			},
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				data = data || [];
				$.each(data, function (index, element) {
					var dayViewModel = self.DayViewModels[element.Date];
					if (element.Preference)
						dayViewModel.ReadPreference(element.Preference);
					if (element.DayOff)
						dayViewModel.ReadDayOff(element.DayOff);
					if (element.Absence)
						dayViewModel.ReadAbsence(element.Absence);
					if (element.PersonAssignment)
						dayViewModel.ReadPersonAssignment(element.PersonAssignment);
					dayViewModel.Feedback(element.Feedback);
					if (element.StyleClassName)
						dayViewModel.StyleClassName(element.StyleClassName);
					if (element.Meetings) {
						dayViewModel.Meetings(element.Meetings);
					}
					if (element.PersonalShifts) {
						dayViewModel.PersonalShifts(element.PersonalShifts);
					}
					dayViewModel.IsLoading(false);
				});
				deferred.resolve();
			}
		});
		return deferred.promise();
	};
};





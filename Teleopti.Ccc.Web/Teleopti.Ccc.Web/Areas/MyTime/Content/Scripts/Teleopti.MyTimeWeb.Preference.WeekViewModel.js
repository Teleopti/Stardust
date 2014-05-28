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

Teleopti.MyTimeWeb.Preference.WeekViewModel = function () {
	var self = this;

	this.DayViewModels = ko.observableArray();

	self.PossibleResultWeeklyContractTimeMinutesLower = ko.computed(function() {
		var sum = 0;
		$.each(self.DayViewModels(), function (index, day) {
			 if (!(day.EditableHasNoSchedule()) || (day.EditableHasNoSchedule() && day.EditableIsInOpenPeriod() && day.DisplayFeedback())) {
			 	 var value = day.PossibleContractTimeMinutesLower();
			 	 if (value)
			 	 	 sum += parseInt(value);
			 	 var absenceValue = day.AbsenceContractTimeMinutes();
			 	 if (absenceValue)
			 	 	 sum += parseInt(absenceValue);
			 	 sum += day.ContractTimeMinutes();
			 }

		});
		return sum;
	});

	self.PossibleResultWeeklyContractTimeMinutesUpper = ko.computed(function() {
		var sum = 0;
		$.each(self.DayViewModels(), function (index, day) {
			 if (!(day.EditableHasNoSchedule()) || (day.EditableHasNoSchedule() && day.EditableIsInOpenPeriod() && day.DisplayFeedback())) {
				 var value = day.PossibleContractTimeMinutesUpper();
				 if (value)
					 sum += parseInt(value);
				 var absenceValue = day.AbsenceContractTimeMinutes();
				 if (absenceValue)
					 sum += parseInt(absenceValue);
				 sum += day.ContractTimeMinutes();
			 }
			
		});
		return sum;
	});

	self.PossibleResultWeeklyContractTimeLower = ko.computed(function() {
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(self.PossibleResultWeeklyContractTimeMinutesLower());
	});
	self.PossibleResultWeeklyContractTimeUpper = ko.computed(function() {
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(self.PossibleResultWeeklyContractTimeMinutesUpper());
	});

	self.IsWeeklyWorkTimeVisible = ko.computed(function() {
		if (self.PossibleResultWeeklyContractTimeMinutesLower() > 0)
			return true;
		return false;
	});

};


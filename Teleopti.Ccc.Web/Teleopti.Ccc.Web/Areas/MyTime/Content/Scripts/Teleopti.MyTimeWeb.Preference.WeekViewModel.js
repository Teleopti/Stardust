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
	this.ContractMaxTimePerWeek = 0;

	self.PossibleResultWeeklyContractTimeMinutesLower = ko.computed(function() {
		var sum = 0;
		$.each(self.DayViewModels(), function (index, day) {
			 if (!(day.EditableHasNoSchedule()) || (day.EditableHasNoSchedule() && day.EditableIsInOpenPeriod())) {
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
			 if (!(day.EditableHasNoSchedule()) || (day.EditableHasNoSchedule() && day.EditableIsInOpenPeriod())) {
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

	self.IsMinHoursBroken = ko.computed(function() {
		return self.PossibleResultWeeklyContractTimeMinutesLower() > self.ContractMaxTimePerWeek;
	});

	self.readContractMaxTimePerWeek = function(data) {
		 self.ContractMaxTimePerWeek = data;
	};

	this.LoadContractMaxTimeSetting = function (ajax) {
	//	 ajax.Ajax({
	//	 	 url: "Preference/ContractMaxTimePerWeek",
	//	 	 dataType: "json",
	//	 	 data: {
	//	 	 	 Date: self.DayViewModels()[0].Date
	//	 	 },
	//	 	 type: 'GET',
	//	 	 success: function (data, textStatus, jqXHR) {
	//	 	 	 self.readContractMaxTimePerWeek(data);
	//	 	 }
		 //	 });
		 self.readContractMaxTimePerWeek(60);
	};
};


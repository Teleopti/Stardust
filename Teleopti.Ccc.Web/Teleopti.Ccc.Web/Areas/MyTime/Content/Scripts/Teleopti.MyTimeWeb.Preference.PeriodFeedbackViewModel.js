﻿/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
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

Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel = function (ajax, dayViewModels, date, weekViewModels) {
	var self = this;

	this.DayViewModels = dayViewModels;
	this.WeekViewModels = weekViewModels;

	this.LoadFeedback = function () {
		ajax.Ajax({
			url: "PreferenceFeedback/PeriodFeedback",
			dataType: "json",
			data: { Date: date },
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				self.TargetDaysOffLower(data.TargetDaysOff.Lower);
				self.TargetDaysOffUpper(data.TargetDaysOff.Upper);
				self.PossibleResultDaysOff(data.PossibleResultDaysOff);
				self.TargetContractTimeLower(data.TargetContractTime.Lower);
				self.TargetContractTimeUpper(data.TargetContractTime.Upper);
			}
		});
	};

	this.TargetDaysOffLower = ko.observable();
	this.TargetDaysOffUpper = ko.observable();
	this.PossibleResultDaysOff = ko.observable();

	this.TargetContractTimeLower = ko.observable();
	this.TargetContractTimeUpper = ko.observable();

	this.IsWeeklyWorkTimeBroken = ko.computed(function () {
		var broken = false;
		$.each(self.WeekViewModels, function(index, week) {
			if ((week.IsMinHoursBroken() || week.IsMaxHoursBroken()) && week.IsInCurrentPeriod()) {
				broken = true;
				return false;
			}
		});
		return broken;
	});

	this.PossibleResultContractTimeMinutesLower = ko.computed(function () {
		var sum = 0;
		$.each(self.DayViewModels, function (index, day) {
			var value = day.PossibleContractTimeMinutesLower();
			if (value)
			    sum += parseInt(value);
		    var absenceValue = day.AbsenceContractTimeMinutes();
		    if (absenceValue)
		        sum += parseInt(absenceValue);
			sum += day.ContractTimeMinutes();
		});
		return sum;
	});

	this.PossibleResultContractTimeMinutesUpper = ko.computed(function () {
		var sum = 0;
		$.each(self.DayViewModels, function (index, day) {
			var value = day.PossibleContractTimeMinutesUpper();
			if (value)
			    sum += parseInt(value);
			var absenceValue = day.AbsenceContractTimeMinutes();
		    if (absenceValue)
		        sum += parseInt(absenceValue);
			sum += day.ContractTimeMinutes();
		});
		return sum;
	});

	this.PossibleResultContractTimeLower = ko.computed(function () {
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(self.PossibleResultContractTimeMinutesLower());
	});

	this.PossibleResultContractTimeUpper = ko.computed(function () {
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(self.PossibleResultContractTimeMinutesUpper());
	});

	this.PreferenceTimeIsOutOfRange = ko.computed(function () {
	    return self.PossibleResultContractTimeUpper() < self.TargetContractTimeLower() || self.TargetContractTimeUpper() < self.PossibleResultContractTimeLower();
	});

	this.PreferenceDaysOffIsOutOfRange = ko.computed(function () {
	    return self.PossibleResultDaysOff() > self.TargetDaysOffUpper() || self.PossibleResultDaysOff() < self.TargetDaysOffLower();
	});

	this.PreferenceFeedbackClass = ko.computed(function () {
	    return self.PreferenceDaysOffIsOutOfRange() || self.PreferenceTimeIsOutOfRange() || self.IsWeeklyWorkTimeBroken() ? 'alert-danger' : 'alert-info';
	});

};


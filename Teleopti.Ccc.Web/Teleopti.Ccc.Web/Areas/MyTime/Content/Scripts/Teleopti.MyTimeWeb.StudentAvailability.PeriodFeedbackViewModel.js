/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.StudentAvailability.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.StudentAvailability.DayViewModel.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
		if (typeof (Teleopti.MyTimeWeb.StudentAvailability) === 'undefined') {
			Teleopti.MyTimeWeb.StudentAvailability = {};
		}
	}
}

Teleopti.MyTimeWeb.StudentAvailability.PeriodFeedbackViewModel = function (ajax, dayViewModels, date, toggleEnabled) {
	var self = this;

	this.ToggleAvailabilityVerifyHours31654Enabled = ko.observable(toggleEnabled);
	this.DayViewModels = dayViewModels;
	
	this.LoadFeedback = function () {
		ajax.Ajax({
			url: "StudentAvailabilityFeedback/PeriodFeedback",
			dataType: "json",
			data: { Date: date },
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				self.TargetContractTimeLowerMinutes(data.TargetContractTime.LowerMinutes);
				self.TargetContractTimeUpperMinutes(data.TargetContractTime.UpperMinutes);
			}
		});
	};

	this.TargetContractTimeLowerMinutes = ko.observable();
	this.TargetContractTimeUpperMinutes = ko.observable();
	this.TargetContractTimeLower = ko.computed(function () {
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(self.TargetContractTimeLowerMinutes());
	});
	this.TargetContractTimeUpper = ko.computed(function () {
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(self.TargetContractTimeUpperMinutes());
	});

	this.PossibleResultContractTimeMinutesLower = ko.computed(function () {
		var sum = 0;
		$.each(self.DayViewModels, function (index, day) {
			if (day.HasAvailability()) {
				var value = day.PossibleContractTimeMinutesLower();
				if (value)
					sum += parseInt(value);
				sum += day.ContractTimeMinutes();
			}
		});
		return sum;
	});

	this.PossibleResultContractTimeMinutesUpper = ko.computed(function () {
		var sum = 0;
		$.each(self.DayViewModels, function (index, day) {
			if (day.HasAvailability()) {
				var value = day.PossibleContractTimeMinutesUpper();
				if (value)
					sum += parseInt(value);
				sum += day.ContractTimeMinutes();
			}
		});
		return sum;
	});

	this.PossibleResultContractTimeLower = ko.computed(function () {
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(self.PossibleResultContractTimeMinutesLower());
	});

	this.PossibleResultContractTimeUpper = ko.computed(function () {
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(self.PossibleResultContractTimeMinutesUpper());
	});

	this.StudentAvailabilityTimeIsOutOfRange = ko.computed(function () {
		return self.PossibleResultContractTimeMinutesUpper() < self.TargetContractTimeLowerMinutes();
	});

	this.StudentAvailabilityFeedbackClass = ko.computed(function () {
		return self.ToggleAvailabilityVerifyHours31654Enabled() && self.StudentAvailabilityTimeIsOutOfRange()
			? 'alert-danger' : 'alert-info';
	}).extend({ throttle: 1 });

};


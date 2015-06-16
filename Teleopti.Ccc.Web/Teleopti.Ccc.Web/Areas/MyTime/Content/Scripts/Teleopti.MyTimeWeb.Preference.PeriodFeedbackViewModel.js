/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
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
				self.TargetContractTimeLowerMinutes(data.TargetContractTime.LowerMinutes);
				self.TargetContractTimeUpperMinutes(data.TargetContractTime.UpperMinutes);
			}
		});
	};

	this.TargetDaysOffLower = ko.observable();
	this.TargetDaysOffUpper = ko.observable();
	this.PossibleResultDaysOff = ko.observable();

	this.TargetContractTimeLowerMinutes = ko.observable();
	this.TargetContractTimeUpperMinutes = ko.observable();
	this.TargetContractTimeLower = ko.computed(function () {
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.TargetContractTimeLowerMinutes());
	});
	this.TargetContractTimeUpper = ko.computed(function () {
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.TargetContractTimeUpperMinutes());
	});

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
	var possibleNightRestViolationsArray = ko.observableArray();

	function sameNightRestVioloation(obj1, obj2) {
		return obj1.nightRestTimes === obj2.nightRestTimes
			&& obj1.firstDay === obj2.firstDay
			&& obj1.secondDay === obj2.secondDay;

	}

	var toggleShowNightViolation = Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_PreferenceShowNightViolation_33152");

	this.PossibleNightRestViolations = function () {

		if (!toggleShowNightViolation) return possibleNightRestViolationsArray;

		possibleNightRestViolationsArray.removeAll();
		$.each(self.DayViewModels, function (index, day) {
			
			if (day.MakeANightRestViolationObj()) {

				var newViolation = day.MakeANightRestViolationObj();

				if (possibleNightRestViolationsArray().filter(function(item) { return sameNightRestVioloation(item, newViolation); }).length === 0) {
					possibleNightRestViolationsArray.push(day.MakeANightRestViolationObj());
				}
				
			}
		});
		return possibleNightRestViolationsArray;
	};


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
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.PossibleResultContractTimeMinutesLower());
	});

	this.PossibleResultContractTimeUpper = ko.computed(function () {
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.PossibleResultContractTimeMinutesUpper());
	});

	this.PreferenceTimeIsOutOfRange = ko.computed(function () {
		return self.PossibleResultContractTimeMinutesUpper() < self.TargetContractTimeLowerMinutes() || self.TargetContractTimeUpperMinutes() < self.PossibleResultContractTimeMinutesLower();
	});

	this.PreferenceDaysOffIsOutOfRange = ko.computed(function () {
	    return self.PossibleResultDaysOff() > self.TargetDaysOffUpper() || self.PossibleResultDaysOff() < self.TargetDaysOffLower();
	});

	this.PreferenceFeedbackClass = ko.computed(function () {
	    return self.PreferenceDaysOffIsOutOfRange() || self.PreferenceTimeIsOutOfRange() || self.IsWeeklyWorkTimeBroken() ? 'alert-danger' : 'alert-info';
	}).extend({ throttle: 1 });

};


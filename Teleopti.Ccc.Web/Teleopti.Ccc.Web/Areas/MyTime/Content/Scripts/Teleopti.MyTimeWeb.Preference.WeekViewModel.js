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

Teleopti.MyTimeWeb.Preference.WeekViewModel = function (ajaxForDate) {
	var self = this;

	this.DayViewModels = ko.observableArray();
	this.MaxTimePerWeekMinutesSetting = ko.observable(0);
	this.MinTimePerWeekMinutesSetting = ko.observable(0);

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

	self.IsWeeklyWorkTimeVisible = ko.computed(function () {
		var isInCurrentPeriod = false;
		$.each(self.DayViewModels(), function(index, day) {
			if (day.EditableIsInOpenPeriod()) {
				isInCurrentPeriod = true;
				return false;
			}
		});
		if (isInCurrentPeriod)
			return true;
		if (self.PossibleResultWeeklyContractTimeMinutesLower() > 0)
			return true;
		return false;
	});

	self.IsMinHoursBroken = ko.computed(function() {
		 return self.PossibleResultWeeklyContractTimeMinutesLower() > self.MaxTimePerWeekMinutesSetting();
	});

	self.IsMaxHoursBroken = ko.computed(function () {
		 return self.PossibleResultWeeklyContractTimeMinutesUpper() < self.MinTimePerWeekMinutesSetting();
	});

	self.readWeeklyWorkTimeSettings = function(data) {
		 self.MaxTimePerWeekMinutesSetting(data.MaxWorkTimePerWeekMinutes);
		 self.MinTimePerWeekMinutesSetting(data.MinWorkTimePerWeekMinutes);
	};

	this.LoadWeeklyWorkTimeSettings = function(complete) {
		if (self.DayViewModels == undefined || self.DayViewModels == null || self.DayViewModels()[0].Date == undefined) {
			complete();
			return null;
		}
		return ajaxForDate.Ajax({
			url: "Preference/WeeklyWorkTimeSetting",
			dataType: "json",
			type: 'GET',
			data: {Date: self.DayViewModels()[0].Date },
			success:function(data) {
				self.readWeeklyWorkTimeSettings(data);
			} ,
		 	 complete: complete,
		 	 statusCode404: function () { }
		 });
	};
};
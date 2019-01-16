Teleopti.MyTimeWeb.Preference.WeekViewModel = function(ajaxForDate) {
	var self = this;

	this.DayViewModels = ko.observableArray();
	this.MaxTimePerWeekMinutesSetting = ko.observable(0);
	this.MinTimePerWeekMinutesSetting = ko.observable(0);

	self.PossibleResultWeeklyContractTimeMinutesLower = ko.computed(function() {
		var sum = 0;
		$.each(self.DayViewModels(), function(index, day) {
			if (!day.EditableIsInOpenPeriod()) {
				sum = 0;
				return false; //break here only count whole weeks now
			}

			var value = day.PossibleContractTimeMinutesLower();
			if (value) sum += parseInt(value);
			var absenceValue = day.AbsenceContractTimeMinutes();
			if (absenceValue) sum += parseInt(absenceValue);
			sum += day.ContractTimeMinutes();

			return true; //here continue the each loop
		});

		return sum;
	});

	self.PossibleResultWeeklyContractTimeMinutesUpper = ko.computed(function() {
		var sum = 0;
		$.each(self.DayViewModels(), function(index, day) {
			if (!day.EditableIsInOpenPeriod()) {
				sum = 0;
				return false; //break here only count whole weeks now
			}

			var value = day.PossibleContractTimeMinutesUpper();
			if (value) sum += parseInt(value);
			var absenceValue = day.AbsenceContractTimeMinutes();
			if (absenceValue) sum += parseInt(absenceValue);
			sum += day.ContractTimeMinutes();

			return true; //here continue the each loop
		});
		return sum;
	});

	self.PossibleResultWeeklyContractTimeLower = ko.computed(function() {
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.PossibleResultWeeklyContractTimeMinutesLower());
	});

	self.PossibleResultWeeklyContractTimeUpper = ko.computed(function() {
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.PossibleResultWeeklyContractTimeMinutesUpper());
	});

	self.IsInCurrentPeriod = ko.computed(function() {
		var isInCurrentPeriod = true;
		$.each(self.DayViewModels(), function(index, day) {
			if (!day.EditableIsInOpenPeriod()) {
				isInCurrentPeriod = false;
				return false;
			}
		});

		return isInCurrentPeriod;
	});

	self.IsEditable = ko.computed(function() {
		var isEditable = false;
		$.each(self.DayViewModels(), function(index, day) {
			if (day.Editable()) {
				isEditable = true;
				return false;
			}
		});

		return isEditable;
	});

	self.IsWeeklyWorkTimeVisible = ko.computed(function() {
		if (self.IsInCurrentPeriod()) return true;
		return false;
	});

	self.IsMinHoursBroken = ko.computed(function() {
		return self.PossibleResultWeeklyContractTimeMinutesLower() > self.MaxTimePerWeekMinutesSetting();
	});

	self.IsMaxHoursBroken = ko.computed(function() {
		return self.PossibleResultWeeklyContractTimeMinutesUpper() < self.MinTimePerWeekMinutesSetting();
	});

	self.readWeeklyWorkTimeSettings = function(data) {
		self.MaxTimePerWeekMinutesSetting(data.MaxWorkTimePerWeekMinutes);
		self.MinTimePerWeekMinutesSetting(data.MinWorkTimePerWeekMinutes);
	};

	this.LoadWeeklyWorkTimeSettings = function(complete) {
		if (
			self.DayViewModels == undefined ||
			self.DayViewModels == null ||
			self.DayViewModels()[0].Date == undefined
		) {
			complete();
			return null;
		}
		return ajaxForDate.Ajax({
			url: 'Preference/WeeklyWorkTimeSetting',
			dataType: 'json',
			type: 'GET',
			data: { Date: self.DayViewModels()[0].Date },
			success: function(data) {
				self.readWeeklyWorkTimeSettings(data);
			},
			complete: complete,
			statusCode404: function() {}
		});
	};
};

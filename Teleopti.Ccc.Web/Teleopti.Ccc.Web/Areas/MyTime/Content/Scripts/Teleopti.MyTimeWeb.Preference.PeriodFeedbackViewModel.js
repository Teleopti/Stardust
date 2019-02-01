Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel = function(ajax, dayViewModels, date, weekViewModels) {
	var self = this;

	this.FeedbackLoaded = ko.observable(false);
	this.DayViewModels = dayViewModels;
	this.WeekViewModels = weekViewModels;

	this.PreferencePeriod = ko.observable();
	this.PreferenceOpenPeriod = ko.observable();

	this.LoadFeedback = function() {
		ajax.Ajax({
			url: 'PreferenceFeedback/PeriodFeedback',
			dataType: 'json',
			data: { Date: date },
			type: 'GET',
			beforeSend: function() {
				//self.FeedbackLoaded(false);
			},
			success: function(data, textStatus, jqXHR) {
				self.TargetDaysOffLower(data.TargetDaysOff.Lower);
				self.TargetDaysOffUpper(data.TargetDaysOff.Upper);
				self.PossibleResultDaysOff(data.PossibleResultDaysOff);
				self.TargetContractTimeLowerMinutes(data.TargetContractTime.LowerMinutes);
				self.TargetContractTimeUpperMinutes(data.TargetContractTime.UpperMinutes);

				self.PreferencePeriod(
					Teleopti.MyTimeWeb.Common.FormatDatePeriod(
						moment(data.PreferencePeriodStart),
						moment(data.PreferencePeriodEnd)
					)
				);

				self.PreferenceOpenPeriod(
					Teleopti.MyTimeWeb.Common.FormatDatePeriod(
						moment(data.PreferenceOpenPeriodStart),
						moment(data.PreferenceOpenPeriodEnd)
					)
				);

				self.FeedbackLoaded(true);
			}
		});
	};

	this.TargetDaysOffLower = ko.observable();
	this.TargetDaysOffUpper = ko.observable();
	this.PossibleResultDaysOff = ko.observable();
	this.IsHostAMobile = ko.observable(Teleopti.MyTimeWeb.Common.IsHostAMobile());
	this.isShowingWarningDetail = ko.observable(false);
	this.toggleWarningDetail = function() {
		self.isShowingWarningDetail(!self.isShowingWarningDetail());
	};

	this.TargetContractTimeLowerMinutes = ko.observable();
	this.TargetContractTimeUpperMinutes = ko.observable();
	this.TargetContractTimeLower = ko.computed(function() {
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.TargetContractTimeLowerMinutes());
	});
	this.TargetContractTimeUpper = ko.computed(function() {
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.TargetContractTimeUpperMinutes());
	});

	this.IsWeeklyWorkTimeBroken = ko.computed(function() {
		var broken = false;
		$.each(self.WeekViewModels, function(index, week) {
			if ((week.IsMinHoursBroken() || week.IsMaxHoursBroken()) && week.IsInCurrentPeriod()) {
				broken = true;
				return false;
			}
		});
		return broken;
	});

	this.PossibleResultContractTimeMinutesLower = ko.computed(function() {
		var sum = 0;
		$.each(self.DayViewModels, function(index, day) {
			var value = day.PossibleContractTimeMinutesLower();
			if (value) sum += parseInt(value);
			var absenceValue = day.AbsenceContractTimeMinutes();
			if (absenceValue) sum += parseInt(absenceValue);
			sum += day.ContractTimeMinutes();
		});
		return sum;
	});
	var possibleNightRestViolationsArray = ko.observableArray();

	function sameNightRestViolation(obj1, obj2) {
		return (
			obj1.nightRestTimes === obj2.nightRestTimes &&
			obj1.firstDay === obj2.firstDay &&
			obj1.secondDay === obj2.secondDay
		);
	}

	this.PossibleNightRestViolations = function() {
		possibleNightRestViolationsArray.removeAll();
		$.each(self.DayViewModels, function(index, day) {
			if (day.MakeNightRestViolationObjs().length > 0) {
				var newViolations = day.MakeNightRestViolationObjs();
				newViolations.forEach(function(newViolation) {
					var sameNightRestViolations = possibleNightRestViolationsArray().filter(function(item) {
						return sameNightRestViolation(item, newViolation);
					});
					if (sameNightRestViolations.length === 0) {
						possibleNightRestViolationsArray.push(newViolation);
					}
				});
			}
		});
		return possibleNightRestViolationsArray;
	};

	this.PossibleResultContractTimeMinutesUpper = ko.computed(function() {
		var sum = 0;
		$.each(self.DayViewModels, function(index, day) {
			var value = day.PossibleContractTimeMinutesUpper();
			if (value) sum += parseInt(value);
			var absenceValue = day.AbsenceContractTimeMinutes();
			if (absenceValue) sum += parseInt(absenceValue);
			sum += day.ContractTimeMinutes();
		});
		return sum;
	});

	this.PossibleResultContractTimeLower = ko.computed(function() {
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.PossibleResultContractTimeMinutesLower());
	});

	this.PossibleResultContractTimeUpper = ko.computed(function() {
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.PossibleResultContractTimeMinutesUpper());
	});

	this.PreferenceTimeIsOutOfRange = ko.computed(function() {
		return (
			self.PossibleResultContractTimeMinutesUpper() < self.TargetContractTimeLowerMinutes() ||
			self.TargetContractTimeUpperMinutes() < self.PossibleResultContractTimeMinutesLower()
		);
	});

	this.PreferenceDaysOffIsOutOfRange = ko.computed(function() {
		return (
			self.PossibleResultDaysOff() > self.TargetDaysOffUpper() ||
			self.PossibleResultDaysOff() < self.TargetDaysOffLower()
		);
	});

	this.PreferenceFeedbackClass = ko
		.computed(function() {
			return self.PreferenceDaysOffIsOutOfRange() ||
				self.PreferenceTimeIsOutOfRange() ||
				self.IsWeeklyWorkTimeBroken() ||
				possibleNightRestViolationsArray().length > 0
				? 'alert-danger'
				: 'alert-info';
		})
		.extend({ throttle: 1 });

	this.WarningCount = ko.computed(function() {
		var count = 2 + possibleNightRestViolationsArray.length;
		if (
			self.TargetContractTimeLower() === self.TargetContractTimeUpper() ||
			(self.TargetContractTimeLower() !== self.TargetContractTimeUpper() &&
				self.PossibleResultContractTimeLower() === self.PossibleResultContractTimeUpper()) ||
			self.PossibleResultContractTimeLower() !== self.PossibleResultContractTimeUpper()
		) {
			count++;
		}
		if (self.IsWeeklyWorkTimeBroken()) {
			count++;
		}
		return count;
	});
};

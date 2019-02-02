Teleopti.MyTimeWeb.StudentAvailability.PeriodFeedbackViewModel = function(ajax, dayViewModels, date) {
	var self = this;

	this.DayViewModels = dayViewModels;

	this.LoadFeedback = function(callback) {
		ajax.Ajax({
			url: 'StudentAvailabilityFeedback/PeriodFeedback',
			dataType: 'json',
			data: { Date: date },
			type: 'GET',
			success: function(data, textStatus, jqXHR) {
				self.TargetContractTimeLowerMinutes(data.TargetContractTime.LowerMinutes);
				self.TargetContractTimeUpperMinutes(data.TargetContractTime.UpperMinutes);
				callback && callback();
			}
		});
	};

	this.TargetContractTimeLowerMinutes = ko.observable();
	this.TargetContractTimeUpperMinutes = ko.observable();
	this.TargetContractTimeLower = ko.computed(function() {
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.TargetContractTimeLowerMinutes());
	});
	this.TargetContractTimeUpper = ko.computed(function() {
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.TargetContractTimeUpperMinutes());
	});

	this.PossibleResultContractTimeMinutesLower = ko.computed(function() {
		var sum = 0;
		$.each(self.DayViewModels, function(index, day) {
			if (day.EditableIsInOpenPeriod() && day.HasAvailability()) {
				var value = day.PossibleContractTimeMinutesLower();
				if (value) sum += parseInt(value);
			}
		});
		return sum;
	});

	this.PossibleResultContractTimeMinutesUpper = ko.computed(function() {
		var sum = 0;
		$.each(self.DayViewModels, function(index, day) {
			if (day.EditableIsInOpenPeriod() && day.HasAvailability()) {
				var value = day.PossibleContractTimeMinutesUpper();
				if (value) sum += parseInt(value);
			}
		});
		return sum;
	});

	this.PossibleResultContractTimeLower = ko.computed(function() {
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.PossibleResultContractTimeMinutesLower());
	});

	this.PossibleResultContractTimeUpper = ko.computed(function() {
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(self.PossibleResultContractTimeMinutesUpper());
	});

	this.StudentAvailabilityTimeIsOutOfRange = ko.computed(function() {
		return self.PossibleResultContractTimeMinutesUpper() < self.TargetContractTimeLowerMinutes();
	});

	this.IsHostAMobile = ko.observable(Teleopti.MyTimeWeb.Common.IsHostAMobile());

	this.WarningCount = ko.computed(function() {
		var studentAvailabilityPeriod = $('#StudentAvailability-period').data('mytime-studentavailabilityperiod');
		if (studentAvailabilityPeriod == null) return 1;
		else return 2;
	});

	this.isShowingWarningDetail = ko.observable(false);
	this.toggleWarningDetail = function () {
		self.isShowingWarningDetail(!self.isShowingWarningDetail());
	};

	this.StudentAvailabilityFeedbackClass = ko
		.computed(function() {
			return self.StudentAvailabilityTimeIsOutOfRange() ? 'alert-danger' : 'alert-info';
		})
		.extend({ throttle: 1 });
};

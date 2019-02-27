Teleopti.MyTimeWeb.StudentAvailability.DayViewModel = function (ajaxForDate) {
	var self = this;

	this.Date = "";

	this.IsLoading = ko.observable(false);
	this.AjaxError = ko.observable('');
	this.AvailableTimeSpan = ko.observable('');
	this.HasAvailability = ko.observable(false);
	this.HasBankHolidayCalendar = ko.observable(false);
	this.DateDescription = ko.observable();

	this.FeedbackError = ko.observable();
	this.PossibleStartTimes = ko.observable();
	this.PossibleEndTimes = ko.observable();
	this.PossibleContractTimeMinutesLower = ko.observable();
	this.PossibleContractTimeMinutesUpper = ko.observable();
	this.Feedback = ko.observable(true);
	this.HasFeedbackError = ko.observable(false);

	this.EditableIsInOpenPeriod = ko.observable(false);

	this.PossibleContractTimeLower = ko.computed(function () {
		var value = self.PossibleContractTimeMinutesLower();
		if (!value)
			return "";
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(value);
	});

	this.PossibleContractTimeUpper = ko.computed(function () {
		var value = self.PossibleContractTimeMinutesUpper();
		if (!value)
			return "";
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(value);
	});
	this.PossibleContractTimes = ko.computed(function () {
		var lower = self.PossibleContractTimeLower();
		var upper = self.PossibleContractTimeUpper();
		if (lower != "")
			return lower + "-" + upper;
		return "";
	});

	this.EditableHasNoSchedule = ko.computed(function () {
		// for future use
		return true;
	});
	
	this.Editable = ko.computed(function () {
		return self.EditableIsInOpenPeriod() && self.EditableHasNoSchedule();
	});

	this.ReadElement = function (element) {
		var item = $(element);
		self.Date = item.attr('data-mytime-date');
		self.EditableIsInOpenPeriod(item.attr('data-mytime-editable') == "True");
	};
	
	this.SetStudentAvailability = function (value, editFormViewModel) {
		var deferred = $.Deferred();
		ajaxForDate(self, {
			type: 'POST',
			data: JSON.stringify(value),
			date: self.Date,
			success: function (data) {
				if (typeof data.Errors != 'undefined') {
					editFormViewModel.ValidationError(data.Errors.join('</br>') || '');
					return;
				}
				self.ReadStudentAvailability(data);
			},
			complete: function() {
				deferred.resolve();
				self.LoadFeedback();
			}
		});
		return deferred.promise();
	};

	this.DeleteStudentAvailability = function () {
		var deferred = $.Deferred();
		ajaxForDate(self, {
			type: 'DELETE',
			data: JSON.stringify({ Date: self.Date }),
			date: self.Date,
			statusCode404: function() {
				self.ReadStudentAvailability();
			},
			success: this.ReadStudentAvailability,
			complete: function () {
				deferred.resolve();
				self.LoadFeedback();
			}
		});
		return deferred.promise();
	};

	this.ReadStudentAvailability = function (data) {
		var hasAvailability = data && data.AvailableTimeSpan;
		self.HasAvailability(hasAvailability);
		self.AvailableTimeSpan(hasAvailability ? data.AvailableTimeSpan : null);
		self.ReadBankHolidayCalendar(data);
	};

	this.ReadBankHolidayCalendar = function (data) {
		var hasBankHolidayCalendar = data != null && data.BankHolidayCalendar != null && Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_Availability_Indicate_BankHoliday_81656');
		self.HasBankHolidayCalendar(hasBankHolidayCalendar);
		self.DateDescription(hasBankHolidayCalendar ? data.BankHolidayCalendar.DateDescription : null)
	};

	this.LoadFeedback = function () {
		ajaxForDate(self, {
			url: "StudentAvailabilityFeedback/Feedback",
			type: 'GET',
			data: { Date: self.Date },
			date: self.Date,
			success: function (data) {
				self.HasFeedbackError(data.FeedbackError != null);
				self.FeedbackError(data.FeedbackError);
				self.PossibleStartTimes(data.PossibleStartTimes);
				self.PossibleEndTimes(data.PossibleEndTimes);
				self.PossibleContractTimeMinutesLower(data.PossibleContractTimeMinutesLower);
				self.PossibleContractTimeMinutesUpper(data.PossibleContractTimeMinutesUpper);
			}
		});
	};
};


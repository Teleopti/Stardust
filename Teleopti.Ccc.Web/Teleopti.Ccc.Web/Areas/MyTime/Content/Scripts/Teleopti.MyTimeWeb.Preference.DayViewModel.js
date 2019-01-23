Teleopti.MyTimeWeb.Preference.DayViewModel = function(ajaxForDate, feedBackData) {
	var self = this;

	var hasStringValue = function(value) {
		return typeof value == 'string' && value.length > 0;
	};

	self.Date = '';

	self.DayString = ko.observable();
	self.MonthString = ko.observable();

	self.HasPreference = true;
	self.IsLoading = ko.observable(true);
	self.Preference = ko.observable();
	self.MustHave = ko.observable();
	self.Extended = ko.observable();
	self.ExtendedTitle = ko.observable();
	self.StartTimeLimitation = ko.observable();
	self.EndTimeLimitation = ko.observable();
	self.WorkTimeLimitation = ko.observable();
	self.Activity = ko.observable();
	self.ActivityStartTimeLimitation = ko.observable();
	self.ActivityEndTimeLimitation = ko.observable();
	self.ActivityTimeLimitation = ko.observable();
	self.Color = ko.observable();
	self.TextColor = ko.computed(function() {
		var backgroundColor = self.Color();
		if (backgroundColor) {
			return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
		}
		return 'black';
	});
	self.AjaxError = ko.observable('');

	self.DayOff = ko.observable('');
	self.Absence = ko.observable('');
	self.AbsenceContractTime = ko.observable('');
	self.AbsenceContractTimeMinutes = ko.observable('');
	self.PersonAssignmentShiftCategory = ko.observable('');
	self.PersonAssignmentTimeSpan = ko.observable('');
	self.PersonAssignmentContractTime = ko.observable('');
	self.ContractTimeMinutes = ko.observable(0);
	self.InitialFeedbackData = feedBackData;

	self.HasPreferenceCategory = ko.computed(function() {
		return self.Preference() != undefined && self.Preference() != '';
	});

	self.tooltipText = ko.computed({
		read: function() {
			if (!self.Extended()) return undefined;

			var text = (
				'<div class="time-limitation"><span class="glyphicon glyphicon-step-backward"></span>{0}</div>' +
				'<div class="time-limitation"><span class="glyphicon glyphicon-step-forward"></span>{1}</div>' +
				'<div class="time-limitation"><span class="glyphicon glyphicon-resize-horizontal"></span>{2}</div>' +
				'<div class="extended-part-title">{3}</div>' +
				'<div class="time-limitation"><span class="glyphicon glyphicon-step-backward"></span>{4}</div>' +
				'<div class="time-limitation"><span class="glyphicon glyphicon-step-forward"></span>{5}</div>' +
				'<div class="time-limitation"><span class="glyphicon glyphicon-resize-horizontal"></span>{6}</div>'
			).format(
				$('<div/>')
					.text(self.StartTimeLimitation())
					.html(),
				$('<div/>')
					.text(self.EndTimeLimitation())
					.html(),
				$('<div/>')
					.text(self.WorkTimeLimitation())
					.html(),
				$('<div/>')
					.text(self.Activity())
					.html(),
				$('<div/>')
					.text(self.ActivityStartTimeLimitation() ? self.ActivityStartTimeLimitation() : '-')
					.html(),
				$('<div/>')
					.text(self.ActivityEndTimeLimitation() ? self.ActivityEndTimeLimitation() : '-')
					.html(),
				$('<div/>')
					.text(self.ActivityTimeLimitation() ? self.ActivityTimeLimitation() : '-')
					.html()
			);

			var tooltip = '<div class="extended-tooltip"><div class="extended-part-title">{0}</div>{1}</div>'.format(
				$('<div/>')
					.text(self.ExtendedTitle())
					.html(),
				text
			);
			return tooltip;
		},
		write: function() {}
	});

	self.HasDayOff = ko.computed(function() {
		return self.DayOff() != '';
	});

	self.HasAjaxError = ko.computed(function() {
		return self.AjaxError() != '';
	});

	self.HasAbsence = ko.computed(function() {
		return self.Absence() != '';
	});

	self.HasPersonAssignmentShiftCategory = ko.computed(function() {
		return self.PersonAssignmentShiftCategory() != '';
	});

	self.Meetings = ko.observableArray();
	self.PersonalShifts = ko.observableArray();

	self.HasMeetings = ko.computed(function() {
		return self.Meetings().length > 0;
	});

	self.HasPersonalShifts = ko.computed(function() {
		return self.PersonalShifts().length > 0;
	});

	self.HasPersonalShiftsOrMeetings = ko.computed(function() {
		return self.HasPersonalShifts() || self.HasMeetings();
	});

	self.EditableIsInOpenPeriod = ko.observable(false);
	self.EditableHasNoSchedule = ko.computed(function() {
		if (hasStringValue(self.DayOff())) return false;
		if (hasStringValue(self.Absence())) return false;
		if (hasStringValue(self.PersonAssignmentShiftCategory())) return false;
		return true;
	});
	self.Editable = ko.computed(function() {
		return self.EditableIsInOpenPeriod() && self.EditableHasNoSchedule();
	});

	self.Feedback = ko.observable(false);

	self.StyleClassName = ko.observable('');

	self.ReadElement = function(element) {
		var item = $(element);
		self.Date = item.attr('data-mytime-date');
		var periodStartDate = item.attr('data-period-start-date');

		self.DayString(Teleopti.MyTimeWeb.Common.FormatDayOnly(self.Date));

		if (self.DayString() === '01' || periodStartDate === self.Date) {
			self.MonthString(Teleopti.MyTimeWeb.Common.FormatMonthOnly(self.Date));
		}

		self.EditableIsInOpenPeriod(item.attr('data-mytime-editable') == 'True');
		self.HasPreference = item.hasClass('preference') || $('.preference', item).length > 0;
	};

	self.ClearPreference = function(decrementMustHave) {
		var originalMustHave = self.MustHave();
		if (originalMustHave && decrementMustHave) decrementMustHave(false, originalMustHave);
		self.ReadPreference({});
	};

	self.CalendarId = ko.observable('');
	self.CalendarName = ko.observable('');
	self.DateDescription = ko.observable('');
	self.HasBankHolidayCalendar = ko.computed(function () {
		return self.CalendarId() != '';
	});
	self.ReadBankHolidayCalendar = function (data) {
		self.CalendarId(data.CalendarId);
		self.CalendarName(data.CalendarName);
		self.DateDescription(data.DateDescription);
	};

	self.ReadPreference = function(data) {
		if (!data) return;

		if (data.Color) self.Color('rgb(' + data.Color + ')');
		else self.Color('');
		self.Preference(data.Preference);
		self.MustHave(data.MustHave);
		self.ExtendedTitle(data.ExtendedTitle);
		self.StartTimeLimitation(data.StartTimeLimitation);
		self.EndTimeLimitation(data.EndTimeLimitation);
		self.WorkTimeLimitation(data.WorkTimeLimitation);
		self.Activity(data.Activity);
		self.ActivityStartTimeLimitation(data.ActivityStartTimeLimitation);
		self.ActivityEndTimeLimitation(data.ActivityEndTimeLimitation);
		self.ActivityTimeLimitation(data.ActivityTimeLimitation);
		self.Extended(data.Extended);
	};

	self.ReadDayOff = function(data) {
		self.DayOff(data.DayOff);
	};

	self.ReadAbsence = function(data) {
		self.Absence(data.Absence);
		self.AbsenceContractTimeMinutes(data.AbsenceContractTimeMinutes);
		self.AbsenceContractTime(data.AbsenceContractTimeMinutes > 0 ? data.AbsenceContractTime : '');
	};

	self.ReadPersonAssignment = function(data) {
		self.PersonAssignmentShiftCategory(data.ShiftCategory);
		self.PersonAssignmentTimeSpan(data.TimeSpan);
		self.PersonAssignmentContractTime(data.ContractTime);
		self.ContractTimeMinutes(data.ContractTimeMinutes);
	};

	self.LoadPreference = function(complete) {
		if (!self.HasPreference) {
			complete();
			return null;
		}
		return ajaxForDate(self, {
			url: 'Preference/Preference',
			type: 'GET',
			data: { Date: self.Date },
			date: self.Date,
			success: self.ReadPreference,
			complete: complete,
			statusCode404: function() {}
		});
	};

	self.HasNightRestViolationToPreviousDay = ko.observable();
	self.HasNightRestViolationToNextDay = ko.observable();
	self.RawDate = ko.observable();
	self.Difference = ko.observable();
	self.RestTimeToNextDay = ko.observable();
	self.RestTimeToPreviousDay = ko.observable();
	self.ExpectedNightRest = ko.observable();
	self.NightRestViolationSwitch = ko.computed(function() {
		return self.HasNightRestViolationToPreviousDay() || self.HasNightRestViolationToNextDay();
	});

	self.MakeNightRestViolationObjs = function() {
		var nightRestViolationObjs = [];
		if (self.NightRestViolationSwitch()) {
			if (self.HasNightRestViolationToPreviousDay()) {
				var dateMoment = moment(self.RawDate());
				var nightRestViolationObj = {};
				nightRestViolationObj.nightRestTimes = self.ExpectedNightRest();
				nightRestViolationObj.sencondDay = Teleopti.MyTimeWeb.Common.FormatDate(dateMoment);
				nightRestViolationObj.firstDay = Teleopti.MyTimeWeb.Common.FormatDate(dateMoment.subtract(1, 'days'));

				nightRestViolationObj.hoursBetweenTwoDays = self.RestTimeToPreviousDay();
				nightRestViolationObjs.push(nightRestViolationObj);
			}

			if (self.HasNightRestViolationToNextDay()) {
				var dateMoment = moment(self.RawDate());
				var nightRestViolationObj = {};
				nightRestViolationObj.nightRestTimes = self.ExpectedNightRest();
				nightRestViolationObj.firstDay = Teleopti.MyTimeWeb.Common.FormatDate(dateMoment);
				nightRestViolationObj.sencondDay = Teleopti.MyTimeWeb.Common.FormatDate(dateMoment.add(1, 'days'));
				nightRestViolationObj.hoursBetweenTwoDays = self.RestTimeToNextDay();
				nightRestViolationObjs.push(nightRestViolationObj);
			}
		}

		return nightRestViolationObjs;
	};

	self.SetPreference = function(value, validationErrorCallback) {
		if (typeof value == 'string') {
			value = {
				Date: self.Date,
				PreferenceId: value
			};
		} else {
			value.Date = self.Date;
		}

		var deferred = $.Deferred();
		ajaxForDate(self, {
			type: 'POST',
			data: JSON.stringify(value),
			date: self.Date,
			statusCode400: function(jqXHR, textStatus, errorThrown) {
				var errorMessage = $.parseJSON(jqXHR.responseText);
				validationErrorCallback(errorMessage);
			},
			success: self.ReadPreference,
			complete: function() {
				deferred.resolve();
				if (!self.HasAjaxError()) {
					self.LoadFeedback();
				}
			}
		});
		return deferred.promise();
	};

	self.SetMustHave = function(value, successCb) {
		value = {
			Date: self.Date,
			MustHave: value
		};

		var originalMustHave = self.MustHave();

		var deferred = $.Deferred();
		ajaxForDate(self, {
			url: 'Preference/MustHave',
			type: 'POST',
			data: JSON.stringify(value),
			date: self.Date,
			success: function(newMustHave) {
				self.MustHave(newMustHave);
				if (successCb) successCb(newMustHave, originalMustHave);
			},
			complete: function() {
				deferred.resolve();
			}
		});
		return deferred.promise();
	};

	self.DeletePreference = function() {
		var deferred = $.Deferred();
		ajaxForDate(self, {
			type: 'DELETE',
			data: JSON.stringify({ Date: self.Date }),
			date: self.Date,
			statusCode404: function() {},
			success: self.ReadPreference,
			complete: function() {
				deferred.resolve();
				self.LoadFeedback();
			}
		});
		return deferred.promise();
	};

	self.FeedbackError = ko.observable();
	self.DisplayFeedbackError = ko.computed(function() {
		return hasStringValue(self.FeedbackError());
	});

	self.PossibleStartTimes = ko.observable();
	self.PossibleEndTimes = ko.observable();

	self.PossibleContractTimeMinutesLower = ko.observable();
	self.PossibleContractTimeMinutesUpper = ko.observable();

	self.PossibleContractTimeLower = ko.computed(function() {
		var value = self.PossibleContractTimeMinutesLower();
		if (!value) return '';
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(value);
	});

	self.PossibleContractTimeUpper = ko.computed(function() {
		var value = self.PossibleContractTimeMinutesUpper();
		if (!value) return '';
		return Teleopti.MyTimeWeb.Common.FormatTimeSpan(value);
	});

	self.PossibleContractTimes = ko.computed(function() {
		var lower = self.PossibleContractTimeLower();
		var upper = self.PossibleContractTimeUpper();
		if (lower != '') return lower + '-' + upper;
		return '';
	});

	self.DisplayFeedback = ko.computed(function() {
		if (self.DisplayFeedbackError()) return false;
		if (hasStringValue(self.PossibleStartTimes())) return true;
		if (hasStringValue(self.PossibleEndTimes())) return true;
		if (hasStringValue(self.PossibleContractTimeMinutesLower())) return true;
		if (hasStringValue(self.PossibleContractTimeMinutesUpper())) return true;
		return false;
	});

	self.AssignFeedbackData = function(data) {
		if (!self.Feedback()) {
			return;
		} else if (data) {
			self.FeedbackError(data.FeedbackError);
			self.PossibleStartTimes(data.PossibleStartTimes);
			self.PossibleEndTimes(data.PossibleEndTimes);
			self.PossibleContractTimeMinutesLower(data.PossibleContractTimeMinutesLower);
			self.PossibleContractTimeMinutesUpper(data.PossibleContractTimeMinutesUpper);

			self.RawDate(data.DateInternal);
			self.HasNightRestViolationToPreviousDay(data.HasNightRestViolationToPreviousDay);
			self.HasNightRestViolationToNextDay(data.HasNightRestViolationToNextDay);
			self.RestTimeToNextDay(data.RestTimeToNextDayTimeSpan);
			self.RestTimeToPreviousDay(data.RestTimeToPreviousDayTimeSpan);
			self.ExpectedNightRest(data.ExpectedNightRestTimeSpan);
		}
	};

	self.LoadFeedback = function() {
		if (!self.Feedback()) {
			self.IsLoading(false);
			return;
		}

		if (self.InitialFeedbackData) {
			self.AssignFeedbackData(self.InitialFeedbackData);
			self.IsLoading(false);
			return;
		}

		self.IsLoading(true);

		return ajaxForDate(self, {
			url: 'PreferenceFeedback/Feedback',
			type: 'GET',
			data: {
				Date: self.Date
			},
			date: self.Date,
			success: function(data) {
				self.AssignFeedbackData(data);
				self.IsLoading(false);
			}
		});
	};
};

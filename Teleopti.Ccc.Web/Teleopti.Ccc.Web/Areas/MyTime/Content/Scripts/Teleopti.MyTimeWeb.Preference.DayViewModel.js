﻿/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.2.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />

Teleopti.MyTimeWeb.Preference.DayViewModel = function (ajax) {
	var self = this;

	var hasStringValue = function (value) {
		return typeof (value) == 'string' && value.length > 0;
	};

	// legacy.
	// should be refactored into using the viewmodel to update the ui etc
	// would result in less text...
	var ajaxForDate = function (options) {

		var type = options.type || 'GET',
		    date = options.date || null, // required
		    data = options.data || {},
		    statusCode400 = options.statusCode400,
		    statusCode404 = options.statusCode404,
		    url = options.url || "Preference/Preference",
		    success = options.success || function () {
		    },
		    complete = options.complete || null;

		return ajax.Ajax({
			url: url,
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			type: type,
			beforeSend: function (jqXHR) {
				self.AjaxError('');
				self.IsLoading(true);
			},
			complete: function (jqXHR, textStatus) {
				self.IsLoading(false);
				if (complete)
					complete(jqXHR, textStatus);
			},
			success: success,
			data: data,
			statusCode404: statusCode404,
			statusCode400: statusCode400,
			error: function (jqXHR, textStatus, errorThrown) {
				var error = {
					ShortMessage: "Error!"
				};
				try {
					error = $.parseJSON(jqXHR.responseText);
				} catch (e) {
				}
				self.AjaxError(error.ShortMessage);
			}
		});
	};


	this.Date = "";

	this.HasPreference = true;
	this.IsLoading = ko.observable(false);
	this.Preference = ko.observable();
	this.MustHave = ko.observable();
	this.Extended = ko.observable();
	this.ExtendedTitle = ko.observable();
	this.StartTimeLimitation = ko.observable();
	this.EndTimeLimitation = ko.observable();
	this.WorkTimeLimitation = ko.observable();
	this.Activity = ko.observable();
	this.ActivityStartTimeLimitation = ko.observable();
	this.ActivityEndTimeLimitation = ko.observable();
	this.ActivityTimeLimitation = ko.observable();
	this.Color = ko.observable();
	this.AjaxError = ko.observable('');

	this.DayOff = ko.observable('');
	this.Absence = ko.observable('');
	this.PersonAssignmentShiftCategory = ko.observable('');
	this.PersonAssignmentTimeSpan = ko.observable('');
	this.PersonAssignmentContractTime = ko.observable('');
	this.ContractTimeMinutes = ko.observable(0);

	this.Meetings = ko.observableArray();
	this.PersonalShifts = ko.observableArray();
	
	this.EditableIsInOpenPeriod = ko.observable(false);
	this.EditableHasNoSchedule = ko.computed(function () {
		if (hasStringValue(self.DayOff()))
			return false;
		if (hasStringValue(self.Absence()))
			return false;
		if (hasStringValue(self.PersonAssignmentShiftCategory()))
			return false;
		return true;
	});
	this.Editable = ko.computed(function () {
		return self.EditableIsInOpenPeriod() && self.EditableHasNoSchedule();
	});

	this.Fulfilled = ko.observable(true);

	this.Feedback = ko.observable(false);

	this.StyleClassName = ko.observable('');

	this.ReadElement = function (element) {
		var item = $(element);
		self.Date = item.attr('data-mytime-date');
		self.EditableIsInOpenPeriod(item.attr('data-mytime-editable') == "True");
		self.HasPreference = item.hasClass("preference") || $(".preference", item).length > 0;
	};

	this.ReadPreference = function (data) {
		if (!data) return;

		self.Color(data.Color);
		self.Preference(data.Preference);
		self.Extended(data.Extended);
		self.MustHave(data.MustHave);
		self.ExtendedTitle(data.ExtendedTitle);
		self.StartTimeLimitation(data.StartTimeLimitation);
		self.EndTimeLimitation(data.EndTimeLimitation);
		self.WorkTimeLimitation(data.WorkTimeLimitation);
		self.Activity(data.Activity);
		self.ActivityStartTimeLimitation(data.ActivityStartTimeLimitation);
		self.ActivityEndTimeLimitation(data.ActivityEndTimeLimitation);
		self.ActivityTimeLimitation(data.ActivityTimeLimitation);
	};

	this.ReadDayOff = function (data) {
		self.DayOff(data.DayOff);
	};

	this.ReadAbsence = function (data) {
		self.Absence(data.Absence);
	};

	this.ReadPersonAssignment = function (data) {
		self.PersonAssignmentShiftCategory(data.ShiftCategory);
		self.PersonAssignmentTimeSpan(data.TimeSpan);
		self.PersonAssignmentContractTime(data.ContractTime);
		self.ContractTimeMinutes(data.ContractTimeMinutes);
	};

	this.LoadPreference = function (complete) {
		if (!self.HasPreference) {
			complete();
			return null;
		}
		return ajaxForDate({
			url: "Preference/Preference",
			type: 'GET',
			data: { Date: self.Date },
			date: self.Date,
			success: this.ReadPreference,
			complete: complete,
			statusCode404: function () { }
		});
	};

	this.LoadFeedback = function () {
		if (!self.Feedback())
			return null;
		return ajaxForDate({
			url: "PreferenceFeedback/Feedback",
			type: 'GET',
			data: { Date: self.Date },
			date: self.Date,
			success: function (data) {
				self.FeedbackError(data.FeedbackError);
				self.PossibleStartTimes(data.PossibleStartTimes);
				self.PossibleEndTimes(data.PossibleEndTimes);
				self.PossibleContractTimeMinutesLower(data.PossibleContractTimeMinutesLower);
				self.PossibleContractTimeMinutesUpper(data.PossibleContractTimeMinutesUpper);
			}
		});
	};

	this.SetPreference = function (value, validationErrorCallback) {
		if (typeof (value) == 'string') {
			value = {
				Date: self.Date,
				PreferenceId: value
			};
		} else {
			value.Date = self.Date;
		}

		var deferred = $.Deferred();
		ajaxForDate({
			type: 'POST',
			data: JSON.stringify(value),
			date: self.Date,
			statusCode400: function (jqXHR, textStatus, errorThrown) {
				var errorMessage = $.parseJSON(jqXHR.responseText);
				validationErrorCallback(errorMessage);
			},
			success: this.ReadPreference,
			complete: function () {
				deferred.resolve();
				self.LoadFeedback();
			}
		});
		return deferred.promise();
	};

	this.SetMustHave = function (value) {
		value = {
			Date: self.Date,
			MustHave: value
		};

		var deferred = $.Deferred();
		ajaxForDate({
			url: 'Preference/MustHave',
			type: 'POST',
			data: JSON.stringify(value),
			date: self.Date,
			success: function (data) {
				self.MustHave(data);
			},
			complete: function () {
				deferred.resolve();
			}
		});
		return deferred.promise();
	};

	this.DeletePreference = function () {
		var deferred = $.Deferred();
		ajaxForDate({
			type: 'DELETE',
			data: JSON.stringify({ Date: self.Date }),
			date: self.Date,
			statusCode404: function () { },
			success: this.ReadPreference,
			complete: function () {
				deferred.resolve();
				self.LoadFeedback();
			}
		});
		return deferred.promise();
	};

	this.FeedbackError = ko.observable();
	this.DisplayFeedbackError = ko.computed(function () {
		return hasStringValue(self.FeedbackError());
	});

	this.PossibleStartTimes = ko.observable();
	this.PossibleEndTimes = ko.observable();

	this.PossibleContractTimeMinutesLower = ko.observable();
	this.PossibleContractTimeMinutesUpper = ko.observable();

	this.PossibleContractTimeLower = ko.computed(function () {
		var value = self.PossibleContractTimeMinutesLower();
		if (!value)
			return "";
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(value);
	});

	this.PossibleContractTimeUpper = ko.computed(function () {
		var value = self.PossibleContractTimeMinutesUpper();
		if (!value)
			return "";
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(value);
	});

	this.PossibleContractTimes = ko.computed(function () {
		var lower = self.PossibleContractTimeLower();
		var upper = self.PossibleContractTimeUpper();
		if (lower != "")
			return lower + "-" + upper;
		return "";
	});

	this.DisplayFeedback = ko.computed(function () {
		if (self.DisplayFeedbackError())
			return false;
		if (hasStringValue(self.PossibleStartTimes()))
			return true;
		if (hasStringValue(self.PossibleEndTimes()))
			return true;
		if (hasStringValue(self.PossibleContractTimeMinutesLower()))
			return true;
		if (hasStringValue(self.PossibleContractTimeMinutesUpper()))
			return true;
		return false;
	});

};





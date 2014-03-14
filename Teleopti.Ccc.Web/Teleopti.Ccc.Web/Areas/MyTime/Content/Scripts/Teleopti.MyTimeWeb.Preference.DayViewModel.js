/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
		if(typeof (Teleopti.MyTimeWeb.Preference) ==='undefined') {
			Teleopti.MyTimeWeb.Preference = {};
		}
	}
}

Teleopti.MyTimeWeb.Preference.DayViewModel = function (ajaxForDate) {
	var self = this;

	var hasStringValue = function (value) {
		return typeof (value) == 'string' && value.length > 0;
	};

	self.Date = "";

	self.HasPreference = true;
	self.IsLoading = ko.observable(false);
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
	self.TextColor = ko.computed(function () {
		var backgroundColor = self.Color();
		if (backgroundColor) {
			return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
		}
		return 'black';
	});
	self.AjaxError = ko.observable('');

	self.DayOff = ko.observable('');
	self.Absence = ko.observable('');
	this.AbsenceContractTime = ko.observable('');
    this.AbsenceContractTimeMinutes = ko.observable('');
	self.PersonAssignmentShiftCategory = ko.observable('');
	self.PersonAssignmentTimeSpan = ko.observable('');
	self.PersonAssignmentContractTime = ko.observable('');
	self.ContractTimeMinutes = ko.observable(0);

    self.HasPreferenceCategory = ko.computed(function() {
        return self.Preference() != undefined && self.Preference() != '';
    });

	self.tooltipText = ko.computed(function() {
		if (!self.Extended())
			return undefined;

		var text = ('<div class="time-limitation"><span class="glyphicon glyphicon-step-backward"></span>{0}</div>' +
			'<div class="time-limitation"><span class="glyphicon glyphicon-step-forward"></span>{1}</div>' +
			'<div class="time-limitation"><span class="glyphicon glyphicon-resize-horizontal"></span>{2}</div>' +
			'<div class="extended-part-title">{3}</div>' +
			'<div class="time-limitation"><span class="glyphicon glyphicon-step-backward"></span>{4}</div>' +
			'<div class="time-limitation"><span class="glyphicon glyphicon-step-forward"></span>{5}</div>' +
			'<div class="time-limitation"><span class="glyphicon glyphicon-resize-horizontal"></span>{6}</div>')
			.format($('<div/>').text(self.StartTimeLimitation()).html(),
				$('<div/>').text(self.EndTimeLimitation()).html(),
				$('<div/>').text(self.WorkTimeLimitation()).html(),
				$('<div/>').text(self.Activity()).html(),
				$('<div/>').text(self.ActivityStartTimeLimitation() ? self.ActivityStartTimeLimitation() : '-').html(),
				$('<div/>').text(self.ActivityEndTimeLimitation() ? self.ActivityEndTimeLimitation() : '-').html(),
				$('<div/>').text(self.ActivityTimeLimitation() ? self.ActivityTimeLimitation() : '-').html());

		return '<div class="extended-tooltip"><div class="extended-part-title">{0}</div>{1}</div>'.format(
			$('<div/>').text(self.ExtendedTitle()).html(), text);
	});

    this.HasDayOff = ko.computed(function() {
        return self.DayOff() != '';
    });

    this.HasAjaxError = ko.computed(function () {
        return self.AjaxError() != '';
    });

    this.HasAbsence = ko.computed(function () {
        return self.Absence() != '';
    });

    this.HasPersonAssignmentShiftCategory = ko.computed(function () {
        return self.PersonAssignmentShiftCategory() != '';
    });

	this.Meetings = ko.observableArray();
	this.PersonalShifts = ko.observableArray();

	this.HasMeetings = ko.computed(function () {
	    return self.Meetings().length>0;
	});

	this.HasPersonalShifts = ko.computed(function () {
	    return self.PersonalShifts().length > 0;
	});

	this.HasPersonalShiftsOrMeetings = ko.computed(function () {
	    return self.HasPersonalShifts() || self.HasMeetings();
	});

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

		if (data.Color)
			self.Color('rgb(' + data.Color + ')');
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

	this.ReadDayOff = function (data) {
		self.DayOff(data.DayOff);
	};

	this.ReadAbsence = function (data) {
	    self.Absence(data.Absence);
	    self.AbsenceContractTimeMinutes(data.AbsenceContractTimeMinutes);
	    self.AbsenceContractTime(data.AbsenceContractTimeMinutes > 0 ? data.AbsenceContractTime : '');
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
		return ajaxForDate(self, {
			url: "Preference/Preference",
			type: 'GET',
			data: { Date: self.Date },
			date: self.Date,
			success: self.ReadPreference,
			complete: complete,
			statusCode404: function () { }
		});
	};

	this.LoadFeedback = function () {
		if (!self.Feedback())
			return null;
		return ajaxForDate(self,{
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
		ajaxForDate(self,{
			type: 'POST',
			data: JSON.stringify(value),
			date: self.Date,
			statusCode400: function (jqXHR, textStatus, errorThrown) {
				var errorMessage = $.parseJSON(jqXHR.responseText);
				validationErrorCallback(errorMessage);
			},
			success: self.ReadPreference,
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
		ajaxForDate(self, {
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
		ajaxForDate(self, {
			type: 'DELETE',
			data: JSON.stringify({ Date: self.Date }),
			date: self.Date,
			statusCode404: function () { },
			success: self.ReadPreference,
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

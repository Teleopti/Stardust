/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.1.custom.js" />
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
	this.AbsenceContractTime = ko.observable('');
    this.AbsenceContractTimeMinutes = ko.observable('');
	this.PersonAssignmentShiftCategory = ko.observable('');
	this.PersonAssignmentTimeSpan = ko.observable('');
	this.PersonAssignmentContractTime = ko.observable('');
	this.ContractTimeMinutes = ko.observable(0);

    this.HasDayOff = ko.computed(function() {
        return self.DayOff() != '';
    });

    this.HasAjaxError = ko.computed(function () {
        return self.AjaxError() != '';
    });

    this.HasAbsence = ko.computed(function () {
        return self.Absence() != '';
    });

    this.HasDayOff = ko.computed(function () {
        return self.DayOff() != '';
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





/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
}
if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
	Teleopti.MyTimeWeb = {};
}
if (typeof (Teleopti.MyTimeWeb.StudentAvailability) === 'undefined') {
	Teleopti.MyTimeWeb.StudentAvailability = {};
}


Teleopti.MyTimeWeb.StudentAvailability.DayViewModel = function (ajaxForDate) {
	var self = this;

	this.Date = "";

	this.IsLoading = ko.observable(false);
	this.AjaxError = ko.observable('');
	this.AvailableTimeSpan = ko.observable('');
	this.HasAvailibility = ko.observable(false);
	this.ToggleAvailabilityVerifyHours31654Enabled = ko.observable(false);

	this.EditableIsInOpenPeriod = ko.observable(false);
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
				//				self.LoadFeedback();
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
			statusCode404: function () { },
			success: this.ReadStudentAvailability,
			complete: function () {
				deferred.resolve();
				//				self.LoadFeedback();
			}
		});
		return deferred.promise();
	};

	this.ReadStudentAvailability = function (data, toggleAvailabilityVerifyHours31654Enabled) {
		self.ToggleAvailabilityVerifyHours31654Enabled(toggleAvailabilityVerifyHours31654Enabled);
		if (!data) {
			self.HasAvailibility(false);
		} else {
			self.HasAvailibility(true);
			self.AvailableTimeSpan(data.AvailableTimeSpan);
		}
	};
};


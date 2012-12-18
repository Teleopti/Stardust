/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.2.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />

Teleopti.MyTimeWeb.StudentAvailability.EditFormViewModel = function () {
	var self = this;

	this.StartTime = ko.observable('');
	this.EndTime = ko.observable('');
	this.NextDay = ko.observable(false);

	this.ValidationError = ko.observable();

	this.reset = function () {
		self.StartTime('');
		self.EndTime('');
		self.NextDay(false);
		self.ValidationError(undefined);
	};
};

Teleopti.MyTimeWeb.StudentAvailability.DayViewModel = function () {
	var self = this;

	this.Date = "";

	this.IsLoading = ko.observable(false);
	this.AvailableTimeSpan = ko.observable('');

	this.ReadElement = function (element) {
		var item = $(element);
		self.Date = item.attr('data-mytime-date');
//		self.EditableIsInOpenPeriod(item.attr('data-mytime-editable') == "True");
	};

	this.ReadStudentAvailability = function (data) {
		if (!data) return;

		self.AvailableTimeSpan(data.AvailableTimeSpan);
	};
};


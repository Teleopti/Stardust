/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.2.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />

Teleopti.MyTimeWeb.StudentAvailability.EditStudentAvailabilityFormViewModel = function () {
	var self = this;

	this.StartTime = ko.observable('');
	this.EndTime = ko.observable('');
	this.NextDay = ko.observable(false);

	this.ValidationError = ko.observable();
};

Teleopti.MyTimeWeb.StudentAvailability.StudentAvailabilityViewModel = function () {
	var self = this;

};

Teleopti.MyTimeWeb.StudentAvailability.DayViewModel = function () {
	var self = this;

};


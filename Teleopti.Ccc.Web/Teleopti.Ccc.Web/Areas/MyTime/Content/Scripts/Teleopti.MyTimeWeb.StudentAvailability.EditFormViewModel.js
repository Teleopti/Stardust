/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.2.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />

Teleopti.MyTimeWeb.StudentAvailability.EditFormViewModel = function (ajax, showMeridian) {
	var self = this;

	this.StartTime = ko.observable(undefined);
	this.EndTime = ko.observable(undefined);
	this.NextDay = ko.observable(false);
    
	this.AddAvailabilityFormVisible = ko.observable(false);
	this.IsAvailabilityInputVisible = ko.observable(true);
	this.IsTimeInputVisible = ko.observable(true);
	this.ShowMeridian = ko.observable(showMeridian);

	this.ValidationError = ko.observable();

	this.IsAvailabilityInputVisibleToggle = function () {
	    self.IsAvailabilityInputVisible(!self.IsAvailabilityInputVisible());
	};

	this.IsAvailabilityInputVisibleToggleCss = ko.computed(function () {
	    return !self.IsAvailabilityInputVisible() ? 'icon-circle-arrow-down' : undefined;
	});

	this.ToggleAddAvailabilityFormVisible = function () {
	    self.AddAvailabilityFormVisible(!self.AddAvailabilityFormVisible());
	};
    
	this.ShowError = ko.computed(function () {
	    return self.ValidationError() !== undefined && self.ValidationError() !== '';
	});

	this.EnableApply = ko.computed(function () {
		return self.StartTime() && self.EndTime();
	});

	this.reset = function () {
		self.StartTime(undefined);
		self.EndTime(undefined);
		self.NextDay(false);
		self.ValidationError(undefined);
	};
};
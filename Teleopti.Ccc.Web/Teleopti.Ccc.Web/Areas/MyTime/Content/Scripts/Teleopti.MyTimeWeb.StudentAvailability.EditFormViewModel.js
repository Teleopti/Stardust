﻿Teleopti.MyTimeWeb.StudentAvailability.EditFormViewModel = function (ajax, showMeridian) {
	var self = this;

	this.StartTime = ko.observable(undefined);
	this.EndTime = ko.observable(undefined);
	this.NextDay = ko.observable(false);
    
	this.AddAvailabilityFormVisible = ko.observable(false);
	this.IsHostAMobile = ko.observable(Teleopti.MyTimeWeb.Common.IsHostAMobile());
	this.IsAvailabilityInputVisible = ko.observable(true);
	this.IsTimeInputVisible = ko.observable(true);
	this.ShowMeridian = ko.observable(showMeridian);
	this.IsPostingData = ko.observable(false);

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
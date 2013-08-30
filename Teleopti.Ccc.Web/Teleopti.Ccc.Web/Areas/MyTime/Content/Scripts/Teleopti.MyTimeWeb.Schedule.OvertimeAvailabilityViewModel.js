/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>

Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel = function OvertimeAvailabilityViewModel() {
	var self = this;
	
	this.Template = "add-overtime-availability-template";

	this.DateFrom = ko.observable(moment().startOf('day'));
	this.DateTo = ko.observable(moment().startOf('day'));
	this.StartTime = ko.observable('');
	this.EndTime = ko.observable('');
	this.NextDay = ko.observable(false);

	this.ShowMeridian = ($('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true');
	this.DateFormat = $('#Request-detail-datepicker-format').val().toUpperCase();

	this.ValidationError = ko.observable();
	
	this.ShowError = ko.computed(function () {
		return self.ValidationError() !== undefined && self.ValidationError() !== '';
	});
	
	this.Reset = function () {
		self.StartTime('');
		self.EndTime('');
		self.NextDay(false);
		self.ValidationError(undefined);
	};

	this.SetOvertimeAvailability = function() {

	};

	this.LoadData = function(day) {
		if (day.overtimeAvailability().HasOvertimeAvailability) {
			self.StartTime(day.overtimeAvailability().StartTime);
			self.EndTime(day.overtimeAvailability().EndTime);
			self.NextDay(day.overtimeAvailability().NextDay);
		} else {
			self.StartTime('');
			self.EndTime('');
			self.NextDay(false);
		}
	};

};
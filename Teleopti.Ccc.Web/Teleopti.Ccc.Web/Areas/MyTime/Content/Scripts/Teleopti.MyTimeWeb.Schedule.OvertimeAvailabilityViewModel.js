/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>

Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel = function OvertimeAvailabilityViewModel(ajax, displayOvertimeAvailability) {
	var self = this;
	
	this.Template = "add-overtime-availability-template";

	this.HasOvertimeAvailability = ko.observable(false);
	this.DateFrom = ko.observable(moment().startOf('day'));
	this.DateTo = ko.observable(moment().startOf('day'));
	this.StartTime = ko.observable('');
	this.EndTime = ko.observable('');
	this.EndTimeNextDay = ko.observable(false);
	this.DateToForDisplay = ko.computed(function () {
		return self.EndTimeNextDay() ? self.DateFrom().clone().add('d', 1) : self.DateFrom().clone();
	});

	this.ShowMeridian = ($('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true');
	this.DateFormat = ko.observable($('#Request-detail-datepicker-format').val().toUpperCase());

	this.ErrorMessage = ko.observable('');
	
	this.ShowError = ko.computed(function () {
		return self.ErrorMessage() !== undefined && self.ErrorMessage() !== '';
	});

	this.SetOvertimeAvailability = function () {
		ajax.Ajax({
			url: "Schedule/OvertimeAvailability",
			dataType: "json",
			data: { Date: self.DateFrom().format('YYYY-MM-DD'), StartTime: self.StartTime(), EndTime: self.EndTime(), EndTimeNextDay: self.EndTimeNextDay() },
			type: 'POST',
			success: function (data, textStatus, jqXHR) {
				displayOvertimeAvailability(data);
			},
			error: function (jqXHR, textStatus, errorThrown) {
			if (jqXHR.status == 400) {
				var data = $.parseJSON(jqXHR.responseText);
				self.ErrorMessage(data.Errors.join('</br>'));
				return;
			}
			Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
		}
		});
	};

	this.LoadRequestData = function(day) {
		self.HasOvertimeAvailability(day.overtimeAvailability().HasOvertimeAvailability);
		if (day.overtimeAvailability().HasOvertimeAvailability) {
			self.StartTime(day.overtimeAvailability().StartTime);
			self.EndTime(day.overtimeAvailability().EndTime);
			self.EndTimeNextDay(day.overtimeAvailability().EndTimeNextDay);
		} else {
			self.StartTime(day.overtimeAvailability().DefaultStartTime);
			self.EndTime(day.overtimeAvailability().DefaultEndTime);
			self.EndTimeNextDay(day.overtimeAvailability().DefaultEndTimeNextDay);
		}
	};

};
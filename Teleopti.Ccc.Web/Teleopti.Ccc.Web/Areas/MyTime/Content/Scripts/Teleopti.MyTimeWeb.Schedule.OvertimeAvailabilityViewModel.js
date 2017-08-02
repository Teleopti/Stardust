/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Content/moment/moment.js" />

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
	this.DateFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);
	this.ShowMeridian = ($('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true');
	
	this.ErrorMessage = ko.observable('');
	
	this.ShowError = ko.computed(function () {
		return self.ErrorMessage() !== undefined && self.ErrorMessage() !== '';
	});
	self.IsPostingData = ko.observable(false);

	this.SetOvertimeAvailability = function () {
		if (self.IsPostingData()) { return;}
		self.IsPostingData(true);

		ajax.Ajax({
			url: "Schedule/OvertimeAvailability",
			dataType: "json",
			data: {
				Date: Teleopti.MyTimeWeb.Common.FormatServiceDate(self.DateFrom()),
				StartTime: self.StartTime(),
				EndTime: self.EndTime(),
				EndTimeNextDay: self.EndTimeNextDay()
			},
			type: 'POST',
			success: function(data, textStatus, jqXHR) {
				displayOvertimeAvailability(data);
				self.IsPostingData(false);
			},
			error: function(jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var data = $.parseJSON(jqXHR.responseText);
					self.ErrorMessage(data.Errors.join('</br>'));
					self.IsPostingData(false);
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
				self.IsPostingData(false);
			}
		});
	};
	
	this.DeleteOvertimeAvailability = function () {
		ajax.Ajax({
			url: "Schedule/DeleteOvertimeAvailability",
			dataType: "json",
			data: { Date: Teleopti.MyTimeWeb.Common.FormatServiceDate(self.DateFrom()) },
			type: 'DELETE',
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

	this.LoadRequestData = function(data) {
		self.HasOvertimeAvailability(data.HasOvertimeAvailability);
		if (data.HasOvertimeAvailability) {
			self.StartTime(data.StartTime);
			self.EndTime(data.EndTime);
			self.EndTimeNextDay(data.EndTimeNextDay);
		} else {
			self.StartTime(data.DefaultStartTime);
			self.EndTime(data.DefaultEndTime);
			self.EndTimeNextDay(data.DefaultEndTimeNextDay);
		}
	};

};
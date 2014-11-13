/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Content/moment/moment.js" />

Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel = function AbsenceReportViewModel(ajax, reloadSchedule) {
	var self = this;
	
	this.Template = "add-absence-report-detail-template";
	this.AbsenceId = ko.observable();
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

	// TODO: Implement absence report save

	this.SaveAbsenceReport = function () {
		ajax.Ajax({
			url: "Schedule/ReportAbsence",
			dataType: "json",
			data: { Date: self.DateFrom().format(self.DateFormat()), AbsenceId: self.AbsenceId() },
			type: 'POST',
			success: function (data, textStatus, jqXHR) {
				reloadSchedule(data);
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
};

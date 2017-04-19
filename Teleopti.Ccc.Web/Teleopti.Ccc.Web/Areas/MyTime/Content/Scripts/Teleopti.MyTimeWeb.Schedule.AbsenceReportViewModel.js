/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>

Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel = function(ajax, reloadSchedule) {
	var self = this;
	
	this.Template = "add-absence-report-detail-template";
	this.AbsenceId = ko.observable();
	this.DateFrom = ko.observable(moment().startOf('day'));
	this.DateTo = ko.observable(moment().startOf('day'));
	this.StartTime = ko.observable('');
	this.EndTime = ko.observable('');
	this.EndTimeNextDay = ko.observable(false);
	this.DateFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);
		
	this.DateToForDisplay = ko.computed(function () {
		var date = self.EndTimeNextDay() ? self.DateFrom().clone().add('d', 1) : self.DateFrom().clone();
		return date.format(self.DateFormat());
	});

	this.ShowMeridian = ($('div[data-culture-show-meridian]').attr('data-culture-show-meridian') === 'true');

	this.ErrorMessage = ko.observable('');
	
	this.ShowError = ko.computed(function () {
		return self.ErrorMessage() !== undefined && self.ErrorMessage() !== '';
	});

	self.IsPostingData = ko.observable(false);

	this.SaveAbsenceReport = function () {
		if (self.IsPostingData()) { return; }

		self.IsPostingData(true);

		ajax.Ajax({
			url: "Schedule/ReportAbsence",
			dataType: "json",
			data: {
				Date: Teleopti.MyTimeWeb.Common.FormatServiceDate(self.DateFrom()),
				AbsenceId: self.AbsenceId()
			},
			type: 'POST',
			success: function (data, textStatus, jqXHR) {
				reloadSchedule(data);
				self.IsPostingData(false);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status === 400) {
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
};

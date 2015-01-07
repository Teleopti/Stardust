/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Content/moment/moment.js" />

Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel = function ShiftExchangeOfferViewModel(ajax, doneCallback, dateFormat) {
	var self = this;
	
	this.Template = "add-shift-exchange-offer-template";

	this.StartTime = ko.observable(dateFormat.defaultTimes.defaultStartTime);
	this.EndTime = ko.observable(dateFormat.defaultTimes.defaultEndTime);
	this.EndTimeNextDay = ko.observable(false);
	self.IsTimeLegal = ko.computed(function() {
		var startMoment = moment('1900-01-01 ' + self.StartTime());
		var endMoment = moment('1900-01-01 ' + self.EndTime());
		if (self.EndTimeNextDay()) {
			endMoment.add(1, 'days');
		}
		return startMoment.isBefore(endMoment);
	});

	this.ShowMeridian = ($('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true');
	this.DateFormat = ko.observable(dateFormat.format());

	self.Absence = ko.observable();
	//Interface....
	this.DateFrom = function (date) {
		self.loadPeriod(date);

	};

	this.DateTo = ko.observable();
	this.DateToFormatted = ko.computed(function() {
		var dateTo = self.DateTo();
		if (dateTo) return dateTo.format(self.DateFormat());
		return undefined;
	});

	self.OpenPeriodRelativeStart = ko.observable(1);
	self.OpenPeriodRelativeEnd = ko.observable(10);
	this.OfferValidTo = ko.observable(moment().startOf('day'));
	self.DateToForPublish = ko.computed({
		read: function() {
			return self.DateTo();
		},
		write: function (date) {
			self.DateTo(date);
			self.getAbsence(date.format(self.DateFormat()));
			self.OfferValidTo(moment(date).add('days', -1));
		}
	});

	self.IsSelectedDateInShiftTradePeriod = ko.computed(function () {
		var now = moment().startOf('day');
		var min = moment(now).add('days', self.OpenPeriodRelativeStart());
		var max = moment(now).add('days', self.OpenPeriodRelativeEnd());

		if (moment(self.DateTo()) >= min && moment(self.DateTo()) <= max) return true;
		return false;
	});

	self.IsValidToLegal = ko.computed(function() {
		var today = moment().startOf('day');
		var isValideToLegalStart = self.OfferValidTo() >= today ? true : false;
		var isValideToLegalEnd = self.OfferValidTo() <= moment(self.DateTo()).add('days', -1) ? true : false;

		return isValideToLegalStart && isValideToLegalEnd;
	});

	self.IsNotAbsence = ko.computed(function () {
		var absenceCount = 0;
		if (self.Absence() != null) absenceCount = self.Absence().length;

		return (self.Absence() == null) || absenceCount == 0;
	});
	
	this.SaveEnabled = ko.computed(function () {
		return self.IsSelectedDateInShiftTradePeriod() && self.IsValidToLegal() && self.IsTimeLegal() && self.IsNotAbsence();
	});

	this.ErrorMessage = ko.observable('');
	this.ShowError = ko.computed(function () {
		return self.ErrorMessage() !== undefined && self.ErrorMessage() !== '';
	});

	this.SaveShiftExchangeOffer = function () {
		ajax.Ajax({
			url: "ShiftExchange/NewOffer",
			dataType: "json",
			data: { Date: self.DateTo().format(self.DateFormat()), OfferValidTo: self.OfferValidTo().format(self.DateFormat()), StartTime: self.StartTime(), EndTime: self.EndTime(), EndTimeNextDay: self.EndTimeNextDay() },
			type: 'POST',
			success: function (data, textStatus, jqXHR) {
				doneCallback(data);
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

	self.missingWorkflowControlSet = ko.observable(true);
	self.loadPeriod = function (date) {
		ajax.Ajax({
			url: "Requests/ShiftTradeRequestPeriod",
			dataType: "json",
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				if (data.HasWorkflowControlSet) {
					self.OpenPeriodRelativeStart(data.OpenPeriodRelativeStart);
					self.OpenPeriodRelativeEnd(data.OpenPeriodRelativeEnd);
				}
				self.missingWorkflowControlSet(!data.HasWorkflowControlSet);
			}
		});
	};

	self.getAbsence = function (date) {
		ajax.Ajax({
			url: "ShiftExchange/GetAbsence",
			dataType: "json",
			type: 'GET',
			data: {
				Date: date
			},
			success: function (data, textStatus, jqXHR) {
				self.Absence(null);
				self.Absence(data.PersonAbsences);
			}
		});
	};
};
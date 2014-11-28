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

	this.ShowMeridian = ($('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true');
	this.DateFormat = ko.observable(dateFormat.format());
	this.OfferValidTo = ko.observable(moment().startOf('day'));

	//Interface....
	this.DateFrom = function (date) {
		self.loadPeriod(date);
		self.OfferValidTo(date.clone().add('days', -7));
	};

	this.DateTo = ko.observable();
	this.DateToFormatted = ko.computed(function() {
		var dateTo = self.DateTo();
		if (dateTo) return dateTo.format(self.DateFormat());
		return undefined;
	});

	self.IsSelectedDateInShiftTradePeriod = ko.observable(false);
	this.SaveEnabled = ko.computed(function () {
		var today = moment().startOf('day');
		var isValideToAfterToday = self.OfferValidTo() > today ? true : false;
		var isValideToBeforDateTo = self.OfferValidTo() < self.DateTo() ? true : false;
		var isValidToLegal = isValideToAfterToday && isValideToBeforDateTo;
		return self.IsSelectedDateInShiftTradePeriod() && isValidToLegal;
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
					var now = moment(new Date(data.NowYear, data.NowMonth - 1, data.NowDay));
					var min = moment(now).add('days', data.OpenPeriodRelativeStart);
					var max = moment(now).add('days', data.OpenPeriodRelativeEnd);
					if (moment(date) >= min && moment(date) <= max) self.IsSelectedDateInShiftTradePeriod(true);
					else self.IsSelectedDateInShiftTradePeriod(false);
				}
				self.missingWorkflowControlSet(!data.HasWorkflowControlSet);
			}
		});
	};
};
/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Content/moment/moment.js" />

Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel = function ShiftExchangeOfferViewModel(ajax, doneCallback, dateFormat) {
	var self = this;
	
	this.Template = "add-shift-exchange-offer-template";

	this.OfferValidTo = ko.observable(moment().startOf('day'));
	this.SaveEnabled = ko.computed(function() {
		return self.OfferValidTo() > moment().startOf('day');
	});
	this.StartTime = ko.observable(dateFormat.defaultTimes.defaultStartTime);
	this.EndTime = ko.observable(dateFormat.defaultTimes.defaultEndTime);
	this.EndTimeNextDay = ko.observable(false);

	this.ShowMeridian = ($('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true');
	this.DateFormat = ko.observable(dateFormat.format());

	//Interface....
	this.DateFrom = function(date) {
		self.OfferValidTo(date.clone().add('days', -2));
	};

	this.DateTo = ko.observable();
	this.DateToFormatted = ko.computed(function() {
		var dateTo = self.DateTo();
		if (dateTo) return dateTo.format(self.DateFormat());
		return undefined;
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

	this.LoadRequestData = function() {
	};
};
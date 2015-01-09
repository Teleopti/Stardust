
/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Content/moment/moment.js" />

Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModelFactory = function ShiftExchangeOfferViewModelFactory(ajax, doneCallback) {

	var self = this;

	self.Update = function (offer) {
		var vm = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel(ajax, doneCallback);
		vm.LoadOffer(offer);
		return vm;
	};
	self.Create = function (defaultData) {
		var vm = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel(ajax, doneCallback);
		vm.LoadDefaultData(defaultData);
		return vm;
	};
};

Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel = function ShiftExchangeOfferViewModel(ajax, doneCallback) {
	var self = this;

	self.Template = "shift-exchange-offer-template";
	self.DenyReason = ko.observable("");
	self.StartTime = ko.observable();
	self.EndTime = ko.observable();
	self.DateFormat = ko.observable();
	self.Id = ko.observable(null);

	self.LoadDefaultData = function (defaultData) {
		self.StartTime(defaultData.defaultStartTime);
		self.EndTime(defaultData.defaultEndTime);
	}

	// interface called externally from schedule.js
	self.DateFrom = function () {
		self.LoadPeriod();
	};

	self.IsUpdating = ko.computed(function () { return (self.Id() !== null); });

	self.DateTo = ko.observable();

	self.LoadOffer = function (offer) {
		var date = moment(new Date(offer.DateFromYear, offer.DateFromMonth - 1, offer.DateFromDayOfMonth));
		self.DateFrom(date);
		self.DateTo(date);
		self.StartTime(offer.RawTimeFrom);
		self.EndTime(offer.RawTimeTo);
		self.EndTimeNextDay(offer.IsNextDay);
		self.Id(offer.Id);
	};

	self.IsEditable = ko.observable();
	self.EndTimeNextDay = ko.observable(false);
	self.IsTimeLegal = ko.computed(function () {
		//return !(self.EndTime() < self.StartTime() && !self.EndTimeNextDay());
		var startMoment = moment('1900-01-01 ' + self.StartTime());
		var endMoment = moment('1900-01-01 ' + self.EndTime());
		if (self.EndTimeNextDay()) {
			endMoment.add(1, 'days');
		}
		return startMoment.isBefore(endMoment);
	});

	//ROBTODO: Revisit for refactoring?
	self.ShowMeridian = ($('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true');
	self.Absence = ko.observable();
	self.OpenPeriodRelativeStart = ko.observable(1);
	self.OpenPeriodRelativeEnd = ko.observable(10);
	self.OfferValidTo = ko.observable(moment().startOf('day'));

	self.DateToForPublish = ko.computed({
		read: function () {
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
		return (moment(self.DateTo()) >= min && moment(self.DateTo()) <= max);
	});

	self.IsValidToLegal = ko.computed(function () {
		var today = moment().startOf('day');
		var isValidToLegalStart = (self.OfferValidTo() >= today);
		var isValidToLegalEnd = (self.OfferValidTo() <= moment(self.DateTo()).add('days', -1));

		return isValidToLegalStart && isValidToLegalEnd;
	});

	self.IsNotAbsence = ko.computed(function () {
		var absenceCount = 0;
		if (self.Absence() != null) absenceCount = self.Absence().length;
		return (self.Absence() == null) || absenceCount == 0;
	});

	self.IsValid = ko.computed(function () {
		return (self.IsSelectedDateInShiftTradePeriod() && self.IsValidToLegal() && self.IsTimeLegal() && self.IsNotAbsence());
	});

	self.SaveEnabled = ko.computed(function () {
		return (self.IsValid() && self.IsEditable());
	});

	self.ColumnSizings = ko.computed(function () {
		return self.IsUpdating() ? "col-md-6 col-xs-12" : "col-md-3 col-xs-12";
	});

	self.ErrorMessage = ko.observable('');
	self.ShowError = ko.computed(function () {
		return self.ErrorMessage() !== undefined && self.ErrorMessage() !== '';
	});

	self.SaveShiftExchangeOffer = function () {
		ajax.Ajax({
			url: "ShiftExchange/NewOffer",
			dataType: "json",
			//data: { Date: self.DateTo().format(self.DateFormat()), OfferValidTo: self.OfferValidTo().format(self.DateFormat()), StartTime: self.StartTime(), EndTime: self.EndTime(), EndTimeNextDay: self.EndTimeNextDay() },
			data: { Date: self.DateTo().format(self.DateFormat()),
				OfferValidTo: self.OfferValidTo().format(self.DateFormat()),
				StartTime: moment('1900-01-01 ' + self.StartTime()).format('HH:mm'),
				EndTime: moment('1900-01-01 ' + self.EndTime()).format('HH:mm'),
				EndTimeNextDay: self.EndTimeNextDay(),
				Id: self.Id()
			},
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
	self.LoadPeriod = function () {
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



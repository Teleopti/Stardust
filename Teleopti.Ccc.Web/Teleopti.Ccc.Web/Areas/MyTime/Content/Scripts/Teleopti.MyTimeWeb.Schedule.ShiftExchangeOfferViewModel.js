﻿/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
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

	self.Toggle31317Enabled = ko.observable(false);

	self.AllShiftTypes = ko.observable([]);

	self.WishShiftTypeOption = ko.observable();

	self.RequireDetails = ko.computed(function() {
		return !self.Toggle31317Enabled() || (self.WishShiftTypeOption() && self.WishShiftTypeOption().RequireDetails);
	});

	self.LoadOptions = function (setShiftType) {
		self.Toggle31317Enabled(Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_TradeWithDayOffAndEmptyDay_31317'));
		self.getWishShiftTypes(setShiftType);
	}

	self.LoadDefaultData = function (defaultData) {
		self.StartTime(defaultData.defaultStartTime);
		self.EndTime(defaultData.defaultEndTime);
		self.LoadOptions(function() {
			self.WishShiftTypeOption(self.AllShiftTypes()[0]);
		});
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

		if (offer.ExchangeOffer) {
			self.OfferValidTo(moment(offer.ExchangeOffer.ValidTo));

			self.LoadOptions(function () {
				var allOptions = self.AllShiftTypes();
				var wishShiftType = offer.ExchangeOffer.WishShiftType;
				for (var i = 0; i < allOptions.length; i++) {
					var option = allOptions[i];
					if (option.Id === wishShiftType) {
						self.WishShiftTypeOption(option);
						break;
					}
				}
			});
		}
	};

	self.convert12To24Hour = function (time) {
		time = time.toUpperCase();
		var hours = Number(time.match(/^(\d+)/)[1]);
		var minutes = Number(time.match(/:(\d+)/)[1]);
		var AMPM = time.match(/\s(.*)$/)[1];
		if (AMPM == "PM" && hours < 12) hours = hours + 12;
		if (AMPM == "AM" && hours == 12) hours = hours - 12;
		var sHours = hours.toString();
		var sMinutes = minutes.toString();
		if (hours < 10) sHours = "0" + sHours;
		if (minutes < 10) sMinutes = "0" + sMinutes;
		return sHours + ":" + sMinutes;
	};

	self.IsEditable = ko.observable(true);
	self.EndTimeNextDay = ko.observable(false);
	self.startTimeInternal = ko.observable();
	self.endTimeInternal = ko.observable();
	self.IsTimeLegal = ko.computed(function () {
		if (self.StartTime() !== undefined && self.EndTime() !== undefined) {
			if (self.StartTime().indexOf("am") > -1 || self.StartTime().indexOf("AM") > -1
				|| self.StartTime().indexOf("pm") > -1 || self.StartTime().indexOf("PM") > -1) {
				var start = self.convert12To24Hour(self.StartTime());
				self.startTimeInternal(start);
			} else {
				self.startTimeInternal(self.StartTime());
			}

			if (self.EndTime().indexOf("am") > -1 || self.EndTime().indexOf("AM") > -1
				|| self.EndTime().indexOf("pm") > -1 || self.EndTime().indexOf("PM") > -1) {
				var end = self.convert12To24Hour(self.EndTime());
				self.endTimeInternal(end);
			} else {
				self.endTimeInternal(self.EndTime());
			}
		}		
		if (self.endTimeInternal() < self.startTimeInternal() && !self.EndTimeNextDay()) return false;
		return true;
	});

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
			if (!self.IsUpdating() || (self.IsUpdating() && self.OfferValidTo() >= (moment(date)))) {
				self.OfferValidTo(moment(date).add('days', -1));
			}
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
		return self.IsUpdating() ? "col-md-6" : "col-md-3";
	});

	self.ErrorMessage = ko.observable('');
	self.ShowError = ko.computed(function () {
		return self.ErrorMessage() !== undefined && self.ErrorMessage() !== '';
	});

	self.SaveShiftExchangeOffer = function () {
		var wishShiftTypeId = self.Toggle31317Enabled()
			? self.WishShiftTypeOption().Id
			: self.AllShiftTypes()[0].Id;

		ajax.Ajax({
			url: "ShiftExchange/NewOffer",
			dataType: "json",
			data: {
				Date: self.DateTo().format(self.DateFormat()),
				OfferValidTo: self.OfferValidTo().format(self.DateFormat()),
				StartTime:self.startTimeInternal(),
				EndTime: self.endTimeInternal(),
				EndTimeNextDay: self.EndTimeNextDay(),
				Id: self.Id(),
				WishShiftType: wishShiftTypeId
			},
			type: 'POST',
			success: function(data, textStatus, jqXHR) {
				doneCallback(data);
			},
			error: function(jqXHR, textStatus, errorThrown) {
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

	self.getAbsence = function(date) {
		ajax.Ajax({
			url: "ShiftExchange/GetAbsence",
			dataType: "json",
			type: 'GET',
			data: {
				Date: date
			},
			success: function(data, textStatus, jqXHR) {
				self.Absence(null);
				self.Absence(data.PersonAbsences);
			}
		});
	};

	self.getWishShiftTypes = function (setWishShiftType) {
		ajax.Ajax({
			url: "ShiftExchange/GetAllWishShiftOptions",
			dataType: "json",
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				self.AllShiftTypes(data);
				if (typeof setWishShiftType === 'function') {
					setWishShiftType(data[0]);
				}
			}
		});
	}
};

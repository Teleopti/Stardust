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

Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel = function ShiftExchangeOfferViewModel(ajax, doneCallback,mywindow) {
	var self = this;
	mywindow = mywindow || window;
	self.Template = "shift-exchange-offer-template";
	self.DenyReason = ko.observable("");
	self.StartTime = ko.observable();
	self.EndTime = ko.observable();
	self.DateFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);
	self.TimeFormat = ko.observable(Teleopti.MyTimeWeb.Common.TimeFormat);

	self.isReadyLoaded = ko.observable(false);

	self.Id = ko.observable(null);
	self.WeekStart = ko.observable(1);

	self.AllShiftTypes = ko.observable([]);

	self.WishShiftTypeOption = ko.observable();

	self.RequireDetails = ko.computed(function() {
		return self.WishShiftTypeOption() && self.WishShiftTypeOption().RequireDetails;
	});

	self.getFormattedDateForServiceCall = function(date) {
		return Teleopti.MyTimeWeb.Common.FormatServiceDate(date);
	};
	
	self.LoadOptions = function (setShiftType) {
		self.getWishShiftTypes(setShiftType);
		Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function (data) {
			self.WeekStart(data.WeekStart);
		});
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
		
		self.StartTime(Teleopti.MyTimeWeb.Common.FormatTime(offer.DateTimeFrom));
		self.EndTime(Teleopti.MyTimeWeb.Common.FormatTime(offer.DateTimeTo));
		
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

		if (Teleopti.MyTimeWeb.Common.Meridiem.AM !== "") {
			var AMPM = time.match(/\s(.*)$/)[1];
			if (AMPM == Teleopti.MyTimeWeb.Common.Meridiem.PM && hours < 12) hours = hours + 12;
			if (AMPM == Teleopti.MyTimeWeb.Common.Meridiem.AM && hours == 12) hours = hours - 12;
		}

		var sHours = hours.toString();
		var sMinutes = minutes.toString();
		if (hours < 10) sHours = "0" + sHours;
		if (minutes < 10) sMinutes = "0" + sMinutes;
		return sHours + ":" + sMinutes;
	};

	self.IsEditable = ko.observable(true);
	self.IsPostingData = ko.observable(false);
	self.EndTimeNextDay = ko.observable(false);
	self.startTimeInternal = ko.observable();
	self.endTimeInternal = ko.observable();
	self.IsTimeLegal = ko.computed(function () {
		if (self.StartTime() !== undefined && self.EndTime() !== undefined) {
			if (self.StartTime().indexOf(Teleopti.MyTimeWeb.Common.Meridiem.AM) > -1 
				|| self.StartTime().indexOf(Teleopti.MyTimeWeb.Common.Meridiem.PM) > -1 ) {
				var start = self.convert12To24Hour(self.StartTime());
				self.startTimeInternal(start);
			} else if (Teleopti.MyTimeWeb.Common.TimeFormat.indexOf(".") > -1) {
				self.startTimeInternal(self.StartTime().replace(":", "."));
			} else {
				self.startTimeInternal(self.StartTime());
			}

			if (self.EndTime().indexOf(Teleopti.MyTimeWeb.Common.Meridiem.AM) > -1 
				|| self.EndTime().indexOf(Teleopti.MyTimeWeb.Common.Meridiem.PM) > -1) {
				var end = self.convert12To24Hour(self.EndTime());
				self.endTimeInternal(end);
			} else if (Teleopti.MyTimeWeb.Common.TimeFormat.indexOf(".") > -1) {
				self.endTimeInternal(self.EndTime().replace(":", "."));
			}else {
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
	self.InitAbsence = ko.observable(false);

	self.DateToForPublish = ko.computed({
		read: function () {
			return self.DateTo();
		},
		write: function (date) {
			var unixDateTo = moment(self.DateTo()).format('X');
			var unixDate = moment(date).format('X');
			if (self.InitAbsence() && unixDateTo == unixDate) {
				return;
			}
			self.DateTo(date);
			self.getAbsence(self.getFormattedDateForServiceCall(date));
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
		return self.IsValid() && self.IsEditable() && !self.IsPostingData();
	});

	self.ColumnSizings = ko.computed(function () {
		return self.IsUpdating() ? "col-md-6" : "col-md-3";
	});

	self.ErrorMessage = ko.observable('');
	self.ShowError = ko.computed(function () {
		return self.ErrorMessage() !== undefined && self.ErrorMessage() !== '';
	});

	self.SaveShiftExchangeOffer = function () {
		if (self.IsPostingData()) {
			return;
		}

		self.IsPostingData(true);

		ajax.Ajax({
			url: "ShiftExchange/NewOffer",
			dataType: "json",
			data: {
				Date: self.getFormattedDateForServiceCall(self.DateTo()),
				OfferValidTo: self.getFormattedDateForServiceCall(self.OfferValidTo()),
				StartTime:self.startTimeInternal(),
				EndTime: self.endTimeInternal(),
				EndTimeNextDay: self.EndTimeNextDay(),
				Id: self.Id(),
				WishShiftType: self.WishShiftTypeOption().Id
			},
			type: 'POST',
			success: function(data, textStatus, jqXHR) {
				doneCallback(data);
				self.IsPostingData(false);
			},
			error: function(jqXHR, textStatus, errorThrown) {
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

	self.missingWorkflowControlSet = ko.observable(false);
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
				self.isReadyLoaded(true);
			}
		});
	};

	self.getAbsence = function(date) {
		if (!self.InitAbsence()) {
			self.InitAbsence(true);
		}
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

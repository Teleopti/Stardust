﻿/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>

Teleopti.MyTimeWeb.Request.RequestViewModel = function (addRequestMethod, callback, firstDayOfWeek, defaultDateTimes, mywindow) {
	var self = this;
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	mywindow = mywindow || window;

	self.Templates = ["text-request-detail-template", "absence-request-detail-template", "shifttrade-request-detail-template", "shiftexchangeoffer-request-detail-template", "add-overtime-request-template"];
	self.IsFullDay = ko.observable(false);
	self.IsUpdate = ko.observable(false);

	self.IsLoadingPersonalAccount = ko.observable(true);
	self.IsPostingData = ko.observable(false);

	var urlDate = Teleopti.MyTimeWeb.Portal.ParseHash().dateHash.replace(/\//g, "-");
	if (urlDate && urlDate.indexOf("/") >= 0) {
		// Change date in style "2017/12/31" to "2017-12-31" to fix momentjs warning
		urlDate = urlDate.replace(/\//g, "-");
	}

	self.DateFrom = ko.observable(urlDate && moment(urlDate).isValid() ? moment(urlDate).startOf('day') : moment().startOf('day'));
	self.DateTo = ko.observable(urlDate && moment(urlDate).isValid() ? moment(urlDate).startOf('day') : moment().startOf('day'));

	self.PreviousDateTo = ko.observable(moment());
	self.TimeFromInternal = ko.observable(defaultDateTimes ? defaultDateTimes.defaultStartTime : null);
	self.TimeToInternal = ko.observable(defaultDateTimes ? defaultDateTimes.defaultEndTime : null);
	self.DateFormat = ko.observable();
	self.TimeFrom = ko.computed({
		read: function () {
			if (self.IsFullDay()) {
				return defaultDateTimes.defaultFulldayStartTime;
			}
			return self.TimeFromInternal();
		},
		write: function (value) {
			if (self.IsFullDay()) return;
			self.TimeFromInternal(value);
		}
	});
	self.TimeTo = ko.computed({
		read: function () {
			if (self.IsFullDay()) {
				return defaultDateTimes.defaultFulldayEndTime;
			}
			return self.TimeToInternal();
		},
		write: function (value) {
			if (self.IsFullDay()) return;
			self.TimeToInternal(value);
		}
	});

	self.ShowMeridian = ($('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true');
	self.TypeEnum = ko.observable(0);
	self.ShowError = ko.observable(false);
	self.ErrorMessage = ko.observable('');
	self.AbsenceId = ko.observable();
	self.PreviousAbsenceId = ko.observable();
	self.AbsenceTrackedAsDay = ko.observable(false);
	self.AbsenceTrackedAsHour = ko.observable(false);
	self.AbsenceAccountExists = ko.observable(false);
	self.AbsenceAccountPeriodStart = ko.observable(moment());
	self.AbsenceAccountPeriodEnd = ko.observable(moment());
	self.AbsenceUsed = ko.observable();
	self.AbsenceRemaining = ko.observable();
	self.Subject = ko.observable();
	self.Message = ko.observable();
	self.EntityId = ko.observable();
	self.WaitlistPosition = ko.observable();
	self.DenyReason = ko.observable();
	self.IsEditable = ko.observable(true);
	self.IsDateInputEditable = !Teleopti.MyTimeWeb.Portal.IsMobile(mywindow);
	self.IsNewInProgress = ko.observable(false);
	self.weekStart = ko.observable(firstDayOfWeek);
	self.IsTimeInputVisible = ko.computed(function () {
		return !self.IsFullDay();
	});

	self.Initialize = function (data) {
		self.Subject(data.Subject);
		self.Message(data.Text);
		self.DateFrom(moment(new Date(data.DateFromYear, data.DateFromMonth - 1, data.DateFromDayOfMonth)));

		self.TimeFrom(Teleopti.MyTimeWeb.Common.FormatTime(data.DateTimeFrom));
		self.TimeTo(Teleopti.MyTimeWeb.Common.FormatTime(data.DateTimeTo));

		self.DateTo(moment(new Date(data.DateToYear, data.DateToMonth - 1, data.DateToDayOfMonth)));

		self.EntityId(data.Id);
		self.AbsenceId(data.PayloadId);
		self.DenyReason(data.DenyReason);
		self.IsFullDay(data.IsFullDay);
		self.TypeEnum(data.TypeEnum);
		self.WaitlistPosition(data.WaitlistPosition);

		self.onLoadComplete();

	};

	self.onLoadComplete = function () {

		self.AbsenceId.subscribe(function () {
			loadAbsenceAccount(false);
		});

		self.DateTo.subscribe(function () {
			loadAbsenceAccount(false);
		});

		loadAbsenceAccount(true);

	};

	self.checkMessageLength = function (data, event) {
		var text = $(event.target)[0].value;
		if (text.length > 2000) {
			self.Message(text.substr(0, 2000));
		}
	};

	self.readAbsenceAccount = function (data) {
		if (data) {
			self.AbsenceAccountExists(true);
			self.AbsenceTrackedAsDay(data.TrackerType == "Days");
			self.AbsenceTrackedAsHour(data.TrackerType == "Hours");
			self.AbsenceAccountPeriodStart(moment(data.PeriodStart));
			self.AbsenceAccountPeriodEnd(moment(data.PeriodEnd));
			self.AbsenceRemaining(data.Remaining);
			self.AbsenceUsed(data.Used);
		} else {
			var today = moment().startOf('day');

			self.AbsenceAccountExists(false);
			self.AbsenceTrackedAsDay(false);
			self.AbsenceTrackedAsHour(false);
			self.AbsenceAccountPeriodStart(today);
			self.AbsenceAccountPeriodEnd(today);
			self.AbsenceUsed("0");
			self.AbsenceRemaining("0");
		}

		self.IsLoadingPersonalAccount(false);
	};

	function loadAbsenceAccount(forceAccountUpdate) {
		if (!self.PersonalAccountPermission || self.AbsenceId() == undefined || self.AbsenceId() == null)
			return;
		var absenceChanged = self.AbsenceId() != self.PreviousAbsenceId();
		var dateToChanged = !self.DateTo().isSame(self.PreviousDateTo().format("YYYY-MM-DD HH:mm:ss"));
		var isOutOfPeriodRange = self.DateTo().isBefore(self.AbsenceAccountPeriodStart()) || self.DateTo().isAfter(self.AbsenceAccountPeriodEnd());
		if (forceAccountUpdate || absenceChanged || (dateToChanged && isOutOfPeriodRange)) {

			self.IsLoadingPersonalAccount(true);

			ajax.Ajax({
				url: "Requests/FetchAbsenceAccount",
				dataType: "json",
				type: 'GET',
				contentType: 'application/json; charset=utf-8',
				data: {
					absenceId: self.AbsenceId(),
					date: Teleopti.MyTimeWeb.Common.FormatServiceDate(self.DateTo())
				},
				success: function (data, textStatus, jqXHR) {
					self.readAbsenceAccount(data);
				},
				error: function (e) {
					self.readAbsenceAccount();
				},
				complete: function () {
				}
			});
		}

		if ((absenceChanged)) {
			self.PreviousAbsenceId(self.AbsenceId());
		}

		if (dateToChanged) {
			self.PreviousDateTo(self.DateTo());
		}
	};
	self.PersonalAccountPermission = ko.observable(false);
	self.readPersonalAccountPermission = function (data) {
		self.PersonalAccountPermission(data);
	};

	self.ShowAbsenceAccount = ko.computed(function () {
		return self.PersonalAccountPermission() && self.AbsenceAccountExists() && self.IsEditable();
	});


	self.Template = ko.computed(function () {
		return self.IsUpdate() ? self.Templates[self.TypeEnum()] : "add-new-request-detail-template";
	});

	self.ShowAbsencesCombo = ko.computed(function () {
		return self.TypeEnum() === 1 ? true : false;
	});

	self.AddRequestErrorCallback = function () {
		self.IsPostingData(false);
	};

	self.AddRequest = function () {
		validateRequestSubjectAndTimeInput();
		if (self.ShowError())
			return;

		self.IsPostingData(true);
		addRequestMethod && addRequestMethod(self, callback, self.AddRequestErrorCallback);
	};

	function _setDefaultDates() {
		var year = defaultDateTimes.todayYear;
		var month = defaultDateTimes.todayMonth;
		var day = defaultDateTimes.todayDay;

		self.DateFrom(moment(new Date(year, month - 1, day)));
		self.DateTo(moment(new Date(year, month - 1, day)));
	}

	self.AddTextRequest = function (useDefaultDates) {
		if (useDefaultDates != undefined && useDefaultDates === true)
			_setDefaultDates();
		self.IsNewInProgress(true);
		self.TypeEnum(0);
		self.IsFullDay(false);

	};

	self.AddAbsenceRequest = function (useDefaultDates) {
		if (useDefaultDates != undefined && useDefaultDates === true)
			_setDefaultDates();
		self.IsNewInProgress(true);
		self.TypeEnum(1);
		self.IsFullDay(true);
		self.onLoadComplete();
	};

	self.CancelAddingNewRequest = function () {

		self.IsNewInProgress(false);
	};

	self.SetAjax = function (ajaxobj) { ajax = ajaxobj; };

	self.checkSubject = function () {
		if (self.Subject && (!self.Subject() || !/\S/.test(self.Subject()))) {
			self.ShowError(true);
			self.ErrorMessage(requestsMessagesUserTexts.MISSING_SUBJECT);
			return;
		} else {
			self.ShowError(false);
			self.ErrorMessage('');
		}
	};

	function validateRequestSubjectAndTimeInput() {
		if (!self.Subject || !self.TimeFrom() || !self.TimeTo())
			return;

		if (!self.Subject() || !/\S/.test(self.Subject())) {
			self.ShowError(true);
			self.ErrorMessage(requestsMessagesUserTexts.MISSING_SUBJECT);
			return;
		}
		
		var timeFrom = moment(self.DateFrom().format('YYYY-MM-DD') + ' ' + self.TimeFrom());
		var timeTo = moment(self.DateTo().format('YYYY-MM-DD') + ' ' + self.TimeTo());
		if (timeFrom.isAfter(timeTo) || timeFrom.isSame(timeTo)) {
			self.ShowError(true);
			self.ErrorMessage(requestsMessagesUserTexts.ENDTIME_MUST_BE_GREATER_THAN_STARTTIME);
		} else {
			self.ShowError(false);
			self.ErrorMessage('');
		}
	};
};
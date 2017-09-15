Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel = function (ajax, doneCallback, parentViewModel, weekStart, isViewingDetail) {
	var self = this,
		dateTimeFormats = Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat;

	self.Id = ko.observable();
	self.Template = "add-overtime-request-template";
	self.IsPostingData = ko.observable(false);
	self.IsEditable = ko.observable(true);

	self.Subject = ko.observable();
	self.Message = ko.observable();

	self.Today = ko.observable(moment());
	self.DateFrom = ko.observable();
	self.StartTime = ko.observable();
	self.DateFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);

	self.weekStart = ko.observable(weekStart);
	self.ShowMeridian = $('div[data-culture-show-meridian]').attr('data-culture-show-meridian') === 'true';
	self.ShowError = ko.observable();
	self.ErrorMessage = ko.observable();
	self.ShowCancelButton = ko.observable();
	self.MultiplicatorDefinitionSetId = ko.observable();
	self.TimeList = undefined;
	self.RequestDuration = ko.observable();
	self.IsTimeListOpened = ko.observable(false);

	self.IsMobile = Teleopti.MyTimeWeb.Portal.IsMobile();

	self.checkMessageLength = function (data, event) {
		var text = $(event.target)[0].value;
		if (text.length > 2000) {
			self.Message(text.substr(0, 2000));
		}
	};

	self.validateDuration = function (data, event) {
		var value = $(event.target)[0].value;
		if (!value) return;

		var duration = value.split(':');
		duration.length = 2;

		var hourReg = /^(([0-1][0-9])|([2][0-3]))$/gi;
		if (!hourReg.test(duration[0])) {
			duration[0] = '00';
			self.RequestDuration(duration.join(':'));
		}

		var minuteReg = /^[0-5][0-9]$/gi;
		if (!minuteReg.test(duration[1])) {
			duration[1] = '59';
			self.RequestDuration(duration.join(':'));
		}
	};

	self.AddRequest = function () {
		if (!_validateRequiredFields()) {
			return;
		}

		self.IsPostingData(true);

		ajax.Ajax({
			url: 'Requests/PersistOvertimeRequest',
			data: _buildPostData(),
			dataType: 'json',
			type: 'POST',
			success: function (data) {
				self.IsPostingData(false);
				doneCallback(data);
				self.CancelAddRequest();
			},
			error: function () {
				self.IsPostingData(false);
			}
		});
	};

	self.ToggleDropdownTimeList = function () {
		self.IsTimeListOpened(!self.IsTimeListOpened());
	};

	self.CloseDropdownTimeList = function() {
		self.IsTimeListOpened(false);
	};

	self.AssignRequestDuration = function (data, event) {
		self.RequestDuration(data);
		self.CloseDropdownTimeList();
	};

	self.Initialize = function (data) {
		if (isViewingDetail && data) {
			self.Id(data.Id);

			self.Subject(data.Subject);
			self.Message(data.Text);

			self.DateFrom(moment(data.DateTimeFrom).format(dateTimeFormats.dateOnly));
			self.StartTime(moment(data.DateTimeFrom).format(Teleopti.MyTimeWeb.Common.TimeFormat));

			var seconds = (moment(data.DateTimeTo) - moment(data.DateTimeFrom)) / 1000;
			var hours = '0' + moment.duration(seconds, 'seconds').hours();
			var minutes = '0' + moment.duration(seconds, 'seconds').minutes();

			self.RequestDuration(hours.substr(-2, 2) + ":" + minutes.substr(-2, 2));
			self.MultiplicatorDefinitionSetId(data.MultiplicatorDefinitionSet);
			self.ShowCancelButton(false);
		}
	};

	function _createTimeList() {
		var timeList = [];
		for (var i = 1; i < 24; i++) {
			if (i < 10)
				timeList.push('0' + i + ':00');
			else
				timeList.push(i + ':00');
		}
		return timeList;
	}

	function _validateRequiredFields() {
		var dataValid = false;
		if (!self.Subject() || !/\S/.test(self.Subject())) {
			self.ErrorMessage(requestsMessagesUserTexts.MISSING_SUBJECT);
		} else if (_buildPostData().Period.StartTime.length != 5 || self.StartTime() == '') {
			self.ErrorMessage(requestsMessagesUserTexts.MISSING_STARTTIME);
		} else if (!self.RequestDuration() || self.RequestDuration().length != 5) {
			self.ErrorMessage(requestsMessagesUserTexts.MISSING_DURATION);
		} else if (_isDateFromPast()) {
			self.ErrorMessage(requestsMessagesUserTexts.OVERTIME_REQUEST_DATE_IS_PAST);
		} else if (_isDateFromExceeds14Days()) {
			self.ErrorMessage(requestsMessagesUserTexts.OVERTIME_REQUEST_DATE_EXCEEDS_14DAYS);
		}
		else {
			dataValid = true;
			self.ErrorMessage('');
		}

		self.ShowError(!dataValid);
		return dataValid;
	}

	function _isDateFromPast() {
		var dateFromMoment = self.DateFrom();
		if (!moment.isMoment(dateFromMoment))
			dateFromMoment = moment(dateFromMoment, dateTimeFormats.dateOnly);
		var days = Math.ceil(moment.duration(dateFromMoment - moment().startOf("day")).asDays());
		return days < 0;
	}

	function _isDateFromExceeds14Days() {
		var dateFromMoment = self.DateFrom();
		if (!moment.isMoment(dateFromMoment))
			dateFromMoment = moment(dateFromMoment, dateTimeFormats.dateOnly);
		var days = Math.ceil(moment.duration(dateFromMoment - moment().startOf("day")).asDays());
		return days > 13;
	}

	function _buildPostData() {
		var startDate = !moment.isMoment(self.DateFrom()) ? moment(self.DateFrom()) : moment().startOf("day");
		var format = dateTimeFormats.dateOnly + " " + Teleopti.MyTimeWeb.Common.TimeFormat;
		var periodStart = moment(startDate.format(dateTimeFormats.dateOnly) + " " + self.StartTime(), format);
		var periodEnd = moment(periodStart);
		if (self.RequestDuration()) {
			var durationParts = self.RequestDuration().split(":");
			periodEnd.add(durationParts[0], "hours").add(durationParts[1], "minutes");
		}

		return {
			Id: self.Id(),
			Subject: self.Subject(),
			Message: self.Message(),
			MultiplicatorDefinitionSet: self.MultiplicatorDefinitionSetId(),
			Period: {
				StartDate: periodStart.format(dateTimeFormats.dateOnly),
				StartTime: periodStart.format(dateTimeFormats.timeOnly),
				EndDate: periodEnd.format(dateTimeFormats.dateOnly),
				EndTime: periodEnd.format(dateTimeFormats.timeOnly)
			}
		};
	}

	function _initializeViewModel(){
		self.ShowError(false);
		self.ShowCancelButton(true);
		var currentUserDateTime = parentViewModel.baseUtcOffsetInMinutes
			? Teleopti.MyTimeWeb.Common.GetCurrentUserDateTime(parentViewModel.baseUtcOffsetInMinutes, parentViewModel.daylightSavingAdjustment)
			: moment();
		var defaultStartTime = currentUserDateTime.add(20, 'minutes');
		self.StartTime(defaultStartTime.format(Teleopti.MyTimeWeb.Common.TimeFormat));
		self.TimeList = _createTimeList();
		self.RequestDuration(self.TimeList[0]);
		self.CancelAddRequest = parentViewModel.CancelAddingNewRequest;
	}

	_initializeViewModel();
}
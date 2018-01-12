﻿Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel = function (ajax, doneCallback, parentViewModel, weekStart, isViewingDetail) {
	var self = this,
		dateTimeFormats = Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat,
		dateOnlyFormat =Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly,
		defaultStartTimeSubscription;

	self.Id = ko.observable();
	self.Template = "add-overtime-request-template";
	self.IsPostingData = ko.observable(false);
	self.IsLoading = ko.observable(false);
	self.IsEditable = ko.observable(true);

	self.Subject = ko.observable();
	self.Message = ko.observable();

	self.PeriodStartDate = ko.observable(moment());
	self.PeriodEndDate = ko.observable(null);

	self.DateFrom = ko.observable();
	self.getDefaultStartTime = ko.computed(self.DateFrom).extend({throttle: 50});

	self.StartTime = ko.observable();
	self.DateFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);

	self.weekStart = weekStart;
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
			url: 'OvertimeRequests/Save',
			data: _buildPostData(),
			dataType: 'json',
			type: 'POST',
			success: function (data) {
				self.IsPostingData(false);
				doneCallback && doneCallback(data);
				self.CancelAddRequest();
			},
			error: function (response, textStatus) {
				if (response.responseJSON) {
					var errors = response.responseJSON.Errors;
					if (errors && errors.length > 0) {
						self.ErrorMessage(errors[0]);
						self.ShowError(true);
					} else {
						Teleopti.MyTimeWeb.Common.AjaxFailed(response, null, textStatus);
					}
				}
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

	function setAvailableDays() {
		self.IsLoading(true);
		ajax.Ajax({
			url: 'OvertimeRequests/GetAvailableDays',
			dataType: 'json',
			type: 'GET',
			async: false,
			success: function (days) {
				self.PeriodEndDate(moment().add(days, 'days'));
				self.IsLoading(false);
			},
			error: function (response, textStatus) {
				_ajaxErrorFn(response, textStatus);
			}
		});
	}

	function setDefaultStartTime() {
		var requestDate = self.DateFrom()._isAMomentObject ? self.DateFrom().format('YYYY/MM/DD') : moment(self.DateFrom(), dateOnlyFormat).format('YYYY/MM/DD');

		if(moment(requestDate).isBefore(self.PeriodStartDate().format('YYYY-MM-DD')) || moment(requestDate).isAfter(self.PeriodEndDate().format('YYYY-MM-DD'))) {
			return;
		}
			
		self.IsLoading(true);
		ajax.Ajax({
			url: 'OvertimeRequests/GetDefaultStartTime',
			dataType: 'json',
			data: {
				date: requestDate
			},
			type: 'GET',
			success: function (startDate) {
				if(moment(startDate).format('YYYY-MM-DD') !== self.DateFrom().format('YYYY-MM-DD')){
					disposeDefaultStartTimeSubscription();
					self.DateFrom(moment(startDate));
				}
				
				self.StartTime(moment(startDate).format(Teleopti.MyTimeWeb.Common.TimeFormat));
				setDefaultStartTimeSubscription();

				self.IsLoading(false);
			},
			error: function (response, textStatus) {
				_ajaxErrorFn(response, textStatus);
			}
		});
	}

	function setDefaultStartTimeSubscription(currentUserDateTime) {
		disposeDefaultStartTimeSubscription();
		defaultStartTimeSubscription = self.getDefaultStartTime.subscribe(function() {
			setDefaultStartTime();
		});
	}

	function disposeDefaultStartTimeSubscription() {
		if(defaultStartTimeSubscription && defaultStartTimeSubscription.dispose) {
			defaultStartTimeSubscription.dispose();
		}
	}

	function _ajaxErrorFn(response, textStatus) {
		if (response.responseJSON) {
			var errors = response.responseJSON.Errors;
			if (errors && errors.length > 0) {
				self.ErrorMessage(errors[0]);
				self.ShowError(true);
			} else {
				Teleopti.MyTimeWeb.Common.AjaxFailed(response, null, textStatus);
			}
		}
		self.IsLoading(false);
	}

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
		if (_isSubjectEmpty()) {
			self.ErrorMessage(requestsMessagesUserTexts.MISSING_SUBJECT);
		} else if (_isOvertimeTypeEmpty()) {
			self.ErrorMessage(requestsMessagesUserTexts.MISSING_OVERTIME_TYPE);
		}  else if (_isStarttimeEmpty()) {
			self.ErrorMessage(requestsMessagesUserTexts.MISSING_STARTTIME);
		} else if (_isDurationEmpty()) {
			self.ErrorMessage(requestsMessagesUserTexts.MISSING_DURATION);
		} else if (_isDateFromPast()) {
			self.ErrorMessage(requestsMessagesUserTexts.OVERTIME_REQUEST_DATE_IS_PAST);
		} else {
			dataValid = true;
			self.ErrorMessage('');
		}

		self.ShowError(!dataValid);
		return dataValid;
	}

	function _isSubjectEmpty() {
		var subject = self.Subject();
		return !subject || !/\S/.test(subject);
	}

	function _isOvertimeTypeEmpty() {
		var overtimeType = self.MultiplicatorDefinitionSetId();
		return !overtimeType;
	}

	function _isStarttimeEmpty() {
		return !self.StartTime() || _buildPostData().Period.StartTime.length != 5;
	}

	function _isDurationEmpty() {
		return !self.RequestDuration() || self.RequestDuration().length != 5 || self.RequestDuration() === '00:00';
	}
	
	function _isDateFromPast() {
		var dateFromMoment = self.DateFrom();
		if (!moment.isMoment(dateFromMoment))
			dateFromMoment = moment(dateFromMoment, dateTimeFormats.dateOnly);
		var days = Math.ceil(moment.duration(dateFromMoment - moment().startOf("day")).asDays());
		return days < 0;
	}

	function _isDateFromExceedsAvailableDays() {
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
		if (Teleopti.MyTimeWeb.Common.IsToggleEnabled('OvertimeRequestPeriodSetting_46417')) {
			setAvailableDays();
		}
		if (Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_OvertimeRequestDefaultStartTime_47513')) {
			setDefaultStartTimeSubscription(currentUserDateTime);
		}
	}

	_initializeViewModel();
}
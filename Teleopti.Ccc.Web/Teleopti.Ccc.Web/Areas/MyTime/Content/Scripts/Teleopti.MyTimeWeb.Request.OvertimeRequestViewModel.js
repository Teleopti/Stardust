﻿Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel = function (ajax, doneCallback, parentViewModel, weekStart, isViewingDetail) {
	var self = this,
		postDataStartDate,
		dateOnlyFormat = Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly,
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
	self.DateFromChangeSubscription = undefined;
	self.StartTimeChangeSubscription = undefined;

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

	self.GetDefaultStartTime = function() {
		ajax.Ajax({
			url: '../api/Schedule/GetIntradayScheduleEdgeTime',
			data: {
				date: moment.isMoment(self.DateFrom()) ? self.DateFrom().format(dateOnlyFormat) : self.DateFrom()
			},
			dataType: 'json',
			type: 'Get',
			success: function(data) {
				if(moment(data.EndDateTime).isAfter(moment(self.DateFrom()).endOf('day'))){
					postDataStartDate = moment(data.EndDateTime).format(dateOnlyFormat);
				}

				self.StartTime(moment(data.EndDateTime).format('HH:mm'));
				_buildStartTimeSubscription();
			}
		});
	};

	self.Initialize = function (data) {
		if (isViewingDetail && data) {
			self.Id(data.Id);

			self.Subject(data.Subject);
			self.Message(data.Text);
			!self.DateFromChangeSubscription && _buildDateFromSubscription();

			self.DateFrom(moment(data.DateTimeFrom).format(dateOnlyFormat));

			if (self.ShowMeridian) {
				self.StartTime(moment(data.DateTimeFrom).format("hh:mm A"));
			} else {
				self.StartTime(moment(data.DateTimeFrom).format("HH:mm"));
			}

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
		} else if (_buildPostData().Period.StartTime.length != 5) {
			self.ErrorMessage(requestsMessagesUserTexts.MISSING_STARTTIME);
		} else if (!self.RequestDuration() || self.RequestDuration().length != 5) {
			self.ErrorMessage(requestsMessagesUserTexts.MISSING_DURATION);
		} else if (!_isDateFromWithin14Days()) {
			self.ErrorMessage(requestsMessagesUserTexts.REQUESTDATE_EXCEEDS_14DAYS);
		}
		else {
			dataValid = true;
			self.ErrorMessage('');
		}

		self.ShowError(!dataValid);
		return dataValid;
	}

	function _isDateFromWithin14Days() {
		var dateFromMoment = self.DateFrom();
		if (!moment.isMoment(dateFromMoment))
			dateFromMoment = moment(dateFromMoment, dateOnlyFormat);

		var days = Math.ceil(moment.duration(dateFromMoment - moment()).asDays());
		return days <= 14 && days >= 0;
	}

	function _getStartDateForPostData(){
		var startDate = !moment.isMoment(self.DateFrom()) ? moment(self.DateFrom()) : moment().startOf("day");
		if(startDate.format(dateOnlyFormat) != postDataStartDate){
			startDate = moment(postDataStartDate);
		}
		return startDate.format(dateTimeFormats.dateOnly);
	}

	function _buildPostData() {
		var periodStart = moment(_getStartDateForPostData() + " " + self.StartTime());
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

	function _buildDateFromSubscription(){
		self.DateFromChangeSubscription = self.DateFrom.subscribe(self.GetDefaultStartTime);
	}

	function _buildStartTimeSubscription(){
		self.StartTimeChangeSubscription = self.StartTime.subscribe(function(newValue){
			postDataStartDate = moment(self.DateFrom()).format(dateOnlyFormat);
		});
	}

	function _initializeViewModel(){
		self.ShowError(false);
		self.ShowCancelButton(true);
		self.TimeList = _createTimeList();
		self.RequestDuration(self.TimeList[0]);
		self.CancelAddRequest = parentViewModel.CancelAddingNewRequest;
		!isViewingDetail &&	_buildDateFromSubscription();
	}

	_initializeViewModel();
}
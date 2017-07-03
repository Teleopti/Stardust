Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel = function (ajax, requestListViewModel, requestDetailParentViewModel, weekStart) {
	var self = this;

	self.Template = "add-overtime-request-template";
	self.IsPostingData = ko.observable(false);
	self.IsEditable = ko.observable(true);

	self.Subject = ko.observable();
	self.Message = ko.observable();

	self.StartDate = ko.observable();
	self.StartTime = ko.observable();
	self.DateFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);

	self.weekStart = ko.observable(weekStart);
	self.ShowMeridian = $('div[data-culture-show-meridian]').attr('data-culture-show-meridian') === 'true';
	self.ShowError = ko.observable(false);
	self.ErrorMessage = ko.observable();
	self.RequestDuration = ko.observable();
	self.MultiplicatorDefinitionSetId = ko.observable();
	self.TimeList = _createTimeList();

	self.checkMessageLength = function(data, event) {
		var text = $(event.target)[0].value;
		if (text.length > 2000) {
			self.Message(text.substr(0, 2000));
		}
	};

	self.validateDuration = function(data, event) {
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

	self.AddRequest = function() {
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
				requestListViewModel.AddItemAtTop(data, true);
				self.CancelAddRequest();
			},
			error: function() {
				self.IsPostingData(false);
			}
		});
	};

	self.CancelAddRequest = function() {
		requestDetailParentViewModel.CancelAddingNewRequest();
	};

	self.Initialize = function (data) {
		if (data) {
			self.IsEditable = ko.observable(true);

			self.Subject = ko.observable(data.Subject);
			self.Message = ko.observable(data.Text);

			self.StartDate = ko.observable(moment(data.DateTimeFrom).format(self.DateFormat()));

			if (self.ShowMeridian) {
				self.StartTime = ko.observable(moment(data.DateTimeFrom).format("hh:mm A"));
			} else {
				self.StartTime = ko.observable(moment(data.DateTimeFrom).format("HH:mm"));
			}

			var seconds = (moment(data.DateTimeTo) - moment(data.DateTimeFrom)) / 1000;
			var hours = '0' + moment.duration(seconds, 'seconds').hours();
			var minutes = '0' + moment.duration(seconds, 'seconds').minutes();
			
			self.RequestDuration(hours.substr(-2, 2) + ":" + minutes.substr(-2, 2));
			self.MultiplicatorDefinitionSetId(data.MultiplicatorDefinitionSet);
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
		self.ShowError(false);
		self.ErrorMessage('');

		var result = true;
		if (!self.Subject()) {
			result = false;
			self.ShowError(true);
			self.ErrorMessage(messages.MissingSubject);
		}
		return result;
	}

	function _buildPostData() {
		var startDateMoment = moment(self.StartDate());
		var endDateMoment = moment(self.StartDate()).startOf('day');
		if (self.StartTime()) {
			endDateMoment = moment(startDateMoment.format(Teleopti.MyTimeWeb.Common.ServiceDateFormat) + ' ' + self.StartTime());
		}
		if (self.RequestDuration()) {
			endDateMoment.add(self.RequestDuration().split(':')[0], 'hours')
				.add(self.RequestDuration().split(':')[1], 'minutes');
		}

		var endDate = startDateMoment.format(Teleopti.MyTimeWeb.Common.ServiceDateFormat);
		if (endDateMoment.isAfter(startDateMoment.endOf('day'))) {
			endDate = endDateMoment.format(Teleopti.MyTimeWeb.Common.ServiceDateFormat);
		}

		return {
			Subject: self.Subject(),
			Message: self.Message(),
			MultiplicatorDefinitionSet: self.MultiplicatorDefinitionSetId(),
			Period: {
				StartDate: startDateMoment.format(Teleopti.MyTimeWeb.Common.ServiceDateFormat),
				StartTime: moment(self.StartDate() + ' ' + self.StartTime()).format('HH:mm'),
				EndDate: endDate,
				EndTime: endDateMoment.format('HH:mm')
			}
		};
	}
}
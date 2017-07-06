Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel = function (ajax, doneCallback, parentViewModel, weekStart) {
	var self = this;

	self.Id = ko.observable();
	self.Template = "add-overtime-request-template";
	self.IsPostingData = ko.observable(false);
	self.IsEditable = ko.observable(true);

	self.Subject = ko.observable();
	self.Message = ko.observable();

	self.DateFrom = ko.observable();
	self.StartTime = ko.observable();
	self.DateFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);

	self.weekStart = ko.observable(weekStart);
	self.ShowMeridian = $('div[data-culture-show-meridian]').attr('data-culture-show-meridian') === 'true';
	self.ShowError = ko.observable(false);
	self.ErrorMessage = ko.observable();
	self.ShowCancelButton = ko.observable(true);
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
				doneCallback(data);
				self.CancelAddRequest();
			},
			error: function() {
				self.IsPostingData(false);
			}
		});
	};

	self.CancelAddRequest = function() {
		parentViewModel.CancelAddingNewRequest();
	};

	self.Initialize = function (data) {
		if (data) {
			self.Id(data.Id);

			self.Subject(data.Subject);
			self.Message(data.Text);
			self.DateFrom(moment(data.DateTimeFrom));

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
		var dateTimeFormats = Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat;

		var startDate = self.DateFrom() ? moment(self.DateFrom()) : moment().startOf("day");
		var periodStart = moment(startDate.format(dateTimeFormats.dateOnly) + " " + self.StartTime());

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
}
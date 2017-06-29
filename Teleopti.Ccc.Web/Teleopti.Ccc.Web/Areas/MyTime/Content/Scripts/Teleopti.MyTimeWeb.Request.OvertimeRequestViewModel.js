Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel = function (ajax, requestListViewModel) {
	var self = this;

	self.Template = "add-overtime-request-template";
	self.IsPostingData = ko.observable(false);

	self.Subject = ko.observable();
	self.Message = ko.observable();

	self.DateFrom = ko.observable();
	self.DateTo = ko.observable();
	self.TimeFrom = ko.observable();
	self.TimeTo = ko.observable();
	self.DateFormat = ko.observable();

	self.weekStart = ko.observable(1);
	self.ShowMeridian = ko.observable(true);
	self.ShowError = ko.observable(false);
	self.ErrorMessage = ko.observable();
	self.RequestDuration = ko.observable();
	self.MultiplicatorDefinitionSets = ko.observableArray();
	self.MultiplicatorDefinitionSet = ko.observable();
	self.TimeList = [];

	self.checkMessageLength = function (data, event) {
		var text = $(event.target)[0].value;
		if (text.length > 2000) {
			self.Message(text.substr(0, 2000));
		}
	};

	self.validateDuration = function (data, event) {
		var duration = $(event.target)[0].value.split(':');
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
		var postData = {
			Subject: self.Subject(),
			Message: self.Message(),
			MultiplicatorDefinitionSet: self.MultiplicatorDefinitionSet().Id,
			Period: {
				StartDate: self.DateFrom(),
				StartTime: self.TimeFrom(),
				EndDate: self.DateTo(),
				EndTime: self.TimeTo()
			}
		};

		self.IsPostingData(true);

		ajax.Ajax({
			url: '../api/Request/AddOvertime',
			data: postData,
			success: function (data) {
				self.IsPostingData(false);
				requestListViewModel.AddItemAtTop(data, true);
			}
		});
	};

	_init();

	function _init() {
		_loadMultiplicatorDefinitionSets();
		self.TimeList = _createTimeList();
	}

	function _loadMultiplicatorDefinitionSets(){
		ajax.Ajax({
			url: "../api/MultiplicatorDefinitionSet/Mine",
			success: function (data) {
				self.MultiplicatorDefinitionSets(data);
			}
		});
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
}
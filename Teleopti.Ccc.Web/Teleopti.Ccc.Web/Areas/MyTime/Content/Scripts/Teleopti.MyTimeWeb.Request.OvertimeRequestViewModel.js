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

	//self.weekStart = ko.observable(1);
	//self.ShowMeridian = ko.observable(true);
	//self.ShowError = ko.observable(false);
	//self.ErrorMessage = ko.observable();
	self.MultiplicatorDefinitionSets = [];
	self.MultiplicatorDefinitionSet = ko.observable();

	self.checkMessageLength = function (data, event) {
		var text = $(event.target)[0].value;
		if (text.length > 2000) {
			self.Message(text.substr(0, 2000));
		}
	};

	self.AddRequest = function () {
		var postData = {
			Subject: self.Subject(),
			Message: self.Message(),
			MultiplicatorDefinitionSet: self.MultiplicatorDefinitionSet(),
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

	ajax.Ajax({
		url: "../api/MultiplicatorDefinitionSet/Mine",
		success: function (data) {
			self.MultiplicatorDefinitionSets = data;
		}
	});

}
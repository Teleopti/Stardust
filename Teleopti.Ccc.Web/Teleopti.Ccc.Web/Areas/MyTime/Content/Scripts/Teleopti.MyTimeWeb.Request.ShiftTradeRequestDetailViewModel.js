/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>

Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel = function (ajax) {

	var self = this;
	self.Id = ko.observable();
	self.IsUpdate = ko.observable(true);
	self.TypeEnum = ko.observable(2);
	self.IsFullDay = ko.observable(true);
	self.Template = ko.observable("shifttrade-request-detail-template");
	self.CanApprove = ko.observable(true);
	self.ajax = ajax;
	self.From = ko.observable("");
	self.To = ko.observable("");
	self.Approve = function () {
		self.respondToRequest("Requests/ApproveShiftTrade/" + self.Id());
		Teleopti.MyTimeWeb.Request.RequestDetail.FadeEditSection();
	};
	self.pixelPerMinute = ko.observable(0.3);
	self.Deny = function () {
		self.respondToRequest("Requests/DenyShiftTrade/" + self.Id());
		Teleopti.MyTimeWeb.Request.RequestDetail.FadeEditSection();
	};
	self.respondToRequest = function (url) {

		self.ajax.Ajax({
			url: url,
			dataType: "json",
			type: "POST",
			success: function (data) {
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data);
			},
			error: function (error) {
				//todo
			}
		});
	};

	//test:
	self.hours = ko.observableArray();
	self.hourWidth = ko.observable(10);
	self.henkeTest = function () {
		self.ajax.Ajax({
			url: "Requests/ShiftTradeRequestSwapDetails/" + self.Id(),
			dataType: "json",
			type: "POST",
			success: function (data) {
				console.log('Henke... det fungerar');
				console.log(data);

				self.hours.removeAll();
				for (var i = 0; i < data.TimeLineHours.length; i++) {
					console.log(data);
					self.hours.push(new TimeLineHourViewModel(data.TimeLineHours[i], self));
				}
			},
			error: function (error) {
				console.log('henke det fungerar inte......');
				console.log(error);
			}
		});
	};

};

//henke: separate class used by both addshifttrade and show shifttrade
function TimeLineHourViewModel(hour, parentViewModel) {
	var self = this;
	 self.borderSize = 1;

	 self.hourText = hour.HourText;
	self.lengthInMinutes = hour.LengthInMinutesToDisplay;
	self.leftPx = ko.observable('-8px');
	
	self.hourWidth = ko.computed(function () {
		return self.lengthInMinutes * parentViewModel.pixelPerMinute() - self.borderSize + 'px';
	});

}

ko.utils.extend(Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel.prototype, {
	Initialize: function (data) {
		
		var self = this;
				self.Id(data.Id);
				self.CanApprove(!data.IsCreatedByUser);
				self.From(data.From);
				self.To(data.To);
	}
});
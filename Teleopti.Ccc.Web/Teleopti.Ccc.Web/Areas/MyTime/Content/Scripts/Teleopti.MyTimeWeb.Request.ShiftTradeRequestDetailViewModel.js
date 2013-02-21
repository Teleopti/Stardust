/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>

Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel = function (ajax) {

	var self = this;
	self.Id = ko.observable();
	self.Subject = ko.observable();
	self.MessageText = ko.observable();
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
	function personScheduleViewModel(layers, scheduleObject) {
		var self = this;
		var minutesSinceTimeLineStart = 0;
		var agentName = '';
		var dayOffText = '';
		var hasUnderlyingDayOff = false;
		if (scheduleObject) {
			agentName = scheduleObject.Name;
			minutesSinceTimeLineStart = scheduleObject.MinutesSinceTimeLineStart;
			dayOffText = scheduleObject.DayOffText;
			hasUnderlyingDayOff = scheduleObject.HasUnderlyingDayOff;
		}

		self.agentName = agentName;
		self.layers = layers;
		self.minutesSinceTimeLineStart = minutesSinceTimeLineStart;
		self.dayOffText = dayOffText;
		self.hasUnderlyingDayOff = ko.observable(hasUnderlyingDayOff);
		self.showDayOffStyle = function () {
			if (self.hasUnderlyingDayOff() == true | self.dayOffText.length > 0) {
				return true;
			}
			return false;
		};

	}

	function layerViewModel(layer, minutesSinceTimeLineStart, pixelPerMinute) {
		var self = this;

		self.payload = layer.Payload;
		self.backgroundColor = layer.Color;
		self.lengthInMinutes = layer.LengthInMinutes;
		self.leftPx = ko.computed(function () {
			var timeLineoffset = minutesSinceTimeLineStart;
			return (layer.ElapsedMinutesSinceShiftStart + timeLineoffset) * pixelPerMinute + 'px';
		});
		self.paddingLeft = ko.computed(function () {
			return self.lengthInMinutes * pixelPerMinute + 'px';
		});
		self.title = ko.computed(function () {
			if (self.payload) {
				return layer.Title + ' ' + self.payload;
			}
			return '';
		});
	}

	self.mySchedule = ko.observable(new personScheduleViewModel());
	self.otherSchedule = ko.observable(new personScheduleViewModel());
	self.hours = ko.observableArray();
	self.hourWidth = ko.observable(10);
	self.loadSwapDetails = function () {
		self.ajax.Ajax({
			url: "Requests/ShiftTradeRequestSwapDetails/" + self.Id(),
			dataType: "json",
			type: "POST",
			success: function (data) {
			  
				self.hours.removeAll();
				for (var i = 0; i < data.TimeLineHours.length; i++) {

					self.hours.push(new TimeLineHourViewModel(data.TimeLineHours[i], self));
				}
				self._createMySchedule(data.From);
				self._createOtherSchedule(data.To);
				
			},
			error: function (error) {
				//todo
			}
		});
	};

	self._createMySchedule = function (myScheduleObject) {
		var mappedlayers = ko.utils.arrayMap(myScheduleObject.ScheduleLayers, function (layer) {
			return new layerViewModel(layer, myScheduleObject.MinutesSinceTimeLineStart, self.pixelPerMinute());
		});
		self.mySchedule(new personScheduleViewModel(mappedlayers, myScheduleObject));
	};

	self._createOtherSchedule = function (myScheduleObject) {
		var mappedlayers = ko.utils.arrayMap(myScheduleObject.ScheduleLayers, function (layer) {
			return new layerViewModel(layer, myScheduleObject.MinutesSinceTimeLineStart, self.pixelPerMinute());
		});
		self.otherSchedule(new personScheduleViewModel(mappedlayers, myScheduleObject));
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
		self.Subject(data.Subject);
		self.MessageText(data.Text);
		self.Id(data.Id);
		self.CanApprove(!data.IsCreatedByUser);
		self.From(data.From);
		self.To(data.To);
	}
});
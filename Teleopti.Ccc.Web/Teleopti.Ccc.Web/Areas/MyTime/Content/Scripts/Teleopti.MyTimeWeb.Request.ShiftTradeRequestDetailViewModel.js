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
			}
		});
	};
	self.mySchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleViewModel());
	self.otherSchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleViewModel());
	self.hours = ko.observableArray();
	self.hourWidth = ko.observable(10);
	self.loadSwapDetails = function () {
		self.ajax.Ajax({
			url: "Requests/ShiftTradeRequestSwapDetails/" + self.Id(),
			dataType: "json",
			type: "POST",
			success: function (data) {
				var numberOfShownHours = data.TimeLineHours.length;
				var showNumberRatio = Math.floor(numberOfShownHours / 24);
				self.pixelPerMinute(72 / (numberOfShownHours * 10));
				self.hours.removeAll();
				for (var i = 0; i < data.TimeLineHours.length; i++) {
					var timelineHour = new Teleopti.MyTimeWeb.Request.TimeLineHourViewModel(data.TimeLineHours[i], self);
					timelineHour.showLabel(!(i % showNumberRatio));
					self.hours.push(timelineHour);
				}
				self.createMySchedule(data.From);
				self.createOtherSchedule(data.To);

			}
		});
	};

	self.createMySchedule = function (myScheduleObject) {
		var mappedlayers = ko.utils.arrayMap(myScheduleObject.ScheduleLayers, function (layer) {
			return new Teleopti.MyTimeWeb.Request.LayerViewModel(layer, myScheduleObject.MinutesSinceTimeLineStart, self.pixelPerMinute());
		});
		self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleViewModel(mappedlayers, myScheduleObject));
	};

	self.createOtherSchedule = function (myScheduleObject) {
		var mappedlayers = ko.utils.arrayMap(myScheduleObject.ScheduleLayers, function (layer) {
			return new Teleopti.MyTimeWeb.Request.LayerViewModel(layer, myScheduleObject.MinutesSinceTimeLineStart, self.pixelPerMinute());
		});
		self.otherSchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleViewModel(mappedlayers, myScheduleObject));
	};

};

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

Teleopti.MyTimeWeb.Request.TimeLineHourViewModel = function (hour, parentViewModel) {
	var self = this;
	self.borderSize = 1;
	self.showLabel = ko.observable(true);
	self.hourText = hour.HourText;
	self.lengthInMinutes = hour.LengthInMinutesToDisplay;
	self.leftPx = ko.observable('-8px');

	self.hourWidth = ko.computed(function () {
		return self.lengthInMinutes * parentViewModel.pixelPerMinute() - self.borderSize + 'px';
	});

};

Teleopti.MyTimeWeb.Request.LayerViewModel = function(layer, minutesSinceTimeLineStart, pixelPerMinute) {
	var self = this;

	self.payload = layer.Payload;
	self.backgroundColor = layer.Color;
	self.lengthInMinutes = layer.LengthInMinutes;
	self.leftPx = ko.computed(function() {
		var timeLineoffset = minutesSinceTimeLineStart;
		return (layer.ElapsedMinutesSinceShiftStart + timeLineoffset) * pixelPerMinute + 'px';
	});
	self.paddingLeft = ko.computed(function() {
		return self.lengthInMinutes * pixelPerMinute + 'px';
	});
	self.title = ko.computed(function() {
		if (self.payload) {
			return layer.Title + ' ' + self.payload;
		}
		return '';
	});
};

Teleopti.MyTimeWeb.Request.PersonScheduleViewModel = function(layers, scheduleObject) {
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
	self.showDayOffStyle = function() {
		if (self.hasUnderlyingDayOff() == true | self.dayOffText.length > 0) {
			return true;
		}
		return false;
	};

};

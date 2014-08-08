/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>

Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel = function (ajax) {
	var self = this;
	self.DetailedDays = ko.observableArray();
	self.Link = ko.observable();
	self.EntityId = ko.observable();
	self.Subject = ko.observable();
	self.Message = ko.observable();
	self.IsUpdate = ko.observable(true);
	self.TypeEnum = ko.observable(2);
	self.IsFullDay = ko.observable(true);
    self.DateFrom = ko.observable(moment().startOf('day'));
    self.DateTo = ko.observable(moment().startOf('day'));
    self.TimeFrom = ko.observable();
    self.TimeTo = ko.observable();
    self.AbsenceId = ko.observable();
    self.DenyReason = ko.observable();
	self.Template = ko.observable("shifttrade-request-detail-template");
	self.IsTradeCreatedByMe = ko.observable(false);
	self.ajax = ajax;
	self.From = ko.observable("");
	self.To = ko.observable("");
	self.CanApproveAndDeny = ko.observable(true);
	self.IsSelected = ko.observable(false);
	self.IsEditable = ko.observable();
	self.IsNewInProgress = ko.observable(false);
	self.ToggleSelected = function () {
	    self.IsSelected(!self.IsSelected());
	};
	self.IsEditMessageEnabled = ko.observable(false);
	self.loadIsEditMessageEnabled = function () {
		ajax.Ajax({
			url: "../ToggleHandler/IsEnabled?toggle=Request_GiveCommentWhenDenyOrApproveShiftTradeRequest_28341",
			success: function (data) {
				if (data.IsEnabled) {
					self.IsEditMessageEnabled(true);
				}
			}
		});
	};
	self.UpdatedMessage = ko.observable();
	self.Approve = function () {
		self.CanApproveAndDeny(false);
		if (self.UpdatedMessage() != undefined) {
			if (self.Message().search(self.personFrom()) != 0) {
				self.Message(self.personFrom() + ": " + self.Message() + "\n" + self.personTo() + ": " + self.UpdatedMessage());
			} else {
				self.Message(self.Message() + "\n" + self.personTo() + ": " + self.UpdatedMessage());
			}
		}

		self.ajax.Ajax({
			url: "Requests/ApproveShiftTrade/",
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: "POST",
			data: JSON.stringify({
				ID: self.EntityId(),
				Message: self.Message()
			}),
			success: function (data) {
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data, true);
				self.CanApproveAndDeny(true);
				self.UpdatedMessage("");
			}
		});
	};

	self.Deny = function () {
		self.CanApproveAndDeny(false);

		if (self.UpdatedMessage() != undefined) {
			if (self.Message().search(self.personFrom()) != 0) {
				self.Message(self.personFrom() + ": " + self.Message() + "\n" + self.personTo() + ": " + self.UpdatedMessage());
			} else {
				self.Message(self.Message() + "\n" + self.personTo() + ": " + self.UpdatedMessage());
			}
		}

		self.ajax.Ajax({
			url: "Requests/DenyShiftTrade/",
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: "POST",
			data: JSON.stringify({
				ID: self.EntityId(),
				Message: self.Message()
			}),
			success: function (data) {
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data,false);
				self.CanApproveAndDeny(false);
			}
		});
	};
	self.reSend = function() {
		self.CanApproveAndDeny(false);
		self.ajax.Ajax({
			url: "Requests/ReSendShiftTrade/" + self.EntityId(),
			dataType: "json",
			cache: false,
			type: "POST",
			success: function (data) {
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data, true);
				self.isReferred(false);
				self.CanApproveAndDeny(true);
			}
		});
	};
	self.cancelReferred = function () {
		ko.eventAggregator.notifySubscribers( { id: self.id() }, 'cancel_request');
	};
	self.hourWidth = ko.observable(10);
    self.IsPending = ko.observable(false);
    self.showInfo = ko.observable(false);
	self.personFrom = ko.observable();
	self.personTo = ko.observable();
    self.loadSwapDetails = function () {
		self.ajax.Ajax({
		    url: "Requests/ShiftTradeRequestSwapDetails/" + self.EntityId(),
			dataType: "json",
			type: "GET",
			success: function (data) {
				self.readDataForDetailedDays(data);
			}
		});
    };

    self.readDataForDetailedDays = function (data) {
    	if (data.length > 0) {
			self.personFrom(data[0].PersonFrom);
			self.personTo(data[0].PersonTo);
	    }
	    ko.utils.arrayForEach(data, function (day) {
			var vm = new ShiftTradeRequestDetailedDayViewModel(day);
			self.DetailedDays.push(vm);
		});
	}

	self.id = ko.observable();
	self.isReferred = ko.observable(false);
	
};

ko.utils.extend(Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel.prototype, {
	Initialize: function (data) {
		var self = this;
		self.id(data.Id);
        self.showInfo(!data.IsPending);
		self.Subject(data.Subject);
		self.Message(data.Text);
		self.EntityId(data.Id);
		self.IsTradeCreatedByMe(data.IsCreatedByUser);
		self.From(data.From);
		self.To(data.To);
		self.IsPending(data.IsPending);
		self.isReferred(data.IsReferred);
	}
});

Teleopti.MyTimeWeb.Request.TimeLineHourEditShiftTradeViewModel = function (hour, pixelPerMinute) {
	var self = this;
	self.showLabel = ko.observable(true);
	self.hourText = hour.HourText;
	self.isTimeLineVisible = (hour.HourText.length > 0);
	self.leftPx = Teleopti.MyTimeWeb.Common.IsRtl() ? "17px" : "-17px";
	self.pixelPerMinute = ko.observable(pixelPerMinute);

	self.hourWidth = ko.computed(function () {
	    return hour.LengthInMinutesToDisplay * self.pixelPerMinute() + 'px';
	});
};

Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel = function (hour, timeLineStartTime, pixelPerMinute, isVisible) {
    var self = this;
    self.hourText = hour.HourText;
    self.startTime = moment(hour.StartTime);
    self.timeLineStartTime = ko.observable(timeLineStartTime);
    self.pixelPerMinute = ko.observable(pixelPerMinute);
	self.isVisible = ko.observable(isVisible);

	self.leftPos = ko.computed(function () {
		var minutesSinceTimeLineStart = self.startTime.diff(self.timeLineStartTime(), 'minutes');
		return minutesSinceTimeLineStart * self.pixelPerMinute();
    });
	
    var isRtl = Teleopti.MyTimeWeb.Common.IsRtl();
	
    self.rulerStyleJson = ko.computed(function () {
    	if (isRtl)
    		return { 'right': self.leftPos() + 'px' };
	    return { 'left': self.leftPos() + 'px' };
    });
    self.labelStyleJson = ko.computed(function () {
	    if (isRtl)
	    	return { 'right': self.leftPos() - 16 + 'px' };
    	return { 'left': self.leftPos() - 16 + 'px' };
    });

    self.showHourLine = ko.computed(function () {
        return self.hourText.length > 0;
    });
};

Teleopti.MyTimeWeb.Request.CloneTimeLineHourAddShiftTradeViewModel = function (hour, timeLineStartTime, pixelPerMinute) {
    var self = this;
    self.hourText = hour.hourText;
    self.startTime = hour.startTime;
    self.timeLineStartTime = ko.observable(timeLineStartTime);
    self.pixelPerMinute = ko.observable(pixelPerMinute);
    self.isVisible = ko.observable(hour.isVisible());

	self.leftPos = ko.computed(function () {
		var minutesSinceTimeLineStart = self.startTime.diff(self.timeLineStartTime(), 'minutes');
		return minutesSinceTimeLineStart * self.pixelPerMinute();
    });
	
    var isRtl = Teleopti.MyTimeWeb.Common.IsRtl();
	
    self.rulerStyleJson = ko.computed(function () {
    	if (isRtl)
    		return { 'right': self.leftPos() + 'px' };
	    return { 'left': self.leftPos() + 'px' };
    });
    self.labelStyleJson = ko.computed(function () {
	    if (isRtl)
	    	return { 'right': self.leftPos() - 16 + 'px' };
    	return { 'left': self.leftPos() - 16 + 'px' };
    });

    self.showHourLine = ko.computed(function () {
        return self.hourText.length > 0;
    });
};

Teleopti.MyTimeWeb.Request.LayerEditShiftTradeViewModel = function (layer, minutesSinceTimeLineStart, pixelPerMinute) {
	var self = this;
	self.payload = layer.Payload;
	self.backgroundColor = layer.Color;
	self.pixelPerMinute = ko.observable(pixelPerMinute);
	self.leftPx = ko.computed(function () {
		var timeLineoffset = minutesSinceTimeLineStart;
		return (layer.ElapsedMinutesSinceShiftStart + timeLineoffset) * self.pixelPerMinute() + 'px';
	});
	self.widthPx = ko.computed(function () {
		return layer.LengthInMinutes * self.pixelPerMinute() + 'px';
	});
	self.title = ko.computed(function () {
		return self.payload ? "<div>{0}</div>{1}".format(self.payload, layer.TitleTime) : '';
	});
	var isRtl = Teleopti.MyTimeWeb.Common.IsRtl();
	self.styleJson = ko.computed(function () {
		if (isRtl)
			return { 'right': self.leftPx(), 'backgroundColor': self.backgroundColor, 'paddingRight': self.widthPx() };
		return { 'left': self.leftPx(), 'backgroundColor': self.backgroundColor, 'paddingLeft': self.widthPx() };
	});
};

Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel = function (layer, minutesSinceTimeLineStart, pixelPerMinute) {
    var self = this;
    self.payload = layer.Payload;
    self.backgroundColor = layer.Color;
    self.pixelPerMinute = ko.observable(pixelPerMinute);
    self.minutesSinceTimeLineStart = ko.observable(minutesSinceTimeLineStart);
    self.lengthInMinutes = ko.observable(layer.LengthInMinutes);
	self.titleHeader = layer.TitleHeader;
	self.titleTime = layer.TitleTime;
	self.startTime = layer.Start;
    self.leftPx = ko.computed(function () {
    	return (self.minutesSinceTimeLineStart() * self.pixelPerMinute()) + 'px';
    });
    self.widthPx = ko.computed(function () {
    	return self.lengthInMinutes() * self.pixelPerMinute() + 'px';
    });
    self.tooltipText = ko.computed(function () {
        return "<div>{0}</div>{1}".format(layer.TitleHeader, layer.TitleTime);
    });
    var isRtl = Teleopti.MyTimeWeb.Common.IsRtl();
    self.styleJson = ko.computed(function () {
    	if (isRtl)
    		return { 'right': self.leftPx(), 'backgroundColor': self.backgroundColor, 'paddingRight': self.widthPx() };
    	return { 'left': self.leftPx(), 'backgroundColor': self.backgroundColor, 'paddingLeft': self.widthPx() };
    });
};

Teleopti.MyTimeWeb.Request.CloneLayerAddShiftTradeViewModel = function (layer, minutesSinceTimeLineStart) {
	var self = this;
	self.payload = layer.payload;
	self.backgroundColor = layer.backgroundColor;
	self.pixelPerMinute = ko.observable(layer.pixelPerMinute());
	self.minutesSinceTimeLineStart = ko.observable(minutesSinceTimeLineStart);
	self.lengthInMinutes = ko.observable(layer.lengthInMinutes());
	self.leftPx = ko.computed(function () {
		return (self.minutesSinceTimeLineStart() * self.pixelPerMinute()) + 'px';
	});
	self.widthPx = ko.computed(function () {
		return layer.lengthInMinutes() * self.pixelPerMinute() + 'px';
	});
	self.tooltipText = ko.computed(function () {
		return "<div>{0}</div>{1}".format(layer.titleHeader, layer.titleTime);
	});
	var isRtl = Teleopti.MyTimeWeb.Common.IsRtl();
	self.styleJson = ko.computed(function () {
		if (isRtl)
			return { 'right': self.leftPx(), 'backgroundColor': self.backgroundColor, 'paddingRight': self.widthPx() };
		return { 'left': self.leftPx(), 'backgroundColor': self.backgroundColor, 'paddingLeft': self.widthPx() };
	});
};

Teleopti.MyTimeWeb.Request.PersonScheduleEditShiftTradeViewModel = function(layers, scheduleObject) {
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

Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel = function (layers, scheduleStartTime, scheduleEndTime, agentName, personId, isDayOff) {
	var self = this;

	self.scheduleStartTime = ko.observable(scheduleStartTime);
	self.scheduleEndTime = ko.observable(scheduleEndTime);
	self.personId = personId;
	self.isVisible = ko.observable(true);
	self.agentName = agentName;
	self.layers = layers;
	self.isDayOff = isDayOff;
};

ShiftTradeRequestDetailedDayViewModel = function(data) {
	var self = this;

	self.mySchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleEditShiftTradeViewModel());
	self.otherSchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleEditShiftTradeViewModel());

	self.pixelPerMinute = function () { return (72 / (data.TimeLineHours.length * 10)); }
	self.hours = ko.observableArray();
	for (var i = 0; i < data.TimeLineHours.length; i++) {
		var numberOfShownHours = data.TimeLineHours.length;
		var showNumberRatio = numberOfShownHours > 8 ? Math.round(numberOfShownHours / 8) : 1;
		var timelineHour = new Teleopti.MyTimeWeb.Request.TimeLineHourEditShiftTradeViewModel(data.TimeLineHours[i], self.pixelPerMinute());
		timelineHour.showLabel(!(i % showNumberRatio) && timelineHour.hourText.length > 0);
		self.hours.push(timelineHour);
	}

	self.createMySchedule = function (myScheduleObject) {
		var mappedlayers = ko.utils.arrayMap(myScheduleObject.ScheduleLayers, function (layer) {
			return new Teleopti.MyTimeWeb.Request.LayerEditShiftTradeViewModel(layer, myScheduleObject.MinutesSinceTimeLineStart, self.pixelPerMinute());
		});
		self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleEditShiftTradeViewModel(mappedlayers, myScheduleObject));
	};
	self.createOtherSchedule = function (myScheduleObject) {
		var mappedlayers = ko.utils.arrayMap(myScheduleObject.ScheduleLayers, function (layer) {
			return new Teleopti.MyTimeWeb.Request.LayerEditShiftTradeViewModel(layer, myScheduleObject.MinutesSinceTimeLineStart, self.pixelPerMinute());
		});
		self.otherSchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleEditShiftTradeViewModel(mappedlayers, myScheduleObject));
	};

	var dateFormat = $("#Request-detail-datepicker-format").val().toUpperCase();
	self.TradeDate = ko.observable(moment(data.Date).format(dateFormat));

	self.createMySchedule(data.From);
	self.createOtherSchedule(data.To);
}

Teleopti.MyTimeWeb.Request.ChooseHistoryViewModel = function(chooseHistory, canvasPixelWidth) {
	var self = this;
	self.agentName = chooseHistory.tradedSchedule.agentName;
	var dateFormat = $("#Request-detail-datepicker-format").val().toUpperCase();
	self.selectedDateInFormat = ko.observable(moment(chooseHistory.date).format(dateFormat));
	self.selectedDate = ko.observable(chooseHistory.date);
	self.timeLineStartTime = ko.observable();
	self.timeLineLengthInMinutes = ko.observable();
	self.pixelWidth = ko.observable(canvasPixelWidth);
	self.canvasPixelWidth = ko.computed({
		read: function() {
			return self.pixelWidth();
		},
		write: function(width) {
			var canvasWidth;
			if (width != null) {
				canvasWidth = width;
			} else {
				canvasWidth = $("td.the-other-agent-schedule").width();
			}
			self.pixelWidth(canvasWidth);
		}
	});


	self.setTimeLineLengthInMinutes = function(firstHour, lastHour) {
		self.timeLineStartTime(firstHour);
		self.timeLineLengthInMinutes(lastHour.diff(firstHour, 'minutes'));
	};


	self.pixelPerMinute = ko.computed(function() {
		return self.canvasPixelWidth() / self.timeLineLengthInMinutes();
	});

	self.hours = ko.computed(function() {
		var allHours = chooseHistory.hours;

		if (chooseHistory.mySchedule != undefined && chooseHistory.tradedSchedule != undefined) {
			var mySchedule = chooseHistory.mySchedule;
			var selectedSchedule = chooseHistory.tradedSchedule;
			var scheduleStartTime;
			var scheduleEndTime;

			if (mySchedule.isDayOff && !selectedSchedule.isDayOff) {
				scheduleStartTime = selectedSchedule.scheduleStartTime();
				scheduleEndTime = selectedSchedule.scheduleEndTime();
			} else if (!mySchedule.isDayOff && selectedSchedule.isDayOff) {
				scheduleStartTime = mySchedule.scheduleStartTime();
				scheduleEndTime = mySchedule.scheduleEndTime();
			} else if (mySchedule.isDayOff && selectedSchedule.isDayOff) {
				return allHours;
			} else {
				scheduleStartTime = mySchedule.scheduleStartTime() < selectedSchedule.scheduleStartTime() ? mySchedule.scheduleStartTime() : selectedSchedule.scheduleStartTime();
				scheduleEndTime = selectedSchedule.scheduleEndTime() > mySchedule.scheduleEndTime() ? selectedSchedule.scheduleEndTime() : mySchedule.scheduleEndTime();
			}

			self.setTimeLineLengthInMinutes(scheduleStartTime, scheduleEndTime);

			var truncatedHours = [];
			$.each(allHours, function(index, hour) {
				if ((hour.startTime >= scheduleStartTime) && (hour.startTime <= scheduleEndTime)) {
					var newHour = new Teleopti.MyTimeWeb.Request.CloneTimeLineHourAddShiftTradeViewModel(hour, scheduleStartTime, self.pixelPerMinute());
					truncatedHours.push(newHour);
				}
			});
			if (truncatedHours.length < 18) {
				$.each(truncatedHours, function(index, hour) {
					hour.isVisible(true);
				});
			}
			return truncatedHours;
		} else {
			return allHours;
		}
	});


	self.mySchedule = ko.computed(function() {
		$.each(chooseHistory.mySchedule.layers, function(index, vmScheduleAddShiftTrade) {
			vmScheduleAddShiftTrade.pixelPerMinute(self.pixelPerMinute());
		});
		return chooseHistory.mySchedule;
	});

	self.selectedSchedule = ko.computed(function() {
		$.each(chooseHistory.tradedSchedule.layers, function(index, vmScheduleAddShiftTrade) {
			vmScheduleAddShiftTrade.pixelPerMinute(self.pixelPerMinute());
		});
		return chooseHistory.tradedSchedule;
	});
};

Teleopti.MyTimeWeb.Request.PageView = function(pageIndex) {
	var self = this;
	self.index = ko.observable(pageIndex);
	self.isSelected = ko.observable(false);
};

Teleopti.MyTimeWeb.Request.FilterStartTimeView = function(text, start, end, checked) {
	var self = this;
	self.text = text;
	self.start = start;
	self.end = end;
	self.isChecked = ko.observable(checked || false);
};

Teleopti.MyTimeWeb.Request.FilterEndTimeView = function (text, start, end, checked) {
	var self = this;
	self.text = text;
	self.start = start;
	self.end = end;
	self.isChecked = ko.observable(checked || false);
};
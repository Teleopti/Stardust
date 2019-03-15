Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel = function(ajax) {
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
	self.Template = ko.observable('shifttrade-request-detail-template');
	self.IsTradeCreatedByMe = ko.observable(false);
	self.ajax = ajax;
	self.From = ko.observable('');
	self.To = ko.observable('');
	self.CanApproveAndDeny = ko.observable(true);
	self.IsSelected = ko.observable(false);
	self.IsEditable = ko.observable();
	self.IsNewInProgress = ko.observable(false);
	self.ToggleSelected = function() {
		self.IsSelected(!self.IsSelected());
	};
	self.IsEditMessageEnabled = ko.observable(true);
	self.loadIsEditMessageEnabled = function() {
		self.IsEditMessageEnabled();
	};
	self.isAnonymousTrading = ko.observable(false);
	self.setMiscSetting = function(id) {
		if (id !== null) {
			self.ajax.Ajax({
				url: 'Requests/GetShiftTradeRequestMiscSetting',
				dataType: 'json',
				type: 'GET',
				data: {
					ID: id
				},
				success: function(data) {
					self.isAnonymousTrading(data.AnonymousTrading);
				}
			});
		}
	};

	self.UpdatedMessage = ko.observable();

	self.checkMessageLength = function(data, event) {
		var text = $(event.target)[0].value;
		if (text.length > 2000) {
			self.Message(text.substr(0, 2000));
		}
	};

	self.PrefixAgentNameToMessage = function() {
		var existingMsg = self.Message();
		var newMsg = self.UpdatedMessage();
		var lineCount = existingMsg.split('\n').length;
		var personFromName = self.personFrom();
		var personToName = self.personTo();

		// should we prefix the first msg with the from agent name?
		if (existingMsg !== '' && existingMsg[0] !== '[' && lineCount === 1) {
			self.Message('[' + personFromName + '] ' + existingMsg);
		}

		if (newMsg !== undefined && newMsg !== '') {
			self.Message(self.Message() + '\n[' + personToName + '] ' + newMsg);
		}
	};

	self.Approve = function() {
		self.CanApproveAndDeny(false);
		self.PrefixAgentNameToMessage();

		self.ajax.Ajax({
			url: 'Requests/ApproveShiftTrade',
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			data: JSON.stringify({
				ID: self.EntityId(),
				Message: self.Message()
			}),
			success: function(data) {
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data, true);
				self.CanApproveAndDeny(false);
				self.UpdatedMessage('');
			},
			error: function(response, textStatus) {
				if (response.responseJSON) {
					Teleopti.MyTimeWeb.Common.AjaxFailed(response, null, textStatus);
				}
				self.CanApproveAndDeny(true);
			}
		});
	};

	self.Deny = function() {
		self.CanApproveAndDeny(false);
		self.PrefixAgentNameToMessage();

		self.ajax.Ajax({
			url: 'Requests/DenyShiftTrade',
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			data: JSON.stringify({
				ID: self.EntityId(),
				Message: self.Message()
			}),
			success: function(data) {
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data, false);
				self.CanApproveAndDeny(false);
			},
			error: function(response, textStatus) {
				if (response.responseJSON) {
					Teleopti.MyTimeWeb.Common.AjaxFailed(response, null, textStatus);
				}
				self.CanApproveAndDeny(true);
			}
		});
	};
	self.reSend = function() {
		self.CanApproveAndDeny(false);
		self.ajax.Ajax({
			url: 'Requests/ReSendShiftTrade/' + self.EntityId(),
			dataType: 'json',
			cache: false,
			type: 'POST',
			success: function(data) {
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data, true);
				self.isReferred(false);
				self.CanApproveAndDeny(true);
			},
			error: function(response, textStatus) {
				if (response.responseJSON) {
					Teleopti.MyTimeWeb.Common.AjaxFailed(response, null, textStatus);
				}
				self.CanApproveAndDeny(true);
			}
		});
	};
	self.cancelReferred = function() {
		ko.eventAggregator.notifySubscribers({ id: self.id() }, 'cancel_request');
	};
	self.hourWidth = ko.observable(10);
	self.IsPending = ko.observable(false);
	self.showInfo = ko.observable(false);
	self.personFrom = ko.observable();
	self.personTo = ko.observable();
	self.loadSwapDetails = function() {
		self.ajax.Ajax({
			url: 'Requests/ShiftTradeRequestSwapDetails/' + self.EntityId(),
			dataType: 'json',
			type: 'GET',
			success: function(data) {
				self.readDataForDetailedDays(data);
			}
		});
	};

	self.readDataForDetailedDays = function(data) {
		if (data.length > 0) {
			self.personFrom(data[0].PersonFrom);
			self.personTo(data[0].PersonTo);
		}
		ko.utils.arrayForEach(data, function(day) {
			self.DetailedDays.push(new ShiftTradeRequestDetailedDayViewModel(day));
		});
	};

	self.id = ko.observable();
	self.isReferred = ko.observable(false);
};

ko.utils.extend(Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel.prototype, {
	Initialize: function(data) {
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
		self.DenyReason('');
	}
});

Teleopti.MyTimeWeb.Request.TimeLineHourEditShiftTradeViewModel = function(hour, totalWidthInMinutes) {
	var self = this;
	self.showLabel = ko.observable(true);
	self.hourText = hour.HourText;
	self.isTimeLineVisible = hour.HourText.length > 0;
	self.leftPx = Teleopti.MyTimeWeb.Common.IsRtl() ? '17px' : '-17px';

	self.hourWidth = ko.computed(function() {
		return (hour.LengthInMinutesToDisplay / totalWidthInMinutes) * 100 + '%';
	});
};

Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel = function(
	index,
	hour,
	baseDiff,
	pixelPerMinute,
	isVisible
) {
	var self = this;
	self.hourText = hour.HourText;
	self.startTime = moment(hour.StartTime);
	self.pixelPerMinute = ko.observable(pixelPerMinute);
	self.isVisible = ko.observable(isVisible);

	self.leftPos = ko.computed(function() {
		var minutesSinceTimeLineStart;
		if (index === 0) {
			minutesSinceTimeLineStart = 0;
		} else {
			//use index calculate timeline in consider daylight saving case
			minutesSinceTimeLineStart = baseDiff + (index - 1) * 60;
		}
		return minutesSinceTimeLineStart * self.pixelPerMinute();
	});

	var isRtl = Teleopti.MyTimeWeb.Common.IsRtl();

	self.rulerStyleJson = ko.computed(function() {
		if (isRtl) return { right: self.leftPos() + 'px' };
		return { left: self.leftPos() + 'px' };
	});
	self.labelStyleJson = ko.computed(function() {
		if (isRtl) return { right: self.leftPos() - 16 + 'px' };
		return { left: self.leftPos() - 16 + 'px' };
	});

	self.showHourLine = ko.computed(function() {
		return self.hourText.length > 0;
	});
};

Teleopti.MyTimeWeb.Request.CloneTimeLineHourAddShiftTradeViewModel = function(index, hour, baseDiff, pixelPerMinute) {
	var self = this;
	self.hourText = hour.hourText;
	//self.startTime = hour.startTime;
	//self.timeLineStartTime = ko.observable(timeLineStartTime);
	self.pixelPerMinute = ko.observable(pixelPerMinute);
	self.isVisible = ko.observable(hour.isVisible());

	self.leftPos = ko.computed(function() {
		var minutesSinceTimeLineStart;
		if (index === 0) {
			minutesSinceTimeLineStart = 0;
		} else {
			//use index calculate timeline in consider daylight saving case
			minutesSinceTimeLineStart = baseDiff + (index - 1) * 60;
		}

		return minutesSinceTimeLineStart * self.pixelPerMinute();
	});

	var isRtl = Teleopti.MyTimeWeb.Common.IsRtl();

	self.rulerStyleJson = ko.computed(function() {
		if (isRtl) return { right: self.leftPos() + 'px' };
		return { left: self.leftPos() + 'px' };
	});
	self.labelStyleJson = ko.computed(function() {
		if (isRtl) return { right: self.leftPos() - 16 + 'px' };
		return { left: self.leftPos() - 16 + 'px' };
	});

	self.showHourLine = ko.computed(function() {
		return self.hourText.length > 0;
	});
};

Teleopti.MyTimeWeb.Request.LayerEditShiftTradeViewModel = function(
	layer,
	minutesSinceTimeLineStart,
	totalWidthInMinutes
) {
	var self = this;
	self.payload = layer.Payload;
	self.backgroundColor = layer.Color;
	self.leftPercentage = ko.computed(function() {
		var timeLineoffset = minutesSinceTimeLineStart;
		return (100 * (layer.ElapsedMinutesSinceShiftStart + timeLineoffset)) / totalWidthInMinutes + '%';
	});
	self.widthPercentage = ko.computed(function() {
		return (100 * layer.LengthInMinutes) / totalWidthInMinutes + '%';
	});
	self.title = ko.computed(function() {
		return self.payload ? '<div>{0}</div>{1}'.format(self.payload, layer.TitleTime) : '';
	});
	self.styleJson = ko.computed(function() {
		if (Teleopti.MyTimeWeb.Common.IsRtl())
			return {
				right: self.leftPercentage(),
				backgroundColor: Teleopti.MyTimeWeb.Common.ConvertColorToRGB(self.backgroundColor),
				width: self.widthPercentage()
			};
		return {
			left: self.leftPercentage(),
			backgroundColor: Teleopti.MyTimeWeb.Common.ConvertColorToRGB(self.backgroundColor),
			width: self.widthPercentage()
		};
	});
};

Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel = function(
	layer,
	minutesSinceTimeLineStart,
	pixelPerMinute,
	layerOffset,
	shiftLength
) {
	var self = this;
	self.payload = layer.Payload;
	self.backgroundColor = layer.Color;
	self.pixelPerMinute = ko.observable(pixelPerMinute);
	self.minutesSinceTimeLineStart = ko.observable(minutesSinceTimeLineStart);
	self.lengthInMinutes = ko.observable(layer.LengthInMinutes);
	self.titleHeader = layer.TitleHeader;
	self.titleTime = layer.TitleTime;
	self.startTime = layer.Start;
	self.isOvertime = layer.IsOvertime;
	self.leftPx = ko.computed(function() {
		return self.minutesSinceTimeLineStart() * self.pixelPerMinute() + 'px';
	});
	self.leftPercentage = ko.computed(function() {
		return (100 * layerOffset) / shiftLength + '%';
	});
	self.widthPx = ko.computed(function() {
		return self.lengthInMinutes() * self.pixelPerMinute() + 'px';
	});
	self.widthPercentage = ko.computed(function() {
		return (100 * self.lengthInMinutes()) / shiftLength + '%';
	});
	self.tooltipText = '<div>{0}</div>{1}'.format(layer.TitleHeader, layer.TitleTime);

	var isRtl = Teleopti.MyTimeWeb.Common.IsRtl();
	self.styleJson = ko.computed(function() {
		if (isRtl)
			return {
				right: self.leftPx(),
				'background-size': self.isOvertime ? '11px 11px' : 'initial',
				'background-image': self.isOvertime
					? 'linear-gradient(45deg,transparent,transparent 4px,rgba(251,251,251,.8) 6px,transparent 10px,transparent)'
					: '',
				'background-color': Teleopti.MyTimeWeb.Common.ConvertColorToRGB(self.backgroundColor),
				paddingRight: self.widthPx()
			};
		return {
			left: self.leftPx(),
			'background-size': self.isOvertime ? '11px 11px' : 'initial',
			'background-image': self.isOvertime
				? 'linear-gradient(45deg,transparent,transparent 4px,rgba(251,251,251,.8) 6px,transparent 10px,transparent)'
				: 'initial',
			'background-color': Teleopti.MyTimeWeb.Common.ConvertColorToRGB(self.backgroundColor),
			paddingLeft: self.widthPx()
		};
	});
	self.styleJson74947 = ko.computed(function() {
		if (isRtl)
			return {
				right: self.leftPercentage(),
				'background-size': self.isOvertime ? '11px 11px' : 'initial',
				'background-image': self.isOvertime
					? 'linear-gradient(45deg,transparent,transparent 4px,rgba(251,251,251,.8) 6px,transparent 10px,transparent)'
					: '',
				'background-color': Teleopti.MyTimeWeb.Common.ConvertColorToRGB(self.backgroundColor),
				width: self.widthPercentage()
			};
		return {
			left: self.leftPercentage(),
			'background-size': self.isOvertime ? '11px 11px' : 'initial',
			'background-image': self.isOvertime
				? 'linear-gradient(45deg,transparent,transparent 4px,rgba(251,251,251,.8) 6px,transparent 10px,transparent)'
				: 'initial',
			'background-color': Teleopti.MyTimeWeb.Common.ConvertColorToRGB(self.backgroundColor),
			width: self.widthPercentage()
		};
	});
};

Teleopti.MyTimeWeb.Request.CloneLayerAddShiftTradeViewModel = function(layer, minutesSinceTimeLineStart) {
	var self = this;
	self.payload = layer.payload;
	self.backgroundColor = layer.backgroundColor;
	self.pixelPerMinute = ko.observable(layer.pixelPerMinute());
	self.minutesSinceTimeLineStart = ko.observable(minutesSinceTimeLineStart);
	self.lengthInMinutes = ko.observable(layer.lengthInMinutes());
	self.titleHeader = layer.titleHeader;
	self.isOvertime = layer.isOvertime;

	self.leftPx = ko.computed(function() {
		return self.minutesSinceTimeLineStart() * self.pixelPerMinute() + 'px';
	});
	self.widthPx = ko.computed(function() {
		return layer.lengthInMinutes() * self.pixelPerMinute() + 'px';
	});
	self.tooltipText = '<div>{0}</div>{1}'.format(layer.titleHeader, layer.titleTime);

	var isRtl = Teleopti.MyTimeWeb.Common.IsRtl();
	self.styleJson = ko.computed(function() {
		if (isRtl)
			return {
				right: self.leftPx(),
				'background-size': self.isOvertime ? '11px 11px' : 'initial',
				'background-image': self.isOvertime
					? 'linear-gradient(45deg,transparent,transparent 4px,rgba(251,251,251,.8) 6px,transparent 10px,transparent)'
					: '',
				'background-color': Teleopti.MyTimeWeb.Common.ConvertColorToRGB(self.backgroundColor),
				paddingRight: self.widthPx()
			};
		return {
			left: self.leftPx(),
			'background-size': self.isOvertime ? '11px 11px' : 'initial',
			'background-image': self.isOvertime
				? 'linear-gradient(45deg,transparent,transparent 4px,rgba(251,251,251,.8) 6px,transparent 10px,transparent)'
				: 'initial',
			'background-color': Teleopti.MyTimeWeb.Common.ConvertColorToRGB(self.backgroundColor),
			paddingLeft: self.widthPx()
		};
	});
};

Teleopti.MyTimeWeb.Request.PersonScheduleEditShiftTradeViewModel = function(layers, scheduleObject) {
	var self = this;
	var minutesSinceTimeLineStart = 0;
	var agentName = '';
	var dayOffText = '';
	var hasUnderlyingDayOff = false;
	var isMySchedule = false;
	if (scheduleObject) {
		agentName = scheduleObject.Name;
		minutesSinceTimeLineStart = scheduleObject.MinutesSinceTimeLineStart;
		dayOffText = scheduleObject.DayOffText;
		hasUnderlyingDayOff = scheduleObject.HasUnderlyingDayOff;
		isMySchedule = scheduleObject.IsMySchedule;
	}

	self.agentName = agentName;
	self.layers = layers;
	self.minutesSinceTimeLineStart = minutesSinceTimeLineStart;
	self.dayOffText = dayOffText;
	self.hasUnderlyingDayOff = ko.observable(hasUnderlyingDayOff);
	self.showDayOffStyle = function() {
		if ((self.hasUnderlyingDayOff() === true) | (self.dayOffText.length > 0)) {
			return true;
		}
		return false;
	};
	self.isMySchedule = isMySchedule;
};

Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel = function(
	layers,
	scheduleStartTime,
	scheduleEndTime,
	agentName,
	personId,
	isDayOff,
	dayOffName,
	isEmptyDay,
	isFullDayAbsence,
	offerId,
	contractTime,
	isNotScheduled,
	scheduleDate,
	categoryName,
	categoryColor,
	startTimeString,
	endTimeString,
	hasOvertime,
	isIntradayAbsence,
	absenceColor,
	absenceCategoryShortName,
	overtimeCategoryColor,
	contractMinutes,
	start,
	end
) {
	var self = this;

	self.scheduleStart = startTimeString;
	self.scheduleEnd = endTimeString;
	self.scheduleStartTime = ko.observable(scheduleStartTime);
	self.scheduleEndTime = ko.observable(scheduleEndTime);
	self.personId = personId;
	self.isVisible = ko.observable(true);
	self.agentName = agentName;
	self.layers = layers;
	self.isDayOff = isDayOff;
	self.dayOffName = isDayOff ? dayOffName : '';
	self.isEmptyDay = isEmptyDay;
	self.isFullDayAbsence = isFullDayAbsence;
	self.ShiftExchangeOfferId = offerId;
	self.contractTime = contractTime === undefined ? '0:00' : contractTime;
	self.isNotScheduled = isNotScheduled;
	self.date = scheduleDate;
	self.categoryName = categoryName;
	self.categoryColor = categoryColor;
	self.hasOvertime = hasOvertime;
	self.isIntradayAbsence = isIntradayAbsence;
	self.absenceColor = absenceColor;
	self.absenceCategoryShortName = absenceCategoryShortName;
	self.overtimeCategoryColor = overtimeCategoryColor;
	self.contractMinutes = contractMinutes;
	self.start = start;
	self.end = end;
};

function ShiftTradeRequestDetailedDayViewModel(data) {
	var self = this;

	self.mySchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleEditShiftTradeViewModel());
	self.otherSchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleEditShiftTradeViewModel());

	var fromScheuduleLayers = data.From.ScheduleLayers;
	var fromScheuduleLengthInMinutes = 0;
	if (fromScheuduleLayers && fromScheuduleLayers.length > 0) {
		fromScheuduleLengthInMinutes =
			fromScheuduleLayers[fromScheuduleLayers.length - 1].ElapsedMinutesSinceShiftStart +
			fromScheuduleLayers[fromScheuduleLayers.length - 1].LengthInMinutes;
	}

	var toScheuduleLayers = data.To.ScheduleLayers;
	var toScheuduleLengthInMinutes = 0;
	if (toScheuduleLayers && toScheuduleLayers.length > 0) {
		toScheuduleLengthInMinutes =
			toScheuduleLayers[toScheuduleLayers.length - 1].ElapsedMinutesSinceShiftStart +
			toScheuduleLayers[toScheuduleLayers.length - 1].LengthInMinutes;
	}

	var maxScheduleLengthInMinutes =
		Math.max(fromScheuduleLengthInMinutes, toScheuduleLengthInMinutes) +
		Math.max(data.From.MinutesSinceTimeLineStart, data.To.MinutesSinceTimeLineStart);

	var totalWidthInMinutes = Math.max(maxScheduleLengthInMinutes, data.TimeLineHours.length * 60);

	self.hours = ko.observableArray();
	for (var i = 0; i < data.TimeLineHours.length; i++) {
		var numberOfShownHours = data.TimeLineHours.length;
		var showNumberRatio = numberOfShownHours > 8 ? Math.round(numberOfShownHours / 8) : 1;
		var timelineHour = new Teleopti.MyTimeWeb.Request.TimeLineHourEditShiftTradeViewModel(
			data.TimeLineHours[i],
			totalWidthInMinutes
		);
		timelineHour.showLabel(!(i % showNumberRatio) && timelineHour.hourText.length > 0);
		self.hours.push(timelineHour);
	}

	self.createMySchedule = function(myScheduleObject) {
		var mappedlayers = ko.utils.arrayMap(myScheduleObject.ScheduleLayers, function(layer) {
			return new Teleopti.MyTimeWeb.Request.LayerEditShiftTradeViewModel(
				layer,
				myScheduleObject.MinutesSinceTimeLineStart,
				totalWidthInMinutes
			);
		});
		self.mySchedule(
			new Teleopti.MyTimeWeb.Request.PersonScheduleEditShiftTradeViewModel(mappedlayers, myScheduleObject)
		);
	};
	self.createOtherSchedule = function(myScheduleObject) {
		var mappedlayers = ko.utils.arrayMap(myScheduleObject.ScheduleLayers, function(layer) {
			return new Teleopti.MyTimeWeb.Request.LayerEditShiftTradeViewModel(
				layer,
				myScheduleObject.MinutesSinceTimeLineStart,
				totalWidthInMinutes
			);
		});
		self.otherSchedule(
			new Teleopti.MyTimeWeb.Request.PersonScheduleEditShiftTradeViewModel(mappedlayers, myScheduleObject)
		);
	};

	self.TradeDate = ko.observable(Teleopti.MyTimeWeb.Common.FormatDate(data.Date));

	self.createMySchedule(data.From);
	self.createOtherSchedule(data.To);
}

Teleopti.MyTimeWeb.Request.ChooseHistoryViewModelUtil = {
	findScheduleTimeRange: function(mySchedule, selectedSchedule) {
		var scheduleStartTime;
		var scheduleEndTime;

		var myScheduleFormatedStartTime =
			mySchedule.scheduleStartTime() !== undefined
				? mySchedule.scheduleStartTime().format('MMMM Do YYYY, HH:mm')
				: null;
		var myScheduleFormatedEndTime =
			mySchedule.scheduleEndTime() !== undefined
				? mySchedule.scheduleEndTime().format('MMMM Do YYYY, HH:mm')
				: null;
		var selectedScheduleFormatedStartTime =
			selectedSchedule.scheduleStartTime() !== undefined
				? selectedSchedule.scheduleStartTime().format('MMMM Do YYYY, HH:mm')
				: null;
		var selectedScheduleFormatedEndTime =
			selectedSchedule.scheduleEndTime() !== undefined
				? selectedSchedule.scheduleEndTime().format('MMMM Do YYYY, HH:mm')
				: null;
		if (
			(myScheduleFormatedStartTime === null || myScheduleFormatedStartTime === myScheduleFormatedEndTime) &&
			selectedScheduleFormatedStartTime !== selectedScheduleFormatedEndTime
		) {
			scheduleStartTime = selectedSchedule.scheduleStartTime();
			scheduleEndTime = selectedSchedule.scheduleEndTime();
		} else if (
			(selectedScheduleFormatedStartTime === null ||
				selectedScheduleFormatedStartTime === selectedScheduleFormatedEndTime) &&
			myScheduleFormatedStartTime !== myScheduleFormatedEndTime
		) {
			scheduleStartTime = mySchedule.scheduleStartTime();
			scheduleEndTime = mySchedule.scheduleEndTime();
		} else {
			scheduleStartTime =
				mySchedule.scheduleStartTime() < selectedSchedule.scheduleStartTime()
					? mySchedule.scheduleStartTime()
					: selectedSchedule.scheduleStartTime();
			scheduleEndTime =
				selectedSchedule.scheduleEndTime() > mySchedule.scheduleEndTime()
					? selectedSchedule.scheduleEndTime()
					: mySchedule.scheduleEndTime();
		}

		return {
			scheduleStartTime: scheduleStartTime,
			scheduleEndTime: scheduleEndTime
		};
	}
};

Teleopti.MyTimeWeb.Request.ChooseHistoryViewModel = function(chooseHistory, canvasPixelWidth) {
	var self = this;
	self.agentName = chooseHistory.tradedSchedule ? chooseHistory.tradedSchedule.agentName : '';
	self.selectedDateInFormat = ko.observable(chooseHistory.date.format(Teleopti.MyTimeWeb.Common.DateFormat));
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
			if (width !== null) {
				canvasWidth = width;
			} else {
				canvasWidth = $('td.the-other-agent-schedule').width();
			}
			self.pixelWidth(canvasWidth);
		}
	});

	self.setTimeLineLengthInMinutes = function(firstHour, mins) {
		self.timeLineStartTime(firstHour);
		self.timeLineLengthInMinutes(mins);
	};

	self.pixelPerMinute = ko.computed(function() {
		return self.canvasPixelWidth() / self.timeLineLengthInMinutes();
	});

	self.hours = ko.computed(function() {
		var allHours = chooseHistory.hours;

		if (chooseHistory.mySchedule !== undefined && chooseHistory.tradedSchedule !== undefined) {
			var mySchedule = chooseHistory.mySchedule;
			var selectedSchedule = chooseHistory.tradedSchedule;
			var scheduleStartTime;
			var scheduleEndTime;

			if (
				mySchedule.isDayOff &&
				!selectedSchedule.isDayOff &&
				selectedSchedule.layers !== null &&
				selectedSchedule.layers.length > 0
			) {
				scheduleStartTime = selectedSchedule.scheduleStartTime();
				scheduleEndTime = selectedSchedule.scheduleEndTime();
			} else if (
				!mySchedule.isDayOff &&
				mySchedule.layers !== null &&
				mySchedule.layers.length > 0 &&
				selectedSchedule.isDayOff
			) {
				scheduleStartTime = mySchedule.scheduleStartTime();
				scheduleEndTime = mySchedule.scheduleEndTime();
			} else if (
				(mySchedule.isDayOff || mySchedule.layers === null || mySchedule.layers.length === 0) &&
				(selectedSchedule.isDayOff || selectedSchedule.layers === null || selectedSchedule.layers.length === 0)
			) {
				return allHours;
			} else {
				var timeRange = Teleopti.MyTimeWeb.Request.ChooseHistoryViewModelUtil.findScheduleTimeRange(
					mySchedule,
					selectedSchedule
				);

				scheduleStartTime = timeRange.scheduleStartTime;
				scheduleEndTime = timeRange.scheduleEndTime;
			}

			var mins =
				(chooseHistory.hours.length - 1) * 60 +
				(moment(chooseHistory.hours[chooseHistory.hours.length - 1].EndTime).get('minutes') -
					moment(chooseHistory.hours[1].StartTime).get('minutes'));
			self.setTimeLineLengthInMinutes(scheduleStartTime, mins);

			var truncatedHours = [];
			$.each(allHours, function(index, hour) {
				if (hour.startTime >= scheduleStartTime && hour.startTime <= scheduleEndTime) {
					var start = moment(allHours[1].startTime);
					var diff = start.diff(scheduleStartTime, 'minutes');
					var newHour = new Teleopti.MyTimeWeb.Request.CloneTimeLineHourAddShiftTradeViewModel(
						index,
						hour,
						diff,
						self.pixelPerMinute()
					);
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
		if (chooseHistory.mySchedule !== undefined) {
			$.each(chooseHistory.mySchedule.layers, function(index, vmScheduleAddShiftTrade) {
				vmScheduleAddShiftTrade.pixelPerMinute(self.pixelPerMinute());
			});
			return chooseHistory.mySchedule;
		}
		return undefined;
	});

	self.selectedSchedule = ko.computed(function() {
		if (chooseHistory.tradedSchedule !== undefined) {
			$.each(chooseHistory.tradedSchedule.layers, function(index, vmScheduleAddShiftTrade) {
				vmScheduleAddShiftTrade.pixelPerMinute(self.pixelPerMinute());
			});
			return chooseHistory.tradedSchedule;
		}
		return undefined;
	});
};

Teleopti.MyTimeWeb.Request.PageView = function(pageIndex) {
	var self = this;
	self.index = ko.observable(pageIndex);
	self.isSelected = ko.observable(false);
};

Teleopti.MyTimeWeb.Request.FilterStartTimeView = function(text, start, end, checked, isDayOff, isEmptyDay) {
	var self = this;
	self.text = text;
	self.start = start;
	self.end = end;
	self.isChecked = ko.observable(checked || false);
	self.isDayOff = ko.observable(isDayOff || false);
	self.isEmptyDay = ko.observable(isEmptyDay || false);
};

Teleopti.MyTimeWeb.Request.FilterEndTimeView = function(text, start, end, checked) {
	var self = this;
	self.text = text;
	self.start = start;
	self.end = end;
	self.isChecked = ko.observable(checked || false);
};

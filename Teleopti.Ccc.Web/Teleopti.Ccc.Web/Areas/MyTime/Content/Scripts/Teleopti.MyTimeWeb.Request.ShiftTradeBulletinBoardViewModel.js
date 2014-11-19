/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="jquery.visible.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>


Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel = function(ajax) {
	var self = this;
	self.maxShiftsPerPage = 20;
	self.layerCanvasPixelWidth = ko.observable();
	self.missingWorkflowControlSet = ko.observable(true);
	self.openPeriodStartDate = ko.observable(moment().startOf('year').add('days', -1));
	self.openPeriodEndDate = ko.observable(moment().startOf('year').add('days', -1));
	self.requestedDateInternal = ko.observable(moment().startOf('day'));
	self.IsLoading = ko.observable(false);
	self.isReadyLoaded = ko.observable(false);
	self.weekStart = ko.observable(1);
	var datePickerFormat = $('#Request-detail-datepicker-format').val() ? $('#Request-detail-datepicker-format').val().toUpperCase() : "YYYY-MM-DD";
	self.DatePickerFormat = ko.observable(datePickerFormat);
	self.hours = ko.observableArray();
	self.timeLineStartTime = ko.observable();
	self.timeLineLengthInMinutes = ko.observable();
	self.mySchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel());
	self.possibleTradeSchedules = ko.observableArray();
	self.noPossibleShiftTrades = ko.observable(false);

	self.getDateWithFormat = function () {
		return self.requestedDateInternal().format(self.DatePickerFormat());
	};

	self.requestedDate = ko.computed({
		read: function () {
			return self.requestedDateInternal();
		},
		write: function (value) {
			if (self.requestedDateInternal().diff(value) == 0) return;
			self.requestedDateInternal(value);

			self.loadBulletinSchedules(self.getDateWithFormat());
		}
	});

	self.isRequestedDateValid = function (date) {
		if (date.diff(self.openPeriodStartDate()) < 0) {
			return false;
		} else if (self.openPeriodEndDate().diff(date) < 0) {
			return false;
		}
		return true;
	};

	self.changeRequestedDate = function (movement) {
		var date = moment(self.requestedDateInternal()).add('days', movement);
		if (self.isRequestedDateValid(date)) {
			self.requestedDate(date);
		}
	};

	self.previousDate = function () {
		self.changeRequestedDate(-1);
	};

	self.previousDateValid = ko.computed(function () {
		return self.requestedDateInternal().diff(self.openPeriodStartDate()) > 0;
	});

	self.setDatePickerRange = function (now, relativeStart, relativeEnd) {
		self.openPeriodStartDate(moment(now).add('days', relativeStart));
		self.openPeriodEndDate(moment(now).add('days', relativeEnd));
	};

	self.setTimeLineLengthInMinutes = function (firstHour, mins) {
		self.timeLineStartTime(firstHour);
		self.timeLineLengthInMinutes(mins);
	};

	self.pixelPerMinute = ko.computed(function () {
		return self.layerCanvasPixelWidth() / self.timeLineLengthInMinutes();
	});

	self._createTimeLine = function (hours) {
		var firstTimeLineHour = moment(hours[0].StartTime);
		//modify for daylight save close day
		var mins = (hours.length - 1) * 60 + (moment(hours[hours.length - 1].EndTime).get('minutes') - moment(hours[1].StartTime).get('minutes'));
		self.setTimeLineLengthInMinutes(firstTimeLineHour, mins);
		self.hours([]);
		var start = moment(hours[1].StartTime);
		var diff = start.diff(self.timeLineStartTime(), 'minutes');
		if (hours.length < 18) {
			for (var j = 0; j < hours.length; j++) {
				var newHour = new Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel(j, hours[j], diff, self.pixelPerMinute(), true);
				self.hours.push(newHour);
			}
		} else {
			for (var i = 0; i < hours.length; i++) {
				var isVisible = (i % 2 != 0);
				var newHour = new Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel(i, hours[i], diff, self.pixelPerMinute(), isVisible);
				self.hours.push(newHour);
			}
		}
	};

	self._createMySchedule = function (myScheduleObject) {
		var mappedlayers = [];
		if (myScheduleObject != null && myScheduleObject.ScheduleLayers != null) {
			var layers = myScheduleObject.ScheduleLayers;
			var scheduleStartTime = moment(layers[0].Start);
			var scheduleEndTime = moment(layers[layers.length - 1].End);
			mappedlayers = ko.utils.arrayMap(layers, function (layer) {
				var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
				return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart, self.pixelPerMinute());
			});
			self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, scheduleStartTime, scheduleEndTime, myScheduleObject.Name, myScheduleObject.PersonId, myScheduleObject.IsDayOff));
		} else {
			self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, moment(), moment(), '', '', false));
		}
	};

	self._createPossibleTradeSchedules = function (possibleTradeSchedules) {
		self.possibleTradeSchedules.removeAll();
		var mappedPersonsSchedule = ko.utils.arrayMap(possibleTradeSchedules, function (personSchedule) {
			var mappedLayers = [];
			if (personSchedule != null && personSchedule.ScheduleLayers != null) {
				var layers = personSchedule.ScheduleLayers;
				var scheduleStartTime = moment(layers[0].Start);
				var scheduleEndTime = moment(layers[layers.length - 1].End);

				mappedLayers = ko.utils.arrayMap(personSchedule.ScheduleLayers, function (layer) {
					var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
					return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart, self.pixelPerMinute());
				});
			}
			var model = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedLayers, scheduleStartTime, scheduleEndTime, personSchedule.Name, personSchedule.PersonId, personSchedule.IsDayOff);
			self.possibleTradeSchedules.push(model);
			return model;
		});

		self.noPossibleShiftTrades(mappedPersonsSchedule.length == 0 ? true : false);
	};

	self.getCanvasWidth = function () {
		var canvasWidth;
		var containerWidth = $("#Request-add-shift-trade").width();
		var nameCellWidth = $("td.shift-trade-agent-name").width();
		canvasWidth = containerWidth - nameCellWidth;
		return canvasWidth;
	};

	self.redrawLayers = function () {
		var canvasWidth;

		if (self.isReadyLoaded()) {
			canvasWidth = $("td.shift-trade-possible-trade-schedule:visible").width();
			if (canvasWidth == null) canvasWidth = self.getCanvasWidth();
		} else {
			canvasWidth = self.getCanvasWidth();
		}

		self.layerCanvasPixelWidth(canvasWidth);

		if (self.mySchedule() != undefined) {
			$.each(self.mySchedule().layers, function (index, selfScheduleAddShiftTrade) {
				selfScheduleAddShiftTrade.pixelPerMinute(self.pixelPerMinute());
			});
		}

		if (self.possibleTradeSchedules() != undefined) {
			$.each(self.possibleTradeSchedules(), function (index, selfPersonScheduleAddShiftTrade) {
				$.each(selfPersonScheduleAddShiftTrade.layers, function (index, selfScheduleAddShiftTrade) {
					selfScheduleAddShiftTrade.pixelPerMinute(self.pixelPerMinute());
				});
			});
		}

		if (self.hours() != undefined) {
			$.each(self.hours(), function (index, hour) {
				hour.pixelPerMinute(self.pixelPerMinute());
			});
		}
	};

	self.loadBulletinSchedules = function (date) {
		ajax.Ajax({
			url: "Team/TeamsForShiftTrade",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				date: date
			},
			success: function (data, textStatus, jqXHR) {
				var teamIds = [];
				for (var i = 0; i < data.length; ++i) {
					teamIds.push(data[i].id);
				}

				self.loadSchedule(date, teamIds);
			},
			error: function (e) {
				//console.log(e);
			}
		});
	};

	self.loadSchedule = function(date, teamIds) {
		if (self.IsLoading()) return;
		var take = self.maxShiftsPerPage;
		var skip = 0;//(self.selectedPageIndex() - 1) * take;

		ajax.Ajax({
			url: "Requests/ShiftTradeRequestScheduleForAllTeams",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				selectedDate: date,
				teamIds: teamIds.join(","),
				Take: take,
				Skip: skip
			},
			beforeSend: function () {
				self.IsLoading(true);
			},
			success: function (data, textStatus, jqXHR) {
				//self.setPagingInfo(data.PageCount);

				self._createTimeLine(data.TimeLineHours);
				self._createMySchedule(data.MySchedule);

				self._createPossibleTradeSchedules(data.PossibleTradeSchedules);
				//self.keepSelectedAgentVisible();
				self.isReadyLoaded(true);

				// Redraw layers after data loaded
				self.redrawLayers();
			},
			error: function (e) {
				//console.log(e);
			},
			complete: function () {
				self.IsLoading(false);
			}
		});
	};

	self.loadPeriod = function (date) {
		ajax.Ajax({
			url: "Requests/ShiftTradeRequestPeriod",
			dataType: "json",
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				if (data.HasWorkflowControlSet) {
					var now = moment(new Date(data.NowYear, data.NowMonth - 1, data.NowDay));
					self.setDatePickerRange(now, data.OpenPeriodRelativeStart, data.OpenPeriodRelativeEnd);
					var requestedDate = moment(now).add('days', data.OpenPeriodRelativeStart);
					if (date && Object.prototype.toString.call(date) === '[object Date]') {
						var md = moment(date);
						if (self.isRequestedDateValid(md)) {
							requestedDate = md;
						}
					}
					self.requestedDate(requestedDate);
				}
				self.missingWorkflowControlSet(!data.HasWorkflowControlSet);
			}
		});
	};
};
/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="jquery.visible.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>


Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel = function (ajax) {
	var self = this;
	self.maxShiftsPerPage = 20;
	self.layerCanvasPixelWidth = ko.observable();
	self.missingWorkflowControlSet = ko.observable(true);
	self.openPeriodStartDate = ko.observable(moment().startOf('year').add('days', -1));
	self.openPeriodEndDate = ko.observable(moment().startOf('year').add('days', -1));
	self.requestedDateInternal = ko.observable(moment().startOf('day').add('days', -1));
	self.IsLoading = ko.observable(false);
	self.isReadyLoaded = ko.observable(false);
	self.weekStart = ko.observable(1);
	self.DatePickerFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);
	self.hours = ko.observableArray();
	self.timeLineStartTime = ko.observable();
	self.timeLineLengthInMinutes = ko.observable();
	self.mySchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel());
	self.possibleTradeSchedules = ko.observableArray();
	self.noPossibleShiftTrades = ko.observable(false);
	self.selectedPageIndex = ko.observable(1);
	self.pageCount = ko.observable(1);
	self.selectablePages = ko.observableArray();
	self.isMore = ko.observable(false);
	self.isPreviousMore = ko.observable(false);
	self.isPageVisible = ko.observable(true);
	self.availableTeams = ko.observableArray();
	self.agentChoosed = ko.observable(null);
	self.subject = ko.observable();
	self.message = ko.observable();
	self.isSendEnabled = ko.observable(true);
	self.errorMessage = ko.observable();
	self.filterStartTimeList = ko.observableArray();
	self.filteredStartTimesText = ko.observableArray();
	self.isDayoffFiltered = ko.observable(false);
	self.isEmptyDayFiltered = ko.observable(false);
	self.filterEndTimeList = ko.observableArray();
	self.filteredEndTimesText = ko.observableArray();
	self.Toggle31317Enabled = ko.observable(false);
	self.preloadTimeFilterFinished = true;
	self.isAnonymousTrading = ko.observable(false);
	self.Toggle31638Enabled = ko.observable(false);

	self.isDetailVisible = ko.computed(function () {
		if (self.agentChoosed() === null) {
			return false;
		}
		return true;
	});

	self.getFormattedDateForDisplay = function () {
		return self.requestedDateInternal().format(self.DatePickerFormat());
	};

	self.getFormattedDateForServiceCall = function () {
		return Teleopti.MyTimeWeb.Common.FormatServiceDate(self.requestedDateInternal());
	};

	self.getAllTeamIds = function () {
		var teamIds = [];
		for (var i = 0; i < self.availableTeams().length; ++i) {
			teamIds.push(self.availableTeams()[i].id);
		}
		return teamIds;
	};

	self.requestedDate = ko.computed({
		read: function () {
			return self.requestedDateInternal();
		},
		write: function (value) {
			if (self.requestedDateInternal().diff(value) == 0) return;
			self.requestedDateInternal(value);

			self.prepareLoad();
			self.loadBulletinSchedules(self.getFormattedDateForServiceCall());
		}
	});

	self.checkMessageLength = function (data, event) {
		var text = $(event.target)[0].value;
		if (text.length > 2000) {
			self.message(text.substr(0, 2000));
		}
	};

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

	self.nextDate = function () {
		self.changeRequestedDate(1);
	};

	self.previousDateValid = ko.computed(function () {
		return self.requestedDateInternal().diff(self.openPeriodStartDate()) > 0;
	});

	self.nextDateValid = ko.computed(function () {
		return self.openPeriodEndDate().diff(self.requestedDateInternal()) > 0;
	});

	self.hideMessageBox = ko.computed(function () {
		return self.Toggle31638Enabled() && self.isAnonymousTrading();
	});

	self.filterStartEndTimeClick = function () {
		$('.dropdown-menu').on('click', function (e) {
			if ($(this).hasClass('filter-time-dropdown-form')) {
				e.stopPropagation();
			}
		});
	};

	self.filterTime = ko.computed(function () {
		self.filteredStartTimesText.removeAll();
		self.filteredEndTimesText.removeAll();

		$.each(self.filterStartTimeList(), function (idx, timeInFilter) {
			if (timeInFilter.isChecked()) {
				if (timeInFilter.isDayOff()) {
					self.isDayoffFiltered(true);
				} else if (timeInFilter.isEmptyDay()) {
					self.isEmptyDayFiltered(true);
				} else {
					var timeText = timeInFilter.start + ":00-" + timeInFilter.end + ":00";
					self.filteredStartTimesText.push(timeText);
				}
			} else {
				if (timeInFilter.isDayOff()) {
					self.isDayoffFiltered(false);
				}
				if (timeInFilter.isEmptyDay()) {
					self.isEmptyDayFiltered(false);
				}
			}
		});

		$.each(self.filterEndTimeList(), function (idx, timeInFilter) {
			if (timeInFilter.isChecked()) {
				var timeText = timeInFilter.start + ":00-" + timeInFilter.end + ":00";
				self.filteredEndTimesText.push(timeText);
			}
		});
	});

	self.filterTime.subscribe(function () {
		if (self.preloadTimeFilterFinished) {
			self.prepareLoad();
			self.loadSchedule(self.requestedDateInternal().format("YYYY-MM-DD"), self.getAllTeamIds());
		}
	});

	self.setTimeFilters = function (hourTexts) {
		var rangStart = 0;
		for (var i = 0; i < 24; i += 2) {
			var rangEnd = rangStart + 2;
			var hourText = hourTexts[i] + " - " + hourTexts[i + 2];
			var filterStartTime = new Teleopti.MyTimeWeb.Request.FilterStartTimeView(hourText, rangStart, rangEnd, false, false);
			var filterEndTime = new Teleopti.MyTimeWeb.Request.FilterEndTimeView(hourText, rangStart, rangEnd, false);
			self.filterStartTimeList.push(filterStartTime);
			self.filterEndTimeList.push(filterEndTime);
			rangStart += 2;
		}
	};

	self.cleanTimeFiler = function () {
		self.filteredStartTimesText.removeAll();
		self.filteredEndTimesText.removeAll();

		$.each(self.filterStartTimeList(), function (idx, filter) {
			if (filter.isChecked()) filter.isChecked(false);
		});

		$.each(self.filterEndTimeList(), function (idx, filter) {
			if (filter.isChecked()) filter.isChecked(false);
		});
	};

	self.keepSelectedAgentVisible = function () {
		if (self.agentChoosed() != null && self.possibleTradeSchedules() != null) {
			$.each(self.possibleTradeSchedules(), function (index, value) {
				value.isVisible(value.personId == self.agentChoosed().personId);
			});
		}
	}

	self.chooseAgent = function (agent) {
		if (agent != null) {
			self.redrawLayers();
			//rk - don't really like to put DOM stuff here...
			window.scrollTo(0, 0);
			self.isSendEnabled(true);
			self.cleanTimeFiler();
		}
		self.agentChoosed(agent);
		self.keepSelectedAgentVisible();
		self.errorMessage('');
	};

	self.sendRequest = function () {
		self.isSendEnabled(false);
		self.saveNewShiftTrade();
		self.chooseAgent(null);
		self.goToFirstPage();
	};

	self.cancelRequest = function () {
		self.chooseAgent(null);
		if (self.subject() != undefined) {
			self.subject("");
		}
		if (self.message() != undefined) {
			self.message("");
		}
		self.goToFirstPage();
	};

	self.prepareLoad = function () {
		self.selectedPageIndex(1);
		self.selectablePages.removeAll();
		self.isPreviousMore(false);
		self.isMore(false);
		self.chooseAgent(null);
		self.IsLoading(false);
	};

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

	self.goToFirstPage = function () {
		self.selectedPageIndex(1);
		self.isPreviousMore(false);
		self.initSelectablePages(self.pageCount());
		self.loadSchedule(self.getFormattedDateForServiceCall(), self.getAllTeamIds());
	};

	self.goToLastPage = function () {
		self.isMore(false);
		if (self.pageCount() > 5) self.isPreviousMore(true);
		var timesOfNumPerPage = self.pageCount() / 5;
		var modeOfNumPerPage = self.pageCount() % 5;
		if (timesOfNumPerPage > 0) {
			self.selectablePages.removeAll();
			if (modeOfNumPerPage != 0) {
				for (var i = 1; i <= modeOfNumPerPage; i++) {
					self.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(Math.floor(timesOfNumPerPage) * 5 + i));
				}
			} else {
				for (var j = 1; j <= 5; j++) {
					self.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(self.pageCount() - 5 + j));
				}
			}
		}
		self.setSelectPage(self.pageCount());
	};

	self.initSelectablePages = function (pageCount) {
		self.selectablePages.removeAll();
		for (var i = 1; i <= pageCount; ++i) {
			if (i <= 5) {
				self.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(i));
			} else {
				break;
			}
		}
		$.each(self.selectablePages(), function (index, item) {
			if (item.index() == self.selectedPageIndex()) {
				item.isSelected(true);
			}
		});

		var currentLastPageNumber = self.selectablePages().length > 0 ? self.selectablePages()[self.selectablePages().length - 1].index() : 0;
		if (currentLastPageNumber != 0 && currentLastPageNumber < pageCount) self.isMore(true);
	};

	self.goNextPages = function () {
		for (var i = 0; i < self.selectablePages().length; ++i) {
			var item = self.selectablePages()[i];
			if ((item.index() + 5) <= self.pageCount()) {
				item.index(item.index() + 5);
			} else {
				self.isMore(false);
				self.selectablePages.remove(item);
				i--;
			}
		}

		if (self.selectablePages()[0].index() > 5) self.isPreviousMore(true);

		self.setSelectPage(self.selectablePages()[0].index());
	};

	self.goPreviousPages = function () {
		$.each(self.selectablePages(), function (index, item) {
			if (index + 1 <= self.selectablePages().length) {
				item.index(item.index() - 5);
			}
			if (self.selectablePages()[0].index() == 1) self.isPreviousMore(false);
		});

		if (self.selectablePages().length < 5) {
			for (var i = self.selectablePages().length + 1; i <= 5; ++i) {
				var page = new Teleopti.MyTimeWeb.Request.PageView(i);
				self.selectablePages.push(page);
				self.isPreviousMore(false);
			}
		}

		if (self.selectablePages()[4].index() < self.pageCount()) self.isMore(true);

		self.setSelectPage(self.selectablePages()[0].index());
	};

	self.selectPage = function (page) {
		self.setSelectPage(page.index());
	};

	self.setSelectPage = function (pageIdx) {
		self.selectedPageIndex(pageIdx);
		self.loadSchedule(self.getFormattedDateForServiceCall(), self.getAllTeamIds());
	};

	self.setPagingInfo = function (pageCount) {
		self.pageCount(pageCount);

		if (self.pageCount() == 0) {
			self.isPageVisible(false);
		} else {
			self.isPageVisible(true);
		}

		if (self.selectablePages().length == 0) {
			self.initSelectablePages(self.pageCount());
		}

		$.each(self.selectablePages(), function (index, item) {
			if (item.index() == self.selectedPageIndex()) {
				item.isSelected(true);
			} else {
				item.isSelected(false);
			}
		});
	};

	self._createTimeLine = function (hours) {
		var firstTimeLineHour = moment(hours[0].StartTime);
		var mins = hours[0].LengthInMinutesToDisplay + (hours.length - 1) * 60 + (moment(hours[hours.length - 1].EndTime).get('minutes') - moment(hours[1].StartTime).get('minutes'));
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
			self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, scheduleStartTime, scheduleEndTime, myScheduleObject.Name, myScheduleObject.PersonId, myScheduleObject.IsDayOff, '',false,false,null));
		} else {
			self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, moment(), moment(), '', '', false,'',false,false, null));
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
			var name = '';
			if (!self.isAnonymousTrading()) name = personSchedule.Name;
			var model = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedLayers, scheduleStartTime, scheduleEndTime, name, personSchedule.PersonId, personSchedule.IsDayOff, '', false, false, personSchedule.ShiftExchangeOfferId);
			self.possibleTradeSchedules.push(model);
			return model;
		});

		self.noPossibleShiftTrades(mappedPersonsSchedule.length == 0 ? true : false);
	};

	self.getCanvasWidth = function () {
		var canvasWidth;
		var containerWidth = $("#Request-shift-trade-bulletin-board").width();
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
				self.availableTeams(data);
				self.loadSchedule(date, self.getAllTeamIds());
				self.isReadyLoaded(true);
			},
			error: function (e) {
				//console.log(e);
			}
		});
	};

	self.isFiltered = function () {
		if (self.filteredStartTimesText().length == 0 && self.filteredEndTimesText().length == 0
			&& !self.isDayoffFiltered() && !self.isEmptyDayFiltered()) {
			return false;
		}
		return true;
	};

	self.loadSchedule = function (date, teamIds) {
		if (!self.isFiltered()) {
			self.loadBulletinSchedule(date, teamIds);
		}
		else {
			self.loadScheduleWithFilter(date, teamIds);
		}
	}

	self.loadScheduleWithFilter = function (date, teamIds) {
		if (teamIds.length > 0) {
			if (self.IsLoading()) return;
			var take = self.maxShiftsPerPage;
			var skip = (self.selectedPageIndex() - 1) * take;

			ajax.Ajax({
				url: "RequestsShiftTradeBulletinBoard/BulletinSchedulesWithTimeFilter",
				dataType: "json",
				type: 'POST',
				contentType: 'application/json; charset=utf-8',
				data: JSON.stringify({
					selectedDate: date,
					teamIds: teamIds.join(","),
					filteredStartTimes: self.filteredStartTimesText().join(","),
					filteredEndTimes: self.filteredEndTimesText().join(","),
					isDayOff: self.isDayoffFiltered(),
					isEmptyDay: self.isEmptyDayFiltered(),
					Take: take,
					Skip: skip
				}),
				beforeSend: function () {
					self.IsLoading(true);
				},
				success: function (data, textStatus, jqXHR) {
					self.setPagingInfo(data.PageCount);

					self._createTimeLine(data.TimeLineHours);
					self._createMySchedule(data.MySchedule);

					self._createPossibleTradeSchedules(data.PossibleTradeSchedules);
					self.keepSelectedAgentVisible();
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
		}
	};

	self.loadBulletinSchedule = function (date, teamIds) {
		if (teamIds.length > 0) {
			if (self.IsLoading()) return;
			var take = self.maxShiftsPerPage;
			var skip = (self.selectedPageIndex() - 1) * take;

			ajax.Ajax({
				url: "RequestsShiftTradeBulletinBoard/BulletinSchedules",
				dataType: "json",
				type: 'POST',
				contentType: 'application/json; charset=utf-8',
				data: JSON.stringify({
					selectedDate: date,
					teamIds: teamIds.join(","),
					Take: take,
					Skip: skip
				}),
				beforeSend: function () {
					self.IsLoading(true);
				},
				success: function (data, textStatus, jqXHR) {
					self.setPagingInfo(data.PageCount);

					self._createTimeLine(data.TimeLineHours);
					self._createMySchedule(data.MySchedule);

					self._createPossibleTradeSchedules(data.PossibleTradeSchedules);
					self.keepSelectedAgentVisible();
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
		}
	};

	self.hideWindow = function () {
		$('#Request-shift-trade-bulletin-board').hide();
	};

	self.requestedDates = ko.computed(function () {
		var dates = [];
		dates.push(self.getFormattedDateForServiceCall());
		return dates;
	});

	self.saveNewShiftTrade = function () {
		ajax.Ajax({
			url: "Requests/ShiftTradeRequest",
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			data: JSON.stringify({
				Dates: self.requestedDates(),
				Subject: self.subject(),
				Message: self.message(),
				PersonToId: self.agentChoosed().personId,
				ShiftExchangeOfferId: self.agentChoosed().ShiftExchangeOfferId
			}),
			success: function (data) {
				self.agentChoosed(null);
				self.isSendEnabled(true);
				self.hideWindow();
				if (data.ExchangeOffer.IsOfferAvailable) {
					Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data);
				} else {
					$('#lock-trade-conflict').modal('show');
				}
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var data = $.parseJSON(jqXHR.responseText);
					self.errorMessage(data.Errors.join('</br>'));
					self.isSendEnabled(true);
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	};

	self.loadFilterTimes = function () {
		self.Toggle31317Enabled(Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_TradeWithDayOffAndEmptyDay_31317'));

		if (self.filterStartTimeList().length == 0) {
			var dayOffNames = "";
			ajax.Ajax({
				url: "RequestsShiftTradeScheduleFilter/Get",
				dataType: "json",
				type: 'GET',
				contentType: 'application/json; charset=utf-8',
				success: function (data) {
					//set dayoff only in start time filter
					if (data != null) {
						self.preloadTimeFilterFinished = false;
						self.setTimeFilters(data.HourTexts);
						$.each(data.DayOffShortNames, function (idx, name) {
							if (idx < data.DayOffShortNames.length - 1) dayOffNames += name + ", ";
							else dayOffNames += name;
						});
						self.filterStartTimeList.push(new Teleopti.MyTimeWeb.Request.FilterStartTimeView(dayOffNames, 0, 24, false, true, false));
						if (self.Toggle31317Enabled) self.filterStartTimeList.push(new Teleopti.MyTimeWeb.Request.FilterStartTimeView(data.EmptyDayText, 0, 24, false, false, true));
						self.preloadTimeFilterFinished = true;
					}
				}
			});
		}
	};

	self.loadPeriod = function (date) {
		ajax.Ajax({
			url: "Requests/ShiftTradeRequestPeriod",
			dataType: "json",
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				if (data.HasWorkflowControlSet) {
					self.Toggle31638Enabled(Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_AnonymousTrades_31638'));
					if (self.Toggle31638Enabled()) self.isAnonymousTrading(data.MiscSetting.AnonymousTrading);
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
				} else {
					self.isReadyLoaded(true);
				}
				self.missingWorkflowControlSet(!data.HasWorkflowControlSet);
			}
		});
	};
};
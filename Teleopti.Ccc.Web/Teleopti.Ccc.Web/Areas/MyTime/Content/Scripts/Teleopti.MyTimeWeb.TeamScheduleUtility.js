Teleopti.MyTimeWeb.DayScheduleMixin = function() {
	var self = this;

	self.openPeriodStartDate = ko.observable(moment().startOf('year').add('days', -1));
	self.openPeriodEndDate = ko.observable(moment().startOf('year').add('days', -1));
	self.DatePickerFormat = ko.observable("YYYY-MM-DD");
	self.weekStart = ko.observable(1);

	var changeHandler = null;

	self.setDayMixinChangeHandler = function (cb) {
		changeHandler = cb;
	};

	var changeHandlerSuspended = false;

	self.suspendDayMixinChangeHandler = function() {
		changeHandlerSuspended = true;
	};

	self.activateDayMixinChangeHandler = function() {
		changeHandlerSuspended = false;
	};


	self.requestedDate = ko.observable(moment().startOf('day'));	
	
	self.reloadDatePickerFormat = function($hiddenInput) {
		self.DatePickerFormat($hiddenInput.val());
	};

	self.requestedDateWithFormat = ko.computed(function() {
		return self.requestedDate().format(self.DatePickerFormat());
	});

	self.requestedDateInternal = ko.observable();
	self.requestedDate.subscribe(function(newValue) {
		self.requestedDateInternal(newValue.format("YYYY-MM-DD"));
	});

	self.requestedDateInternal.subscribe(function (newValue) {
		if (changeHandler != null && !changeHandlerSuspended) {		
			changeHandler(self.requestedDateWithFormat());
		}
	});

	self.isRequestedDateValid = function(date) {
		if (date.diff(self.openPeriodStartDate()) < 0) {
			return false;
		} else if (self.openPeriodEndDate().diff(date) < 0) {
			return false;
		}
		return true;
	};

	self.changeRequestedDate = function(movement) {
		var date = moment(self.requestedDate()).add('days', movement);
		if (self.isRequestedDateValid(date)) {
			self.requestedDate(date);
		}
	};

	self.nextDate = function() {
		self.changeRequestedDate(1);
	};

	self.previousDate = function() {
		self.changeRequestedDate(-1);
	};

	self.setDatePickerRange = function(now, relativeStart, relativeEnd) {
		self.openPeriodStartDate(moment(now).add('days', relativeStart));
		self.openPeriodEndDate(moment(now).add('days', relativeEnd));
	};

	self.nextDateValid = ko.computed(function () {
		return self.openPeriodEndDate().diff(self.requestedDate()) > 0;
	});

	self.previousDateValid = ko.computed(function () {
		return self.requestedDate().diff(self.openPeriodStartDate()) > 0;
	});

};

Teleopti.MyTimeWeb.PagingMixin = function() {

	var self = this;
	self.pageCount = ko.observable(1);
	self.isPreviousMore = ko.observable(false);
	self.maxItemsPerPage = ko.observable(4);
	self.isMore = ko.observable(false);
	self.isPageVisible = ko.observable(false);

	var changeHandler = null;
	self.selectablePages = ko.observableArray();

	self.setPagingMixinChangeHandler = function(cb) {
		changeHandler = cb;
	};

	var changeHandlerSuspended = false;

	self.suspendPagingMixinChangeHandler = function () {
		changeHandlerSuspended = true;
	};

	self.activatePagingMixinChangeHandler = function () {
		changeHandlerSuspended = false;
	};


	self.selectedPageIndex = ko.observable(1);
	self.selectedPageIndex.subscribe(function() {
		if (changeHandler != null && !changeHandlerSuspended) {
			changeHandler.call();			
		}
	});


	self.requestedPaging = function () {
		return {
			take: self.maxItemsPerPage(),
			skip: (self.selectedPageIndex() - 1) * self.maxItemsPerPage()
		};
	};

	self.goToFirstPage = function() {
		self.selectedPageIndex(1);
		self.isPreviousMore(false);
		self.initSelectablePages(self.pageCount());
	};

	self.goToLastPage = function() {
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
		self.selectedPageIndex(self.pageCount());
	};

	self.initSelectablePages = function(pageCount) {
		self.selectablePages.removeAll();
		for (var i = 1; i <= pageCount; ++i) {
			if (i <= 5) {
				self.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(i));
			} else {
				break;
			}
		}
		$.each(self.selectablePages(), function(index, item) {
			if (item.index() == self.selectedPageIndex()) {
				item.isSelected(true);
			}
		});

		var currentLastPageNumber = self.selectablePages().length > 0 ? self.selectablePages()[self.selectablePages().length - 1].index() : 0;
		if (currentLastPageNumber != 0 && currentLastPageNumber < pageCount) self.isMore(true);
	};

	self.goNextPages = function() {
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

		self.selectedPageIndex(self.selectablePages()[0].index());
	};

	self.goPreviousPages = function() {
		$.each(self.selectablePages(), function(index, item) {
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

		self.selectedPageIndex(self.selectablePages()[0].index());
	};


	self.setPagingInfo = function(pageCount) {
		self.pageCount(pageCount);

		if (self.pageCount() == 0) {
			self.isPageVisible(false);
		} else {
			self.isPageVisible(true);
		}

		if (self.selectablePages().length == 0) {
			self.initSelectablePages(self.pageCount());
		}

		$.each(self.selectablePages(), function(index, item) {
			if (item.index() == self.selectedPageIndex()) {
				item.isSelected(true);
			} else {
				item.isSelected(false);
			}
		});
	};

	self.selectPage = function (page) {
		self.selectedPageIndex(page.index());
	};
};

Teleopti.MyTimeWeb.TeamScheduleDrawerMixin = function () {
	var self = this;
	self.layerCanvasPixelWidth = ko.observable();
	self.timeLineStartTime = ko.observable();
	self.timeLineLengthInMinutes = ko.observable();
	self.hours = ko.observableArray();
	self.toDrawSchedules = ko.observableArray();
	self.showHourLine = ko.observable(false);

	self.setTimeLineLengthInMinutes = function (firstHour, mins) {
		self.timeLineStartTime(firstHour);
		self.timeLineLengthInMinutes(mins);
	};

	self.pixelPerMinute = ko.computed(function () {
		return self.layerCanvasPixelWidth() / self.timeLineLengthInMinutes();
	});

	var mapLayerViewModelForSchedule = function (personSchedule) {
		var mappedLayers = [];
		var layers = personSchedule.ScheduleLayers;
		var model;


		if (layers == null) {
			model = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
				mappedLayers,
				null,
				null,
				personSchedule.Name,
				personSchedule.PersonId,
				personSchedule.IsDayOff);
		} else {
			var scheduleStartTime = moment(layers[0].Start);
			var scheduleEndTime = moment(layers[layers.length - 1].End);

			mappedLayers = ko.utils.arrayMap(personSchedule.ScheduleLayers, function (layer) {
				var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
				return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(
					layer,
					minutesSinceTimeLineStart,
					self.pixelPerMinute());
			});

			model = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
				mappedLayers,
				scheduleStartTime,
				scheduleEndTime,
				personSchedule.Name,
				personSchedule.PersonId,
				personSchedule.IsDayOff);
		}					
		return model;
	};

	self.CleanTimeHourLine = function () {
		self.hours([]);
	};

	self.createToDrawSchedules = function(personSchedules) {
		self.toDrawSchedules.removeAll();

		var index;
		for (index = 0; index < personSchedules.length; ++index) {
			self.toDrawSchedules.push(mapLayerViewModelForSchedule(personSchedules[index]));
			self.showHourLine(true);
		}
	};

	self.createTimeLine = function (hours) {
		var firstTimeLineHour = moment(hours[0].StartTime);
		//modify for daylight save close day
		var mins = hours[0].LengthInMinutesToDisplay + (hours.length - 1) * 60;
		self.setTimeLineLengthInMinutes(firstTimeLineHour, mins);
		self.hours([]);
		var start = moment(hours[1].StartTime);
		var diff = start.diff(self.timeLineStartTime(), 'minutes');
		var newHour;
		if (hours.length < 18) {
			for (var j = 0; j < hours.length; j++) {
				newHour = new Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel(j, hours[j], diff, self.pixelPerMinute(), true);
				self.hours.push(newHour);
			}
		} else {
			for (var i = 0; i < hours.length; i++) {
				var isVisible = (i % 2 != 0);
				newHour = new Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel(i, hours[i], diff, self.pixelPerMinute(), isVisible);
				self.hours.push(newHour);
			}
		}
	};

	self.getCanvasWidth = function () {		
		var containerWidth = $("#draw-schedule-container").width();
		var nameCellWidth = $("td#shift-trade-agent-name").width();
		return containerWidth - nameCellWidth;		
	};

	self.redrawLayers = function() {
		var canvasWidth;

		if (!self.IsLoading) {
			canvasWidth = $("td.shift-trade-possible-trade-schedule:visible").width();
			if (canvasWidth == null) canvasWidth = self.getCanvasWidth();
		} else {
			canvasWidth = self.getCanvasWidth();
		}

		self.layerCanvasPixelWidth(canvasWidth);

		if (self.toDrawSchedules() != undefined) {
			$.each(self.toDrawSchedules(), function (index, schedule) {
				$.each(schedule.layers, function (index, layer) {
					layer.pixelPerMinute(self.pixelPerMinute());
				});
			});
		}
		
		if (self.hours() != undefined) {
			$.each(self.hours(), function(index, hour) {
				hour.pixelPerMinute(self.pixelPerMinute());
			});
		}
	};
};

Teleopti.MyTimeWeb.TeamScheduleFilterMixin = function () {

	var self = this;
		
	self.availableTeams = ko.observableArray();
	self.defaultTeam = ko.observable();
	self.selectedTeam = ko.observable();

	self.filteredStartTimesText = ko.observableArray();
	self.filteredEndTimesText = ko.observableArray();
	self.filterStartTimeList = ko.observableArray();
	self.filterEndTimeList = ko.observableArray();
	self.timeSortOrder = ko.observable(null);
	self.isDayoffFiltered = ko.observable(false);
	self.searchNameText = ko.observable();
	self.showTeamPicker = ko.observable(false);
	self.showGroupings = ko.observable(false);


	self.requestedFilter = ko.computed(function () {

		var selectedTeams = [];
		if (self.selectedTeam() != null) selectedTeams.push(self.selectedTeam());

		return {
			selectedTeams: selectedTeams,
			filteredStartTimesText: self.filteredStartTimesText(),
			filteredEndTimesText: self.filteredEndTimesText(),
			timeSortOrder: self.timeSortOrder(),
			isDayoffFiltered: self.isDayoffFiltered(),
			searchNameText: self.searchNameText()
		};
	});
		

	self.isLocked = ko.observable(false);


	var changeHandler = null;
	self.setFilterMixinChangeHandler = function (cb) {
		changeHandler = cb;
	};

	var changeHandlerSuspended = false;

	self.suspendFilterMixinChangeHandler = function () {
		changeHandlerSuspended = true;
	};

	self.activateFilterMixinChangeHandler = function () {
		changeHandlerSuspended = false;
	};


	self.filterStartEndTimeClick = function() {
		$('.dropdown-menu.filter-time-dropdown-form').on('click', function(e) {
			e.stopPropagation();
		});
	};

	self.cleanTimeFilter = function() {
		self.filteredStartTimesText.removeAll();
		self.filteredEndTimesText.removeAll();

		$.each(self.filterStartTimeList(), function(idx, filter) {
			if (filter.isChecked()) filter.isChecked(false);
		});

		$.each(self.filterEndTimeList(), function(idx, filter) {
			if (filter.isChecked()) filter.isChecked(false);
		});
	};

	self.setTimeFilters = function(data) {
		var rangStart = 0;
		for (var i = 0; i < 24; i += 2) {
			var rangEnd = rangStart + 2;
			var hourText = data.HourTexts[i] + " - " + data.HourTexts[i + 2];
			var filterStartTime = new Teleopti.MyTimeWeb.Request.FilterStartTimeView(hourText, rangStart, rangEnd, false, false);
			var filterEndTime = new Teleopti.MyTimeWeb.Request.FilterEndTimeView(hourText, rangStart, rangEnd, false);
			self.filterStartTimeList.push(filterStartTime);
			self.filterEndTimeList.push(filterEndTime);
			rangStart += 2;
		}
		var dayOffNames = data.DayOffShortNames.join();
		self.filterStartTimeList.push(new Teleopti.MyTimeWeb.Request.FilterStartTimeView(dayOffNames, 0, 24, false, true));
	};

	self.setTeamPicker = function (teams) {
		self.showTeamPicker(teams.length > 1);
		self.showGroupings(teams[0] && teams[0].children != null);
		self.availableTeams(teams);
	};

	self.isFiltered = function() {
		if (self.filteredStartTimesText().length == 0 && self.filteredEndTimesText().length == 0 && !self.isDayoffFiltered()) {
			return false;
		}
		return true;
	};


	self.selectedTeam.subscribe(function (newValue) {
		if (changeHandler != null && !changeHandlerSuspended) {
			changeHandler(newValue, self.requestedFilter());
		} 
	});
		

	self.filterTime = ko.computed(function() {
		self.filteredStartTimesText.removeAll();
		self.filteredEndTimesText.removeAll();

		$.each(self.filterStartTimeList(), function(idx, timeInFilter) {
			if (timeInFilter.isChecked()) {
				if (timeInFilter.isDayOff()) {
					self.isDayoffFiltered(true);
				} else {
					var timeText = timeInFilter.start + ":00-" + timeInFilter.end + ":00";
					self.filteredStartTimesText.push(timeText);
				}
			} else {
				if (timeInFilter.isDayOff()) {
					self.isDayoffFiltered(false);
				}
			}
		});

		$.each(self.filterEndTimeList(), function(idx, timeInFilter) {
			if (timeInFilter.isChecked()) {
				var timeText = timeInFilter.start + ":00-" + timeInFilter.end + ":00";
				self.filteredEndTimesText.push(timeText);
			}
		});

		return { filterStartTimeList: self.filterStartTimeList(), filterEndTimeList: self.filterEndTimeList() };
	});

	self.filterTime.subscribe(function(newValue) {
		if (!self.isLocked() && changeHandler != null && !changeHandlerSuspended) {
			changeHandler(newValue, self.requestedFilter());
		}
	});

	self.displaySortOrderTemplateList = ko.observable([
		{ Description: 'glyphicon glyphicon-arrow-up', Value: 'start asc', IsStart: true },
		{ Description: 'glyphicon glyphicon-arrow-up', Value: 'end asc', IsStart: false },
		{ Description: 'glyphicon glyphicon-arrow-down', Value: 'start desc', IsStart: true },
		{ Description: 'glyphicon glyphicon-arrow-down', Value: 'end desc', IsStart: false }
	]);

	self.updateTimeSortOrder = function(data) {
		if (self.timeSortOrder() == data.Value) {
			self.timeSortOrder(null);
		} else {
			self.timeSortOrder(data.Value);
		}
	};

	self.isSortingTimeActive = function(value) {
		return self.timeSortOrder() == value.Value;
	}

	self.timeSortOrder.subscribe(function(newValue) {
		if (!self.isLocked() && changeHandler != null && !changeHandlerSuspended) {
			changeHandler(newValue,self.requestedFilter());
		}
	});

	self.isStartTimeFilterActived = ko.computed(function() {
		return (self.filteredStartTimesText().length != 0 || self.isDayoffFiltered() == true || (self.timeSortOrder() == 'start asc') || (self.timeSortOrder() == 'start desc'));
	});

	self.isEndTimeFilterActived = ko.computed(function() {
		return (self.filteredEndTimesText().length != 0 || (self.timeSortOrder() == 'end asc') || (self.timeSortOrder() == 'end desc'));
	});

	
	var currentTimer = null;
	var resetTimer = function () {
		if (currentTimer !== null) {
			clearTimeout(currentTimer);
			currentTimer = null;
		}
	};

	// used to support script-triggered change, e.g. web scenario test
	self.suppressChangeInSearchBox = false;
	self.changeInSearchBoxSuppressed = function (data, event) {
		if (self.suppressChangeInSearchBox) return;
		else {
			self.changeInSearchBox($(event.target));
		}
	};
	// -------------------------------------------

	self.changeInSearchBox = function ($target) {
		resetTimer();
		self.refocusToNameSearch = function () { $target.focus(); };
		if (changeHandler != null && !changeHandlerSuspended) {
			changeHandler($target.val(), self.requestedFilter());
		}
	};

	self.typeInSearchBox = function (data, event) {
		var $target = $(event.target);
		resetTimer();
		self.suppressChangeInSearchBox = true;
		currentTimer = setTimeout(function () { self.changeInSearchBox($target); }, 500);
	};


};

Teleopti.MyTimeWeb.TeamScheduleDataProviderMixin = function(ajax) {

	var self = this;

	self.loadFilterTimes = function (success, error) {
		if (self.filterStartTimeList().length == 0) {
			var dayOffNames = "";
			ajax.Ajax({
				url: "RequestsShiftTradeScheduleFilter/Get",
				dataType: "json",
				type: 'GET',
				contentType: 'application/json; charset=utf-8',
				success: function (data) {					
					if (success != null) success(data);					
				},
				error: function(e) {
					if (error != null) error(e);
				}
			});
		}
	};

	self.loadMyTeam = function (date, success, error) {
		ajax.Ajax({
			url: "Requests/ShiftTradeRequestMyTeam",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				selectedDate: date
			},
			success: function (data) {
				if (success != null) success(data);
			},
			error: function (e) {
				if (error != null) error(e);
			}
		});
	};

	self.loadTeams = function (date, success, error) {		
		ajax.Ajax({
			url: "Team/TeamsForShiftTrade",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				date: date
			},
			success: function (data) {					
				if (success != null) success(data);					
			},
			error: function(e) {
				if (error != null) error(e);
			}			
		});
	};

	self.loadSchedule = function(date, filter, paging, beforeSend, success, error, complete) {
						
		if (filter.selectedTeams.length === 0) return;

		ajax.Ajax({
			url: "TeamSchedule/TeamSchedule",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				selectedDate: date,
				teamIds: filter.selectedTeams.join(","),
				searchNameText: filter.searchNameText,
				filteredStartTimes: filter.filteredStartTimesText.join(","),
				filteredEndTimes: filter.filteredEndTimesText.join(","),
				isDayOff: filter.isDayoffFiltered,
				Take: paging == null ? 20 : paging.take,
				Skip: paging == null ? 0 : paging.skip,
				TimeSortOrder: filter.timeSortOrder
			},
			beforeSend: function() {
				if (beforeSend != null) beforeSend();
			},
			success: function (data) {
				if (success != null) success(data);
			},
			error: function(e) {
				if (error != null) error(e);
			},
			complete: function () {
				if (complete != null) complete();
			}
		});
	};
};

Teleopti.MyTimeWeb.DayScheduleMixin = function () {
	var self = this;

	self.openPeriodStartDate = ko.observable();
	self.openPeriodEndDate = ko.observable();
	self.DatePickerFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);

	self.weekStart = ko.observable();

	var changeHandler = null;

	self.setDayMixinChangeHandler = function (cb) {
		changeHandler = cb;
	};

	var changeHandlerSuspended = false;

	self.suspendDayMixinChangeHandler = function () {
		changeHandlerSuspended = true;
	};

	self.activateDayMixinChangeHandler = function () {
		changeHandlerSuspended = false;
	};


	self.requestedDate = ko.observable(moment().startOf('day'));

	self.setDatePickerFormat = function (format) {
		self.DatePickerFormat(format);
	};

	self.requestedDateWithFormat = ko.computed(function () {
		return self.requestedDate().format(self.DatePickerFormat());
	});

	self.requestedDateInternal = ko.observable();
	self.requestedDate.subscribe(function (newValue) {
		self.requestedDateInternal(Teleopti.MyTimeWeb.Common.FormatServiceDate(newValue));
	});

	self.requestedDateInternal.subscribe(function (newValue) {
		if (changeHandler != null && !changeHandlerSuspended) {
			changeHandler(Teleopti.MyTimeWeb.Common.FormatServiceDate(self.requestedDateInternal()));
		}
	});


	self.isRequestedDateValid = function (date) {
		if (self.openPeriodStartDate() && date.diff(self.openPeriodStartDate()) < 0) {
			return false;
		} else if (self.openPeriodEndDate() && self.openPeriodEndDate().diff(date) < 0) {
			return false;
		}
		return true;
	};

	self.changeRequestedDate = function (movement) {
		var date = moment(self.requestedDate()).add('days', movement);
		if (self.isRequestedDateValid(date)) {
			self.requestedDate(date);
		}
	};

	self.nextDate = function () {
		self.changeRequestedDate(1);
	};

	self.previousDate = function () {
		self.changeRequestedDate(-1);
	};

	self.setDatePickerRange = function (now, relativeStart, relativeEnd) {
		self.openPeriodStartDate(moment(now).add('days', relativeStart));
		self.openPeriodEndDate(moment(now).add('days', relativeEnd));
	};

	self.nextDateValid = ko.computed(function () {
		return self.openPeriodEndDate() == null || self.openPeriodEndDate().diff(self.requestedDate()) > 0;
	});

	self.previousDateValid = ko.computed(function () {
		return self.openPeriodStartDate() == null || self.requestedDate().diff(self.openPeriodStartDate()) > 0;
	});

};

Teleopti.MyTimeWeb.PagingMixin = function () {

	var self = this;
	self.maxPagesVisible = 5;
	self.pageCount = ko.observable(1);
	self.isPreviousMore = ko.observable(false);
	self.maxItemsPerPage = ko.observable(20);
	self.isMore = ko.observable(false);
	self.isPageVisible = ko.observable(false);

	var changeHandler = null;
	self.selectablePages = ko.observableArray();

	self.setPagingMixinChangeHandler = function (cb) {
		changeHandler = cb;
	};

	var changeHandlerSuspended = false;

	self.suspendPagingMixinChangeHandler = function () {
		changeHandlerSuspended = true;
	};

	self.activatePagingMixinChangeHandler = function () {
		changeHandlerSuspended = false;
	};


	self.selectedPageIndex = ko.observable();
	self.selectedPageIndex.subscribe(function () {
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

	self.goToFirstPage = function () {
		self.selectedPageIndex(1);
		self.isPreviousMore(false);
		self.initSelectablePages(self.pageCount());
	};

	self.pageCount.subscribe(function (value) { self.initSelectablePages(value) });

	self.goToLastPage = function () {
		self.isMore(false);
		if (self.pageCount() > self.maxPagesVisible) self.isPreviousMore(true);
		var timesOfNumPerPage = self.pageCount() / self.maxPagesVisible;
		var modeOfNumPerPage = self.pageCount() % self.maxPagesVisible;
		if (timesOfNumPerPage > 0) {
			self.selectablePages.removeAll();
			if (modeOfNumPerPage != 0) {
				for (var i = 1; i <= modeOfNumPerPage; i++) {
					self.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(Math.floor(timesOfNumPerPage) * self.maxPagesVisible + i));
				}
			} else {
				for (var j = 1; j <= self.maxPagesVisible; j++) {
					self.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(self.pageCount() - self.maxPagesVisible + j));
				}
			}
		}
		self.selectedPageIndex(self.pageCount());
	};

	self.initSelectablePages = function (pageCount) {
		self.selectablePages.removeAll();
		for (var i = 1; i <= pageCount; ++i) {
			if (i <= self.maxPagesVisible) {
				self.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(i));
			} else {
				break;
			}
		}
		self.selectedPageIndex(1);
		self.isPreviousMore(false);

		$.each(self.selectablePages(), function (index, item) {
			if (item.index() == self.selectedPageIndex()) {
				item.isSelected(true);
			}
		});

		var currentLastPageNumber = self.selectablePages().length > 0 ? self.selectablePages()[self.selectablePages().length - 1].index() : 0;
		self.isMore(currentLastPageNumber != 0 && currentLastPageNumber < pageCount);
	};

	self.goNextPages = function () {
		var nextPages = [];
		for (var i = 0; i < self.selectablePages().length; ++i) {
			var item = self.selectablePages()[i];
			if ((item.index() + self.maxPagesVisible) <= self.pageCount()) {
				item.index(item.index() + self.maxPagesVisible);
				nextPages.push(item);
			}
		}
		self.selectablePages(nextPages);
		if (nextPages.length === 0 || nextPages.length < self.maxPagesVisible || nextPages[self.maxPagesVisible - 1].index() === self.pageCount()) {
			self.isMore(false);
		}

		if (self.selectablePages()[0].index() > self.maxPagesVisible) self.isPreviousMore(true);
		self.selectedPageIndex(self.selectablePages()[0].index());
	};

	self.goPreviousPages = function () {
		var start = self.selectablePages()[0].index() - self.maxPagesVisible;
		self.selectablePages.removeAll();

		for (var i = 0; i < self.maxPagesVisible; i++) {
			self.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(start + i));
		}

		self.isPreviousMore(self.selectablePages()[0].index() !== 1);
		self.isMore(self.selectablePages()[self.maxPagesVisible - 1].index() < self.pageCount());
		self.selectedPageIndex(self.selectablePages()[self.maxPagesVisible -1 ].index());
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
	self.mySchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel());
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
		if (personSchedule == null) {
			return new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedLayers, moment(), moment(), '', '', false,'', false, false, null);
		}
		var layers = personSchedule.ScheduleLayers;
		var model;

		if (layers == null || layers.length == 0) {
			model = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
				mappedLayers,
				null,
				null,
				personSchedule.Name,
				personSchedule.PersonId,
				personSchedule.IsDayOff,
				personSchedule.DayOffName,
				null,
				personSchedule.IsFullDayAbsence,
				null);
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
				personSchedule.IsDayOff,
				personSchedule.DayOffName,
				null,
				personSchedule.IsFullDayAbsence,
				null
				);
		}
		return model;
	};

	self.CleanTimeHourLine = function () {
		self.hours([]);
	};
	self.createMySchedule = function (mySchedule) {
		self.mySchedule(mapLayerViewModelForSchedule(mySchedule));
		self.showHourLine(mySchedule != null);
	};
	self.createToDrawSchedules = function (personSchedules) {
		self.toDrawSchedules.removeAll();

		var index;
		for (index = 0; index < personSchedules.length; ++index) {
			self.toDrawSchedules.push(mapLayerViewModelForSchedule(personSchedules[index]));
		}
		self.showHourLine(personSchedules.length > 0);
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

	self.redrawLayers = function () {
		var canvasWidth;

		if (!self.IsLoading) {
			canvasWidth = $("td.shift-trade-possible-trade-schedule:visible").width();
			if (canvasWidth == null) canvasWidth = $("td.shift-trade-my-schedule").width();
		} else {
			canvasWidth = self.getCanvasWidth();
		}

		self.layerCanvasPixelWidth(canvasWidth);

		if (self.toDrawSchedules() != undefined || self.mySchedule() != undefined) {
			$.each(self.toDrawSchedules(), function (index, schedule) {
				$.each(schedule.layers, function (index, layer) {
					layer.pixelPerMinute(self.pixelPerMinute());
				});
			});
		}

		if (self.hours() != undefined) {
			$.each(self.hours(), function (index, hour) {
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
	self.refocusToNameSearch = { callable: null };

	self.requestedFilter = ko.computed(function () {

		var selectedTeams = [];

		if (self.selectedTeam() != null) {
			if (self.selectedTeam() == -1) {
				if (self.showGroupings()) {
					var businessHierarchyGroup = undefined;
					var allGroups = self.availableTeams();
					for (var i = 0; i < allGroups.length; i++) {
						var group = allGroups[i];
						// Only get teams from business hierarchy
						if (group.PageId === "6ce00b41-0722-4b36-91dd-0a3b63c545cf") {
							businessHierarchyGroup = group;
							break;
						}
					}

					if (businessHierarchyGroup != undefined) {
						selectedTeams = businessHierarchyGroup.children.map(function (e) {
							return e.id;
						});
					};
				} else {
					var availableTeams = self.availableTeams().slice(1);
					selectedTeams = availableTeams.map(function (v) {
						return v.id;
					});
				}
			} else {
				selectedTeams = [self.selectedTeam()];
			}
		}

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


	self.filterStartEndTimeClick = function () {
		$('.dropdown-menu.filter-time-dropdown-form').on('click', function (e) {
			e.stopPropagation();
		});
	};

	self.cleanTimeFilter = function () {
		self.filteredStartTimesText.removeAll();
		self.filteredEndTimesText.removeAll();

		$.each(self.filterStartTimeList(), function (idx, filter) {
			if (filter.isChecked()) filter.isChecked(false);
		});

		$.each(self.filterEndTimeList(), function (idx, filter) {
			if (filter.isChecked()) filter.isChecked(false);
		});
	};

	self.setTimeFilters = function (data) {
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

	self.setAllStartTimeFilter = ko.observable(false);
	self.setAllStartTimeFilter.subscribe(function (value) {
		self.suspendFilterMixinChangeHandler();
		$.each(self.filterStartTimeList(), function (index, item) { item.isChecked(value); });
		self.activateFilterMixinChangeHandler();
		if (changeHandler) changeHandler();
	});

	self.setAllEndTimeFilter = ko.observable(false);
	self.setAllEndTimeFilter.subscribe(function (value) {
		self.suspendFilterMixinChangeHandler();
		$.each(self.filterEndTimeList(), function (index, item) { item.isChecked(value); });
		self.activateFilterMixinChangeHandler();
		if (changeHandler) changeHandler();

	});

	self.setTeamPicker = function (teams, defaultTeam, allTeam) {
		self.showTeamPicker(teams.length >= 1);
		self.showGroupings(teams[0] && teams[0].children != null);

		if (allTeam !== null) {
			allTeam.id = -1;

			if (teams.length > 1) {
				if (self.showGroupings()) {
					teams.unshift({ children: [allTeam], text: "" });
				} else {
					teams.unshift(allTeam);
				}
			}
		}

		self.availableTeams(teams);

		var isSelectedTeamAllTeam = self.selectedTeam() && self.selectedTeam() == -1;
		var isSelectedTeamNotIncluded = self.showGroupings() ?
			self.availableTeams().reduce(function (v, e) { return v.concat(e.children); }, []).map(function (e) { return e.id; }).indexOf(self.selectedTeam()) < 0 :
			self.availableTeams().map(function (v) { return v.id; }).indexOf(self.selectedTeam()) < 0;

		self.defaultTeam(defaultTeam);

		if (!self.selectedTeam()) {
			self.selectedTeam(self.defaultTeam());
		} else if (isSelectedTeamAllTeam) {
			self.selectedTeam(allTeam.id !== null ? allTeam.id : self.defaultTeam());
		} else if (isSelectedTeamNotIncluded) {
			self.selectedTeam(self.defaultTeam());
		} else {
			self.selectedTeam(self.selectedTeam());
		}
	};

	self.isFiltered = function () {
		if (self.filteredStartTimesText().length == 0 && self.filteredEndTimesText().length == 0 && !self.isDayoffFiltered()) {
			return false;
		}
		return true;
	};

	self.selectedTeam.subscribe(function () {
		if (changeHandler != null && !changeHandlerSuspended) {
			changeHandler();
		}
	});

	self.selectedTeam.extend({ notify: "always" });

	self.filterTime = ko.computed(function () {
		self.filteredStartTimesText.removeAll();
		self.filteredEndTimesText.removeAll();

		$.each(self.filterStartTimeList(), function (idx, timeInFilter) {
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

		$.each(self.filterEndTimeList(), function (idx, timeInFilter) {
			if (timeInFilter.isChecked()) {
				var timeText = timeInFilter.start + ":00-" + timeInFilter.end + ":00";
				self.filteredEndTimesText.push(timeText);
			}
		});

		return { filterStartTimeList: self.filterStartTimeList(), filterEndTimeList: self.filterEndTimeList() };
	});

	self.filterTime.subscribe(function () {
		if (!self.isLocked() && changeHandler != null && !changeHandlerSuspended) {
			changeHandler();
		}
	});

	self.displaySortOrderTemplateList = ko.observable([
		{ Description: 'glyphicon glyphicon-arrow-up', Value: 'start asc', IsStart: true },
		{ Description: 'glyphicon glyphicon-arrow-up', Value: 'end asc', IsStart: false },
		{ Description: 'glyphicon glyphicon-arrow-down', Value: 'start desc', IsStart: true },
		{ Description: 'glyphicon glyphicon-arrow-down', Value: 'end desc', IsStart: false }
	]);

	self.updateTimeSortOrder = function (data) {
		if (self.timeSortOrder() == data.Value) {
			self.timeSortOrder(null);
		} else {
			self.timeSortOrder(data.Value);
		}
	};

	self.isSortingTimeActive = function (value) {
		return self.timeSortOrder() == value.Value;
	}

	self.timeSortOrder.subscribe(function () {
		if (!self.isLocked() && changeHandler != null && !changeHandlerSuspended) {
			changeHandler();
		}
	});

	self.isStartTimeFilterActived = ko.computed(function () {
		return (self.filteredStartTimesText().length != 0 || self.isDayoffFiltered() == true || (self.timeSortOrder() == 'start asc') || (self.timeSortOrder() == 'start desc'));
	});

	self.isEndTimeFilterActived = ko.computed(function () {
		return (self.filteredEndTimesText().length != 0 || self.isDayoffFiltered() == true || (self.timeSortOrder() == 'end asc') || (self.timeSortOrder() == 'end desc'));
	});

	self.changeInSearchBox = function (data, event) {
		var $target = $(event.target);
		self.refocusToNameSearch.callable = function () { $target.focus(); };
		if (changeHandler != null && !changeHandlerSuspended) {
			changeHandler();
		}
	};

};

Teleopti.MyTimeWeb.TeamScheduleDataProviderMixin = function (ajax, endpoints) {
	var self = this;

	self.loadCulture = function (success, error) {
		ajax.Ajax({
			url: "../UserInfo/Culture",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			success: function (data) {
				if (success != null) success(data);
			},
			error: function (e) {
				if (error != null) error(e);
			}
		});
	};

	self.loadCurrentDate = function (success, error) {
		ajax.Ajax({
			url: endpoints.loadCurrentDate,
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			success: function (data) {
				if (success != null) success(data);
			},
			error: function (e) {
				if (error != null) error(e);
			}
		});
	};

	self.loadFilterTimes = function (success, error) {
		if (self.filterStartTimeList().length == 0) {
			ajax.Ajax({
				url: endpoints.loadFilterTimes,
				dataType: "json",
				type: 'GET',
				contentType: 'application/json; charset=utf-8',
				success: function (data) {
					if (success != null) success(data);
				},
				error: function (e) {
					if (error != null) error(e);
				}
			});
		}
	};

	self.loadMyTeam = function (date, success, error) {
		ajax.Ajax({
			url: endpoints.loadMyTeam,
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				date: date
			},
			success: function (data) {
				if (success != null) success(data);
			},
			error: function (e) {
				if (error != null) error(e);
			}
		});
	};

	self.loadDefaultTeam = function (date, success, error) {
		ajax.Ajax({
			url: endpoints.loadDefaultTeam,
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				date: date
			},
			success: function (data) {
				if (success != null) success(data.DefaultTeam);
			},
			error: function (xhr) {
				if (error != null) {
					var errorData = $.parseJSON(xhr.responseText);
					error(errorData);
				}
			}
		});
	};

	self.loadTeams = function (date, success, error) {
		ajax.Ajax({
			url: endpoints.loadTeams,
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				date: date
			},
			success: function (data) {
				if (success != null) success(data);
			},
			error: function (e) {
				if (error != null) error(e);
			}
		});
	};

	self.loadSchedule = function (date, filter, paging, beforeSend, success, error, complete) {
		if (filter.selectedTeams.length === 0) return;

		ajax.Ajax({
			url: endpoints.loadSchedule,
			dataType: "json",
			type: 'POST',
			contentType: 'application/json; charset=utf-8',
			data: JSON.stringify({
				selectedDate: date,
				teamIds: filter.selectedTeams.join(","),
				searchNameText: filter.searchNameText,
				filteredStartTimes: filter.filteredStartTimesText.join(","),
				filteredEndTimes: filter.filteredEndTimesText.join(","),
				isDayOff: filter.isDayoffFiltered,
				Take: paging == null ? 20 : paging.take,
				Skip: paging == null ? 0 : paging.skip,
				TimeSortOrder: filter.timeSortOrder
			}),
			beforeSend: function () {
				if (beforeSend != null) beforeSend();
			},
			success: function (data) {
				if (success != null) success(data);
			},
			error: function (e) {
				if (error != null) error(e);
			},
			complete: function () {
				if (complete != null) complete();
			}
		});
	};
};
/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="jquery.visible.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.js"/>

Teleopti.MyTimeWeb.Request.ShiftTradeViewModel = function (ajax) {
	var self = this;
	self.layerCanvasPixelWidth = ko.observable();
	self.weekStart = ko.observable(1);
	self.openPeriodStartDate = ko.observable(moment().startOf('year').add('days', -1));
	self.openPeriodEndDate = ko.observable(moment().startOf('year').add('days', -1));
	self.missingWorkflowControlSet = ko.observable(true);
	self.noPossibleShiftTrades = ko.observable(false);
	self.hours = ko.observableArray();
	self.mySchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel());
	self.possibleTradeSchedules = ko.observableArray();
	self.possibleTradeSchedulesRaw = [];
	self.agentChoosed = ko.observable(null);
	self.isSendEnabled = ko.observable(true);
	self.IsLoading = ko.observable(false);
	self.errorMessage = ko.observable();
	self.isReadyLoaded = ko.observable(false);
	self.requestedDateInternal = ko.observable(moment().startOf('day').add('days', -1));
	self.myTeamFilter = ko.observable(false);
	self.timeLineStartTime = ko.observable();
	self.timeLineLengthInMinutes = ko.observable();
	self.DatePickerFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);
	self.availableTeams = ko.observableArray();
	self.selectedTeamInternal = ko.observable();
	self.missingMyTeam = ko.observable();
	self.myTeamId = ko.observable();
	self.maxShiftsPerPage = 20;
	self.maxPagesVisible = 5;
	self.selectedPageIndex = ko.observable(1);
	self.pageCount = ko.observable();
	self.selectablePages = ko.observableArray();
	self.isMore = ko.observable(false);
	self.isPreviousMore = ko.observable(false);
	self.isPageVisible = ko.observable(true);
	self.filterStartTimeList = ko.observableArray();
	self.filterEndTimeList = ko.observableArray();
	self.filteredStartTimesText = ko.observableArray();
	self.filteredEndTimesText = ko.observableArray();
	self.isDayoffFiltered = ko.observable(false);
	self.searchNameText = ko.observable();
	self.refocusToNameSearch = null;

	self.isDetailVisible = ko.computed(function () {
		if (self.agentChoosed() === null) {
			return false;
		}
		return true;
	});

	self.changeInSearchBox = function (data, event) {
		var $target = $(event.target);
		self.refocusToNameSearch = function () { $target.focus(); };
		self.loadSchedule(self.getFormattedDateForServiceCall(), self.selectedTeam());
	};


	self.subject = ko.observable();
	self.message = ko.observable();

	self.checkMessageLength = function (data, event) {
		var text = $(event.target)[0].value;
		if (text.length > 2000) {
			self.message(text.substr(0, 2000));
		}
	};


	self.getFormattedDateForDisplay = function () {
		return self.requestedDateInternal().format(self.DatePickerFormat());
	};

	self.getFormattedDateForServiceCall = function () {
		return Teleopti.MyTimeWeb.Common.FormatServiceDate(self.requestedDateInternal());
	};

	self.sortByDate = function () {
		if (self.chooseHistorys().length > 1) {
			self.chooseHistorys.sort(function (a, b) {
				return a.selectedDate() > b.selectedDate();
			});
		}
	};
	self.isPossibleSchedulesForAllEnabled = ko.observable(false);
	self.isTradeForMultiDaysEnabled = ko.observable(false);
	self.isFilterByTimeEnabled = ko.observable(false);
	self.isTeamScheduleSortingFeatureEnabled = ko.observable(false);
	self.chooseHistorys = ko.observableArray();
	self.requestedDates = ko.computed(function () {
		var dates = [];

		if (self.isTradeForMultiDaysEnabled()) {
			$.each(self.chooseHistorys(), function (index, chooseHistoryViewModel) {
				dates.push(Teleopti.MyTimeWeb.Common.FormatServiceDate(chooseHistoryViewModel.selectedDate()));
			});
		} else {
			dates.push(self.getFormattedDateForServiceCall());
		}

		return dates;
	});
	self.selectedInternal = ko.observable(false);
	self.add = function () {
		var currentTrade = {
			date: self.requestedDate(),
			hours: self.hours(),
			mySchedule: null,
			tradedSchedule: null
		};
		var scheduleStartTime;
		var tradedScheduleModel;
		if (self.selectedInternal()) {
			return;
		}
		$.each(self.possibleTradeSchedules(), function (index, schedule) {
			if (self.agentChoosed().agentName == schedule.agentName) {
				if (self.mySchedule().isDayOff && !schedule.isDayOff) {
					scheduleStartTime = schedule.scheduleStartTime();
				} else if (!self.mySchedule().isDayOff && schedule.isDayOff) {
					scheduleStartTime = self.mySchedule().scheduleStartTime();
				} else {
					var myScheduleFormatedStartTime = self.mySchedule().scheduleStartTime() != undefined ? self.mySchedule().scheduleStartTime().format('MMMM Do YYYY, h:mm') : null;
					var myScheduleFormatedEndTime = self.mySchedule().scheduleEndTime() != undefined ? self.mySchedule().scheduleEndTime().format('MMMM Do YYYY, h:mm') : null;
					var tradedScheduleFormatedStartTime = schedule.scheduleStartTime() != undefined ? schedule.scheduleStartTime().format('MMMM Do YYYY, h:mm') : null;
					var tradedScheduleFormatedEndTime = schedule.scheduleEndTime() != undefined ? schedule.scheduleEndTime().format('MMMM Do YYYY, h:mm') : null;
					if (((myScheduleFormatedStartTime == null) || (myScheduleFormatedStartTime == myScheduleFormatedEndTime)) && (tradedScheduleFormatedStartTime != tradedScheduleFormatedEndTime)) {
						scheduleStartTime = schedule.scheduleStartTime();
					}
					else if (((tradedScheduleFormatedStartTime == null) || (tradedScheduleFormatedStartTime == tradedScheduleFormatedEndTime)) && (myScheduleFormatedStartTime != myScheduleFormatedEndTime)) {
						scheduleStartTime = self.mySchedule().scheduleStartTime();
					} else {
						scheduleStartTime = self.mySchedule().scheduleStartTime() < schedule.scheduleStartTime() ? self.mySchedule().scheduleStartTime() : schedule.scheduleStartTime();
					}
				}

				var mappedLayers = ko.utils.arrayMap(schedule.layers, function (layer) {
					var minutesSinceTimeLineStart = moment(layer.startTime).diff(scheduleStartTime, 'minutes');
					return new Teleopti.MyTimeWeb.Request.CloneLayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart);
				});
				tradedScheduleModel = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedLayers, schedule.scheduleStartTime(), schedule.scheduleEndTime(), schedule.agentName, schedule.personId, schedule.isDayOff);

			}
		});

		var mappedlayers = [];
		if (self.mySchedule() != null) {
			mappedlayers = ko.utils.arrayMap(self.mySchedule().layers, function (layer) {
				var minutesSinceTimeLineStart = moment(layer.startTime).diff(scheduleStartTime, 'minutes');
				return new Teleopti.MyTimeWeb.Request.CloneLayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart);
			});
		}
		currentTrade.mySchedule = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, self.mySchedule().scheduleStartTime(), self.mySchedule().scheduleEndTime(), self.mySchedule().agentName, self.mySchedule().personId, self.mySchedule().isDayOff);

		currentTrade.tradedSchedule = tradedScheduleModel;
		var currentChooseView = new Teleopti.MyTimeWeb.Request.ChooseHistoryViewModel(currentTrade, self.layerCanvasPixelWidth());
		self.chooseHistorys.push(currentChooseView);
		self.sortByDate();
		self.selectedInternal(true);
		self.isSendEnabled(true);
	};

	self.remove = function (chooseHistoryViewModel) {
		var date = chooseHistoryViewModel.selectedDate;
		var dayToDelete = $.grep(self.chooseHistorys(), function (e) { return e.selectedDate == date; })[0];
		self.chooseHistorys.remove(dayToDelete);

		if (chooseHistoryViewModel.selectedDateInFormat() == self.getFormattedDateForDisplay()) {
			self.selectedInternal(false);
		}
		if (self.chooseHistorys().length < 1) self.isSendEnabled(false);
	};

	self.isAddVisible = ko.computed(function () {
		var addVisible = false;
		if (self.isDetailVisible() && !self.selectedInternal() && !self.IsLoading()) {
			addVisible = true;
		}
		return addVisible;
	});

	self.filterStartEndTimeClick = function () {
		$('.dropdown-menu').on('click', function (e) {
			if ($(this).hasClass('filter-time-dropdown-form')) {
				e.stopPropagation();
			}
		});
	};

	self.isShowList = ko.computed(function () {
		var showList = false;
		if (self.isDetailVisible() && self.chooseHistorys().length > 0) {
			showList = true;
		}
		return showList;
	});

	self.tradeCartHeight = ko.computed(function () {
		var oneCandidateDayHeight = 110;
		return oneCandidateDayHeight * self.chooseHistorys().length + 'px';
	});

	self.setTimeLineLengthInMinutes = function (firstHour, mins) {
		self.timeLineStartTime(firstHour);
		self.timeLineLengthInMinutes(mins);
	};
	self.pixelPerMinute = ko.computed(function () {
		return self.layerCanvasPixelWidth() / self.timeLineLengthInMinutes();
	});

	self._createMySchedule = function (myScheduleObject) {
		var mappedlayers = [];
		if (myScheduleObject != null && myScheduleObject.ScheduleLayers != null && myScheduleObject.ScheduleLayers.length > 0) {
			var layers = myScheduleObject.ScheduleLayers;
			var scheduleStartTime = moment(layers[0].Start);
			var scheduleEndTime = moment(layers[layers.length - 1].End);
			mappedlayers = ko.utils.arrayMap(layers, function (layer) {
				var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
				return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart, self.pixelPerMinute());
			});
			self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, scheduleStartTime, scheduleEndTime, myScheduleObject.Name, myScheduleObject.PersonId, myScheduleObject.IsDayOff, myScheduleObject.DayOffName,false, myScheduleObject.IsFullDayAbsence,null));
		} else {
			self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, moment(), moment(), '', '', false));
		}
	};

	self._createPossibleTradeSchedules = function (possibleTradeSchedules) {
		self.possibleTradeSchedules.removeAll();
		var mappedPersonsSchedule = ko.utils.arrayMap(possibleTradeSchedules, function (personSchedule) {
			var mappedLayers = [];
			if (personSchedule != null && personSchedule.ScheduleLayers != null && personSchedule.ScheduleLayers.length > 0) {
				var layers = personSchedule.ScheduleLayers;
				var scheduleStartTime = moment(layers[0].Start);
				var scheduleEndTime = moment(layers[layers.length - 1].End);

				mappedLayers = ko.utils.arrayMap(personSchedule.ScheduleLayers, function (layer) {
					var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
					return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart, self.pixelPerMinute());
				});
			}
			var model = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedLayers, scheduleStartTime, scheduleEndTime, personSchedule.Name, personSchedule.PersonId, personSchedule.IsDayOff, personSchedule.DayOffName, false, false, null);
			self.possibleTradeSchedules.push(model);
			return model;
		});

		self.noPossibleShiftTrades(mappedPersonsSchedule.length == 0 ? true : false);
	};

	self.setSendEnableStatus = function () {
		if (self.isTradeForMultiDaysEnabled() && self.chooseHistorys().length < 1) {
			self.isSendEnabled(false);
		} else {
			self.isSendEnabled(true);
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

	self.setAllStartTimeFilter = ko.observable(false);
	self.setAllStartTimeFilter.subscribe(function (value) {
		self.IsLoading(true);
		$.each(self.filterStartTimeList(), function (index, item) { item.isChecked(value); });
		self.IsLoading(false);
		self.prepareLoad();

		self.loadSchedule(self.getFormattedDateForServiceCall(), self.selectedTeamInternal());
	});

	self.setAllEndTimeFilter = ko.observable(false);
	self.setAllEndTimeFilter.subscribe(function (value) {
		self.IsLoading(true);
		$.each(self.filterEndTimeList(), function (index, item) { item.isChecked(value); });
		self.IsLoading(false);
		self.prepareLoad();
		
		self.loadSchedule(self.getFormattedDateForServiceCall(), self.selectedTeamInternal());
	});

	self.chooseAgent = function (agent) {
		//hide or show all agents
		$.each(self.possibleTradeSchedules(), function (index, value) {
			value.isVisible(agent == null);
		});
		if (agent != null) {
			agent.isVisible(true);
			self.redrawLayers();
			//rk - don't really like to put DOM stuff here...
			window.scrollTo(0, 0);
			self.setSendEnableStatus();

			self.cleanTimeFiler();
		}
		self.agentChoosed(agent);
		self.errorMessage('');

		if (self.isTradeForMultiDaysEnabled() && agent != null) {
			self.add();
		}
		if (agent == null) {
			self.selectedInternal(false);
		}
	};

	self.hideShiftTradeWindow = function () {
		$('#Request-add-shift-trade').hide();
	};

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
				PersonToId: self.agentChoosed().personId
			}),
			success: function (data) {
				self.agentChoosed(null);
				self.setSendEnableStatus();
				self.hideShiftTradeWindow();
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var data = $.parseJSON(jqXHR.responseText);
					self.errorMessage(data.Errors.join('</br>'));
					self.setSendEnableStatus();
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	};

	self.sendRequest = function () {
		self.isSendEnabled(false);
		self.saveNewShiftTrade();
		self.chooseAgent(null);
		self.chooseHistorys.removeAll();
		self.goToFirstPage();
	};

	self.cancelRequest = function () {
		self.chooseAgent(null);
		self.chooseHistorys.removeAll();
		self.selectedInternal(false);
		if (self.subject() != undefined) {
			self.subject("");
		}
		if (self.message() != undefined) {
			self.message("");
		}
		self.goToFirstPage();
	};

	self._createTimeLine = function (hours) {
		var firstTimeLineHour = moment(hours[0].StartTime);
		//modify for daylight save close day
		var mins = hours[0].LengthInMinutesToDisplay + (hours.length - 1) * 60;
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

	self.requestedDate = ko.computed({
		read: function () {
			return self.requestedDateInternal();
		},
		write: function (value) {
			if (self.requestedDateInternal().diff(value) == 0) return;
			self.prepareLoad();
			self.requestedDateInternal(value);

			self.loadMyTeamId(self.getFormattedDateForServiceCall());
		}
	});

	self.isFiltered = function () {
		if (self.filteredStartTimesText().length == 0 && self.filteredEndTimesText().length == 0 && !self.isDayoffFiltered()) {
			return false;
		}
		return true;
	};


	self.getAllTeamIds = function () {
		var allTeamIds = [];

		for (var i = 0; i < self.availableTeams().length; ++i) {
			// skip the first one as contains "allTeams"
			if (self.availableTeams()[i].id != "allTeams") {

				allTeamIds.push(self.availableTeams()[i].id);
			}
		}

		return allTeamIds;
	};

	self.loadSchedule = function (date, selectedTeamOption) {

		if (selectedTeamOption == undefined) return;
		if (self.IsLoading()) return;

		var teamIds = (selectedTeamOption == "allTeams") ? self.getAllTeamIds() : [selectedTeamOption];
		var take = self.maxShiftsPerPage;
		var skip = (self.selectedPageIndex() - 1) * take;

		ajax.Ajax({
			url: "Requests/ShiftTradeRequestSchedule",
			dataType: "json",
			type: 'POST',
			contentType: 'application/json; charset=utf-8',
			data: JSON.stringify({
				selectedDate: date,
				teamIds: teamIds.join(","),
				SearchNameText: self.searchNameText(),
				filteredStartTimes: self.filteredStartTimesText().join(","),
				filteredEndTimes: self.filteredEndTimesText().join(","),
				isDayOff: self.isDayoffFiltered(),
				Take: take,
				Skip: skip,
				TimeSortOrder: self.TimeSortOrder()
			}),
			beforeSend: function () {
				self.IsLoading(true);
			},
			success: function (data, textStatus, jqXHR) {
				self.setPagingInfo(data.PageCount);
				self._createTimeLine(data.TimeLineHours);
				self._createMySchedule(data.MySchedule);
				self.setPossibleTradeSchedulesRaw(date, data);
				self._createPossibleTradeSchedules(self.possibleTradeSchedulesRaw);
				self.keepSelectedAgentVisible();
				// Redraw layers after data loaded
				self.redrawLayers();
			},
			error: function (e) {
				//console.log(e);
			},
			complete: function () {
				self.IsLoading(false);
				self.isReadyLoaded(true);
				if (self.refocusToNameSearch != null) {
					self.refocusToNameSearch();
					self.refocusToNameSearch = null;
					self.suppressChangeInSearchBox = false;
				}
			}
		});
	};

	self.selectedTeam = ko.computed({
		read: function () {
			return self.selectedTeamInternal();
		},
		write: function (teamId) {
			if (teamId != null && self.selectedTeamInternal() != null && teamId != self.selectedTeamInternal()) {
				self.chooseAgent(null);
				self.chooseHistorys.removeAll();
			}
			self.selectedTeamInternal(teamId);
			if (teamId == null) return;
			self.prepareLoad();
			self.loadSchedule(self.getFormattedDateForServiceCall(), teamId);
		}
	});

	self.nextDateValid = ko.computed(function () {
		return self.openPeriodEndDate().diff(self.requestedDateInternal()) > 0;
	});

	self.previousDateValid = ko.computed(function () {
		return self.requestedDateInternal().diff(self.openPeriodStartDate()) > 0;
	});

	self.prepareLoad = function () {
		self.possibleTradeSchedulesRaw = [];
		self.selectedPageIndex(1);
		//self.selectablePages.removeAll();
		self.isPreviousMore(false);
		self.isMore(false);
		if (self.agentChoosed() != null && self.isTradeForMultiDaysEnabled()) {
			self.keepSelectedAgentVisible();
		} else
			self.chooseAgent(null);
		self.IsLoading(false);
	};

	self.isRequestedDateValid = function (date) {
		if (date.diff(self.openPeriodStartDate()) < 0) {
			return false;
		} else if (self.openPeriodEndDate().diff(date) < 0) {
			return false;
		}
		return true;
	};

	self.keepSelectedAgentVisible = function () {
		if (self.agentChoosed() != null && self.possibleTradeSchedules() != null) {
			$.each(self.possibleTradeSchedules(), function (index, value) {
				value.isVisible(value.agentName == self.agentChoosed().agentName);
			});
		}

		var isAddAvaiable = false;
		$.each(self.chooseHistorys(), function (index, chooseHistoryViewModel) {
			if (self.getFormattedDateForDisplay() == chooseHistoryViewModel.selectedDateInFormat()) {
				isAddAvaiable = true;
				return false;
			}
		});
		self.selectedInternal(isAddAvaiable);
	};

	self.goToFirstPage = function () {
		self.initSelectablePages(self.pageCount());
	};
	self.goToLastPage = function () {
		var start = Math.floor(self.pageCount() / self.maxPagesVisible) * self.maxPagesVisible + 1;
		if (start > self.pageCount()) start = Math.max(1, start - self.maxPagesVisible);
		self.selectablePages.removeAll();

		for (var i = start; i <= self.pageCount() ; ++i) {
			var page = new Teleopti.MyTimeWeb.Request.PageView(i);
			self.selectablePages.push(page);
		}

		self.isPreviousMore(start !== 1);
		self.isMore(false);
		self.setSelectPage(self.pageCount());
	};


	self.initSelectablePages = function (pageCount) {
		self.selectablePages.removeAll();

		for (var i = 1; i <= Math.min(pageCount, self.maxPagesVisible) ; ++i) {
			var page = new Teleopti.MyTimeWeb.Request.PageView(i);
			self.selectablePages.push(page);
		}

		self.isPreviousMore(false);
		self.isMore(pageCount > self.maxPagesVisible);
		if (pageCount > 0) self.setSelectPage(1);
	};

	self.goNextPages = function () {
		var end = self.selectablePages().slice(-1)[0].index() + self.maxPagesVisible;
		self.selectablePages.removeAll();

		var i;
		for (i = end - self.maxPagesVisible + 1; i <= Math.min(end, self.pageCount()) ; i++) {
			self.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(i));
		}

		self.isPreviousMore(true);
		self.isMore(end < self.pageCount());
		self.setSelectPage(self.selectablePages()[0].index());
	};

	self.goPreviousPages = function () {
		var start = self.selectablePages()[0].index() - self.maxPagesVisible;
		self.selectablePages.removeAll();

		if (start > 0) {
			for (var i = 0; i < self.maxPagesVisible; i++) {
				self.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(start + i));
			}
			self.isPreviousMore(self.selectablePages()[0].index() !== 1);
			self.isMore(self.selectablePages()[self.maxPagesVisible - 1].index() < self.pageCount());
			self.setSelectPage(self.selectablePages()[0].index());
		} else {
			self.goToFirstPage();
		}
	};

	self.isSelected = function (page) {
		return page.index() === self.selectedPageIndex();
	};

	self.selectPage = function (page) {
		self.setSelectPage(page.index());
	};

	self.setSelectPage = function (pageIdx) {
		self.selectedPageIndex(pageIdx);

		self.loadSchedule(self.getFormattedDateForServiceCall(), self.selectedTeamInternal());
	};

	self.loadMyTeamId = function (date) {
		ajax.Ajax({
			url: "Requests/ShiftTradeRequestMyTeam",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				selectedDate: date
			},
			success: function (data, textStatus, jqXHR) {
				if (!data) {
					self.myTeamId(undefined);
					self.missingMyTeam(true);
					self.IsLoading(false);
					self.isReadyLoaded(true);
					return;
				}
				self.missingMyTeam(false);
				self.myTeamId(data);
				self.loadTeams(date);

			},
			error: function (e) {
			}
		});
	};

	self.setTeamAll = function () {
		var text = $("#Request-all-permitted-teams").val() ? $("#Request-all-permitted-teams").val() : "Team All";

		if (self.isPossibleSchedulesForAllEnabled()) {
			self.availableTeams.unshift({ id: "allTeams", text: text });
		}
	};

	self.loadTeams = function (date) {
		var teamToSelect = self.selectedTeamInternal() ? self.selectedTeamInternal() : self.myTeamId();

		ajax.Ajax({
			url: "Team/TeamsForShiftTrade",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				date: date
			},
			success: function (data, textStatus, jqXHR) {
				self.selectedTeam(null);
				self.availableTeams(data);
				self.setTeamAll();
				self.selectedTeam(teamToSelect);

			},
			error: function (e) {
				//console.log(e);
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
				} else {
					self.IsLoading(false);
					self.isReadyLoaded(true);
				}
				self.missingWorkflowControlSet(!data.HasWorkflowControlSet);
			}
		});
	};

	self.pageCount.subscribe(function (value) {
		self.initSelectablePages(value)
	});

	self.setPagingInfo = function (pageCount) {
		self.pageCount(pageCount);
		self.isPageVisible(pageCount > 0);

	};

	self.setPossibleTradeSchedulesRaw = function (date, data) {
		if (self.possibleTradeSchedulesRaw.length > 0) {
			self.possibleTradeSchedulesRaw = [];
		}

		var findTradedAgent = false;
		$.each(data.PossibleTradeSchedules, function (i, item) {
			self.possibleTradeSchedulesRaw.push(item);
			if (self.agentChoosed() && self.isTradeForMultiDaysEnabled()) {
				if (item.Name == self.agentChoosed().agentName) {
					findTradedAgent = true;
				}
			}
		});
		if (self.agentChoosed() && self.isTradeForMultiDaysEnabled()) {
			if (!findTradedAgent && (self.selectedPageIndex() < self.pageCount())) {
				self.selectedPageIndex(self.selectedPageIndex() + 1);
				self.IsLoading(false);
				//keep loading until find the current chosen agent in agent chosen view
				self.loadSchedule(date, self.selectedTeamInternal());
			}
		}
	};

	self.getCanvasWidth = function () {
		var canvasWidth;
		var containerWidth = $("#Request-add-shift-trade").width();
		var nameCellWidth = $("td.shift-trade-agent-name").width();
		canvasWidth = containerWidth - nameCellWidth;
		if (self.isTradeForMultiDaysEnabled()) {
			var buttonAddCellWidth = $("td.shift-trade-button-cell").width();
			canvasWidth = canvasWidth - buttonAddCellWidth;
		}
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
		if (self.chooseHistorys() != undefined) {
			$.each(self.chooseHistorys(), function (index, chooseHistory) {
				chooseHistory.canvasPixelWidth(canvasWidth);
			});
		}
		if (self.hours() != undefined) {
			$.each(self.hours(), function (index, hour) {
				hour.pixelPerMinute(self.pixelPerMinute());
			});
		}
	};

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
	});

	self.filterTime.subscribe(function () {
		if (!self.IsLoading()) {
			self.prepareLoad();

			self.loadSchedule(self.getFormattedDateForServiceCall(), self.selectedTeamInternal());
		}
	});

	self.changeRequestedDate = function (movement) {
		var date = moment(self.requestedDateInternal()).add('days', movement);
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

	self.featureCheck = function () {
		var tradeForMultiDaysEnabled = Teleopti.MyTimeWeb.Common.IsToggleEnabled("Request_ShiftTradeRequestForMoreDays_20918");
		self.isTradeForMultiDaysEnabled(tradeForMultiDaysEnabled);

		var possibleSchedulesForAllEnabled = Teleopti.MyTimeWeb.Common.IsToggleEnabled("Request_SeePossibleShiftTradesFromAllTeams_28770");
		self.isPossibleSchedulesForAllEnabled(possibleSchedulesForAllEnabled);

		var filterByTimeEnabled = Teleopti.MyTimeWeb.Common.IsToggleEnabled("Request_FilterPossibleShiftTradeByTime_24560");
		self.isFilterByTimeEnabled(filterByTimeEnabled);

		self.isTeamScheduleSortingFeatureEnabled(Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_SortSchedule_32092"));
	};

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

	self.loadFilterTimes = function () {
		if (self.filterStartTimeList().length == 0) {
			var dayOffNames = "";
			ajax.Ajax({
				url: "RequestsShiftTradeScheduleFilter/Get",
				dataType: "json",
				type: 'GET',
				contentType: 'application/json; charset=utf-8',
				success: function (data) {
					////set dayoff only in start time filter
					if (data != null) {
						self.setTimeFilters(data.HourTexts);
						dayOffNames += data.DayOffShortNames.join();
						self.filterStartTimeList.push(new Teleopti.MyTimeWeb.Request.FilterStartTimeView(dayOffNames, 0, 24, false, true));
					}
				}
			});
		}
	};

	self.TimeSortOrder = ko.observable(null);
	self.setDatePickerRange = function (now, relativeStart, relativeEnd) {
		self.openPeriodStartDate(moment(now).add('days', relativeStart));
		self.openPeriodEndDate(moment(now).add('days', relativeEnd));
	};

	self.displaySortOrderTemplateList = ko.observable([
		{ Description: 'glyphicon glyphicon-arrow-up', Value: 'start asc', IsStart: true },
		{ Description: 'glyphicon glyphicon-arrow-up', Value: 'end asc', IsStart: false },
		{ Description: 'glyphicon glyphicon-arrow-down', Value: 'start desc', IsStart: true },
		{ Description: 'glyphicon glyphicon-arrow-down', Value: 'end desc', IsStart: false }
	]);

	self.updateTimeSortOrder = function (data) {
		if (self.TimeSortOrder() == data.Value) {
			self.TimeSortOrder(null);
		} else {
			self.TimeSortOrder(data.Value);
		}
	};

	self.isSortingTimeActive = function (value) {
		return self.TimeSortOrder() == value.Value;
	}

	self.TimeSortOrder.subscribe(function () {
		if (!self.IsLoading()) {
			self.prepareLoad();
			
			self.loadSchedule(self.getFormattedDateForServiceCall(), self.selectedTeamInternal());
		}
	});

	self.isStartTimeFilterActived = ko.computed(function () {
		return (self.filteredStartTimesText().length != 0 || self.isDayoffFiltered() == true || (self.TimeSortOrder() == 'start asc') || (self.TimeSortOrder() == 'start desc'));
	});

	self.isEndTimeFilterActived = ko.computed(function () {
		return (self.filteredEndTimesText().length != 0 || (self.TimeSortOrder() == 'end asc') || (self.TimeSortOrder() == 'end desc'));
	});
};

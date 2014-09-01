/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="jquery.visible.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.js"/>


Teleopti.MyTimeWeb.Request.ShiftTradeViewModel = function(ajax) {
	var self = this;
	self.layerCanvasPixelWidth = ko.observable();

	self.now = null;
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
	self.requestedDateInternal = ko.observable(moment().startOf('day'));
	self.myTeamFilter = ko.observable(false);
	self.timeLineStartTime = ko.observable();
	self.timeLineLengthInMinutes = ko.observable();
	self.DatePickerFormat = ko.observable();
	var datePickerFormat = $('#Request-detail-datepicker-format').val() ? $('#Request-detail-datepicker-format').val().toUpperCase() : "YYYY-MM-DD";
	self.DatePickerFormat(datePickerFormat);
	self.availableTeams = ko.observableArray();
	self.selectedTeamInternal = ko.observable();
	self.missingMyTeam = ko.observable();
	self.myTeamId = ko.observable();
	self.maxShiftsPerPage = 20;
	self.selectedPageIndex = ko.observable(1);
	self.pageCount = ko.observable(1);
	self.selectablePages = ko.observableArray();
	self.isMore = ko.observable(false);
	self.isPreviousMore = ko.observable(false);
	self.isPageVisible = ko.observable(true);
	self.filterStartTimeList = ko.observableArray();
	self.filterEndTimeList = ko.observableArray();
	self.filteredStartTimesText = ko.observableArray();
	self.filteredEndTimesText = ko.observableArray();
	self.isDayoffFiltered = ko.observable(false);

	self.isDetailVisible = ko.computed(function() {
		if (self.agentChoosed() === null) {
			return false;
		}
		return true;
	});
	self.subject = ko.observable();
	self.message = ko.observable();

	self.getDateWithFormat = function () {
		return self.requestedDateInternal().format(self.DatePickerFormat());
	};

	self.sortByDate = function() {
		if (self.chooseHistorys().length > 1) {
			self.chooseHistorys.sort(function(a, b) {
				return a.selectedDate() > b.selectedDate();
			});
		}
	};
	self.isPossibleSchedulesForAllEnabled = ko.observable(false);
	self.isTradeForMultiDaysEnabled = ko.observable(false);
	self.isFilterByTimeEnabled = ko.observable(false);
	self.chooseHistorys = ko.observableArray();
	self.requestedDates = ko.computed(function() {
		var dates = [];

		if (self.isTradeForMultiDaysEnabled()) {
			$.each(self.chooseHistorys(), function(index, chooseHistoryViewModel) {
				dates.push(chooseHistoryViewModel.selectedDateInFormat());
			});
		} else {
			dates.push(self.getDateWithFormat());
		}

		return dates;
	});
	self.selectedInternal = ko.observable(false);
	self.add = function() {
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
		$.each(self.possibleTradeSchedules(), function(index, schedule) {
			if (self.agentChoosed().agentName == schedule.agentName) {
				if (self.mySchedule().isDayOff && !schedule.isDayOff) {
					scheduleStartTime = schedule.scheduleStartTime();
				} else if (!self.mySchedule().isDayOff && schedule.isDayOff) {
					scheduleStartTime = self.mySchedule().scheduleStartTime();
				} else {
					scheduleStartTime = self.mySchedule().scheduleStartTime() < schedule.scheduleStartTime() ? self.mySchedule().scheduleStartTime() : schedule.scheduleStartTime();
				}

				var mappedLayers = ko.utils.arrayMap(schedule.layers, function(layer) {
					var minutesSinceTimeLineStart = moment(layer.startTime).diff(scheduleStartTime, 'minutes');
					return new Teleopti.MyTimeWeb.Request.CloneLayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart);
				});
				tradedScheduleModel = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedLayers, schedule.scheduleStartTime(), schedule.scheduleEndTime(), schedule.agentName, schedule.personId, schedule.isDayOff);

			}
		});

		var mappedlayers = [];
		if (self.mySchedule() != null) {
			mappedlayers = ko.utils.arrayMap(self.mySchedule().layers, function(layer) {
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

	self.remove = function(chooseHistoryViewModel) {
		var date = chooseHistoryViewModel.selectedDate;
		var dayToDelete = $.grep(self.chooseHistorys(), function(e) { return e.selectedDate == date; })[0];
		self.chooseHistorys.remove(dayToDelete);

		if (chooseHistoryViewModel.selectedDateInFormat() == self.requestedDate().format(self.DatePickerFormat())) {
			self.selectedInternal(false);
		}
		if (self.chooseHistorys().length < 1) self.isSendEnabled(false);
	};

	self.isAddVisible = ko.computed(function() {
		var addVisible = false;
		if (self.isDetailVisible() && !self.selectedInternal()) {
			addVisible = true;
		}
		return addVisible;
	});

	self.filterStartEndTimeClick = function() {
		$('.dropdown-menu').on('click', function(e) {
			if ($(this).hasClass('filter-time-dropdown-form')) {
				e.stopPropagation();
			}
		});
	};

	self.isShowList = ko.computed(function() {
		var showList = false;
		if (self.isDetailVisible() && self.chooseHistorys().length > 0) {
			showList = true;
		}
		return showList;
	});

	self.tradeCartHeight = ko.computed(function() {
		var oneCandidateDayHeight = 110;
		return oneCandidateDayHeight * self.chooseHistorys().length + 'px';
	});

	self.setTimeLineLengthInMinutes = function(firstHour, lastHour) {
		self.timeLineStartTime(firstHour);
		self.timeLineLengthInMinutes(lastHour.diff(firstHour, 'minutes'));
	};
	self.pixelPerMinute = ko.computed(function() {
		return self.layerCanvasPixelWidth() / self.timeLineLengthInMinutes();
	});

	self._createMySchedule = function(myScheduleObject) {
		var mappedlayers = [];
		if (myScheduleObject != null) {
			var layers = myScheduleObject.ScheduleLayers;
			var scheduleStartTime = moment(layers[0].Start);
			var scheduleEndTime = moment(layers[layers.length - 1].End);
			mappedlayers = ko.utils.arrayMap(layers, function(layer) {
				var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
				return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart, self.pixelPerMinute());
			});
			self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, scheduleStartTime, scheduleEndTime, myScheduleObject.Name, myScheduleObject.PersonId, myScheduleObject.IsDayOff));
		} else {
			self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, moment(), moment(), '', '', false));
		}
	};

	self._createPossibleTradeSchedules = function(possibleTradeSchedules) {
		self.possibleTradeSchedules.removeAll();
		var mappedPersonsSchedule = ko.utils.arrayMap(possibleTradeSchedules, function(personSchedule) {
			var layers = personSchedule.ScheduleLayers;
			var scheduleStartTime = moment(layers[0].Start);
			var scheduleEndTime = moment(layers[layers.length - 1].End);

			var mappedLayers = ko.utils.arrayMap(personSchedule.ScheduleLayers, function(layer) {
				var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
				return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart, self.pixelPerMinute());
			});
			var model = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedLayers, scheduleStartTime, scheduleEndTime, personSchedule.Name, personSchedule.PersonId, personSchedule.IsDayOff);
			self.possibleTradeSchedules.push(model);
			return model;
		});

		self.noPossibleShiftTrades(mappedPersonsSchedule.length == 0 ? true : false);
	};

	self.setSendEnableStatus = function() {
		if (self.isTradeForMultiDaysEnabled() && self.chooseHistorys().length < 1) {
			self.isSendEnabled(false);
		} else {
			self.isSendEnabled(true);
		}
	};

	self.chooseAgent = function(agent) {
		//hide or show all agents
		$.each(self.possibleTradeSchedules(), function(index, value) {
			value.isVisible(agent == null);
		});
		if (agent != null) {
			agent.isVisible(true);
			self.redrawLayers();
			//rk - don't really like to put DOM stuff here...
			window.scrollTo(0, 0);
			self.setSendEnableStatus();
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

	self.hideShiftTradeWindow = function() {
		$('#Request-add-shift-trade').hide();
	};

	self.saveNewShiftTrade = function() {
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
	}

	self.sendRequest = function() {
		self.isSendEnabled(false);
		self.saveNewShiftTrade();
		self.chooseAgent(null);
		self.chooseHistorys.removeAll();
		self.goToFirstPage();
	};

	self.cancelRequest = function() {
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

	self._createTimeLine = function(hours) {
		var firstTimeLineHour = moment(hours[0].StartTime);
		var lastTimeLineHour = moment(hours[hours.length - 1].EndTime);
		self.setTimeLineLengthInMinutes(firstTimeLineHour, lastTimeLineHour);
		self.hours([]);
		if (hours.length < 18) {
			var arrayMap = ko.utils.arrayMap(hours, function(hour) {
				return new Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel(hour, self.timeLineStartTime(), self.pixelPerMinute(), true);
			});

			self.hours.push.apply(self.hours, arrayMap);
		} else {
			for (var i = 0; i < hours.length; i++) {
				var isVisible = (i % 2 != 0);
				var newHour = new Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel(hours[i], self.timeLineStartTime(), self.pixelPerMinute(), isVisible);
				self.hours.push(newHour);
			}
		}
	};

	self.requestedDate = ko.computed({
		read: function() {
			return self.requestedDateInternal();
		},
		write: function(value) {
			//remove old layer's tooltip if it still exist
			$("[class='tooltip fade top in']").remove();

			if (self.requestedDateInternal().diff(value) == 0) return;
			self.prepareLoad();
			self.requestedDateInternal(value);

			self.loadMyTeamId();
		}
	});

	self.isFiltered = function() {
		if (self.filteredStartTimesText().length == 0 && self.filteredEndTimesText().length == 0 && !self.isDayoffFiltered()) {
			return false;
		}
		return true;
	}

	self.getAllTeamIds = function() {
		var allTeamIds = [];

		for (var i = 0; i < self.availableTeams().length - 1; ++i) {
			// skip the last one as contains "allTeams"
			allTeamIds.push(self.availableTeams()[i].id);
		}

		return allTeamIds;
	}
	
	self.loadSchedule = function (teamId) {
		if (teamId != undefined) {
			if (teamId != "allTeams") {
				if (self.isFiltered()) {
					self.loadScheduleForOneTeamFilterTime(teamId);
				} else {
					self.loadOneTeamSchedule(self.getDateWithFormat(), teamId);
				}
			} else {
				if (self.isFiltered()) {
					self.loadScheduleForAllTeamsFilterTime(self.getAllTeamIds());
				} else {
					self.loadScheduleForAllTeams(self.getAllTeamIds());
				}
			}
		}
	};

	self.selectedTeam = ko.computed({
		read: function() {
			return self.selectedTeamInternal();
		},
		write: function(value) {
			if (value != null && self.selectedTeamInternal() != null && value != self.selectedTeamInternal()) {
				self.chooseAgent(null);
				self.chooseHistorys.removeAll();
			}
			self.selectedTeamInternal(value);
			if (value == null) return;
			self.prepareLoad();
			self.loadSchedule(value);
		}
	});

	self.nextDateValid = ko.computed(function() {
		return self.openPeriodEndDate().diff(self.requestedDateInternal()) > 0;
	});

	self.previousDateValid = ko.computed(function() {
		return self.requestedDateInternal().diff(self.openPeriodStartDate()) > 0;
	});

	self.prepareLoad = function() {
		self.possibleTradeSchedulesRaw = [];
		self.selectedPageIndex(1);
		self.selectablePages.removeAll();
		self.isPreviousMore(false);
		self.isMore(false);
		if (self.agentChoosed() != null && self.isTradeForMultiDaysEnabled()) {
			self.keepSelectedAgentVisible();
		} else
			self.chooseAgent(null);
		self.IsLoading(false);
	};

	self.isRequestedDateValid = function(date) {
		if (date.diff(self.openPeriodStartDate()) < 0) {
			return false;
		} else if (self.openPeriodEndDate().diff(date) < 0) {
			return false;
		}
		return true;
	};

	self.keepSelectedAgentVisible = function() {
		if (self.agentChoosed() != null && self.possibleTradeSchedules() != null) {
			$.each(self.possibleTradeSchedules(), function(index, value) {
				value.isVisible(value.agentName == self.agentChoosed().agentName);
			});
		}

		var isAddAvaiable = false;
		$.each(self.chooseHistorys(), function(index, chooseHistoryViewModel) {
			if (self.requestedDateInternal().format(self.DatePickerFormat()) == chooseHistoryViewModel.selectedDateInFormat()) {
				isAddAvaiable = true;
				return false;
			}
		});
		self.selectedInternal(isAddAvaiable);
	};

	self.updateSelectedPage = function() {
		$.each(self.selectablePages(), function(index, item) {
			if (item.index() == self.selectedPageIndex()) {
				item.isSelected(true);
			} else {
				item.isSelected(false);
			}
		});
	};
	self.goToFirstPage = function() {
		self.selectedPageIndex(1);
		self.isPreviousMore(false);
		self.initSelectablePages(self.pageCount());
		self.loadSchedule(self.selectedTeamInternal());
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
		self.setSelectPage(self.pageCount());
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

		self.setSelectPage(self.selectablePages()[0].index());
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

		self.setSelectPage(self.selectablePages()[0].index());
	};

	self.selectPage = function(page) {
		self.setSelectPage(page.index());
	};

	self.setSelectPage = function(pageIdx) {
		self.selectedPageIndex(pageIdx);
		self.loadSchedule(self.selectedTeamInternal());
	};

	self.loadMyTeamId = function() {
		ajax.Ajax({
			url: "Requests/ShiftTradeRequestMyTeam",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				selectedDate: self.requestedDateInternal().format(self.DatePickerFormat())
			},
			success: function(data, textStatus, jqXHR) {
				if (!data) {
					self.myTeamId(undefined);
					self.missingMyTeam(true);
					self.isReadyLoaded(true);
					return;
				}
				self.missingMyTeam(false);
				self.myTeamId(data);
				self.loadTeams();

			},
			error: function(e) {
				//console.log(e);
			}
		});
	};

	self.setTeamAll = function() {
		var text = $("#Request-all-permitted-teams").val() ? $("#Request-all-permitted-teams").val() : "Team All";

		if (self.isPossibleSchedulesForAllEnabled()) {
			self.availableTeams.push({ id: "allTeams", text: text });
		}
	};

	self.loadTeams = function() {
		var teamToSelect = self.selectedTeamInternal() ? self.selectedTeamInternal() : self.myTeamId();

		ajax.Ajax({
			url: "Team/TeamsForShiftTrade",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				date: self.requestedDateInternal().format(self.DatePickerFormat())
			},
			success: function(data, textStatus, jqXHR) {
				self.selectedTeam(null);
				self.availableTeams(data);
				self.setTeamAll();
				self.selectedTeam(teamToSelect);

			},
			error: function(e) {
				//console.log(e);
			}
		});
	};

	self.loadPeriod = function(date) {
		ajax.Ajax({
			url: "Requests/ShiftTradeRequestPeriod",
			dataType: "json",
			type: 'GET',
			success: function(data, textStatus, jqXHR) {
				if (data.HasWorkflowControlSet) {
					self.now = moment(new Date(data.NowYear, data.NowMonth - 1, data.NowDay));
					self.setDatePickerRange(data.OpenPeriodRelativeStart, data.OpenPeriodRelativeEnd);
					var requestedDate = moment(self.now).add('days', data.OpenPeriodRelativeStart);
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


	/*these functions are for schedule loading process only, its logic only according to loading schedule*/
	self.setPageVisiblity = function() {
		if (self.pageCount() == 0) {
			self.isPageVisible(false);
		} else {
			self.isPageVisible(true);
		}
	};

	self.resetSelectablePages = function() {
		if (self.selectablePages().length == 0) {
			self.initSelectablePages(self.pageCount());
		}
	};

	self.setPossibleTradeSchedulesRaw = function(data) {
		if (self.possibleTradeSchedulesRaw.length > 0) {
			self.possibleTradeSchedulesRaw = [];
		}

		var findTradedAgent = false;
		$.each(data.PossibleTradeSchedules, function(i, item) {
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
				self.loadSchedule(self.selectedTeamInternal());
			}
		}
	};

	self.getCanvasWidth = function() {
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

	self.redrawLayers = function() {
		var canvasWidth;

		if (self.isReadyLoaded()) {
			canvasWidth = $("td.shift-trade-possible-trade-schedule:visible").width();
			if (canvasWidth == null) canvasWidth = self.getCanvasWidth();
		} else {
			canvasWidth = self.getCanvasWidth();
		}

		self.layerCanvasPixelWidth(canvasWidth);

		if (self.mySchedule() != undefined) {
			$.each(self.mySchedule().layers, function(index, selfScheduleAddShiftTrade) {
				selfScheduleAddShiftTrade.pixelPerMinute(self.pixelPerMinute());
			});
		}

		if (self.possibleTradeSchedules() != undefined) {
			$.each(self.possibleTradeSchedules(), function(index, selfPersonScheduleAddShiftTrade) {
				$.each(selfPersonScheduleAddShiftTrade.layers, function(index, selfScheduleAddShiftTrade) {
					selfScheduleAddShiftTrade.pixelPerMinute(self.pixelPerMinute());
				});
			});
		}
		if (self.chooseHistorys() != undefined) {
			$.each(self.chooseHistorys(), function(index, chooseHistory) {
				chooseHistory.canvasPixelWidth(canvasWidth);
			});
		}
		if (self.hours() != undefined) {
			$.each(self.hours(), function(index, hour) {
				hour.pixelPerMinute(self.pixelPerMinute());
			});
		}
	};
	/*these functions are for schedule loading process only, its logic only according to loading schedule*/

	self.loadOneTeamSchedule = function(date, teamId) {
		if (self.IsLoading()) return;
		var take = self.maxShiftsPerPage;
		var skip = (self.selectedPageIndex() - 1) * take;

		ajax.Ajax({
			url: "Requests/ShiftTradeRequestSchedule",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				selectedDate: date,
				teamId: teamId,
				Take: take,
				Skip: skip
			},
			beforeSend: function() {
				self.IsLoading(true);
			},
			success: function(data, textStatus, jqXHR) {
				self.pageCount(data.PageCount);
				self.setPageVisiblity();
				self.resetSelectablePages();

				self._createTimeLine(data.TimeLineHours);
				self._createMySchedule(data.MySchedule);

				self.setPossibleTradeSchedulesRaw(data);

				self.updateSelectedPage();
				self._createPossibleTradeSchedules(self.possibleTradeSchedulesRaw);
				self.keepSelectedAgentVisible();
				self.isReadyLoaded(true);

				// Redraw layers after data loaded
				self.redrawLayers();
			},
			error: function(e) {
				//console.log(e);
			},
			complete: function() {
				self.IsLoading(false);
			}
		});
	};

	self.loadScheduleForAllTeams = function (date, allTeamIds) {
		if (self.IsLoading()) return;
		var take = self.maxShiftsPerPage;
		var skip = (self.selectedPageIndex() - 1) * take;

		ajax.Ajax({
			url: "Requests/ShiftTradeRequestScheduleForAllTeams",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				selectedDate: date,
				teamIds: allTeamIds.join(","),
				Take: take,
				Skip: skip
			},
			beforeSend: function() {
				self.IsLoading(true);
			},
			success: function(data, textStatus, jqXHR) {
				self.pageCount(data.PageCount);
				self.setPageVisiblity();
				self.resetSelectablePages();

				self._createTimeLine(data.TimeLineHours);
				self._createMySchedule(data.MySchedule);

				self.setPossibleTradeSchedulesRaw(data);

				self.updateSelectedPage();
				self._createPossibleTradeSchedules(self.possibleTradeSchedulesRaw);
				self.keepSelectedAgentVisible();
				self.isReadyLoaded(true);

				// Redraw layers after data loaded
				self.redrawLayers();
			},
			error: function(e) {
				//console.log(e);
			},
			complete: function() {
				self.IsLoading(false);
			}
		});
	};

	self.loadScheduleForOneTeamFilterTime = function (date, teamId) {
		if (self.IsLoading()) return;
		var take = self.maxShiftsPerPage;
		var skip = (self.selectedPageIndex() - 1) * take;

		ajax.Ajax({
			url: "Requests/ShiftTradeRequestScheduleByFilterTime",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				selectedDate: date,
				teamId: teamId,
				filteredStartTimes: self.filteredStartTimesText().join(","),
				filteredEndTimes: self.filteredEndTimesText().join(","),
				isDayOff: self.isDayoffFiltered(),
				Take: take,
				Skip: skip
			},
			beforeSend: function() {
				self.IsLoading(true);
			},
			success: function(data, textStatus, jqXHR) {
				self.pageCount(data.PageCount);
				self.setPageVisiblity();
				self.resetSelectablePages();

				self._createTimeLine(data.TimeLineHours);
				self._createMySchedule(data.MySchedule);

				self.setPossibleTradeSchedulesRaw(data);

				self.updateSelectedPage();
				self._createPossibleTradeSchedules(self.possibleTradeSchedulesRaw);
				self.keepSelectedAgentVisible();
				self.isReadyLoaded(true);

				// Redraw layers after data loaded
				self.redrawLayers();
			},
			error: function(e) {
				//console.log(e);
			},
			complete: function() {
				self.IsLoading(false);
			}
		});
	};

	self.loadScheduleForAllTeamsFilterTime = function (date, allTeamIds) {
		if (self.IsLoading()) return;
		var take = self.maxShiftsPerPage;
		var skip = (self.selectedPageIndex() - 1) * take;

		ajax.Ajax({
			url: "Requests/ShiftTradeRequestScheduleForAllTeamsByFilterTime",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				selectedDate: date,
				teamIds: allTeamIds.join(","),
				filteredStartTimes: self.filteredStartTimesText().join(","),
				filteredEndTimes: self.filteredEndTimesText().join(","),
				isDayOff: self.isDayoffFiltered(),
				Take: take,
				Skip: skip
			},
			beforeSend: function() {
				self.IsLoading(true);
			},
			success: function(data, textStatus, jqXHR) {
				self.pageCount(data.PageCount);
				self.setPageVisiblity();
				self.resetSelectablePages();

				self._createTimeLine(data.TimeLineHours);
				self._createMySchedule(data.MySchedule);

				self.setPossibleTradeSchedulesRaw(data);

				self.updateSelectedPage();
				self._createPossibleTradeSchedules(self.possibleTradeSchedulesRaw);
				self.keepSelectedAgentVisible();
				self.isReadyLoaded(true);

				// Redraw layers after data loaded
				self.redrawLayers();
			},
			error: function(e) {
				//console.log(e);
			},
			complete: function() {
				self.IsLoading(false);
			}
		});
	};

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
	});

	self.filterTime.subscribe(function () {
		if (self.isFilterByTimeEnabled()) {
			self.prepareLoad();
			self.loadSchedule(self.selectedTeamInternal());
		}
	});

	self.changeRequestedDate = function(movement) {
		var date = moment(self.requestedDateInternal()).add('days', movement);
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

	self.featureCheck = function() {
		ajax.Ajax({
			url: "../ToggleHandler/IsEnabled?toggle=Request_ShiftTradeRequestForMoreDays_20918",
			success: function(data) {
				if (data.IsEnabled) {
					self.isTradeForMultiDaysEnabled(true);
				}
			}
		});

		ajax.Ajax({
			url: "../ToggleHandler/IsEnabled?toggle=Request_SeePossibleShiftTradesFromAllTeams_28770",
			success: function(data) {
				if (data.IsEnabled) {
					self.isPossibleSchedulesForAllEnabled(true);
				}
			}
		});

		ajax.Ajax({
			url: "../ToggleHandler/IsEnabled?toggle=Request_FilterPossibleShiftTradeByTime_24560",
			success: function(data) {
				if (data.IsEnabled) {
					self.isFilterByTimeEnabled(true);
				}
			}
		});
	};

	self.setTimeFilters = function(hourTexts) {
		self.filterStartTimeList.removeAll();
		self.filterEndTimeList.removeAll();
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

	self.loadFilterTimes = function() {
		var dayOffNames = "";

		ajax.Ajax({
			url: "RequestsShiftTradeScheduleFilter/Get",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			success: function(data) {
				//set dayoff only in start time filter
				if (data != null) {
					self.setTimeFilters(data.HourTexts);
					$.each(data.DayOffShortNames, function(idx, name) {
						if (idx < data.DayOffShortNames.length - 1) dayOffNames += name + ", ";
						else dayOffNames += name;
					});
					self.filterStartTimeList.push(new Teleopti.MyTimeWeb.Request.FilterStartTimeView(dayOffNames, 0, 24, false, true));
				}
			}
		});
	};

	self.setDatePickerRange = function(relativeStart, relativeEnd) {
		self.openPeriodStartDate(moment(self.now).add('days', relativeStart));
		self.openPeriodEndDate(moment(self.now).add('days', relativeEnd));
	};
};
﻿/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="jquery.visible.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.js"/>

Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel = function (ajax) {
	var self = this;
	self.layerCanvasPixelWidth = ko.observable();
	self.weekStart = ko.observable(1);
	self.openPeriodStartDate = ko.observable(moment().startOf('year').add('days', -1));
	self.openPeriodEndDate = ko.observable(moment().startOf('year').add('days', -1));
	self.missingWorkflowControlSet = ko.observable(false);
	self.noPossibleShiftTrades = ko.observable(false);
	self.hours = ko.observableArray();
	self.mySchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel());
	self.possibleTradeSchedules = ko.observableArray();
	self.possibleTradeSchedulesRaw = [];
	self.agentChoosed = ko.observable(null);
	self.isSendEnabled = ko.observable(true);
    self.IsLoading = ko.observable(false); 
    self.IsLoadingWhenChangingDate = ko.observable(false);
	self.errorMessage = ko.observable();
	self.isReadyLoaded = ko.observable(false);
	self.requestedDateInternal = ko.observable(moment().startOf('day').add('days', -1));
	self.myTeamFilter = ko.observable(false);
	self.timeLineStartTime = ko.observable();
	self.timeLineLengthInMinutes = ko.observable();
	self.DatePickerFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);
	self.availableSites = ko.observableArray();
	self.availableTeams = ko.observableArray();
	self.selectedSite = ko.observable();
	self.selectedSiteInternal = ko.observable();
	self.selectedTeamInternal = ko.observable();
	self.noAnyTeamToShow = ko.observable(false);
	self.noAnySiteToShow = ko.observable(false);
	self.myTeamId = ko.observable();
	self.mySiteId = ko.observable();
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
	self.allTeamsId = "allTeams";
	self.allSitesId = "allSites";
	self.earliestScheduleDate = null;
	self.lastScheduleDate = null;

	self.shiftPageSize = 7;
	self.isLoadingSchedulesOnTop = ko.observable(false);
	self.isLoadingSchedulesOnBottom = ko.observable(false);

	self.myScheduleList = ko.observableArray();
	self.targetScheduleList = ko.observableArray();

	self.loadedSchedulePairs = ko.observableArray();
	self.loadedSchedulePairsRaw = [];

	self.chooseHistorys = ko.observableArray();
	self.isDetailVisible = ko.computed(function () {
		return self.agentChoosed() !== null;
	});

	self.subject = ko.observable();
	self.message = ko.observable();
	self.requestedDates = ko.computed(function () {
		var dates = [];

		$.each(self.chooseHistorys(), function (index, chooseHistoryViewModel) {
			dates.push(Teleopti.MyTimeWeb.Common.FormatServiceDate(chooseHistoryViewModel.selectedDate()));
		});

		return dates;
	});


	self.formatTime = function (dateTime) {
		return Teleopti.MyTimeWeb.Common.FormatTime(dateTime);
	}

	self.scrolled = function (data, event) {
		var element = event.target;
		var wholeHeight = element.scrollHeight;
		var scrollTop = element.scrollTop;
		var divHeight = element.clientHeight;

		if (!self.isLoadingSchedulesOnTop() && !self.isLoadingSchedulesOnBottom()) {
			//scroll to bottom
			if ((scrollTop + divHeight >= wholeHeight)) {
				var agentId = self.agentChoosed().personId;
				var startDate = self.lastScheduleDate;
				var openPeriodEnd = self.openPeriodEndDate();
				if (startDate.isSame(openPeriodEnd,'day')) {
					return;
				}
				var endDate = moment(self.lastScheduleDate).add("days", self.shiftPageSize);
				if (endDate > openPeriodEnd) {
					endDate = openPeriodEnd;
				}


				self.isLoadingSchedulesOnBottom(true);
				loadPeriodSchedule(startDate, endDate, agentId, false);
			}

			//scroll to top
			if (scrollTop === 0) {
				var agentId = self.agentChoosed().personId;
				var endDate = self.earliestScheduleDate;
				var openPeriodStart = self.openPeriodStartDate();
				if (moment(endDate).isSame(openPeriodStart,'day')) {
					return;
				}

				var startDate = moment(self.earliestScheduleDate).add("days", -self.shiftPageSize);
				if (startDate < openPeriodStart) {
					startDate = openPeriodStart
				}

				self.isLoadingSchedulesOnTop(true);
				loadPeriodSchedule(startDate, endDate, agentId, true);
			}
		}
	}

	
	self.dateChanged = ko.observable(false);
	self.requestedDate = ko.computed({
		read: function () {
			return self.requestedDateInternal();
		},
		write: function (value) {
			if (self.requestedDateInternal().diff(value) === 0) return;
			self.dateChanged(true);
			self.prepareLoad();
			self.requestedDateInternal(value);
			self.loadMySiteId(getFormattedDateForServiceCall());
		}
	});

	self.selectedInternal = ko.observable(false);

	var timeSortOrder = ko.observable(null);
	timeSortOrder.subscribe(function () {
		if (!self.IsLoading()) {
			self.prepareLoad();

			loadSchedule(getFormattedDateForServiceCall(), self.selectedTeamInternal());
		}
	});

	self.changeInSearchBox = function (data, event) {
		var $target = $(event.target);
		self.refocusToNameSearch = function () { $target.focus(); };
		loadSchedule(getFormattedDateForServiceCall(), self.selectedTeam());
	};

	self.checkMessageLength = function (data, event) {
		var text = $(event.target)[0].value;
		if (text.length > 2000) {
			self.message(text.substr(0, 2000));
		}
	};

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
			if (self.agentChoosed().agentName === schedule.agentName) {
				if (self.mySchedule().isDayOff && !schedule.isDayOff) {
					scheduleStartTime = schedule.scheduleStartTime();
				} else if (!self.mySchedule().isDayOff && schedule.isDayOff) {
					scheduleStartTime = self.mySchedule().scheduleStartTime();
				} else {
					var myScheduleFormatedStartTime = self.mySchedule().scheduleStartTime() !== undefined ? self.mySchedule().scheduleStartTime().format('MMMM Do YYYY, HH:mm') : null;
					var myScheduleFormatedEndTime = self.mySchedule().scheduleEndTime() !== undefined ? self.mySchedule().scheduleEndTime().format('MMMM Do YYYY, HH:mm') : null;
					var tradedScheduleFormatedStartTime = schedule.scheduleStartTime() !== undefined ? schedule.scheduleStartTime().format('MMMM Do YYYY, HH:mm') : null;
					var tradedScheduleFormatedEndTime = schedule.scheduleEndTime() !== undefined ? schedule.scheduleEndTime().format('MMMM Do YYYY, HH:mm') : null;
					if ((myScheduleFormatedStartTime === null || myScheduleFormatedStartTime === myScheduleFormatedEndTime) && tradedScheduleFormatedStartTime !== tradedScheduleFormatedEndTime) {
						scheduleStartTime = schedule.scheduleStartTime();
					}
					else if ((tradedScheduleFormatedStartTime === null || tradedScheduleFormatedStartTime === tradedScheduleFormatedEndTime) && myScheduleFormatedStartTime !== myScheduleFormatedEndTime) {
						scheduleStartTime = self.mySchedule().scheduleStartTime();
					} else {
						scheduleStartTime = self.mySchedule().scheduleStartTime() < schedule.scheduleStartTime() ? self.mySchedule().scheduleStartTime() : schedule.scheduleStartTime();
					}
				}

				var mappedLayers = ko.utils.arrayMap(schedule.layers, function (layer) {
					var minutesSinceTimeLineStart = moment(layer.startTime).diff(scheduleStartTime, 'minutes');
					return new Teleopti.MyTimeWeb.Request.CloneLayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart);
				});
				tradedScheduleModel = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedLayers,
					schedule.scheduleStartTime(), schedule.scheduleEndTime(), schedule.agentName, schedule.personId,
					schedule.isDayOff, schedule.dayOffName, schedule.isEmptyDay, schedule.isFullDayAbsence,
					schedule.ShiftExchangeOfferId, schedule.contractTime);
			}
		});

		var mappedlayers = [];
		if (self.mySchedule() !== null) {
			mappedlayers = ko.utils.arrayMap(self.mySchedule().layers, function (layer) {
				var minutesSinceTimeLineStart = moment(layer.startTime).diff(scheduleStartTime, 'minutes');
				return new Teleopti.MyTimeWeb.Request.CloneLayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart);
			});
		}
		currentTrade.mySchedule = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers,
			self.mySchedule().scheduleStartTime(), self.mySchedule().scheduleEndTime(), self.mySchedule().agentName,
			self.mySchedule().personId, self.mySchedule().isDayOff, self.mySchedule().dayOffName,
			self.mySchedule().isEmptyDay, self.mySchedule().isFullDayAbsence, self.mySchedule().ShiftExchangeOfferId,
			self.mySchedule().contractTime);
		currentTrade.tradedSchedule = tradedScheduleModel;

		var currentChooseView = new Teleopti.MyTimeWeb.Request.ChooseHistoryViewModel(currentTrade, self.layerCanvasPixelWidth());
		self.chooseHistorys.push(currentChooseView);
		sortChooseHistoryByDate();
		self.selectedInternal(true);
		self.isSendEnabled(true);
	};

	self.remove = function (chooseHistoryViewModel) {
		var date = chooseHistoryViewModel.selectedDate;
		var dayToDelete = $.grep(self.chooseHistorys(), function (e) { return e.selectedDate === date; })[0];
		self.chooseHistorys.remove(dayToDelete);

		if (chooseHistoryViewModel.selectedDateInFormat() === getFormattedDateForDisplay()) {
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

	self.pixelPerMinute = ko.computed(function () {
		return self.layerCanvasPixelWidth() / self.timeLineLengthInMinutes();
	});

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

	self.chooseAgent = function (agent) {
		//hide or show all agents
		var showAllAgent = agent === null;
		//$.each(self.possibleTradeSchedules(), function (index, value) {
		//	value.isVisible(showAllAgent);
		//});
		if (!showAllAgent) {
			agent.isVisible(true);
			redrawLayers();
			//rk - don't really like to put DOM stuff here...
			window.scrollTo(0, 0);
			setSendEnableStatus();

			self.searchNameText(agent.agentName);
			self.cleanTimeFiler();
		}
		self.agentChoosed(agent);
		self.errorMessage('');

		if (showAllAgent) {
			self.selectedInternal(false);
		} else {
			self.add();
		}

		if (agent == null) {
			return;
		}

		self.lastScheduleDate = self.requestedDateInternal();
		self.earliestScheduleDate = self.requestedDateInternal();

		var startDate = self.lastScheduleDate;
		var endDate = moment(self.lastScheduleDate).add("days", self.shiftPageSize);
		var agentId = agent.personId;
		clearSchedulePairs();
		loadPeriodSchedule(startDate, endDate, agentId, false, function () {
			document.querySelector('.shift-trade-list-panel').scrollTo(0, 10);
		});
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
				setSendEnableStatus();
				self.hideShiftTradeWindow();
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status === 400) {
					var data = $.parseJSON(jqXHR.responseText);
					self.errorMessage(data.Errors.join('</br>'));
					setSendEnableStatus();
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
		if (self.subject() !== undefined) {
			self.subject("");
		}
		if (self.message() !== undefined) {
			self.message("");
		}

		if(self.searchNameText().length == 0) {
			self.goToFirstPage();
		} else {
			self.searchNameText("");
			loadSchedule(getFormattedDateForServiceCall(), self.selectedTeamInternal());
		}
	};

	self.isFiltered = function () {
		if (self.filteredStartTimesText().length === 0 && self.filteredEndTimesText().length === 0 && !self.isDayoffFiltered()) {
			return false;
		}
		return true;
	};

	self.selectedTeam = ko.computed({
		read: function () {
			return self.selectedTeamInternal();
		},
		write: function (teamId) {
			if (teamId !== null && self.selectedTeamInternal() !== null && teamId !== self.selectedTeamInternal()) {
				self.chooseAgent(null);
				self.chooseHistorys.removeAll();
			}
			self.selectedTeamInternal(teamId);
			if (teamId === null) return;
			self.prepareLoad();
			loadSchedule(getFormattedDateForServiceCall(), teamId);
		}
	});

	self.selectedSite = ko.computed({
		read: function () {
			return self.selectedSiteInternal();
		},
		write: function (siteId) {
			self.selectedSiteInternal(siteId);
			if (self.selectedTeam() == null || self.dateChanged()) return;
			self.selectedTeamInternal(self.allTeamsId);
			self.loadTeamsUnderSite(siteId, getFormattedDateForServiceCall());
		}
	});

	self.nextDateValid = ko.computed(function () {
		return self.openPeriodEndDate().diff(self.requestedDateInternal()) > 0;
	});

	self.previousDateValid = ko.computed(function () {
		return self.requestedDateInternal().diff(self.openPeriodStartDate()) > 0;
	});

	self.nextDateEnable = ko.computed(function () {
		 if(!self.nextDateValid()){
            return false;
		 }else{  
             return !self.IsLoadingWhenChangingDate();
		 }
	});

	self.previousDateEnable = ko.computed(function () {
		  if(!self.previousDateValid()){
            return false;
		 }else{  
              return !self.IsLoadingWhenChangingDate();
		 }
	});

	self.prepareLoad = function () {
		self.possibleTradeSchedulesRaw = [];
		self.selectedPageIndex(1);
		//self.selectablePages.removeAll();
		self.isPreviousMore(false);
		self.isMore(false);
		if (self.agentChoosed() !== null) {
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
		if (self.agentChoosed() !== null && self.possibleTradeSchedules() !== null) {
			$.each(self.possibleTradeSchedules(), function (index, value) {
				value.isVisible(value.agentName === self.agentChoosed().agentName);
			});
		}

		var isAddAvaiable = false;
		$.each(self.chooseHistorys(), function (index, chooseHistoryViewModel) {
			if (getFormattedDateForDisplay() === chooseHistoryViewModel.selectedDateInFormat()) {
				isAddAvaiable = true;
				return false;
			}
		});
		self.selectedInternal(isAddAvaiable);
	};

	self.goToFirstPage = function () {
		if (self.selectedPageIndex() === 1) return;
		self.initSelectablePages(self.pageCount());
	};
	self.goToLastPage = function () {
		if (self.pageCount() === self.selectedPageIndex()) return;
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
		if (page.index() === self.selectedPageIndex()) return;
		self.setSelectPage(page.index());
	};

	self.setSelectPage = function (pageIdx) {
		self.selectedPageIndex(pageIdx);

		loadSchedule(getFormattedDateForServiceCall(), self.selectedTeamInternal());
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
					self.IsLoading(false);
					self.isReadyLoaded(true);
					tryLoadAllAvialableTeams(date);
					return;
				}
				self.myTeamId(data);
				tryLoadAllAvialableTeams(date);
			}
		});
	};

	self.loadMySiteId = function (date) {
		ajax.Ajax({
			url: "Requests/ShiftTradeRequestMySite",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				selectedDate: date
			},
			success: function (data, textStatus, jqXHR) {
				if (!data) {
					self.mySiteId(undefined);
					self.IsLoading(false);
					self.isReadyLoaded(true);
					self.loadSites(date);
					return;
				}
				self.noAnySiteToShow(false);
				self.mySiteId(data);
				self.loadSites(date);
			}
		});
	}

	self.loadSites = function (date) {
		var siteToSelect = self.selectedSiteInternal() ? self.selectedSiteInternal() : self.mySiteId();

		ajax.Ajax({
			url: "Team/SitesForShiftTrade",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				date: date
			},
			success: function (data, textStatus, jqXHR) {
				self.selectedSite(null);
				self.availableSites(data);
				if (data && data.length > 0) {
					setSiteAll();
					if (siteToSelect == undefined) siteToSelect = self.allSitesId;
				}
				if (siteToSelect !== undefined) self.selectedSite(siteToSelect);
				else self.noAnySiteToShow(true);
				self.loadMyTeamId(getFormattedDateForServiceCall());
			}
		});
	};

	self.setTeamAll = function () {
		var text = $("#Request-all-permitted-teams").val() ? $("#Request-all-permitted-teams").val() : "Team All";
		self.availableTeams.unshift({ id: self.allTeamsId, text: text });
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
				if (data && data.length > 0) {
					self.setTeamAll();
					if (teamToSelect == undefined) teamToSelect = self.allTeamsId;
				}
				if (teamToSelect !== undefined) self.selectedTeam(teamToSelect);
				else self.noAnyTeamToShow(true);
			}
		});
	};

	self.loadTeamsUnderSite = function (siteId, date) {
		var siteIds = (siteId === self.allSitesId) ? getAllSiteIds() : [siteId];
		var teamToSelect = self.selectedTeamInternal() ? self.selectedTeamInternal() : self.myTeamId();

		ajax.Ajax({
			url: "Team/TeamsUnderSiteForShiftTrade",
			dataType: "json",
			type: 'POST',
			contentType: 'application/json; charset=utf-8',
			data: JSON.stringify({
				siteIds: siteIds.join(","),
				date: date
			}),
			success: function (data, textStatus, jqXHR) {
				self.selectedTeam(null);
				self.availableTeams(data);
				if (data && data.length > 0) {
					self.setTeamAll();
					if (teamToSelect == undefined) teamToSelect = self.allTeamsId;
				}
				if (teamToSelect !== undefined) self.selectedTeam(teamToSelect);
				else self.noAnyTeamToShow(true);
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
					setDatePickerRange(now, data.OpenPeriodRelativeStart, data.OpenPeriodRelativeEnd);
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
		self.initSelectablePages(value);
	});

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

			loadSchedule(getFormattedDateForServiceCall(), self.selectedTeamInternal());
		}
	});

    self.nextDate = function () {
        self.IsLoadingWhenChangingDate(true);
		changeRequestedDate(1);
	};

    self.previousDate = function () {
        self.IsLoadingWhenChangingDate(true);
		changeRequestedDate(-1);
	};

	self.loadFilterTimes = function () {
		if (self.filterStartTimeList().length === 0) {
			var dayOffNames = "";
			ajax.Ajax({
				url: "RequestsShiftTradeScheduleFilter/Get",
				dataType: "json",
				type: 'GET',
				contentType: 'application/json; charset=utf-8',
				success: function (data) {
					////set dayoff only in start time filter
					if (data !== null) {
						setTimeFilters(data.HourTexts);
						dayOffNames += data.DayOffShortNames.join();
						self.filterStartTimeList.push(new Teleopti.MyTimeWeb.Request.FilterStartTimeView(dayOffNames, 0, 24, false, true));
					}
				}
			});
		}
	};

	self.updateTimeSortOrder = function (data) {
		if (timeSortOrder() === data.Value) {
			timeSortOrder(null);
		} else {
			timeSortOrder(data.Value);
		}
	};

	self.displaySortOrderTemplateList = ko.observable([
		{ Description: 'glyphicon glyphicon-arrow-up', Value: 'start asc', IsStart: true },
		{ Description: 'glyphicon glyphicon-arrow-up', Value: 'end asc', IsStart: false },
		{ Description: 'glyphicon glyphicon-arrow-down', Value: 'start desc', IsStart: true },
		{ Description: 'glyphicon glyphicon-arrow-down', Value: 'end desc', IsStart: false }
	]);

	self.isSortingTimeActive = function (value) {
		return timeSortOrder() === value.Value;
	};

	self.formatDate = function (date) {
		return date.format(self.DatePickerFormat());
	}

	self.formatTime = function (dateTime) {
		return dateTime.format(Teleopti.MyTimeWeb.Common.TimeFormat);
	}

	self.isStartTimeFilterActived = ko.computed(function () {
		return (self.filteredStartTimesText().length !== 0 || self.isDayoffFiltered() === true || (timeSortOrder() === 'start asc') || (timeSortOrder() === 'start desc'));
	});

	self.isEndTimeFilterActived = ko.computed(function () {
		return (self.filteredEndTimesText().length !== 0 || (timeSortOrder() === 'end asc') || (timeSortOrder() === 'end desc'));
	});

	function changeRequestedDate(movement) {
		var date = moment(self.requestedDateInternal()).add('days', movement);
		if (self.isRequestedDateValid(date)) {
			self.requestedDate(date);
		}
	}

	function setDatePickerRange(now, relativeStart, relativeEnd) {
		self.openPeriodStartDate(moment(now).add('days', relativeStart));
		self.openPeriodEndDate(moment(now).add('days', relativeEnd));
	}

	function setTimeFilters(hourTexts) {
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
	}

	function setSiteAll() {
		var text = $("#Request-all-permitted-sites").val() ? $("#Request-all-permitted-sites").val() : "All Sites";
		self.availableSites.unshift({ id: self.allSitesId, text: text });
	}

	function tryLoadAllAvialableTeams(date) {
		self.loadTeamsUnderSite(self.selectedSite(), date);
	}

	function getFormattedDateForDisplay() {
		return self.requestedDateInternal().format(self.DatePickerFormat());
	}

	function getFormattedDateForServiceCall() {
		return Teleopti.MyTimeWeb.Common.FormatServiceDate(self.requestedDateInternal());
	}

	function sortChooseHistoryByDate () {
		if (self.chooseHistorys().length > 1) {
			self.chooseHistorys.sort(function (a, b) {
				return a.selectedDate() > b.selectedDate();
			});
		}
	}

	function getAllSiteIds() {
		return self.availableSites().filter(function (site) { return site.id !== self.allSitesId; }).map(function (site) { return site.id; });
	}

	function getAllTeamIds() {
		return self.availableTeams().filter(function (team) { return team.id !== self.allTeamsId; }).map(function (team) { return team.id; });
	}

	function loadPeriodSchedule(startDate, endDate, agentId, prepend, callback) {
		if (!startDate || !endDate || !agentId) {
			return;
		}

		ajax.Ajax({
			url: 'Requests/ShiftTradeMultiDaysSchedule',
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			data: JSON.stringify({
				StartDate: startDate,
				EndDate: endDate,
				PersonToId: agentId
			}),
			success: function (data) {
				var mySchedules = [],
					targetSchedules = [],
					schedulePairs = [],
					dateInRange = endDate.clone(),
					previousFirstRowId;

				if (data.MySchedules && data.MySchedules.length > 0) {
					mySchedules = createShiftTradeSchedules(data.MySchedules);
				}

				if (data.PersonToSchedules && data.PersonToSchedules.length > 0) {
					targetSchedules = createShiftTradeSchedules(data.PersonToSchedules);
				}

				while (!dateInRange.isSame(startDate, 'day')) {
					var mySche = mySchedules.filter(filterSchedules)[0];
					if (mySche && mySche.isNotScheduled) {
						mySche = null;
					}

					var tarSche = targetSchedules.filter(filterSchedules)[0];
					if (tarSche && tarSche.isNotScheduled) {
						tarSche = null;
					}
					schedulePairs.unshift({
						date: dateInRange.clone(),
						mySchedule: mySche,
						targetSchedule: tarSche
					});
					dateInRange.add("days", -1);
				}

				if (prepend) {
					previousFirstRowId = '#shift-row-' + self.loadedSchedulePairs()[0].date.valueOf();
					self.loadedSchedulePairs.unshift.apply(self.loadedSchedulePairs, schedulePairs);
					setTimeout(restoreScroll);
				} else {
					self.loadedSchedulePairs.push.apply(self.loadedSchedulePairs, schedulePairs);
				}

				if (prepend) {
					self.earliestScheduleDate = startDate;
				} else {
					self.lastScheduleDate = endDate;
				}

				self.isLoadingSchedulesOnTop(false);
				self.isLoadingSchedulesOnBottom(false);

				function filterSchedules(sched) {
					return sched.date.isSame(dateInRange, 'day');
				}

				function restoreScroll() {
					document.querySelector('.shift-trade-list-panel').scrollTop = document.querySelector(previousFirstRowId).offsetTop;
				}

				if (callback) {
					callback();
				}
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.isLoadingSchedulesOnTop(false);
				self.isLoadingSchedulesOnBottom(false);
				if (jqXHR.status === 400) {
					var data = $.parseJSON(jqXHR.responseText);
					self.errorMessage(data.Errors.join('</br>'));
					setSendEnableStatus();
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}

	function loadSchedule(date, selectedTeamOption) {
		if (selectedTeamOption === undefined) return;
		if (self.IsLoading()) return;

		var teamIds = (selectedTeamOption === self.allTeamsId) ? getAllTeamIds() : [selectedTeamOption];
		var take = self.maxShiftsPerPage;
		var skip = (self.selectedPageIndex() - 1) * take;


		var scheduleReloaded = false;
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
				TimeSortOrder: timeSortOrder()
			}),
			beforeSend: function () {
				self.IsLoading(true);
			},
			success: function (data, textStatus, jqXHR) {
				setPagingInfo(data.PageCount);
				createTimeLine(data.TimeLineHours);
				createMySchedule(data.MySchedule);
				setPossibleTradeSchedulesRaw(date, data);
				createPossibleTradeSchedules(self.possibleTradeSchedulesRaw);
				self.keepSelectedAgentVisible();
				scheduleReloaded = true;
			},
			complete: function () {
                self.IsLoading(false);
                self.IsLoadingWhenChangingDate(false);
                self.isReadyLoaded(true);
				self.dateChanged(false);
				if (self.refocusToNameSearch !== null) {
					self.refocusToNameSearch();
					self.refocusToNameSearch = null;
					self.suppressChangeInSearchBox = false;
				}
				if (scheduleReloaded) {
					// Redraw layers after data loaded
					redrawLayers();
				}
			}
		});
	}

	function createMySchedule(myScheduleObject) {
		var mappedlayers = [];
		if (myScheduleObject !== null && myScheduleObject !== undefined
			&& myScheduleObject.ScheduleLayers !== null && myScheduleObject.ScheduleLayers.length > 0) {
			var layers = myScheduleObject.ScheduleLayers;
			var scheduleStartTime = moment(layers[0].Start);
			var scheduleEndTime = moment(layers[layers.length - 1].End);
			mappedlayers = ko.utils.arrayMap(layers, function (layer) {
				var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
				return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart, self.pixelPerMinute());
			});
			self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, scheduleStartTime, scheduleEndTime, myScheduleObject.Name, myScheduleObject.PersonId,
				myScheduleObject.IsDayOff, myScheduleObject.DayOffName, false, myScheduleObject.IsFullDayAbsence, null, Teleopti.MyTimeWeb.Common.FormatTimeSpan(myScheduleObject.ContractTimeInMinute)));
		} else if (myScheduleObject !== null) {
			self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, moment(), moment(), '', '', myScheduleObject.IsDayOff, myScheduleObject.DayOffName,
				false, myScheduleObject.IsFullDayAbsence, null, Teleopti.MyTimeWeb.Common.FormatTimeSpan(myScheduleObject.ContractTimeInMinute)));
		} else {
			self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, moment(), moment(), '', '', false, '', '0:00'));
		}
	}

	function createPossibleTradeSchedules(possibleTradeSchedules) {
		self.possibleTradeSchedules.removeAll();
		var mappedPersonsSchedule = ko.utils.arrayMap(possibleTradeSchedules, function (personSchedule) {
			var mappedLayers = [];
			if (personSchedule !== null && personSchedule.ScheduleLayers !== null && personSchedule.ScheduleLayers.length > 0) {
				var layers = personSchedule.ScheduleLayers;
				var scheduleStartTime = moment(layers[0].Start);
				var scheduleEndTime = moment(layers[layers.length - 1].End);

				mappedLayers = ko.utils.arrayMap(personSchedule.ScheduleLayers, function (layer) {
					var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
					return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart, self.pixelPerMinute());
				});
			}
			var model = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedLayers, scheduleStartTime, scheduleEndTime, personSchedule.Name,
				personSchedule.PersonId, personSchedule.IsDayOff, personSchedule.DayOffName, false, false, null, Teleopti.MyTimeWeb.Common.FormatTimeSpan(personSchedule.ContractTimeInMinute));
			self.possibleTradeSchedules.push(model);
			return model;
		});

		self.noPossibleShiftTrades(mappedPersonsSchedule.length === 0 ? true : false);
	}


	function createShiftTradeSchedules(shiftTradeSchedules) {
		var models = ko.utils.arrayMap(shiftTradeSchedules, function (personSchedule) {
			var mappedLayers = [];
			var startDateTime = moment(personSchedule.StartTimeUtc);
			if (personSchedule !== null && personSchedule.ScheduleLayers !== null && personSchedule.ScheduleLayers.length > 0) {
				var layers = personSchedule.ScheduleLayers;
				var scheduleStartTime = moment(layers[0].Start);
				var scheduleEndTime = moment(layers[layers.length - 1].End);
				var scheduleLength = scheduleEndTime.diff(scheduleStartTime, 'minutes');

				mappedLayers = ko.utils.arrayMap(personSchedule.ScheduleLayers, function (layer) {
					var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
					var offsetFromScheduleStart = moment(layer.Start).diff(scheduleStartTime, 'minutes');
					return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart, self.pixelPerMinute(), offsetFromScheduleStart, scheduleLength);
				});
			}

			if (personSchedule && personSchedule.ShiftCategory) {
				var categoryName = personSchedule.ShiftCategory.Name;
				var categoryColor = personSchedule.ShiftCategory.DisplayColor;
			}

			var model = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedLayers, scheduleStartTime, scheduleEndTime, personSchedule.Name,
				personSchedule.PersonId, personSchedule.IsDayOff, personSchedule.DayOffName, false, false, null,
				Teleopti.MyTimeWeb.Common.FormatTimeSpan(personSchedule.ContractTimeInMinute), personSchedule.IsNotScheduled,
				startDateTime, categoryName, categoryColor);

			return model;
		});

		return models;
	}

	function setPossibleTradeSchedulesRaw(date, data) {
		if (self.possibleTradeSchedulesRaw.length > 0) {
			self.possibleTradeSchedulesRaw = [];
		}

		var findTradedAgent = false;
		$.each(data.PossibleTradeSchedules, function (i, item) {
			self.possibleTradeSchedulesRaw.push(item);
			if (self.agentChoosed()) {
				if (item.Name === self.agentChoosed().agentName) {
					findTradedAgent = true;
				}
			}
		});
		if (self.agentChoosed()) {
			if (!findTradedAgent && (self.selectedPageIndex() < self.pageCount())) {
				self.selectedPageIndex(self.selectedPageIndex() + 1);
				self.IsLoading(false);
				//keep loading until find the current chosen agent in agent chosen view
				loadSchedule(date, self.selectedTeamInternal());
			}
		}
	}

	function setSendEnableStatus() {
		if (self.chooseHistorys().length < 1) {
			self.isSendEnabled(false);
		} else {
			self.isSendEnabled(true);
		}
	}

	function setTimeLineLengthInMinutes(firstHour, mins) {
		self.timeLineStartTime(firstHour);
		self.timeLineLengthInMinutes(mins);
	}

	function createTimeLine(hours) {
		var firstTimeLineHour = moment(hours[0].StartTime);
		//modify for daylight save close day
		var mins = hours[0].LengthInMinutesToDisplay + (hours.length - 1) * 60;
		setTimeLineLengthInMinutes(firstTimeLineHour, mins);
		self.hours([]);
		var start = moment(hours[1].StartTime);
		var diff = start.diff(self.timeLineStartTime(), 'minutes');

		for (var i = 0; i < hours.length; i++) {
			var isVisible = hours.length < 18 ? true : (i % 2 !== 0);
			var newHour = new Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel(i, hours[i],
				diff, self.pixelPerMinute(), isVisible);
			self.hours.push(newHour);
		}
	}

	function redrawLayers() {
		var canvasWidth;

		if (self.isReadyLoaded()) {
			canvasWidth = $("td.shift-trade-my-schedule:visible").width();
			if (canvasWidth === null) canvasWidth = getCanvasWidth();
		} else {
			canvasWidth = getCanvasWidth();
		}

		self.layerCanvasPixelWidth(canvasWidth);

		if (self.mySchedule() !== undefined) {
			$.each(self.mySchedule().layers, function (index, selfScheduleAddShiftTrade) {
				selfScheduleAddShiftTrade.pixelPerMinute(self.pixelPerMinute());
			});
		}

		if (self.possibleTradeSchedules() !== undefined) {
			$.each(self.possibleTradeSchedules(), function (index, selfPersonScheduleAddShiftTrade) {
				$.each(selfPersonScheduleAddShiftTrade.layers, function (index, selfScheduleAddShiftTrade) {
					selfScheduleAddShiftTrade.pixelPerMinute(self.pixelPerMinute());
				});
			});
		}
		if (self.chooseHistorys() !== undefined) {
			$.each(self.chooseHistorys(), function (index, chooseHistory) {
				chooseHistory.canvasPixelWidth(canvasWidth);
			});
		}
		if (self.hours() !== undefined) {
			$.each(self.hours(), function (index, hour) {
				hour.pixelPerMinute(self.pixelPerMinute());
			});
		}
	}

	function getCanvasWidth() {
		var canvasWidth;
		var containerWidth = $("#Request-add-shift-trade").width();
		var nameCellWidth = $("td.shift-trade-agent-name").width();
		canvasWidth = containerWidth - nameCellWidth;

		var buttonAddCellWidth = $("td.shift-trade-button-cell").width();
		canvasWidth = canvasWidth - buttonAddCellWidth;

		return canvasWidth;
	}

	function setPagingInfo(pageCount) {
		self.pageCount(pageCount);
		self.isPageVisible(pageCount > 0);
	}

	function clearSchedulePairs() {
		self.loadedSchedulePairs([]);
	}

	self.displayView = function () {
		return 'new-shift-trade-request-panel-74947';
	};
};
Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel = function(ajax) {
	var self = this;
	self.layerCanvasPixelWidth = ko.observable();
	self.weekStart = ko.observable(1);
	self.openPeriodStartDate = ko.observable(
		moment()
			.startOf('year')
			.add('days', -1)
	);
	self.openPeriodEndDate = ko.observable(
		moment()
			.startOf('year')
			.add('days', -1)
	);
	self.missingWorkflowControlSet = ko.observable(false);
	self.noPossibleShiftTrades = ko.observable(false);
	self.hours = ko.observableArray();
	self.mySchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel());
	self.possibleTradeSchedules = ko.observableArray();
	self.possibleTradeSchedulesRaw = [];
	self.agentChoosed = ko.observable(null);
	self.IsLoading = ko.observable(false);
	self.IsLoadingWhenChangingDate = ko.observable(false);
	self.errorMessage = ko.observable();
	self.isReadyLoaded = ko.observable(false);
	self.requestedDateInternal = ko.observable(
		moment()
			.startOf('day')
			.add('days', -1)
	);
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
	self.allTeamsId = 'allTeams';
	self.allSitesId = 'allSites';
	self.earliestScheduleDate = null;
	self.lastScheduleDate = null;

	self.toloranceInvalid = ko.observable(false);

	self.shiftPageSize = 15;
	self.isLoadingSchedules = ko.observable(false);
	self.isLoadingSchedulesOnTop = ko.observable(false);
	self.isLoadingSchedulesOnBottom = ko.observable(false);

	self.myScheduleList = ko.observableArray();
	self.targetScheduleList = ko.observableArray();

	self.loadedSchedulePairs = ko.observableArray();

	self.loadedSchedulePairsRaw = [];

	self.selectedSchedulePairs = ko.observableArray();

	self.myToleranceMessages = ko.observableArray();
	self.targetToleranceMessages = ko.observableArray();

	self.showToloranceMessageDetail = ko.observable(false);

	self.showToloranceMessage = ko.computed(function() {
		return self.myToleranceMessages().length > 0 || self.targetToleranceMessages().length > 0;
	});

	self.selectedCount = ko.computed(function() {
		return self.selectedSchedulePairs().length;
	});

	self.isSendEnabled = ko.computed(function() {
		return self.selectedSchedulePairs().length > 0 && !self.showToloranceMessage();
	});

	self.isDetailVisible = ko.computed(function() {
		return self.agentChoosed() !== null;
	});

	self.subject = ko.observable();
	self.message = ko.observable();

	self.isMobile = Teleopti.MyTimeWeb.Common.IsHostAMobile();
	self.isiPad = Teleopti.MyTimeWeb.Common.IsHostAniPad();
	self.isDesktop = !self.isMobile && !self.isiPad;

	self.listCartToggle = ko.observable(true);

	self.showCartPanel = ko.observable(self.isiPad || self.isDesktop);

	self.resetStatus = function() {
		self.listCartToggle(true);
		self.selectedSchedulePairs([]);
	};

	self.select = function(data) {
		if (data) {
			if (data.isSelected()) {
				self.selectedSchedulePairs.push(data);
			} else {
				self.selectedSchedulePairs.remove(function(d) {
					return d.date.isSame(data.date, 'day');
				});
			}

			self.selectedSchedulePairs.sort(function(a, b) {
				if (a.date.isSame(b, 'day')) {
					return 0;
				}

				if (a.date.isBefore(b.date, 'day')) {
					return -1;
				}

				if (a.date.isAfter(b.date, 'day')) {
					return 1;
				}
			});
			validateTolorance();
		}

		return true;
	};

	self.cartMenuClick = function() {
		self.showCartPanel(!self.showCartPanel());
	};

	self.previousPage = function() {
		if (self.showCartPanel()) return self.showCartPanel(!self.showCartPanel());

		Teleopti.MyTimeWeb.Request.HideFab(false);
		return self.cancelRequest();
	};

	self.removeSelect = function(data) {
		if (data) {
			data.isSelected(false);
			self.selectedSchedulePairs.remove(function(d) {
				return d.date.isSame(data.date, 'day');
			});

			validateTolorance();
		}
	};

	self.getOvernightFlag = function(dateTime, scheduleDate) {
		if (dateTime && scheduleDate) {
			if (dateTime.isBefore(scheduleDate, 'day')) return '(-1)';
			if (dateTime.isAfter(scheduleDate, 'day')) return ' +1';
		}

		return '';
	};

	self.getOvertimeAndIntradayAbsenceText = function(personSchedule, otText) {
		if (personSchedule.isIntradayAbsence || personSchedule.hasOvertime) {
			var result = '(';
			if (personSchedule.hasOvertime) {
				result += otText;
			}

			if (personSchedule.isIntradayAbsence) {
				result +=
					result === '('
						? personSchedule.absenceCategoryShortName
						: ', ' + personSchedule.absenceCategoryShortName;
			}

			result += ')';

			return result;
		}

		return '';
	};

	self.scrolled = function(data, event) {
		var element = event.target;
		var wholeHeight = element.scrollHeight;
		var scrollTop = element.scrollTop;
		var divHeight = element.clientHeight;

		if (!self.isLoadingSchedulesOnTop() && !self.isLoadingSchedulesOnBottom() && !self.isLoadingSchedules()) {
			//scroll to bottom
			if (scrollTop + divHeight >= wholeHeight - 5) {
				var agentId = self.agentChoosed().personId;
				var startDate = self.lastScheduleDate;
				var openPeriodEnd = self.openPeriodEndDate();
				if (startDate.isSame(openPeriodEnd, 'day')) {
					return;
				}
				var endDate = moment(self.lastScheduleDate).add('days', self.shiftPageSize);
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
				if (moment(endDate).isSame(openPeriodStart, 'day')) {
					return;
				}

				var startDate = moment(self.earliestScheduleDate).add('days', -self.shiftPageSize);
				if (startDate < openPeriodStart) {
					startDate = openPeriodStart;
				}

				self.isLoadingSchedulesOnTop(true);
				loadPeriodSchedule(startDate, endDate, agentId, true);
			}
		}
	};

	self.dateChanged = ko.observable(false);
	self.requestedDate = ko.computed({
		read: function() {
			return self.requestedDateInternal();
		},
		write: function(value) {
			if (self.requestedDateInternal().diff(value) === 0) return;
			self.dateChanged(true);
			self.prepareLoad();
			self.requestedDateInternal(value);
			self.loadMySiteId(getFormattedDateForServiceCall());
		}
	});

	self.selectedInternal = ko.observable(false);

	var timeSortOrder = ko.observable(null);
	timeSortOrder.subscribe(function() {
		if (!self.IsLoading()) {
			self.prepareLoad();

			loadSchedule(getFormattedDateForServiceCall(), self.selectedTeamInternal());
		}
	});

	self.changeInSearchBox = function(data, event) {
		var $target = $(event.target);
		self.refocusToNameSearch = function() {
			$target.focus();
		};
		loadSchedule(getFormattedDateForServiceCall(), self.selectedTeam());
	};

	self.checkMessageLength = function(data, event) {
		var text = $(event.target)[0].value;
		if (text.length > 2000) {
			self.message(text.substr(0, 2000));
		}
	};

	self.filterStartEndTimeClick = function() {
		$('.dropdown-menu').on('click', function(e) {
			if ($(this).hasClass('filter-time-dropdown-form')) {
				e.stopPropagation();
			}
		});
	};

	self.pixelPerMinute = ko.computed(function() {
		return self.layerCanvasPixelWidth() / self.timeLineLengthInMinutes();
	});

	self.cleanTimeFiler = function() {
		self.filteredStartTimesText.removeAll();
		self.filteredEndTimesText.removeAll();

		$.each(self.filterStartTimeList(), function(idx, filter) {
			if (filter.isChecked()) filter.isChecked(false);
		});

		$.each(self.filterEndTimeList(), function(idx, filter) {
			if (filter.isChecked()) filter.isChecked(false);
		});
	};

	self.chooseAgent = function(agent) {
		self.myToleranceMessages([]);
		self.targetToleranceMessages([]);

		self.showToloranceMessageDetail(false);

		if (agent != null) Teleopti.MyTimeWeb.Request.HideFab(true);

		if (!Teleopti.MyTimeWeb.Common.IsHostAMobile()) self.showCartPanel(true);

		self.selectedSchedulePairs([]);

		//hide or show all agents
		var showAllAgent = agent === null;
		if (!showAllAgent) {
			agent.isVisible(true);
			redrawLayers();
			//rk - don't really like to put DOM stuff here...
			window.scrollTo(0, 0);

			self.cleanTimeFiler();
		}
		self.agentChoosed(agent);
		self.errorMessage('');

		if (showAllAgent) {
			self.selectedInternal(false);
		}

		if (agent == null) {
			return;
		}

		self.lastScheduleDate = self.requestedDateInternal();
		self.earliestScheduleDate = self.requestedDateInternal();

		var startDate = self.lastScheduleDate;
		var endDate = moment(self.lastScheduleDate).add('days', self.shiftPageSize);

		var openPeriodEnd = self.openPeriodEndDate();
		if (endDate > openPeriodEnd) {
			endDate = openPeriodEnd;
		}
		var agentId = agent.personId;
		clearSchedulePairs();

		loadPeriodSchedule(startDate, endDate, agentId, false, function() {
			var element = document.querySelector('.multi-shift-trade-schedules-list-panel');
			if (element) {
				element.scrollTop = 2;
			}

			// select the date when choose an agent
			var item = self.loadedSchedulePairs().filter(function(pair) {
				return pair.date.isSame(self.requestedDateInternal(), 'day');
			})[0];

			loadToleranceInfo(agentId, function() {
				if (item && item.isEnable) {
					item.isSelected(true);
					self.select(item);
				}
			});
		});

		self.isLoadingSchedulesOnBottom(true);
	};

	self.hideShiftTradeWindow = function() {
		$('#Request-add-shift-trade').hide();
	};

	self.saveNewShiftTrade = function() {
		var dates = [];
		self.selectedSchedulePairs().forEach(function(p) {
			dates.push(Teleopti.MyTimeWeb.Common.FormatServiceDate(p.date));
		});

		ajax.Ajax({
			url: 'Requests/ShiftTradeRequest',
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			data: JSON.stringify({
				Dates: dates,
				Subject: self.subject(),
				Message: self.message(),
				PersonToId: self.agentChoosed().personId
			}),
			success: function(data) {
				self.resetStatus();
				self.agentChoosed(null);
				self.hideShiftTradeWindow();
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data);
				Teleopti.MyTimeWeb.Request.HideFab(false);
			},
			error: function(jqXHR, textStatus, errorThrown) {
				if (jqXHR.status === 400) {
					var data = $.parseJSON(jqXHR.responseText);
					self.errorMessage(data.Errors.join('</br>'));
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	};

	self.sendRequest = function() {
		self.saveNewShiftTrade();
		self.chooseAgent(null);
		self.showCartPanel(false);
		Teleopti.MyTimeWeb.Request.List.HideRequests(false);
		self.goToFirstPage();
		Teleopti.MyTimeWeb.Request.ResetToolbarActiveButtons();
		Teleopti.MyTimeWeb.Request.ActiveRequestList();
	};

	self.cancelRequest = function() {
		self.resetStatus();
		self.chooseAgent(null);
		self.selectedInternal(false);
		self.showCartPanel(false);

		Teleopti.MyTimeWeb.Request.HideFab(false);

		if (self.subject() !== undefined) {
			self.subject('');
		}
		if (self.message() !== undefined) {
			self.message('');
		}

		self.searchNameText('');
		loadSchedule(getFormattedDateForServiceCall(), self.selectedTeamInternal());
	};

	self.isFiltered = function() {
		if (
			self.filteredStartTimesText().length === 0 &&
			self.filteredEndTimesText().length === 0 &&
			!self.isDayoffFiltered()
		) {
			return false;
		}
		return true;
	};

	self.selectedTeam = ko.computed({
		read: function() {
			return self.selectedTeamInternal();
		},
		write: function(teamId) {
			if (teamId !== null && self.selectedTeamInternal() !== null && teamId !== self.selectedTeamInternal()) {
				self.chooseAgent(null);
			}
			self.selectedTeamInternal(teamId);
			if (teamId === null) return;
			self.prepareLoad();
			loadSchedule(getFormattedDateForServiceCall(), teamId);
		}
	});

	self.selectedSite = ko.computed({
		read: function() {
			return self.selectedSiteInternal();
		},
		write: function(siteId) {
			self.selectedSiteInternal(siteId);
			if (self.selectedTeam() == null || self.dateChanged()) return;
			self.selectedTeamInternal(self.allTeamsId);
			self.loadTeamsUnderSite(siteId, getFormattedDateForServiceCall());
		}
	});

	self.nextDateValid = ko.computed(function() {
		return self.openPeriodEndDate().diff(self.requestedDateInternal()) > 0;
	});

	self.previousDateValid = ko.computed(function() {
		return self.requestedDateInternal().diff(self.openPeriodStartDate()) > 0;
	});

	self.nextDateEnable = ko.computed(function() {
		if (!self.nextDateValid()) {
			return false;
		} else {
			return !self.IsLoadingWhenChangingDate();
		}
	});

	self.previousDateEnable = ko.computed(function() {
		if (!self.previousDateValid()) {
			return false;
		} else {
			return !self.IsLoadingWhenChangingDate();
		}
	});

	self.prepareLoad = function() {
		self.possibleTradeSchedulesRaw = [];
		self.selectedPageIndex(1);
		self.isPreviousMore(false);
		self.isMore(false);
		if (self.agentChoosed() !== null) {
		} else self.chooseAgent(null);
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

	self.goToFirstPage = function() {
		if (self.selectedPageIndex() === 1) return;
		self.initSelectablePages(self.pageCount());
	};
	self.goToLastPage = function() {
		if (self.pageCount() === self.selectedPageIndex()) return;
		var start = Math.floor(self.pageCount() / self.maxPagesVisible) * self.maxPagesVisible + 1;
		if (start > self.pageCount()) start = Math.max(1, start - self.maxPagesVisible);
		self.selectablePages.removeAll();

		for (var i = start; i <= self.pageCount(); ++i) {
			var page = new Teleopti.MyTimeWeb.Request.PageView(i);
			self.selectablePages.push(page);
		}

		self.isPreviousMore(start !== 1);
		self.isMore(false);
		self.setSelectPage(self.pageCount());
	};

	self.initSelectablePages = function(pageCount) {
		self.selectablePages.removeAll();

		for (var i = 1; i <= Math.min(pageCount, self.maxPagesVisible); ++i) {
			var page = new Teleopti.MyTimeWeb.Request.PageView(i);
			self.selectablePages.push(page);
		}

		self.isPreviousMore(false);
		self.isMore(pageCount > self.maxPagesVisible);
		if (pageCount > 0) self.setSelectPage(1);
	};

	self.goNextPages = function() {
		var end =
			self
				.selectablePages()
				.slice(-1)[0]
				.index() + self.maxPagesVisible;
		self.selectablePages.removeAll();

		var i;
		for (i = end - self.maxPagesVisible + 1; i <= Math.min(end, self.pageCount()); i++) {
			self.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(i));
		}

		self.isPreviousMore(true);
		self.isMore(end < self.pageCount());
		self.setSelectPage(self.selectablePages()[0].index());
	};

	self.goPreviousPages = function() {
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

	self.isSelected = function(page) {
		return page.index() === self.selectedPageIndex();
	};

	self.selectPage = function(page) {
		if (page.index() === self.selectedPageIndex()) return;
		self.setSelectPage(page.index());
	};

	self.setSelectPage = function(pageIdx) {
		self.selectedPageIndex(pageIdx);

		loadSchedule(getFormattedDateForServiceCall(), self.selectedTeamInternal());
	};

	self.loadMyTeamId = function(date) {
		ajax.Ajax({
			url: 'Requests/ShiftTradeRequestMyTeam',
			dataType: 'json',
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				selectedDate: date
			},
			success: function(data, textStatus, jqXHR) {
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

	self.loadMySiteId = function(date) {
		ajax.Ajax({
			url: 'Requests/ShiftTradeRequestMySite',
			dataType: 'json',
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				selectedDate: date
			},
			success: function(data, textStatus, jqXHR) {
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
	};

	self.loadSites = function(date) {
		var siteToSelect = self.selectedSiteInternal() ? self.selectedSiteInternal() : self.mySiteId();

		ajax.Ajax({
			url: 'Team/SitesForShiftTrade',
			dataType: 'json',
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				date: date
			},
			success: function(data, textStatus, jqXHR) {
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

	self.setTeamAll = function() {
		var text = $('#Request-all-permitted-teams').val() ? $('#Request-all-permitted-teams').val() : 'Team All';
		self.availableTeams.unshift({ id: self.allTeamsId, text: text });
	};

	self.loadTeams = function(date) {
		var teamToSelect = self.selectedTeamInternal() ? self.selectedTeamInternal() : self.myTeamId();

		ajax.Ajax({
			url: 'Team/TeamsForShiftTrade',
			dataType: 'json',
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				date: date
			},
			success: function(data, textStatus, jqXHR) {
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

	self.loadTeamsUnderSite = function(siteId, date) {
		var siteIds = siteId === self.allSitesId ? getAllSiteIds() : [siteId];
		var teamToSelect = self.selectedTeamInternal() ? self.selectedTeamInternal() : self.myTeamId();

		ajax.Ajax({
			url: 'Team/TeamsUnderSiteForShiftTrade',
			dataType: 'json',
			type: 'POST',
			contentType: 'application/json; charset=utf-8',
			data: JSON.stringify({
				siteIds: siteIds.join(','),
				date: date
			}),
			success: function(data, textStatus, jqXHR) {
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

	self.loadPeriod = function(date) {
		ajax.Ajax({
			url: 'Requests/ShiftTradeRequestPeriod',
			dataType: 'json',
			type: 'GET',
			success: function(data, textStatus, jqXHR) {
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

	self.pageCount.subscribe(function(value) {
		self.initSelectablePages(value);
	});

	self.filterTime = ko.computed(function() {
		self.filteredStartTimesText.removeAll();
		self.filteredEndTimesText.removeAll();

		$.each(self.filterStartTimeList(), function(idx, timeInFilter) {
			if (timeInFilter.isChecked()) {
				if (timeInFilter.isDayOff()) {
					self.isDayoffFiltered(true);
				} else {
					var timeText = timeInFilter.start + ':00-' + timeInFilter.end + ':00';
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
				var timeText = timeInFilter.start + ':00-' + timeInFilter.end + ':00';
				self.filteredEndTimesText.push(timeText);
			}
		});
	});

	self.filterTime.subscribe(function() {
		if (!self.IsLoading()) {
			self.prepareLoad();

			loadSchedule(getFormattedDateForServiceCall(), self.selectedTeamInternal());
		}
	});

	self.nextDate = function() {
		self.IsLoadingWhenChangingDate(true);
		changeRequestedDate(1);
	};

	self.previousDate = function() {
		self.IsLoadingWhenChangingDate(true);
		changeRequestedDate(-1);
	};

	self.loadFilterTimes = function() {
		if (self.filterStartTimeList().length === 0) {
			var dayOffNames = '';
			ajax.Ajax({
				url: 'RequestsShiftTradeScheduleFilter/Get',
				dataType: 'json',
				type: 'GET',
				contentType: 'application/json; charset=utf-8',
				success: function(data) {
					////set dayoff only in start time filter
					if (data !== null) {
						setTimeFilters(data.HourTexts);
						dayOffNames += data.DayOffShortNames.join();
						self.filterStartTimeList.push(
							new Teleopti.MyTimeWeb.Request.FilterStartTimeView(dayOffNames, 0, 24, false, true)
						);
					}
				}
			});
		}
	};

	self.updateTimeSortOrder = function(data) {
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

	self.isSortingTimeActive = function(value) {
		return timeSortOrder() === value.Value;
	};

	self.formatDate = function(date) {
		return date.format(self.DatePickerFormat());
	};

	self.getTextColor = function(backgroundColor) {
		if (!backgroundColor) {
			return 'none';
		}
		return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
	};

	self.isStartTimeFilterActived = ko.computed(function() {
		return (
			self.filteredStartTimesText().length !== 0 ||
			self.isDayoffFiltered() === true ||
			timeSortOrder() === 'start asc' ||
			timeSortOrder() === 'start desc'
		);
	});

	self.isEndTimeFilterActived = ko.computed(function() {
		return (
			self.filteredEndTimesText().length !== 0 || timeSortOrder() === 'end asc' || timeSortOrder() === 'end desc'
		);
	});

	self.toloranceMessageClicked = function() {
		self.showToloranceMessageDetail(!self.showToloranceMessageDetail());
	};

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
			var hourText = hourTexts[i] + ' - ' + hourTexts[i + 2];
			var filterStartTime = new Teleopti.MyTimeWeb.Request.FilterStartTimeView(
				hourText,
				rangStart,
				rangEnd,
				false,
				false
			);
			var filterEndTime = new Teleopti.MyTimeWeb.Request.FilterEndTimeView(hourText, rangStart, rangEnd, false);
			self.filterStartTimeList.push(filterStartTime);
			self.filterEndTimeList.push(filterEndTime);
			rangStart += 2;
		}
	}

	function setSiteAll() {
		var text = $('#Request-all-permitted-sites').val() ? $('#Request-all-permitted-sites').val() : 'All Sites';
		self.availableSites.unshift({ id: self.allSitesId, text: text });
	}

	function tryLoadAllAvialableTeams(date) {
		self.loadTeamsUnderSite(self.selectedSite(), date);
	}

	function getFormattedDateForServiceCall() {
		return Teleopti.MyTimeWeb.Common.FormatServiceDate(self.requestedDateInternal());
	}

	function getAllSiteIds() {
		return self
			.availableSites()
			.filter(function(site) {
				return site.id !== self.allSitesId;
			})
			.map(function(site) {
				return site.id;
			});
	}

	function getAllTeamIds() {
		return self
			.availableTeams()
			.filter(function(team) {
				return team.id !== self.allTeamsId;
			})
			.map(function(team) {
				return team.id;
			});
	}

	function loadToleranceInfo(agentId, callback) {
		if (!agentId) return;

		ajax.Ajax({
			url: 'Requests/GetWFCTolerance?personToId=' + agentId,
			dataType: 'json',
			type: 'GET',
			success: function(data) {
				self.checkTolorance = data.IsNeedToCheck;
				self.toloranceInfo = {
					myTolorance: data.MyInfos,
					targetTolorance: data.PersonToInfos
				};
				if (callback) callback();
			},
			error: function(jqXHR, textStatus, errorThrown) {
				if (jqXHR.status === 400) {
					var data = $.parseJSON(jqXHR.responseText);
					self.errorMessage(data.Errors.join('</br>'));
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}

	function validateTolorance() {
		self.myToleranceMessages([]);
		self.targetToleranceMessages([]);
		if (!self.checkTolorance) {
			return;
		}

		var selectedSchedulesPairs = self.selectedSchedulePairs().filter(function(p) {
			return p.isSelected;
		});

		if (!self.toloranceInfo || selectedSchedulesPairs.length === 0) {
			return;
		}

		if (self.toloranceInfo.myTolorance && self.toloranceInfo.myTolorance.length > 0) {
			self.toloranceInfo.myTolorance.forEach(function(periodToloranceInfo) {
				validatePeriodTolorance(periodToloranceInfo, selectedSchedulesPairs, true);
			});
		}

		if (self.toloranceInfo.targetTolorance && self.toloranceInfo.targetTolorance.length > 0) {
			self.toloranceInfo.targetTolorance.forEach(function(periodToloranceInfo) {
				validatePeriodTolorance(periodToloranceInfo, selectedSchedulesPairs, false);
			});
		}
	}

	function validatePeriodTolorance(periodToloranceInfo, personSchedules, isCheckSelf) {
		var schedulesInPeriod = personSchedules.filter(function(p) {
			return (
				(p.date.isBefore(periodToloranceInfo.PeriodEnd, 'day') ||
					p.date.isSame(periodToloranceInfo.PeriodEnd, 'day')) &&
				(p.date.isAfter(periodToloranceInfo.PeriodStart, 'day') ||
					p.date.isSame(periodToloranceInfo.PeriodStart, 'day'))
			);
		});

		if (schedulesInPeriod.length === 0) {
			return;
		}

		var gap = 0;
		schedulesInPeriod.forEach(function(p) {
			if (isCheckSelf) {
				if (p.mySchedule != null && p.targetSchedule == null) gap += p.mySchedule.contractMinutes;
				if (p.mySchedule == null && p.targetSchedule != null) gap -= p.targetSchedule.contractMinutes;
				if (p.mySchedule != null && p.targetSchedule != null)
					gap += p.mySchedule.contractMinutes - p.targetSchedule.contractMinutes;
			} else {
				if (p.mySchedule == null && p.targetSchedule != null) gap += p.targetSchedule.contractMinutes;
				if (p.mySchedule != null && p.targetSchedule == null) gap -= p.mySchedule.contractMinutes;
				if (p.mySchedule != null && p.targetSchedule != null)
					gap += p.targetSchedule.contractMinutes - p.mySchedule.contractMinutes;
			}
		});

		var left = 0;
		if (gap > 0)
			left =
				periodToloranceInfo.NegativeToleranceMinutes -
				periodToloranceInfo.RealScheduleNegativeGap +
				periodToloranceInfo.RealSchedulePositiveGap -
				gap;
		if (gap < 0)
			left =
				periodToloranceInfo.PositiveToleranceMinutes -
				periodToloranceInfo.RealSchedulePositiveGap +
				periodToloranceInfo.RealScheduleNegativeGap +
				gap;

		if (left < 0) {
			var contractTimeGap = Teleopti.MyTimeWeb.Common.FormatTimeSpan(-left);
			contractTimeGap = (gap > 0 ? '-' : '+') + contractTimeGap;
			if (isCheckSelf) {
				self.myToleranceMessages.push({
					periodStart: Teleopti.MyTimeWeb.Common.FormatDate(periodToloranceInfo.PeriodStart),
					periodEnd: Teleopti.MyTimeWeb.Common.FormatDate(periodToloranceInfo.PeriodEnd),
					contractTimeGap: contractTimeGap
				});
			} else {
				self.targetToleranceMessages.push({
					periodStart: Teleopti.MyTimeWeb.Common.FormatDate(periodToloranceInfo.PeriodStart),
					periodEnd: Teleopti.MyTimeWeb.Common.FormatDate(periodToloranceInfo.PeriodEnd),
					contractTimeGap: contractTimeGap
				});
			}
		}
	}

	function loadPeriodSchedule(startDate, endDate, agentId, prepend, callback) {
		if (!startDate || !endDate || !agentId) {
			return;
		}

		if (self.isLoadingSchedules()) {
			return;
		} else {
			self.isLoadingSchedules(true);
		}

		ajax.Ajax({
			url: 'Requests/ShiftTradeMultiDaysSchedule',
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			data: JSON.stringify({
				StartDate: startDate.format('YYYY-MM-DD'),
				EndDate: endDate.format('YYYY-MM-DD'),
				PersonToId: agentId
			}),
			success: function(data) {
				var previousFirstRowId;

				if (data.MultiSchedulesForShiftTrade && data.MultiSchedulesForShiftTrade.length > 0) {
					var loadedData = [];

					ko.utils.arrayMap(data.MultiSchedulesForShiftTrade, function(schedulePair) {
						var momentDate = moment(schedulePair.Date);
						if (
							self.loadedSchedulePairs().filter(function(p) {
								return momentDate.isSame(p.date, 'day');
							}).length === 0
						) {
							loadedData.push({
								date: momentDate,
								mySchedule:
									schedulePair.MySchedule && schedulePair.MySchedule.IsNotScheduled
										? null
										: createShiftTradeSchedule(schedulePair.MySchedule),
								targetSchedule:
									schedulePair.PersonToSchedule && schedulePair.PersonToSchedule.IsNotScheduled
										? null
										: createShiftTradeSchedule(schedulePair.PersonToSchedule),
								isEnable: schedulePair.IsSelectable,
								reason: schedulePair.UnselectableReason,
								isSelected: ko.observable(false)
							});
						}
					});

					if (prepend) {
						previousFirstRowId = '#shift-row-' + self.loadedSchedulePairs()[0].date.valueOf();
						self.loadedSchedulePairs.unshift.apply(self.loadedSchedulePairs, loadedData);
						setTimeout(restoreScroll);
					} else {
						self.loadedSchedulePairs.push.apply(self.loadedSchedulePairs, loadedData);
					}
				}

				if (prepend) {
					self.earliestScheduleDate = startDate;
				} else {
					self.lastScheduleDate = endDate;
				}

				self.isLoadingSchedulesOnTop(false);
				self.isLoadingSchedulesOnBottom(false);
				self.isLoadingSchedules(false);

				function restoreScroll() {
					document.querySelector(
						'.multi-shift-trade-schedules-list-panel'
					).scrollTop = document.querySelector(previousFirstRowId).offsetTop;
				}

				if (callback) {
					callback();
				}
			},
			error: function(jqXHR, textStatus, errorThrown) {
				self.isLoadingSchedulesOnTop(false);
				self.isLoadingSchedulesOnBottom(false);
				self.isLoadingSchedules(false);
				if (jqXHR.status === 400) {
					var data = $.parseJSON(jqXHR.responseText);
					self.errorMessage(data.Errors.join('</br>'));
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}

	function loadSchedule(date, selectedTeamOption) {
		if (selectedTeamOption === undefined) return;
		if (self.IsLoading()) return;

		var teamIds = selectedTeamOption === self.allTeamsId ? getAllTeamIds() : [selectedTeamOption];
		var take = self.maxShiftsPerPage;
		var skip = (self.selectedPageIndex() - 1) * take;

		var scheduleReloaded = false;
		ajax.Ajax({
			url: 'Requests/ShiftTradeRequestSchedule',
			dataType: 'json',
			type: 'POST',
			contentType: 'application/json; charset=utf-8',
			data: JSON.stringify({
				selectedDate: date,
				teamIds: teamIds.join(','),
				SearchNameText: self.searchNameText(),
				filteredStartTimes: self.filteredStartTimesText().join(','),
				filteredEndTimes: self.filteredEndTimesText().join(','),
				isDayOff: self.isDayoffFiltered(),
				Take: take,
				Skip: skip,
				TimeSortOrder: timeSortOrder()
			}),
			beforeSend: function() {
				self.IsLoading(true);
			},
			success: function(data, textStatus, jqXHR) {
				setPagingInfo(data.PageCount);
				createTimeLine(data.TimeLineHours);
				createMySchedule(data.MySchedule);
				setPossibleTradeSchedulesRaw(date, data);
				createPossibleTradeSchedules(self.possibleTradeSchedulesRaw);
				scheduleReloaded = true;
			},
			complete: function() {
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
					redrawLayers();
				}
			}
		});
	}

	function createMySchedule(myScheduleObject) {
		var mappedlayers = [];
		if (
			myScheduleObject !== null &&
			myScheduleObject !== undefined &&
			myScheduleObject.ScheduleLayers !== null &&
			myScheduleObject.ScheduleLayers.length > 0
		) {
			var layers = myScheduleObject.ScheduleLayers;
			var scheduleStartTime = moment(layers[0].Start);
			var scheduleEndTime = moment(layers[layers.length - 1].End);
			mappedlayers = ko.utils.arrayMap(layers, function(layer) {
				var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
				return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(
					layer,
					minutesSinceTimeLineStart,
					self.pixelPerMinute()
				);
			});
			self.mySchedule(
				new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
					mappedlayers,
					scheduleStartTime,
					scheduleEndTime,
					myScheduleObject.Name,
					myScheduleObject.PersonId,
					myScheduleObject.IsDayOff,
					myScheduleObject.DayOffName,
					false,
					myScheduleObject.IsFullDayAbsence,
					null,
					Teleopti.MyTimeWeb.Common.FormatTimeSpan(myScheduleObject.ContractTimeInMinute)
				)
			);
		} else if (myScheduleObject !== null) {
			self.mySchedule(
				new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
					mappedlayers,
					moment(),
					moment(),
					'',
					'',
					myScheduleObject.IsDayOff,
					myScheduleObject.DayOffName,
					false,
					myScheduleObject.IsFullDayAbsence,
					null,
					Teleopti.MyTimeWeb.Common.FormatTimeSpan(myScheduleObject.ContractTimeInMinute)
				)
			);
		} else {
			self.mySchedule(
				new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
					mappedlayers,
					moment(),
					moment(),
					'',
					'',
					false,
					'',
					'0:00'
				)
			);
		}
	}

	function createPossibleTradeSchedules(possibleTradeSchedules) {
		self.possibleTradeSchedules.removeAll();
		var mappedPersonsSchedule = ko.utils.arrayMap(possibleTradeSchedules, function(personSchedule) {
			var mappedLayers = [];
			if (
				personSchedule !== null &&
				personSchedule.ScheduleLayers !== null &&
				personSchedule.ScheduleLayers.length > 0
			) {
				var layers = personSchedule.ScheduleLayers;
				var scheduleStartTime = moment(layers[0].Start);
				var scheduleEndTime = moment(layers[layers.length - 1].End);

				mappedLayers = ko.utils.arrayMap(personSchedule.ScheduleLayers, function(layer) {
					var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
					return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(
						layer,
						minutesSinceTimeLineStart,
						self.pixelPerMinute()
					);
				});
			}
			var model = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
				mappedLayers,
				scheduleStartTime,
				scheduleEndTime,
				personSchedule.Name,
				personSchedule.PersonId,
				personSchedule.IsDayOff,
				personSchedule.DayOffName,
				false,
				false,
				null,
				Teleopti.MyTimeWeb.Common.FormatTimeSpan(personSchedule.ContractTimeInMinute)
			);
			self.possibleTradeSchedules.push(model);
			return model;
		});

		self.noPossibleShiftTrades(mappedPersonsSchedule.length === 0 ? true : false);
	}

	function createShiftTradeSchedule(personSchedule) {
		if (personSchedule) {
			var mappedLayers = [];
			var startDateTime = moment(personSchedule.MinStart);
			if (
				personSchedule !== null &&
				personSchedule.ScheduleLayers !== null &&
				personSchedule.ScheduleLayers.length > 0
			) {
				var layers = personSchedule.ScheduleLayers;
				var startTimeInString = layers[0].TitleTime.split('-')[0].trim();
				var endTimeInString = layers[layers.length - 1].TitleTime.split('-')[1].trim();
				var scheduleStartTime = moment(layers[0].Start);
				var scheduleEndTime = moment(layers[layers.length - 1].End);
				var scheduleLength = scheduleEndTime.diff(scheduleStartTime, 'minutes');
				var overtimelayer = personSchedule.ScheduleLayers.filter(function(l) {
					return l.IsOvertime;
				})[0];
				if (overtimelayer) {
					var hasOvertime = true;
					var overtimeCategoryColor = overtimelayer.Color;
				}

				mappedLayers = ko.utils.arrayMap(personSchedule.ScheduleLayers, function(layer) {
					var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
					var offsetFromScheduleStart = moment(layer.Start).diff(scheduleStartTime, 'minutes');
					return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(
						layer,
						minutesSinceTimeLineStart,
						self.pixelPerMinute(),
						offsetFromScheduleStart,
						scheduleLength
					);
				});
			}

			if (personSchedule && personSchedule.ShiftCategory) {
				var categoryName = personSchedule.ShiftCategory.Name;
				var categoryColor = personSchedule.ShiftCategory.DisplayColor;
			}

			var model = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
				mappedLayers,
				scheduleStartTime,
				scheduleEndTime,
				personSchedule.Name,
				personSchedule.PersonId,
				personSchedule.IsDayOff,
				personSchedule.DayOffName,
				false,
				personSchedule.IsFullDayAbsence,
				null,
				Teleopti.MyTimeWeb.Common.FormatTimeSpan(personSchedule.ContractTimeInMinute),
				personSchedule.IsNotScheduled,
				startDateTime,
				categoryName,
				categoryColor,
				startTimeInString,
				endTimeInString,
				hasOvertime,
				personSchedule.IsIntradayAbsence,
				personSchedule.IntradayAbsenceCategory.Color,
				personSchedule.IntradayAbsenceCategory.ShortName,
				overtimeCategoryColor,
				personSchedule.ContractTimeInMinute
			);

			return model;
		}

		return null;
	}

	function setPossibleTradeSchedulesRaw(date, data) {
		if (self.possibleTradeSchedulesRaw.length > 0) {
			self.possibleTradeSchedulesRaw = [];
		}

		var findTradedAgent = false;
		$.each(data.PossibleTradeSchedules, function(i, item) {
			self.possibleTradeSchedulesRaw.push(item);
			if (self.agentChoosed()) {
				if (item.Name === self.agentChoosed().agentName) {
					findTradedAgent = true;
				}
			}
		});
		if (self.agentChoosed()) {
			if (!findTradedAgent && self.selectedPageIndex() < self.pageCount()) {
				self.selectedPageIndex(self.selectedPageIndex() + 1);
				self.IsLoading(false);
				//keep loading until find the current chosen agent in agent chosen view
				loadSchedule(date, self.selectedTeamInternal());
			}
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
			var isVisible = hours.length < 18 ? true : i % 2 !== 0;
			var newHour = new Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel(
				i,
				hours[i],
				diff,
				self.pixelPerMinute(),
				isVisible
			);
			self.hours.push(newHour);
		}
	}

	function redrawLayers() {
		var canvasWidth;

		if (self.isReadyLoaded()) {
			canvasWidth = $('td.shift-trade-my-schedule:visible').width();
			if (canvasWidth === null) canvasWidth = getCanvasWidth();
		} else {
			canvasWidth = getCanvasWidth();
		}

		self.layerCanvasPixelWidth(canvasWidth);

		if (self.mySchedule() !== undefined) {
			$.each(self.mySchedule().layers, function(index, selfScheduleAddShiftTrade) {
				selfScheduleAddShiftTrade.pixelPerMinute(self.pixelPerMinute());
			});
		}

		if (self.possibleTradeSchedules() !== undefined) {
			$.each(self.possibleTradeSchedules(), function(index, selfPersonScheduleAddShiftTrade) {
				$.each(selfPersonScheduleAddShiftTrade.layers, function(index, selfScheduleAddShiftTrade) {
					selfScheduleAddShiftTrade.pixelPerMinute(self.pixelPerMinute());
				});
			});
		}

		if (self.hours() !== undefined) {
			$.each(self.hours(), function(index, hour) {
				hour.pixelPerMinute(self.pixelPerMinute());
			});
		}
	}

	function getCanvasWidth() {
		var canvasWidth;
		var containerWidth = $('#Request-add-shift-trade').width();
		var nameCellWidth = $('td.shift-trade-agent-name').width();
		canvasWidth = containerWidth - nameCellWidth;

		var buttonAddCellWidth = $('td.shift-trade-button-cell').width();
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

	self.displayView = function() {
		return 'new-shift-trade-request-panel-74947';
	};
};

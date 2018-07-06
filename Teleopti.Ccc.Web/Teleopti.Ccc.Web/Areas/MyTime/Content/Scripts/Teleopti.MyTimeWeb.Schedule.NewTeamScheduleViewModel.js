Teleopti.MyTimeWeb.Schedule.NewTeamScheduleViewModel = function(
	filterChangedCallback,
	setDraggableScrollBlockOnDesktop
) {
	var self = this,
		constants = Teleopti.MyTimeWeb.Common.Constants,
		TOTAL_MINUTES_OF_ONE_DAY = constants.totalMinutesOfOneDay,
		FULL_DAY_HOUR_STR = constants.fullDayHourStr,
		PIXEL_OF_ONE_HOUR = constants.pixelOfOneHourInTeamSchedule,
		dateOnlyFormat = constants.serviceDateTimeFormat.dateOnly,
		timeLineOffset = 50,
		minPixelsToDisplayTitle = 30,
		requestDateOnlyFormat = 'YYYY/MM/DD',
		rawTimeline = [];

	self.isHostAMobile = Teleopti.MyTimeWeb.Common.IsHostAMobile();
	self.isHostAniPad = Teleopti.MyTimeWeb.Common.IsHostAniPad();

	self.isMobileEnabled = ko.observable(
		self.isHostAMobile && Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_NewTeamScheduleView_75989')
	);
	self.isDesktopEnabled = ko.observable(
		!self.isHostAMobile && Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_NewTeamScheduleViewDesktop_76313')
	);

	self.selectedDate = ko.observable(moment());
	self.displayDate = ko.observable(self.selectedDate().format(dateOnlyFormat));
	self.availableTeams = ko.observableArray();
	self.selectedTeam = ko.observable();
	self.selectedTeamIds = [];
	self.defaultTeamId = '';
	self.scheduleContainerHeight = ko.observable(0);
	self.timeLines = ko.observableArray();
	self.mySchedule = ko.observable();
	self.teamSchedules = ko.observableArray();
	self.agentNames = ko.observableArray();
	self.filterChangedCallback = filterChangedCallback;
	self.selectedDateSubscription = null;
	self.selectedTeamSubscription = null;
	self.currentPageNum = ko.observable(1);
	self.totalPageNum = ko.observable(0);
	self.totalAgentCount = ko.observable(0);
	self.isScrollbarVisible = ko.observable(false);
	self.isPanelVisible = ko.observable(false);
	self.searchNameText = ko.observable('');
	self.hasFiltered = ko.observable(false);
	self.emptySearchResult = ko.observable(false);
	self.filter = {
		searchNameText: '',
		selectedTeamIds: []
	};

	self.paging = {
		skip: 0,
		take: 20
	};

	self.today = function() {
		var today = moment().format(requestDateOnlyFormat);
		self.paging.skip = 0;
		self.filterChangedCallback(today);
	};

	self.previousDay = function() {
		var previousDate = moment(self.selectedDate())
			.add(-1, 'days')
			.format(requestDateOnlyFormat);
		self.paging.skip = 0;
		self.filterChangedCallback(previousDate);
	};

	self.nextDay = function() {
		var nextDate = moment(self.selectedDate())
			.add(1, 'days')
			.format(requestDateOnlyFormat);
		self.paging.skip = 0;
		self.filterChangedCallback(nextDate);
	};

	self.toggleFilterPanel = function() {
		self.isPanelVisible(!self.isPanelVisible());
	};

	self.cancelClick = function() {
		self.searchNameText(self.filter.searchNameText);
		self.selectedTeam(self.filter.selectedTeamIds[0]);
		self.toggleFilterPanel();
	};

	self.submitSearchForm = function() {
		self.paging.skip = 0;
		self.filter.searchNameText = self.searchNameText();
		self.filter.selectedTeamIds = self.selectedTeamIds.concat();
		self.filterChangedCallback(self.selectedDate().format(requestDateOnlyFormat));
	};

	self.goToPreviousPage = function() {
		if (self.currentPageNum() == 1) return;

		self.paging.skip -= self.paging.take;
		if (self.paging.skip < 0) self.paging.skip = 0;

		self.filterChangedCallback(self.selectedDate().format('YYYY/MM/DD'));
	};

	self.goToNextPage = function() {
		if (self.paging.skip + self.paging.take < self.paging.take * self.totalPageNum()) {
			self.paging.skip += self.paging.take;
			self.filterChangedCallback(self.selectedDate().format('YYYY/MM/DD'));
		}
	};

	self.readTeamsData = function(data) {
		setAvailableTeams(data.allTeam, data.teams);
	};

	self.readDefaultTeamData = function(data) {
		disposeSelectedTeamSubscription();
		self.defaultTeamId = data.DefaultTeam;
		self.selectedTeam(data.DefaultTeam);
		self.selectedTeamIds = [];
		self.selectedTeamIds.push(data.DefaultTeam);
		self.filter.selectedTeamIds = self.selectedTeamIds.concat();
		setSelectedTeamSubscription();
	};

	self.readScheduleData = function(data, date) {
		disposeSelectedDateSubscription();

		self.agentNames(getAgentNames(data.AgentSchedules));
		self.selectedDate(moment(date));
		self.displayDate(moment(date).format(Teleopti.MyTimeWeb.Common.DateFormat));

		rawTimeline = data.TimeLine;
		self.timeLines(createTimeLineViewModel(rawTimeline));
		self.scheduleContainerHeight(self.timeLines().length * PIXEL_OF_ONE_HOUR + timeLineOffset);

		var timelineStart = data.TimeLine[0].Time;
		self.mySchedule(createMySchedule(data.MySchedule, timelineStart));

		self.teamSchedules(createTeamSchedules(data.AgentSchedules, timelineStart));

		setSelectedDateSubscription();
		setPaging(data.PageCount);
		self.totalAgentCount(data.TotalAgentCount);

		self.hasFiltered(
			!!self.filter.searchNameText || (self.selectedTeamIds[0] && self.selectedTeamIds[0] != self.defaultTeamId)
		);
		self.emptySearchResult(data.AgentSchedules.length == 0);

		if (!self.emptySearchResult() && self.isPanelVisible()) {
			self.toggleFilterPanel();
		}

		setDraggableScrollBlockOnDesktop && setDraggableScrollBlockOnDesktop();
	};

	self.readMoreTeamScheduleData = function(data, callback) {
		rawTimeline = mergeRawTimeLine(rawTimeline, data.TimeLine);
		self.timeLines(createTimeLineViewModel(rawTimeline));
		self.scheduleContainerHeight(self.timeLines().length * PIXEL_OF_ONE_HOUR + timeLineOffset);

		var teamSchedule = createTeamSchedules(data.AgentSchedules, self.timeLines()[0].time);
		teamSchedule.forEach(function(schedule) {
			self.teamSchedules.push(schedule);
			self.agentNames.push(schedule.name);
		});

		setPaging();
		callback && callback();
	};

	function mergeRawTimeLine(rawTimeline, newTimeLine) {
		rawTimeline = rawTimeline.concat(newTimeLine);

		rawTimeline.sort(function(cur, next) {
			return getMinuteOfRawTimeline(cur) - getMinuteOfRawTimeline(next);
		});

		var hash = {},
			distinctRawTimeline = [];
		rawTimeline.forEach(function(value) {
			if (!hash[value.Time]) {
				hash[value.Time] = true;
				distinctRawTimeline.push(value);
			}
		});

		return distinctRawTimeline;
	}

	function getMinuteOfRawTimeline(t) {
		var hourMinuteSecond = t.Time.split(':'),
			hour = hourMinuteSecond[0],
			minute = hourMinuteSecond[1],
			minutes = 0;

		if (hour.toString() === FULL_DAY_HOUR_STR) {
			minutes = TOTAL_MINUTES_OF_ONE_DAY;
		} else {
			if (hour.indexOf('.') > -1) {
				hour = 24 * parseInt(hour.split('.')[0]) + parseInt(hour.split('.')[1]);
			}
			minutes = hour * PIXEL_OF_ONE_HOUR + parseInt(minute);
		}

		return minutes;
	}

	function getAgentNames(agentSchedulesData) {
		var agentNames = [];
		if (!agentSchedulesData || agentSchedulesData.length == 0) {
			return agentNames;
		}

		agentSchedulesData.forEach(function(agentSchedule) {
			agentNames.push(agentSchedule.Name);
		});

		return agentNames;
	}

	function setPaging(pageCount) {
		if (pageCount > 0) self.totalPageNum(pageCount);

		self.currentPageNum(parseInt(self.paging.skip / self.paging.take) + 1);
	}

	function setSelectedDateSubscription() {
		self.selectedDateSubscription = self.selectedDate.subscribe(function(value) {
			self.displayDate(value.format(dateOnlyFormat));
			if (self.filterChangedCallback) {
				self.paging.skip = 0;
				self.filterChangedCallback(value.format(requestDateOnlyFormat));
			}
		});
	}

	function disposeSelectedDateSubscription() {
		if (self.selectedDateSubscription) self.selectedDateSubscription.dispose();
	}

	function setSelectedTeamSubscription() {
		self.selectedTeamSubscription = self.selectedTeam.subscribe(function(value) {
			if (value === '-1') {
				setAllTeams();
			} else {
				self.selectedTeamIds = [];
				self.selectedTeamIds.push(value);
			}
		});
	}

	function setAllTeams() {
		var businessHierarchyGroup = undefined;
		var selectedTeams = [];
		var allGroups = self.availableTeams();
		for (var i = 0; i < allGroups.length; i++) {
			var group = allGroups[i];
			// Only get teams from business hierarchy
			if (group.PageId === '6ce00b41-0722-4b36-91dd-0a3b63c545cf') {
				businessHierarchyGroup = group;
				break;
			}
		}

		if (businessHierarchyGroup != undefined) {
			selectedTeams = businessHierarchyGroup.children.map(function(e) {
				return e.id;
			});
		}
		self.selectedTeamIds = selectedTeams;
	}

	function disposeSelectedTeamSubscription() {
		if (self.selectedTeamSubscription) self.selectedTeamSubscription.dispose();
	}

	function createMySchedule(myScheduleData, timelineStart) {
		var mySchedulePeriods = [];

		myScheduleData.Periods.forEach(function(layer, index, periods) {
			var myLayerViewModel = new Teleopti.MyTimeWeb.Schedule.LayerViewModel(
				layer,
				null,
				true,
				timeLineOffset,
				false,
				timelineStart,
				self.selectedDate()
			);

			myLayerViewModel.showTitle = ko.computed(function() {
				return myLayerViewModel.height() > minPixelsToDisplayTitle;
			});

			myLayerViewModel.isLastLayer = index === periods.length - 1;

			mySchedulePeriods.push(myLayerViewModel);
		});

		return {
			name: myScheduleData.Name,
			layers: mySchedulePeriods,
			isDayOff: myScheduleData.IsDayOff,
			dayOffName: myScheduleData.DayOffName
		};
	}

	function createTeamSchedules(agentSchedulesData, timelineStart) {
		var teamSchedules = [];

		if (!agentSchedulesData || agentSchedulesData.length == 0) {
			return teamSchedules;
		}

		agentSchedulesData.forEach(function(agentSchedule) {
			var layers = [];
			agentSchedule.Periods.forEach(function(layer, index, periods) {
				var layerViewModel = new Teleopti.MyTimeWeb.Schedule.LayerViewModel(
					layer,
					null,
					true,
					timeLineOffset,
					false,
					timelineStart,
					self.selectedDate()
				);
				layerViewModel.isLastLayer = index === periods.length - 1;

				layers.push(layerViewModel);
			});

			teamSchedules.push({
				name: agentSchedule.Name,
				layers: layers,
				isDayOff: agentSchedule.IsDayOff,
				dayOffName: agentSchedule.DayOffName
			});
		});

		return teamSchedules;
	}

	function createTimeLineViewModel(timeLine) {
		var timelineArr = [];
		var scheduleHeight = Teleopti.MyTimeWeb.Schedule.GetScheduleHeight();

		timeLine.forEach(function(hour, index) {
			// 5 is half of timeline label height (10px)
			var timelineLayer = new Teleopti.MyTimeWeb.Schedule.TimelineViewModel(
				hour,
				scheduleHeight,
				timeLineOffset - 5,
				false,
				index
			);

			timelineArr.push(timelineLayer);
		});
		return timelineArr;
	}

	function setAvailableTeams(allTeam, teams) {
		if (allTeam !== null) {
			allTeam.id = -1;

			if (teams.length > 1) {
				if (teams[0] && teams[0].children != null) {
					teams.unshift({ children: [allTeam], text: '' });
				} else {
					teams.unshift(allTeam);
				}
			}
		}
		self.availableTeams(teams);
	}
};

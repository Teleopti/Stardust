Teleopti.MyTimeWeb.Schedule.NewTeamScheduleViewModel = function(
	filterChangedCallback,
	loadGroupAndTeams,
	readScheduleDataCallback,
	rebuildTooltipForTimeFilterIcon
) {
	var self = this,
		constants = Teleopti.MyTimeWeb.Common.Constants,
		PIXEL_OF_ONE_HOUR = constants.pixelOfOneHourInTeamSchedule,
		getTextColoFn = Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor,
		timeLineOffset = 50,
		minPixelsToDisplayTitle = 30,
		defaultFilterTime = '00:00',
		rawTimeline = [];

	self.isHostAMobile = Teleopti.MyTimeWeb.Common.IsHostAMobile();
	self.isHostAniPhone = Teleopti.MyTimeWeb.Common.IsHostAniPhone();
	self.isHostAniPad = Teleopti.MyTimeWeb.Common.IsHostAniPad();
	self.isHostADesktop = !self.isHostAMobile && !self.isHostAniPad;

	self.isMobileEnabled =
		self.isHostAMobile && Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_NewTeamScheduleView_75989');
	self.isDesktopEnabled =
		!self.isHostAMobile && Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_NewTeamScheduleViewDesktop_76313');

	self.selectedDate = ko.observable(moment());
	self.displayDate = ko.observable(Teleopti.MyTimeWeb.Common.FormatDate(self.selectedDate()));
	self.availableTeams = ko.observableArray();
	self.selectedTeamIds = [];
	self.defaultTeamId = '';
	self.selectedTeam = ko.observable();
	self.selectedTeamName = ko.observable();
	self.loadGroupAndTeams = loadGroupAndTeams;
	self.isTeamsAndGroupsLoaded = ko.observable(false);
	self.scheduleContainerHeight = ko.observable(0);
	self.timeLines = ko.observableArray();
	self.mySchedule = ko.observable();
	self.teamSchedules = ko.observableArray();
	self.allTeamSchedules = [];
	self.agentNames = ko.observableArray();
	self.allAgentNames = [];
	self.lastAgentIndexInDom = 0;

	self.filterChangedCallback = filterChangedCallback;
	self.selectedDateSubscription = null;
	self.selectedTeamSubscription = null;
	self.currentPageNum = 1;
	self.totalPageNum = 0;
	self.totalAgentCount = 0;
	self.loadedAgentIndex = 0;
	self.showOnlyDayOff = ko.observable(false);
	self.showOnlyDayOffSubscription = undefined;
	self.showOnlyNightShift = ko.observable(false);
	self.showOnlyNightShiftSubscription = undefined;
	self.isPanelVisible = ko.observable(false);
	self.isScrollbarVisible = ko.observable(false);
	self.searchNameText = ko.observable('');
	self.hasFilteredOnMobile = ko.observable(false);
	self.hasTimeFiltered = ko.observable(false);
	self.emptySearchResult = ko.observable(false);
	self.isAgentScheduleLoaded = ko.observable(false);
	self.isLoadingMoreAgentSchedules = false;

	self.showStartTimeStart = ko.observable(true);
	self.startTimeStart = ko.observable(defaultFilterTime);
	self.startTimeEnd = ko.observable(defaultFilterTime);
	self.showEndTimeStart = ko.observable(true);
	self.endTimeStart = ko.observable(defaultFilterTime);
	self.endTimeEnd = ko.observable(defaultFilterTime);

	self.filter = {
		filteredStartTimes: '',
		filteredEndTimes: '',
		searchNameText: '',
		selectedTeamIds: [],
		isDayOff: false,
		onlyNightShift: false
	};

	self.paging = {
		skip: 0,
		take: 50
	};

	self.today = function() {
		if (moment().isSame(self.selectedDate(), 'day')) return;

		self.paging.skip = 0;
		self.filterChangedCallback(moment());
		self.loadedAgentIndex = 0;
		self.lastAgentIndexInDom = 0;
		self.isLoadingMoreAgentSchedules = false;
	};

	self.previousDay = function() {
		self.paging.skip = 0;
		self.loadedAgentIndex = 0;
		self.lastAgentIndexInDom = 0;
		self.filterChangedCallback(moment(self.selectedDate()).add(-1, 'days'));
		self.isLoadingMoreAgentSchedules = false;
	};

	self.nextDay = function() {
		self.paging.skip = 0;
		self.loadedAgentIndex = 0;
		self.lastAgentIndexInDom = 0;
		self.filterChangedCallback(moment(self.selectedDate()).add(1, 'days'));
		self.isLoadingMoreAgentSchedules = false;
	};

	self.openTeamSelectorPanel = function(data, event) {
		var sibling = $($(event.target).siblings()[0]);
		if (self.isTeamsAndGroupsLoaded()) {
			sibling.find('a.select2-choice').trigger('mousedown');
		} else {
			sibling.find('a.select2-choice span.select2-arrow b').addClass('loading-teams-and-groups');

			self.loadGroupAndTeams(function() {
				sibling.find('a.select2-choice').trigger('mousedown');
				sibling.find('a.select2-choice span.select2-arrow b').removeClass('loading-teams-and-groups');

				self.isTeamsAndGroupsLoaded(true);
			});
		}
	};

	self.isPanelVisible.subscribe(function() {
		self.showOnlyDayOff(self.filter.isDayOff);
		self.showOnlyNightShift(self.filter.onlyNightShift);
	});

	self.cancelClick = function() {
		self.searchNameText(self.filter.searchNameText);
		self.selectedTeam(self.filter.selectedTeamIds[0]);
		self.isPanelVisible(false);
		self.showOnlyDayOff(self.filter.isDayOff);
		self.showOnlyNightShift(self.filter.onlyNightShift);
	};

	self.submitSearchForm = function() {
		self.paging.skip = 0;
		self.filter.searchNameText = self.searchNameText();
		self.filter.selectedTeamIds = self.selectedTeamIds.concat();
		self.filter.isDayOff = self.showOnlyDayOff();
		self.filter.onlyNightShift = self.showOnlyNightShift();

		setTimeFilterData();

		if (!self.isHostAMobile) {
			self.isPanelVisible(false);
		}

		self.loadedAgentIndex = 0;
		self.lastAgentIndexInDom = 0;
		self.filterChangedCallback(self.selectedDate());
	};

	self.readTeamsData = function(data) {
		setAvailableTeams(data.allTeam, data.teams);
	};

	self.readDefaultTeamData = function(data) {
		disposeSelectedTeamSubscription();
		self.defaultTeamId = data.DefaultTeam;
		self.selectedTeamIds = [];
		self.selectedTeamIds.push(data.DefaultTeam);
		self.filter.selectedTeamIds = self.selectedTeamIds.concat();

		setAvailableTeams(null, [
			{
				PageId: '',
				children: [
					{
						id: data.DefaultTeam,
						text: data.DefaultTeamName
					}
				],
				text: ''
			}
		]);
		self.selectedTeam(data.DefaultTeam);
		self.selectedTeamName(data.DefaultTeamName);
		setSelectedTeamSubscription();
	};

	self.readScheduleData = function(data, date, keepPanelOpen) {
		disposeSelectedDateSubscription();
		disposeShowOnlyDayOffSubscription();
		disposeShowOnlyNightShiftSubscription();

		self.selectedDate(moment(date));
		self.displayDate(Teleopti.MyTimeWeb.Common.FormatDate(moment(date)));

		rawTimeline = data.TimeLine;
		self.timeLines(createTimeLineViewModel(rawTimeline));
		self.scheduleContainerHeight(self.timeLines().length * PIXEL_OF_ONE_HOUR + timeLineOffset);

		var timelineStart = data.TimeLine[0].Time;
		self.mySchedule(createMySchedule(data.MySchedule, timelineStart));

		self.agentNames(buildAgentNames(data.AgentSchedules, self.loadedAgentIndex));
		self.allAgentNames = buildAgentNames(data.AgentSchedules, self.loadedAgentIndex);

		self.teamSchedules(createTeamSchedules(data.AgentSchedules, timelineStart, self.loadedAgentIndex));
		self.allTeamSchedules = createTeamSchedules(data.AgentSchedules, timelineStart, self.loadedAgentIndex);

		self.loadedAgentIndex = data.AgentSchedules.length - 1;
		self.lastAgentIndexInDom = data.AgentSchedules.length - 1;

		if (data.PageCount > 0) self.totalPageNum = data.PageCount;
		self.totalAgentCount = data.TotalAgentCount;

		self.hasTimeFiltered(
			self.filter.onlyNightShift ||
				self.filter.isDayOff ||
				self.filter.filteredStartTimes.length > 0 ||
				self.filter.filteredEndTimes.length > 0
		);

		self.hasFilteredOnMobile(
			self.hasTimeFiltered() ||
				!!self.filter.searchNameText ||
				(self.selectedTeamIds[0] && self.selectedTeamIds[0] != self.defaultTeamId)
		);
		self.emptySearchResult(data.AgentSchedules.length == 0);

		if (!self.emptySearchResult() && !keepPanelOpen) {
			self.isPanelVisible(false);
		}

		setSelectedDateSubscription();
		setShowOnlyDayOffSubscription();
		setShowOnlyNightShiftSubscription();

		readScheduleDataCallback && readScheduleDataCallback();
		self.isAgentScheduleLoaded(true);
	};

	self.readMoreTeamScheduleData = function(data, callback) {
		rawTimeline = mergeRawTimeLine(rawTimeline, data.TimeLine);
		self.timeLines(createTimeLineViewModel(rawTimeline));

		self.scheduleContainerHeight(self.timeLines().length * PIXEL_OF_ONE_HOUR + timeLineOffset);

		var agentNames = buildAgentNames(data.AgentSchedules, self.loadedAgentIndex + 1);
		agentNames.forEach(function(a) {
			self.allAgentNames.push(a);
		});

		var teamSchedule = createTeamSchedules(
			data.AgentSchedules,
			self.timeLines()[0].time,
			self.loadedAgentIndex + 1
		);
		teamSchedule.forEach(function(agentSchedule) {
			self.allTeamSchedules.push(agentSchedule);
		});

		self.loadedAgentIndex += data.AgentSchedules.length;
		self.lastAgentIndexInDom += data.AgentSchedules.length;

		callback && callback();
	};

	self.buildFilterDetails = function() {
		if (self.isHostAniPad || self.isHostAMobile) return '';

		return rebuildTooltipForTimeFilterIcon();
	};

	function mergeRawTimeLine(rawTimeline, newTimeLine) {
		rawTimeline = rawTimeline.concat(newTimeLine);

		var hash = {},
			distinctRawTimeline = [];
		rawTimeline.forEach(function(value) {
			if (!hash[value.Time]) {
				hash[value.Time] = true;
				distinctRawTimeline.push(value);
			}
		});

		distinctRawTimeline.sort(function(cur, next) {
			return moment(cur.Time) - moment(next.Time);
		});

		return distinctRawTimeline;
	}

	function buildAgentNames(agentSchedulesData, agentIndex) {
		var agentNames = [];
		if (!agentSchedulesData || agentSchedulesData.length == 0) {
			return agentNames;
		}

		agentSchedulesData.forEach(function(agentSchedule, i) {
			agentNames.push({
				index: i + agentIndex,
				name: agentSchedule.Name,
				shiftCategory: getShiftCategory(agentSchedule),
				isDayOff: agentSchedule.IsDayOff
			});
		});

		return agentNames;
	}

	function setTimeFilterData() {
		if (self.startTimeStart() === defaultFilterTime && self.startTimeEnd() === defaultFilterTime) {
			self.filter.filteredStartTimes = '';
		} else {
			self.filter.filteredStartTimes =
				(self.startTimeStart() ? self.startTimeStart() : '') +
				'-' +
				(self.startTimeEnd() ? self.startTimeEnd() : '');
		}

		if (self.endTimeStart() === defaultFilterTime && self.endTimeEnd() === defaultFilterTime) {
			self.filter.filteredEndTimes = '';
		} else {
			self.filter.filteredEndTimes =
				(self.endTimeStart() ? self.endTimeStart() : '') + '-' + (self.endTimeEnd() ? self.endTimeEnd() : '');
		}
	}

	function setShowOnlyNightShiftSubscription() {
		self.showOnlyNightShiftSubscription = self.showOnlyNightShift.subscribe(function(value) {
			disposeShowOnlyDayOffSubscription();

			self.showOnlyDayOff(false);
			if (!self.isMobileEnabled) {
				self.filter.isDayOff = false;

				self.loadedAgentIndex = 0;
				self.lastAgentIndexInDom = 0;

				self.filter.onlyNightShift = value;
				self.filter.searchNameText = self.searchNameText();

				setTimeFilterData();
				self.filterChangedCallback(self.selectedDate(), true);
			}

			setShowOnlyDayOffSubscription();
		});
	}

	function disposeShowOnlyNightShiftSubscription() {
		if (self.showOnlyNightShiftSubscription) self.showOnlyNightShiftSubscription.dispose();
	}

	function setShowOnlyDayOffSubscription() {
		self.showOnlyDayOffSubscription = self.showOnlyDayOff.subscribe(function(value) {
			disposeShowOnlyNightShiftSubscription();

			self.showOnlyNightShift(false);

			if (!self.isMobileEnabled) {
				self.filter.onlyNightShift = false;

				if (value) {
					self.paging.skip = 0;
				}
				self.loadedAgentIndex = 0;
				self.lastAgentIndexInDom = 0;

				self.filter.isDayOff = value;
				self.filter.searchNameText = self.searchNameText();

				setTimeFilterData();
				self.filterChangedCallback(self.selectedDate(), true);
			}

			setShowOnlyNightShiftSubscription();
		});
	}

	function disposeShowOnlyDayOffSubscription() {
		if (self.showOnlyDayOffSubscription) self.showOnlyDayOffSubscription.dispose();
	}

	function setSelectedDateSubscription() {
		self.selectedDateSubscription = self.selectedDate.subscribe(function(value) {
			self.displayDate(Teleopti.MyTimeWeb.Common.FormatDate(value));
			self.paging.skip = 0;
			self.loadedAgentIndex = 0;
			self.lastAgentIndexInDom = 0;
			self.filterChangedCallback && self.filterChangedCallback(moment(value));
		});
	}

	function disposeSelectedDateSubscription() {
		if (self.selectedDateSubscription) self.selectedDateSubscription.dispose();
	}

	function setSelectedTeamSubscription() {
		self.selectedTeamSubscription = self.selectedTeam.subscribe(function(value) {
			if (value === '00000000-0000-0000-0000-000000000000') {
				selectAllTeams();
			} else {
				self.selectedTeamIds = [];
				self.selectedTeamIds.push(value);
			}
			if (self.isTeamsAndGroupsLoaded() && self.isDesktopEnabled) {
				self.loadedAgentIndex = 0;
				self.lastAgentIndexInDom = 0;
				self.submitSearchForm();
			}
		});
	}

	function selectAllTeams() {
		self.selectedTeamIds = [];
		if (self.availableTeams()[0] && self.availableTeams()[0].children) {
			self.availableTeams()[0].children.forEach(function (a, i) {
				if (i > 0) {
					self.selectedTeamIds.push(a.id);
				}
			});
		}
	}

	function disposeSelectedTeamSubscription() {
		if (self.selectedTeamSubscription) self.selectedTeamSubscription.dispose();
	}

	function createMySchedule(myScheduleData, timelineStart) {
		var mySchedulePeriods = [];

		myScheduleData.Periods.forEach(function(layer, index, periods) {
			var myLayerViewModel = new Teleopti.MyTimeWeb.Schedule.LayerViewModel(
				layer,
				timeLineOffset,
				false,
				timelineStart,
				self.selectedDate(),
				null,
				true
			);

			myLayerViewModel.showTitle = myLayerViewModel.height >= minPixelsToDisplayTitle;
			myLayerViewModel.showDetail = myLayerViewModel.height >= PIXEL_OF_ONE_HOUR;
			myLayerViewModel.timeSpan = myLayerViewModel.timeSpan.replace(' - ', '-');
			myLayerViewModel.isLastLayer = index === periods.length - 1;

			mySchedulePeriods.push(myLayerViewModel);
		});

		return {
			name: myScheduleData.Name,
			layers: mySchedulePeriods,
			isDayOff: myScheduleData.IsDayOff,
			isNotScheduled: myScheduleData.IsNotScheduled,
			dayOffName: myScheduleData.DayOffName,
			shiftCategory: getShiftCategory(myScheduleData)
		};
	}

	function createTeamSchedules(agentSchedulesData, timelineStart, agentIndex) {
		var teamSchedules = [];

		if (!agentSchedulesData || agentSchedulesData.length == 0) {
			return teamSchedules;
		}

		agentSchedulesData.forEach(function(agentSchedule, i) {
			var layers = [];
			agentSchedule.Periods.forEach(function(layer, index, periods) {
				var layerViewModel = new Teleopti.MyTimeWeb.Schedule.LayerViewModel(
					layer,
					timeLineOffset,
					false,
					timelineStart,
					self.selectedDate(),
					null,
					false
				);
				layerViewModel.isLastLayer = index === periods.length - 1;
				layerViewModel.showTitle = layerViewModel.height >= minPixelsToDisplayTitle;
				layers.push(layerViewModel);
			});

			teamSchedules.push({
				index: i + agentIndex,
				name: agentSchedule.Name,
				layers: layers,
				isDayOff: agentSchedule.IsDayOff,
				isNotScheduled: agentSchedule.IsNotScheduled,
				dayOffName: agentSchedule.DayOffName
			});
		});

		return teamSchedules;
	}

	function getShiftCategory(scheduleData) {
		var shiftCategory = {
			name: ''
		};

		if (scheduleData.ShiftCategory) {
			var category = scheduleData.ShiftCategory;
			shiftCategory.name = category.ShortName;
			shiftCategory.bgColor = category.DisplayColor;
			shiftCategory.color = getTextColoFn(category.DisplayColor);
		}

		return shiftCategory;
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
				index,
				self.selectedDate()
			);

			timelineArr.push(timelineLayer);
		});
		return timelineArr;
	}

	function setAvailableTeams(allTeam, teams) {
		if (allTeam !== null) {
			allTeam.id = '00000000-0000-0000-0000-000000000000';

			if (teams.length > 0) {
				if (teams[0] && teams[0].children != null) {
					teams[0].children.unshift(allTeam);
				} else {
					teams = [
						{
							PageId: '',
							children: teams,
							text: ''
						}
					];
					teams[0].children.unshift(allTeam);
				}
			}
		}

		self.availableTeams(teams);
	}
};

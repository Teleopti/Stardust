Teleopti.MyTimeWeb.Schedule.NewTeamScheduleViewModel = function(
	filterChangedCallback,
	loadGroupAndTeams,
	readScheduleDataCallback
) {
	var self = this,
		constants = Teleopti.MyTimeWeb.Common.Constants,
		PIXEL_OF_ONE_HOUR = constants.pixelOfOneHourInTeamSchedule,
		dateOnlyFormat = constants.serviceDateTimeFormat.dateOnly,
		getTextColoFn = Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor,
		timeLineOffset = 50,
		minPixelsToDisplayTitle = 30,
		defaultFilterTime = '00:00',
		rawTimeline = [];

	self.isHostAMobile = Teleopti.MyTimeWeb.Common.IsHostAMobile();
	self.isHostAniPad = Teleopti.MyTimeWeb.Common.IsHostAniPad();

	self.isMobileEnabled =
		self.isHostAMobile && Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_NewTeamScheduleView_75989');
	self.isDesktopEnabled =
		!self.isHostAMobile && Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_NewTeamScheduleViewDesktop_76313');

	self.selectedDate = ko.observable(moment());
	self.displayDate = ko.observable(self.selectedDate().format(dateOnlyFormat));
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
	self.agentNames = ko.observableArray();

	self.filterChangedCallback = filterChangedCallback;
	self.selectedDateSubscription = null;
	self.selectedTeamSubscription = null;
	self.currentPageNum = ko.observable(1);
	self.totalPageNum = ko.observable(0);
	self.totalAgentCount = ko.observable(0);
	self.showOnlyDayOff = ko.observable(false);
	self.isPanelVisible = ko.observable(false);
	self.isScrollbarVisible = ko.observable(false);
	self.searchNameText = ko.observable('');
	self.hasFiltered = ko.observable(false);
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
		isDayOff: false
	};

	self.paging = {
		skip: 0,
		take: 20
	};

	self.today = function() {
		self.paging.skip = 0;
		self.filterChangedCallback(moment());
		self.isLoadingMoreAgentSchedules = false;
	};

	self.previousDay = function() {
		self.paging.skip = 0;
		self.filterChangedCallback(moment(self.selectedDate()).add(-1, 'days'));
		self.isLoadingMoreAgentSchedules = false;
	};

	self.nextDay = function() {
		self.paging.skip = 0;
		self.filterChangedCallback(moment(self.selectedDate()).add(1, 'days'));
		self.isLoadingMoreAgentSchedules = false;
	};

	self.showOnlyDayOff.subscribe(function(value) {
		self.filter.isDayOff = value;

		if (!self.isMobileEnabled) {
			if (value) {
				self.paging.skip = 0;
			}
			self.filterChangedCallback(self.selectedDate());
		}
	});

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

	self.cancelClick = function() {
		self.searchNameText(self.filter.searchNameText);
		self.selectedTeam(self.filter.selectedTeamIds[0]);
		self.isPanelVisible(false);
	};

	self.submitSearchForm = function() {
		self.paging.skip = 0;
		self.filter.searchNameText = self.searchNameText();
		self.filter.selectedTeamIds = self.selectedTeamIds.concat();

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

		self.hasTimeFiltered(
			self.startTimeStart() != defaultFilterTime ||
				self.startTimeEnd() != defaultFilterTime ||
				self.endTimeStart() != defaultFilterTime ||
				self.endTimeEnd() != defaultFilterTime
		);
		self.filterChangedCallback(self.selectedDate());
	};

	self.goToPreviousPage = function() {
		if (self.currentPageNum() == 1) return;

		self.paging.skip -= self.paging.take;
		if (self.paging.skip < 0) self.paging.skip = 0;

		self.filterChangedCallback(self.selectedDate());
	};

	self.goToNextPage = function() {
		if (self.paging.skip + self.paging.take < self.paging.take * self.totalPageNum()) {
			self.paging.skip += self.paging.take;
			self.filterChangedCallback(self.selectedDate());
		}
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

	self.readScheduleData = function(data, date) {
		disposeSelectedDateSubscription();

		self.agentNames(buildAgentNames(data.AgentSchedules));
		self.selectedDate(moment(date));
		self.displayDate(moment(date).format(dateOnlyFormat));

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
			self.hasTimeFiltered() ||
				!!self.filter.searchNameText ||
				((self.selectedTeamIds[0] && self.selectedTeamIds[0] != self.defaultTeamId) ||
					(self.isMobileEnabled && self.showOnlyDayOff()))
		);
		self.emptySearchResult(data.AgentSchedules.length == 0);

		if (!self.emptySearchResult() && self.isPanelVisible()) {
			self.isPanelVisible(false);
		}

		readScheduleDataCallback && readScheduleDataCallback();
		self.isAgentScheduleLoaded(true);
	};

	self.readMoreTeamScheduleData = function(data, callback) {
		rawTimeline = mergeRawTimeLine(rawTimeline, data.TimeLine);
		self.timeLines(createTimeLineViewModel(rawTimeline));

		self.scheduleContainerHeight(self.timeLines().length * PIXEL_OF_ONE_HOUR + timeLineOffset);

		var agentNames = buildAgentNames(data.AgentSchedules);
		agentNames.forEach(function(a) {
			self.agentNames.push(a);
		});

		var teamSchedule = createTeamSchedules(data.AgentSchedules, self.timeLines()[0].time);
		teamSchedule.forEach(function(agentSchedule) {
			self.teamSchedules.push(agentSchedule);
		});

		setPaging();
		callback && callback();
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

	function buildAgentNames(agentSchedulesData) {
		var agentNames = [];
		if (!agentSchedulesData || agentSchedulesData.length == 0) {
			return agentNames;
		}

		agentSchedulesData.forEach(function(agentSchedule) {
			agentNames.push({
				name: agentSchedule.Name,
				shiftCategory: getShiftCategory(agentSchedule),
				isDayOff: agentSchedule.IsDayOff
			});
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
				self.filterChangedCallback(moment(value));
			}
		});
	}

	function disposeSelectedDateSubscription() {
		if (self.selectedDateSubscription) self.selectedDateSubscription.dispose();
	}

	function setSelectedTeamSubscription() {
		self.selectedTeamSubscription = self.selectedTeam.subscribe(function(value) {
			if (value === '00000000-0000-0000-0000-000000000000') {
				setAllTeams();
			} else {
				self.selectedTeamIds = [];
				self.selectedTeamIds.push(value);
			}
			if (self.isTeamsAndGroupsLoaded() && self.isDesktopEnabled) {
				self.submitSearchForm();
			}
		});
	}

	function setAllTeams() {
		self.selectedTeamIds = self.availableTeams()[1].children.map(function(c) {
			return c.id;
		});
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
				return myLayerViewModel.height() >= minPixelsToDisplayTitle;
			});
			myLayerViewModel.showDetail = ko.computed(function() {
				return myLayerViewModel.height() >= PIXEL_OF_ONE_HOUR;
			});
			myLayerViewModel.timeSpan = ko.computed(function() {
				return myLayerViewModel.timeSpan().replace(' - ', '-');
			});

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

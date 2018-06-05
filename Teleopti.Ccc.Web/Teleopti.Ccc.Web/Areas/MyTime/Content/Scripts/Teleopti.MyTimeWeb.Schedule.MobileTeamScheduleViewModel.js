/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.LayerViewModel.js" />

Teleopti.MyTimeWeb.Schedule.MobileTeamScheduleViewModel = function (filterChangedCallback) {
	var self = this,
		constants = Teleopti.MyTimeWeb.Common.Constants,
		dateOnlyFormat = constants.serviceDateTimeFormat.dateOnly,
		timeLineOffset = 50,
		requestDateOnlyFormat = 'YYYY/MM/DD';

	self.selectedDate = ko.observable(moment());
	self.displayDate = ko.observable(self.selectedDate().format(dateOnlyFormat));
	self.availableTeams = ko.observableArray();
	self.selectedTeam = ko.observable();
	self.selectedTeamIds = [];
	self.scheduleContainerHeight = Teleopti.MyTimeWeb.Schedule.GetScheduleHeight() + timeLineOffset;
	self.timeLines = ko.observableArray();
	self.mySchedule = ko.observable();
	self.teamSchedules = ko.observableArray();
	self.filterChangedCallback = filterChangedCallback;
	self.selectedDateSubscription = null;
	self.selectedTeamSubscription = null;
	self.isPageVisible = ko.observable(false);
	self.isPreviousPageEnabled = ko.observable(false);
	self.isNextPageEnabled = ko.observable(false);
	self.currentPageNum = ko.observable(1);
	self.totalPageNum = ko.observable(0);

	self.paging = {
		skip: 0,
		take: 5
	};

	self.previousDay = function () {
		var previousDate = moment(self.selectedDate()).add(-1, 'days').format(requestDateOnlyFormat);
		self.paging.skip = 0;
		self.filterChangedCallback(previousDate);
	};

	self.nextDay = function () {
		var nextDate = moment(self.selectedDate()).add(1, 'days').format(requestDateOnlyFormat);
		self.paging.skip = 0;
		self.filterChangedCallback(nextDate);
	};

	self.goToPreviousPage = function() {
		if(self.currentPageNum() == 1) return;

		self.paging.skip -= self.paging.take;
		if (self.paging.skip < 0)
			self.paging.skip = 0;

		self.filterChangedCallback(self.selectedDate().format('YYYY/MM/DD'));
	};

	self.goToNextPage = function() {
		if ((self.paging.skip + self.paging.take) < self.paging.take * self.totalPageNum()) {
			self.paging.skip += self.paging.take;
			self.filterChangedCallback(self.selectedDate().format('YYYY/MM/DD'));
		}
	};

	self.readTeamsData = function (data) {
		setAvailableTeams(data.allTeam, data.teams);
	};

	self.readDefaultTeamData = function (data) {
		disposeSelectedTeamSubscription();
		self.selectedTeam(data.DefaultTeam);
		self.selectedTeamIds = [];
		self.selectedTeamIds.push(data.DefaultTeam);
		setSelectedTeamSubscription();
	};

	self.readScheduleData = function (data, date) {
		disposeSelectedDateSubscription();

		self.selectedDate(moment(date));
		self.displayDate(moment(date).format(Teleopti.MyTimeWeb.Common.DateFormat));
		self.timeLines(createTimeLine(data.TimeLine));

		self.mySchedule(createMySchedule(data.MySchedule));

		self.teamSchedules([]);
		self.teamSchedules(createTeamSchedules(data.AgentSchedules));
		
		setSelectedDateSubscription();
		setPaging(data.PageCount);
	};

	function setPaging(pageCount){
		self.totalPageNum(pageCount);
		self.isPageVisible(pageCount > 1);
		self.isPreviousPageEnabled(self.paging.skip >= self.paging.take);
		self.currentPageNum(parseInt(self.paging.skip / self.paging.take) +1);
		self.isNextPageEnabled(self.currentPageNum() < pageCount);
	}

	function setSelectedDateSubscription() {
		self.selectedDateSubscription = self.selectedDate.subscribe(function (value) {
			self.displayDate(value.format(dateOnlyFormat));
			if (self.filterChangedCallback) {
				self.paging.skip = 0;
				self.filterChangedCallback(value.format(requestDateOnlyFormat));
			}
		});
	};

	function disposeSelectedDateSubscription() {
		if (self.selectedDateSubscription)
			self.selectedDateSubscription.dispose();
	};

	function setSelectedTeamSubscription() {
		self.selectedTeamSubscription = self.selectedTeam.subscribe(function (value) {
			if (value === "-1") {
				setAllTeams();
			} else {
				self.selectedTeamIds = [];
				self.selectedTeamIds.push(value);
			}
			self.paging.skip = 0;
			self.filterChangedCallback(moment(self.selectedDate()).format(requestDateOnlyFormat));
		});
	};

	function setAllTeams() {
		var businessHierarchyGroup = undefined;
		var selectedTeams = [];
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
		self.selectedTeamIds = selectedTeams;
	}

	function disposeSelectedTeamSubscription() {
		if (self.selectedTeamSubscription)
			self.selectedTeamSubscription.dispose();
	};

	function createMySchedule(myScheduleData) {
		var mySchedulePeriods = [];

		myScheduleData.Periods.forEach(function (layer, index, periods) {
			var layerViewModel = new Teleopti.MyTimeWeb.Schedule.LayerViewModel(layer, null, true, timeLineOffset);
			layerViewModel.isLastLayer = index === periods.length - 1;
			mySchedulePeriods.push(layerViewModel);
		});

		return { name: myScheduleData.Name, layers: mySchedulePeriods };
	}

	function createTeamSchedules(agentSchedulesData) {
		var teamSchedules = [];

		if (!agentSchedulesData) {
			return teamSchedules;
		}

		agentSchedulesData.forEach(function (agentSchedule) {
			var layers = [];
			agentSchedule.Periods.forEach(function (layer, index, periods) {
				var layerViewModel = new Teleopti.MyTimeWeb.Schedule.LayerViewModel(
					layer,
					null,
					true,
					timeLineOffset
				);
				layerViewModel.isLastLayer = index === periods.length - 1;

				layers.push(layerViewModel);
			});

			teamSchedules.push({
				name: agentSchedule.Name,
				layers: layers
			});
		});

		return teamSchedules;
	}

	function createTimeLine(timeLine) {
		var timelineArr = [];
		var scheduleHeight = Teleopti.MyTimeWeb.Schedule.GetScheduleHeight();

		timeLine.forEach(function (hour) {
			// 5 is half of timeline label height (10px)
			timelineArr.push(
				new Teleopti.MyTimeWeb.Schedule.TimelineViewModel(
					hour,
					scheduleHeight,
					timeLineOffset - 5
				)
			);
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

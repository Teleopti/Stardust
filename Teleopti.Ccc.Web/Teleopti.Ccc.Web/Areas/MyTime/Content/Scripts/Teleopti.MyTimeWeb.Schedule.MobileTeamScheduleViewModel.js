﻿/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.LayerViewModel.js" />

Teleopti.MyTimeWeb.Schedule.MobileTeamScheduleViewModel = function() {
	var self = this,
		constants = Teleopti.MyTimeWeb.Common.Constants,
		dateOnlyFormat = constants.serviceDateTimeFormat.dateOnly,
		timeLineOffset = 40;

	self.selectedDate = ko.observable(moment());
	self.displayDate = ko.observable(self.selectedDate().format(dateOnlyFormat));
	self.availableTeams = ko.observableArray();
	self.selectedTeam = ko.observable();
	self.scheduleContainerHeight = Teleopti.MyTimeWeb.Schedule.GetScheduleHeight() + timeLineOffset;
	self.timeLines = ko.observableArray();
	self.mySchedule = ko.observable();
	self.teamSchedules = ko.observableArray();

	self.filter = {
		selectedTeamIds: [],
		filteredStartTimesText: '',
		filteredEndTimesText: '',
		timeSortOrder: '',
		isDayoffFiltered: false,
		searchNameText: ''
	};
	self.paging = {
		skip: 0,
		take: 5
	};

	self.selectedDate.subscribe(function(value) {
		self.displayDate(value.format(dateOnlyFormat));
	});

	self.previousDay = function() {
		self.selectedDate(moment(self.selectedDate()).add(-1, 'days'));
	};

	self.nextDay = function() {
		self.selectedDate(moment(self.selectedDate()).add(1, 'days'));
	};

	self.readTeamsData = function(data) {
		setAvailableTeams(data.allTeam, data.teams);
	};

	self.readDefaultTeamData = function(data) {
		self.selectedTeam(data.DefaultTeam);
		self.filter.selectedTeamIds.push(data.DefaultTeam);
	};

	self.readScheduleData = function (data) {
		self.timeLines(createTimeLine(data.TimeLine));

		self.mySchedule(createMySchedule(data.MySchedule));
		self.teamSchedules(createTeamSchedules(data.AgentSchedules));
	};

	function createMySchedule(myScheduleData) {
		var mySchedulePeriods = [];

		myScheduleData.Periods.forEach(function (layer, index, periods) {
			var layerViewModel = new Teleopti.MyTimeWeb.Schedule.LayerViewModel(layer, null, true, timeLineOffset);
			layerViewModel.isLastLayer = index == periods.length - 1;
			mySchedulePeriods.push(layerViewModel);
		});

		return { name: myScheduleData.Name, layers: mySchedulePeriods };
	}

	function createTeamSchedules(agentSchedulesData) {
		var teamSchedules = [];

		agentSchedulesData.forEach(function(agentSchedule) {
			var layers = [];
			agentSchedule.Periods.forEach(function(layer, index, periods) {
				var layerViewModel = new Teleopti.MyTimeWeb.Schedule.LayerViewModel(
					layer,
					null,
					true,
					timeLineOffset
				);
				layerViewModel.isLastLayer = index == periods.length - 1;

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

		timeLine.forEach(function(hour) {
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

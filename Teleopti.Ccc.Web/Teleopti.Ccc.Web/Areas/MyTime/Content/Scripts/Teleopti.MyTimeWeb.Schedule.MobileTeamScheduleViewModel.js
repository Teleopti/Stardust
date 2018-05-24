/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
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
	self.scheduleHeight = constants.mobileMinScheduleHeight + timeLineOffset;
	self.timeLines = ko.observableArray();
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
		take: 1
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

	self.readScheduleData = function(data) {
		self.timeLines(createTimeLine(data.TimeLine));
	};

	function createTimeLine(timeLine) {
		var timelineArr = [];
		var scheduleHeight = self.scheduleHeight - timeLineOffset;

		timeLine.forEach(function(hour) {
			timelineArr.push(new Teleopti.MyTimeWeb.Schedule.TimelineViewModel(hour, scheduleHeight, timeLineOffset));
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

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
	self.scheduleContainerHeight = constants.mobileMinScheduleHeight + timeLineOffset;
	self.timeLines = ko.observableArray();
	self.mySchedule = ko.observable();
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

		//deleteThisFakeScheduleDataFnWhenBackendIsReady(data);

		self.mySchedule({
			name: data.MySchedule.Name,
			layers: createMySchedulePeriods(data.MySchedule)
		});
	};

	function createMySchedulePeriods(myScheduleData) {
		var mySchedulePeriods = [];
		var layersOnMobile = true;

		myScheduleData.Periods.forEach(function(layer, index, periods) {
			var layerViewModel = new Teleopti.MyTimeWeb.Schedule.LayerViewModel(
				layer,
				null,
				layersOnMobile,
				timeLineOffset
			);
			layerViewModel.isLastLayer = index == periods.length - 1;
			mySchedulePeriods.push(layerViewModel);
		});

		return mySchedulePeriods;
	}

	function createTimeLine(timeLine) {
		var timelineArr = [];

		timeLine.forEach(function(hour) {
			// 5 is half of timeline label height (10px)
			timelineArr.push(
				new Teleopti.MyTimeWeb.Schedule.TimelineViewModel(
					hour,
					constants.mobileMinScheduleHeight,
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

	function deleteThisFakeScheduleDataFnWhenBackendIsReady(data) {
		//Faking myschedule data to render schedule cause backend is not ready yet.
		var fistTimeLine = data.TimeLine[1];
		var lastTimeLine = data.TimeLine[data.TimeLine.length - 2];
		data.MySchedule = {
			"Name": "Ashley Andeen",
			"StartTimeUtc": "2018-05-24T05:00:00",
			"PersonId": "b46a2588-8861-42e3-ab03-9b5e015b257c",
			"MinStart": null,
			"Total": 1,
			"DayOffName": null,
			"ContractTimeInMinute": 480.0,
			"Date": "",
			"FixedDate": "",
			"Header": "",
			"HasMainShift": "",
			"HasOvertime": "",
			"IsFullDayAbsence": false,
			"IsDayOff": false,
			"Summary": "",
			"Periods": [
				{
					"Title": "Phone",
					"TimeSpan": fistTimeLine.Time + '-' + lastTimeLine.Time,
					"StartTime": moment().format(dateOnlyFormat) + 'T' + fistTimeLine.Time,
					"EndTime": moment().format(dateOnlyFormat) + 'T' + data.TimeLine[data.TimeLine.length - 1].Time,
					"Summary": "",
					"StyleClassName": "",
					"Meeting": null,
					"StartPositionPercentage": fistTimeLine.PositionPercentage,
					"EndPositionPercentage": lastTimeLine.PositionPercentage,
					"Color": "#80FF80",
					"IsOvertime": false,
					"IsAbsenceConfidential": false,
					"TitleTime": fistTimeLine.Time + '-' + lastTimeLine.Time
				}
			],
			"DayOfWeekNumber": "",
			"HasNotScheduled": ""
		};

		return data;
	}
};


$(document).ready(function () {

	module("Teleopti.MyTimeWeb.TeamScheduleViewModel");

	var timeLineTemplate = [{ HourText: "", LengthInMinutesToDisplay: 15, StartTime: 1433634300000, EndTime: 1433635200000 },
							{ HourText: "08:00", LengthInMinutesToDisplay: 60, StartTime: 1433635200000, EndTime: 1433638800000 },
							{ HourText: "09:00", LengthInMinutesToDisplay: 60, StartTime: 1433638800000, EndTime: 1433642400000 },
							{ HourText: "10:00", LengthInMinutesToDisplay: 60, StartTime: 1433642400000, EndTime: 1433646000000 },
							{ HourText: "11:00", LengthInMinutesToDisplay: 60, StartTime: 1433646000000, EndTime: 1433649600000 },
							{ HourText: "12:00", LengthInMinutesToDisplay: 60, StartTime: 1433649600000, EndTime: 1433653200000 },
							{ HourText: "13:00", LengthInMinutesToDisplay: 60, StartTime: 1433653200000, EndTime: 1433656800000 },
							{ HourText: "14:00", LengthInMinutesToDisplay: 60, StartTime: 1433656800000, EndTime: 1433660400000 },
							{ HourText: "15:00", LengthInMinutesToDisplay: 60, StartTime: 1433660400000, EndTime: 1433664000000 },
							{ HourText: "16:00", LengthInMinutesToDisplay: 60, StartTime: 1433664000000, EndTime: 1433667600000 },
							{ HourText: "17:00", LengthInMinutesToDisplay: 15, StartTime: 1433667600000, EndTime: 1433668500000 }];

	var dayOffScheduleLayersTemplate = [{ Start: 1433606400000, End: 1433692800000, LengthInMinutes: 1440, Color: null, TitleHeader: "Day off", IsAbsenceConfidential: false, TitleTime: "00:00 - 00:00" }];

	var endpoints = {
		loadCurrentDate: "TeamSchedule/TeamScheduleCurrentDate",
		loadFilterTimes: "RequestsShiftTradeScheduleFilter/Get",
		loadMyTeam: "Requests/ShiftTradeRequestMyTeam",
		loadDefaultTeam: "TeamSchedule/DefaultTeam",
		loadTeams: "Team/TeamsAndGroupsWithAllTeam",
		loadSchedule: "TeamSchedule/TeamSchedule"
	};

	test("should send request to server when I type colleauge`s name in name seach box", function () {

		var nameInAjax;
		var ajax = {
			Ajax: function (options) {
				if (options.url == endpoints.loadSchedule) {
					var data = JSON.parse(options.data);
					nameInAjax = data.searchNameText;
				}
			}
		};
		Teleopti.MyTimeWeb.TeamScheduleViewModel.initCurrentDate = function () { };
		var viewModel = Teleopti.MyTimeWeb.TeamScheduleViewModelFactory.createViewModel(endpoints, ajax);
		viewModel.searchNameText("Andy");
		viewModel.selectedTeam("00000000-0000-0000-0000-000000000000");

		equal(nameInAjax, "Andy", "ajax call was sent and the SearchNameText is Andy");
	});

	test("should create view model when load schedules has completed", function () {

		var ajaxResult = {
			AgentSchedules: [
				{
					ScheduleLayers: dayOffScheduleLayersTemplate,
					Name: "Andy Stephen",
					StartTimeUtc: 1433599200000,
					PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
					MinStart: 1433599200000,
					IsDayOff: true,
					Total: 1
				}
			],
			TimeLine: timeLineTemplate,
			TimeLineLengthInMinutes: 0,
			PageCount: 1,
		};

		var ajax = {
			Ajax: function (options) {
				if (options.url == endpoints.loadSchedule) {
					options.success(ajaxResult);
				}
			}
		};

		Teleopti.MyTimeWeb.TeamScheduleViewModel.initCurrentDate = function () { };

		var viewModel = Teleopti.MyTimeWeb.TeamScheduleViewModelFactory.createViewModel(endpoints, ajax);
		viewModel.selectedTeam("00000000-0000-0000-0000-000000000000");

		equal(viewModel.toDrawSchedules()[0].agentName, "Andy Stephen", "view model was created and the agentName is 'Andy Stephen' ");
	});

	test("should keep selected team when date change", function () {

		Teleopti.MyTimeWeb.TeamScheduleViewModel.loadSchedule = function () { };

		var ajax = {
			Ajax: function (options) {
				if (options.url == endpoints.loadTeams) {
					options.success({
						teams: [
							   {
							   	text: "team 1",
							   	id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495"
							   },
							   {
							   	text: "team 2",
							   	id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab496"
							   }
						],
						allTeam: 'All Teams'
					});
				} else if (options.url == endpoints.loadDefaultTeam) {
					options.success({ "DefaultTeam": "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495" });
				}
			}
		};

		Teleopti.MyTimeWeb.TeamScheduleViewModel.initCurrentDate = function () { };
		var viewModel = Teleopti.MyTimeWeb.TeamScheduleViewModelFactory.createViewModel(endpoints, ajax);

		viewModel.selectedTeam("0a1cdb27-bc01-4bb9-b0b3-9b5e015ab496");


		viewModel.requestedDate(moment().add(1, 'day').startOf('day'));
		equal(viewModel.selectedTeam(), "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab496");

	});

	test("should create view model when load schedules including my schedule has completed", function () {

		var ajaxResult = {
			MySchedule: {
				"ScheduleLayers": [{
					"Start": "\/Date(1450393200000)\/",
					"End": "\/Date(1450396800000)\/",
					"LengthInMinutes": 60,
					"Color": "#FF8080",
					"TitleHeader": "Invoice",
					"IsAbsenceConfidential": false,
					"IsOvertime": false,
					"TitleTime": "07:00 - 08:00"
				}, {
					"Start": "\/Date(1450396800000)\/",
					"End": "\/Date(1450400400000)\/",
					"LengthInMinutes": 60,
					"Color": "#80FF80",
					"TitleHeader": "Phone",
					"IsAbsenceConfidential": false,
					"IsOvertime": false,
					"TitleTime": "08:00 - 09:00"
				}, {
					"Start": "\/Date(1450400400000)\/",
					"End": "\/Date(1450401300000)\/",
					"LengthInMinutes": 15,
					"Color": "#FF0000",
					"TitleHeader": "Short break",
					"IsAbsenceConfidential": false,
					"IsOvertime": false,
					"TitleTime": "09:00 - 09:15"
				}, {
					"Start": "\/Date(1450401300000)\/",
					"End": "\/Date(1450407600000)\/",
					"LengthInMinutes": 105,
					"Color": "#80FF80",
					"TitleHeader": "Phone",
					"IsAbsenceConfidential": false,
					"IsOvertime": false,
					"TitleTime": "09:15 - 11:00"
				}, {
					"Start": "\/Date(1450407600000)\/",
					"End": "\/Date(1450411200000)\/",
					"LengthInMinutes": 60,
					"Color": "#FFFF00",
					"TitleHeader": "Lunch",
					"IsAbsenceConfidential": false,
					"IsOvertime": false,
					"TitleTime": "11:00 - 12:00"
				}, {
					"Start": "\/Date(1450411200000)\/",
					"End": "\/Date(1450418400000)\/",
					"LengthInMinutes": 120,
					"Color": "#80FF80",
					"TitleHeader": "Phone",
					"IsAbsenceConfidential": false,
					"IsOvertime": false,
					"TitleTime": "12:00 - 14:00"
				}, {
					"Start": "\/Date(1450418400000)\/",
					"End": "\/Date(1450419300000)\/",
					"LengthInMinutes": 15,
					"Color": "#FF0000",
					"TitleHeader": "Short break",
					"IsAbsenceConfidential": false,
					"IsOvertime": false,
					"TitleTime": "14:00 - 14:15"
				}, {
					"Start": "\/Date(1450419300000)\/",
					"End": "\/Date(1450425600000)\/",
					"LengthInMinutes": 105,
					"Color": "#80FF80",
					"TitleHeader": "Phone",
					"IsAbsenceConfidential": false,
					"IsOvertime": false,
					"TitleTime": "14:15 - 16:00"
				}
				],
				"Name": "John Smith",
				"StartTimeUtc": "\/Date(1450389600000)\/",
				"PersonId": "47a3d4aa-3cd8-4235-a7eb-9b5e015b2560",
				"MinStart": "\/Date(1450364400000)\/",
				"IsDayOff": false,
				"Total": 10
			},
			AgentSchedules: [
				{
					ScheduleLayers: dayOffScheduleLayersTemplate,
					Name: "Andy Stephen",
					StartTimeUtc: 1433599200000,
					PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
					MinStart: 1433599200000,
					IsDayOff: true,
					Total: 1
				}
			],
			TimeLine: timeLineTemplate,
			TimeLineLengthInMinutes: 0,
			PageCount: 1,
		};

		var ajax = {
			Ajax: function (options) {
				if (options.url == endpoints.loadSchedule) {
					options.success(ajaxResult);
				}
			}
		};

		Teleopti.MyTimeWeb.TeamScheduleViewModel.initCurrentDate = function () { };

		var viewModel = Teleopti.MyTimeWeb.TeamScheduleViewModelFactory.createViewModel(endpoints, ajax);
		viewModel.selectedTeam("00000000-0000-0000-0000-000000000000");

		equal(viewModel.toDrawSchedules()[0].agentName, "Andy Stephen", "view model was created and the agentName is 'Andy Stephen' ");
		equal(viewModel.mySchedule().layers.length, 8);
	});

});
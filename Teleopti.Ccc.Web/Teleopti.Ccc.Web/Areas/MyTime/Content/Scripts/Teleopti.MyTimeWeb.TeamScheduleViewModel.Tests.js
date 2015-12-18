
$(document).ready(function() {

	module("Teleopti.MyTimeWeb.TeamScheduleViewModel");

	var timeLineTemplate = [{ HourText:"",LengthInMinutesToDisplay:15,StartTime:Date(1433634300000),EndTime:Date(1433635200000)},
							{ HourText:"08:00",LengthInMinutesToDisplay:60,StartTime:Date(1433635200000),EndTime:Date(1433638800000)},
							{ HourText:"09:00",LengthInMinutesToDisplay:60,StartTime:Date(1433638800000),EndTime:Date(1433642400000)},
							{ HourText:"10:00",LengthInMinutesToDisplay:60,StartTime:Date(1433642400000),EndTime:Date(1433646000000)},
							{ HourText:"11:00",LengthInMinutesToDisplay:60,StartTime:Date(1433646000000),EndTime:Date(1433649600000)},
							{ HourText:"12:00",LengthInMinutesToDisplay:60,StartTime:Date(1433649600000),EndTime:Date(1433653200000)},
							{ HourText:"13:00",LengthInMinutesToDisplay:60,StartTime:Date(1433653200000),EndTime:Date(1433656800000)},
							{ HourText:"14:00",LengthInMinutesToDisplay:60,StartTime:Date(1433656800000),EndTime:Date(1433660400000)},
							{ HourText:"15:00",LengthInMinutesToDisplay:60,StartTime:Date(1433660400000),EndTime:Date(1433664000000)},
							{ HourText:"16:00",LengthInMinutesToDisplay:60,StartTime:Date(1433664000000),EndTime:Date(1433667600000)},
							{ HourText:"17:00",LengthInMinutesToDisplay:15,StartTime:Date(1433667600000),EndTime: Date(1433668500000)}];

	var dayOffScheduleLayersTemplate = [{ Start: Date(1433606400000), End: Date(1433692800000), LengthInMinutes: 1440, Color: null, TitleHeader: "Day off", IsAbsenceConfidential: false, TitleTime: "00:00 - 00:00" }];

	var endpoints = {
		loadCurrentDate: "TeamSchedule/TeamScheduleCurrentDate",
		loadFilterTimes: "RequestsShiftTradeScheduleFilter/Get",
		loadMyTeam: "Requests/ShiftTradeRequestMyTeam",
		loadDefaultTeam: "TeamSchedule/DefaultTeam",
		loadTeams: "Team/TeamsAndGroupsWithAllTeam",
		loadSchedule: "TeamSchedule/TeamSchedule"
	};

	Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(toggleName) { return true; };

	test("should send request to server when I type colleauge`s name in name seach box", function() {

		var nameInAjax;
		var ajax = {
			Ajax: function(options) {
				if (options.url == endpoints.loadSchedule) {
					var data = JSON.parse(options.data);
					nameInAjax = data.searchNameText;
				}
			}
		};
		Teleopti.MyTimeWeb.TeamScheduleViewModel.initCurrentDate = function() {};
		var viewModel = Teleopti.MyTimeWeb.TeamScheduleViewModelFactory.createViewModel(endpoints, ajax);
		viewModel.searchNameText("Andy");
		viewModel.selectedTeam("00000000-0000-0000-0000-000000000000");

		equal(nameInAjax, "Andy","ajax call was sent and the SearchNameText is Andy");
	});

	test("should create view model when load schedules has completed", function() {

		var ajaxResult = {
			AgentSchedules: [
				{
					ScheduleLayers: dayOffScheduleLayersTemplate,
					Name: "Andy Stephen",
					StartTimeUtc: Date(1433599200000),
					PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
					MinStart: Date(1433599200000),
					IsDayOff: true,
					Total: 1
				}
			],
			TimeLine: timeLineTemplate,
			TimeLineLengthInMinutes: 0,
			PageCount: 1,
		};

		var ajax = { Ajax: function(options) {
				if (options.url == endpoints.loadSchedule) {
					options.success(ajaxResult);
				}
			}
		};

		Teleopti.MyTimeWeb.TeamScheduleViewModel.initCurrentDate = function() {};

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

});
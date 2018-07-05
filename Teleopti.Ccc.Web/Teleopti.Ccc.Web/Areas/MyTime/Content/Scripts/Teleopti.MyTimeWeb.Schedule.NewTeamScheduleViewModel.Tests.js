$(document).ready(function() {
	var vm;

	module('Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule', {
		setup: function() {
			vm = new Teleopti.MyTimeWeb.Schedule.NewTeamScheduleViewModel();

			readFakeScheduleData(vm);
		},
		teardown: function() {}
	});

	test('should set selected team ids', function() {
		var defaultTeam = {
			DefaultTeam: 'e5f968d7-6f6d-407c-81d5-9b5e015ab495'
		};

		vm.readDefaultTeamData(defaultTeam);

		equal(vm.selectedTeam(), 'e5f968d7-6f6d-407c-81d5-9b5e015ab495');
		equal(vm.selectedTeamIds.length, 1);
		equal(vm.selectedTeamIds[0], 'e5f968d7-6f6d-407c-81d5-9b5e015ab495');
	});

	test('should not show title when my activity is shorter than 30mins', function() {
		equal(vm.mySchedule().layers[0].showTitle(), false);
	});

	function readFakeScheduleData() {
		var fakeTeamScheduleData = {
			"AgentSchedules": [],
			"TimeLine": [
				{ "Time": "05:00:00", "TimeLineDisplay": "05:00", "PositionPercentage": 0.0714, "TimeFixedFormat": null },
				{ "Time": "06:00:00", "TimeLineDisplay": "06:00", "PositionPercentage": 0.3571, "TimeFixedFormat": null },
				{ "Time": "07:00:00", "TimeLineDisplay": "07:00", "PositionPercentage": 0.6429, "TimeFixedFormat": null },
				{ "Time": "08:00:00", "TimeLineDisplay": "08:00", "PositionPercentage": 0.9286, "TimeFixedFormat": null },
				{ "Time": "23:00:00", "TimeLineDisplay": "23:00", "PositionPercentage": 0.0714, "TimeFixedFormat": null },
				{ "Time": "1.00:00:00", "TimeLineDisplay": "00:00", "PositionPercentage": 0.3571, "TimeFixedFormat": null },
				{ "Time": "1.01:00:00", "TimeLineDisplay": "01:00", "PositionPercentage": 0.6429, "TimeFixedFormat": null },
				{ "Time": "1.02:00:00", "TimeLineDisplay": "02:00", "PositionPercentage": 0.6429, "TimeFixedFormat": null },
				{ "Time": "1.03:00:00", "TimeLineDisplay": "03:00", "PositionPercentage": 0.6429, "TimeFixedFormat": null },
			],
			"TimeLineLengthInMinutes": 210,
			"PageCount": 4,
			"MySchedule": {
				"Name": "Ashley Andeen",
				"DayOffName": 'Day off',
				"IsDayOff": true,
				"Periods": [
					{
						"Title": "Phone",
						"TimeSpan": "05:00 - 05:29",
						"StartTime": "2018-05-24T05:00:00",
						"EndTime": "2018-05-24T05:29:00",
						"Summary": "",
						"StyleClassName": "",
						"Meeting": "",
						"StartPositionPercentage": "0.0714",
						"EndPositionPercentage": "0.1428",
						"Color": "#80FF80",
						"IsOvertime": false,
						"IsAbsenceConfidential": false,
						"TitleTime": "05:00 - 05:29"
					}
				]
			}
		};

		vm.readScheduleData(fakeTeamScheduleData);
	}
});

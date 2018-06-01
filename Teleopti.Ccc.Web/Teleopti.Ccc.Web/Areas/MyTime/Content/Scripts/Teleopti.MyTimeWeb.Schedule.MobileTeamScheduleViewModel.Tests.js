$(document).ready(function() {
	var hash = '',
		constants = Teleopti.MyTimeWeb.Common.Constants,
		dateFormat = constants.serviceDateTimeFormat.dateOnly,
		vm;

	module('Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule', {
		setup: function() {
			vm = new Teleopti.MyTimeWeb.Schedule.MobileTeamScheduleViewModel();
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
});

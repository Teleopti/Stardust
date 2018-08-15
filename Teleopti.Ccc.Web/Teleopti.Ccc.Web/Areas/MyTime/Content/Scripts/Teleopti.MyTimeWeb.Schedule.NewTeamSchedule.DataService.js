Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.DataService = function(ajax) {
	var self = this;

	self.loadGroupAndTeams = function(callback) {
		ajax.Ajax({
			url: 'Team/TeamsAndGroupsWithAllTeam',
			success: function(teams) {
				callback && callback(teams);
			}
		});
	};

	self.loadDefaultTeam = function(callback) {
		ajax.Ajax({
			url: '../api/TeamSchedule/DefaultTeam',
			success: function(defaultTeam) {
				callback && callback(defaultTeam);
			}
		});
	};

	self.loadScheduleData = function(date, paging, filter, callback) {
		ajax.Ajax({
			url: '../api/TeamSchedule/TeamSchedule',
			dataType: 'json',
			type: 'POST',
			contentType: 'application/json; charset=utf-8',
			data: JSON.stringify({
				SelectedDate: date,
				ScheduleFilter: {
					teamIds: filter.selectedTeamIds.join(','),
					searchNameText: filter.searchNameText,
					isDayOff: filter.isDayOff
				},
				Paging: {
					Take: paging == null ? 20 : paging.take,
					Skip: paging == null ? 0 : paging.skip
				}
			}),
			success: function(schedules) {
				callback && callback(schedules);
			}
		});
	};
};

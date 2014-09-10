﻿define(['navigation'
], function (
	navigation
	) {

	return function () {
		var self = this;

		this.changeSchedule = function () {
			navigation.GoToTeamScheduleWithSelectedPerson(self.groupId, self.personId, self.date);
		};
	};
});

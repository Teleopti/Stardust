(function () {
	'use strict';

	angular.module("wfm.teamSchedule").component("underlyingScheduleInfo", {
		templateUrl: 'app/teamSchedule/html/underlyingScheduleInfo.html',
		bindings: {
			personSchedule: '<',
			date:'<'
		}
	});

})();
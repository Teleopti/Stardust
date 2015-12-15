'use strict';

(function () {
	var directive = function () {
		return {
			templateUrl: "js/teamSchedule/html/personschedule.html"
		};
	};
	angular.module('wfm.teamSchedule').directive('personSchedule', directive);
}());
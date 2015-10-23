'use strict';

(function () {
	var directive = function () {
		return {
			templateUrl: "js/teamSchedule/personschedule.html",
			linkFunction: linkFunction
		};
	};
	angular.module('wfm.teamSchedule')
		.directive('personSchedule', directive);

	function linkFunction(scope, element, attributes, controllers) {

	};
}());
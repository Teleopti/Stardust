'use strict';

(function () {
	var directive = function () {
		return {
			controller: 'TimeLineCtrl',
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: "js/teamSchedule/timeline.html",
			scope: {
				data: '=',
			},
			linkFunction: linkFunction
		};
	};
	angular.module('wfm.teamSchedule')
		.directive('timeLine', directive);

	function linkFunction(scope, element, attributes, controllers) {

	};
}());
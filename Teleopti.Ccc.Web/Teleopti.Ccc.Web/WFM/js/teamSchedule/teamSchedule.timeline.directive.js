'use strict';

(function () {
	var timeLineCtrl = function() {
		var vm = this;
	}
	var directive = function () {
		return {
			controller: 'TimeLineCtrl',
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: "js/teamSchedule/timeline.html",
			scope: {
				times: '=?',
			},
			linkFunction: linkFunction
		};
	};
	angular.module('wfm.teamSchedule')
		.directive('timeLine', directive)
	.controller('TimeLineCtrl', timeLineCtrl);

	function linkFunction(scope, element, attributes, controllers) {

	};
}());
'use strict';

(function () {
	var timeLineCtrl = function ($scope) {
		var vm = this;
		$scope.$watch(function () {
			return angular.element($('#team-schedule'))[0].offsetHeight;
		}, function (newValue) {
			if (newValue > 0) {
				vm.height = newValue;
			}
		});
		
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
	.controller('TimeLineCtrl', ['$scope', timeLineCtrl]);

	function linkFunction(scope, element, attributes, controllers) {

	};
}());
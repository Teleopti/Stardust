'use strict';

(function () {
	var timeLineCtrl = function ($scope) {
		var vm = this;
		vm.height = 0;
		$scope.$watch(function () {
			return vm.scheduleCount;
		}, function (newValue) {
			if (newValue > 0) {
				vm.height = 38 * (vm.scheduleCount) + 17; //tr height * count + th height
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
				scheduleCount: '=?'
			},
			linkFunction: linkFunction
		};
	};
	angular.module('wfm.teamSchedule')
		.directive('timeLine', directive)
	.controller('TimeLineCtrl', ['$scope', timeLineCtrl]);

	function linkFunction(scope, element, attributes, vm) {

	};
}());
'use strict';

(function () {
	var timeLineCtrl = function($scope, toggleSvc) {
		var vm = this;
		vm.height = 0;
		$scope.$watch(function() {
			return vm.scheduleCount;
		}, function(newValue) {
			if (newValue > 0) {
				toggleSvc.isFeatureEnabled
					.query({ toggle: 'WfmTeamSchedule_AbsenceReporting_35995' })
					.$promise
					.then(function(result) {
						var withCheckbox = result.IsEnabled;

						var headerHeight = 17; //angular.element($("#time-line-container"))[0].offsetHeight
						var labelHeight = 12; //angular.element($(".label-info"))[0].offsetHeight;
						var rowHeight = withCheckbox ? 42 : 32;

						vm.height = rowHeight * (vm.scheduleCount) + headerHeight + labelHeight;
					});
			}
		});
	}
	var directive = function () {
		return {
			controller: 'TimeLineCtrl',
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: "js/teamSchedule/html/timeline.html",
			scope: {
				times: '=?',
				scheduleCount: '=?'
			},
			linkFunction: linkFunction
		};
	};
	angular.module('wfm.teamSchedule')
		.directive('timeLine', directive)
		.controller('TimeLineCtrl', ['$scope', 'Toggle', timeLineCtrl]);

	function linkFunction(scope, element, attributes, vm) {
	};
}());

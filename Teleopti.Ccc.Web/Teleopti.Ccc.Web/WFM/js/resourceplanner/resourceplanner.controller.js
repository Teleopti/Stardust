(function() {
	'use strict';
	angular.module('wfm.resourceplanner', [])
		.controller('ResourceplannerCtrl', [
			'$scope', 'ResourcePlannerSvrc', function($scope, ResourcePlannerSvrc) {
			$scope.planningPeriod = ResourcePlannerSvrc.getPlanningPeriod.query({});
			}
		]);
})();
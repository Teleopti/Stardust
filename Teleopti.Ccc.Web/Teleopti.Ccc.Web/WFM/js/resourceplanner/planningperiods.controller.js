(function() {
	'use strict';
	angular.module('wfm.resourceplannerA', ['restPlanningPeriodService'])
		.controller('PlanningPeriodsCtrl', [
			'$scope', '$state', 'PlanningPeriodSvrc', function ($scope, $state, PlanningPeriodSvrc) {
				$scope.planningPeriod = PlanningPeriodSvrc.getPlanningPeriod.query();
			}
		]);
})();
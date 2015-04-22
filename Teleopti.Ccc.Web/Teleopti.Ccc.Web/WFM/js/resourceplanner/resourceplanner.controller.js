(function() {
	'use strict';
	angular.module('wfm.resourceplanner', [])
		.controller('ResourceplannerCtrl', [
			'$scope', 'ResourcePlannerSvrc', function($scope, ResourcePlannerSvrc) {
				$scope.scheduledDays = 0;
				$scope.schedulingPerformed = false;
				$scope.planningPeriod = ResourcePlannerSvrc.getPlanningPeriod.query({});
				$scope.recalcualteValidations = function(startDate, endDate) {
					console.log('date is change ' + startDate + ' ' + endDate);
				};
				$scope.LaunchSchedule = function(startDate, endDate) {
					$scope.schedulingPerformed = false;
					var planningPeriod = { StartDate: startDate, EndDate: endDate };
					ResourcePlannerSvrc.launchScheduling.query(JSON.stringify(planningPeriod)).$promise.then(function(result) {
						$scope.schedulingPerformed = true;
						$scope.scheduledDays = result.DaysScheduled;
					});;
				};
			}
		]);
})();
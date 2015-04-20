(function() {
	'use strict';
	angular.module('wfm.resourceplanner', [])
		.controller('ResourceplannerCtrl', [
			'$scope', 'ResourcePlannerSvrc', function ($scope, ResourcePlannerSvrc) {
			$scope.scheduledDays = 0;
				$scope.planningPeriod = ResourcePlannerSvrc.getPlanningPeriod.query({});
				$scope.LaunchSchedule = function(startDate, endDate) {
					var planningPeriod = { StartDate: startDate, EndDate: endDate };
					ResourcePlannerSvrc.launchScheduling.query(JSON.stringify(planningPeriod)).$promise.then(function(result) {
						$scope.scheduledDays = result.DaysScheduled;
						console.log($scope.scheduledDays);
					});;
				};
			}
		]);
})();
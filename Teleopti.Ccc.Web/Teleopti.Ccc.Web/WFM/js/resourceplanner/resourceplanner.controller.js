(function() {
	'use strict';
	angular.module('wfm.resourceplanner', [])
		.controller('ResourceplannerCtrl', [
			'$scope', 'ResourcePlannerSvrc', function($scope, ResourcePlannerSvrc) {
				$scope.scheduledDays = 0;
				$scope.schedulingPerformed = false;
				$scope.planningPeriod = ResourcePlannerSvrc.getPlanningPeriod.query({});
				$scope.recalculateValidation = function(id, startDate, endDate) {
					ResourcePlannerSvrc.updatePlanningPeriod.update(JSON.stringify({ Id: id, StartDate: startDate, EndDate: endDate }));
				};
				$scope.launchSchedule = function(startDate, endDate) {
					$scope.schedulingPerformed = false;
					var planningPeriod = { StartDate: startDate, EndDate: endDate };
					ResourcePlannerSvrc.launchScheduling.query(JSON.stringify(planningPeriod)).$promise.then(function(result) {
						$scope.schedulingPerformed = true;
						$scope.scheduledDays = result.DaysScheduled;
					});;
				};
				$scope.isEnabled = ResourcePlannerSvrc.isEnabled.query({toggle:'Wfm_ChangePlanningPeriod_33043'});
			}
		]);
})();
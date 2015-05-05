(function() {
	'use strict';
	angular.module('wfm.resourceplanner', [])
		.controller('ResourceplannerCtrl', [
			'$scope', 'ResourcePlannerSvrc', function($scope, ResourcePlannerSvrc) {
				//schedulings
				$scope.scheduledDays = 0;
				$scope.schedulingPerformed = false;
				$scope.launchSchedule = function(startDate, endDate) {
					$scope.schedulingPerformed = false;
					var planningPeriod = { StartDate: startDate, EndDate: endDate };
					ResourcePlannerSvrc.launchScheduling.query(JSON.stringify(planningPeriod)).$promise.then(function(result) {
						$scope.schedulingPerformed = true;
						$scope.scheduledDays = result.DaysScheduled;
					});
				};
				//toggle
				ResourcePlannerSvrc.isEnabled.query({ toggle: 'Wfm_ChangePlanningPeriod_33043' }).$promise.then(function(result) {
					$scope.isEnabled = result.IsEnabled;
				});
				//planningperiod
				$scope.planningPeriod = ResourcePlannerSvrc.getPlanningPeriod.query({});
				$scope.suggestions = function (id) {
					$scope.suggestedPlanningPeriods = [];
					ResourcePlannerSvrc.getSuggestions.query({ id: id }).$promise.then(function(result) {
						$scope.suggestedPlanningPeriods = result;
					});
				};
				$scope.rangeUpdated = function(id, rangeDetails) {
					var planningPeriodChangeRangeModel = { Number: rangeDetails.Number, PeriodType: rangeDetails.PeriodType, DateFrom: rangeDetails.StartDate };
					ResourcePlannerSvrc.changeRange.update({ id: id },  JSON.stringify(planningPeriodChangeRangeModel) ).$promise.then(function (result) {
						$scope.planningPeriod = result;
					});
				};
			}
		]);
})();
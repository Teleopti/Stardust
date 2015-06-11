(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('PlanningPeriodsCtrl', [
			'$scope', '$state', 'PlanningPeriodSvrc', '$stateParams', function ($scope, $state, PlanningPeriodSvrc, $stateParams) {
				//schedulings
				$scope.scheduledDays = 0;
				$scope.schedulingPerformed = false;
				$scope.launchSchedule = function(startDate, endDate) {
					$scope.schedulingPerformed = false;
					var planningPeriod = { StartDate: startDate, EndDate: endDate };
					PlanningPeriodSvrc.launchScheduling.query(JSON.stringify(planningPeriod)).$promise.then(function (result) {
						$scope.schedulingPerformed = true;
						$scope.scheduledDays = result.DaysScheduled;
						$state.go('resourceplannerreport', { result: result });
					});
				};

				//toggle
				PlanningPeriodSvrc.isEnabled.query({ toggle: 'Wfm_ChangePlanningPeriod_33043' }).$promise.then(function (result) {
					$scope.isEnabled = result.IsEnabled;
				});
				//planningperiod
				$scope.planningPeriod = PlanningPeriodSvrc.getPlanningPeriod.query({ id: $stateParams.id });
				$scope.suggestions = function (id) {
					$scope.suggestedPlanningPeriods = [];
					PlanningPeriodSvrc.getSuggestions.query({ id: id }).$promise.then(function (result) {
						$scope.suggestedPlanningPeriods = result;
					});
				};
				$scope.rangeUpdated = function(id, rangeDetails) {
					var planningPeriodChangeRangeModel = { Number: rangeDetails.Number, PeriodType: rangeDetails.PeriodType, DateFrom: rangeDetails.StartDate };
					PlanningPeriodSvrc.changeRange.update({ id: id }, JSON.stringify(planningPeriodChangeRangeModel)).$promise.then(function (result) {
						$scope.planningPeriod = result;
					});
				};

		}
		]);
})();
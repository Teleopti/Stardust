(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('PlanningPeriodsCtrl', [
			'$scope', '$state', 'PlanningPeriodSvrc', function ($scope, $state, PlanningPeriodSvrc) {
			console.log('params',$state.params);
				$scope.planningPeriods = PlanningPeriodSvrc.getPlanningPeriod.query();
			}
		]).controller('PlanningPeriodNewCtrl', [
			'$scope', '$state', 'PlanningPeriodNewSvrc', function ($scope, $state, PlanningPeriodNewSvrc) {
			$scope.error = false;
				$scope.suggestions = PlanningPeriodNewSvrc.suggestions.query();
				if ($scope.suggestions.length === 0) {
					$scope.error = true;

				}

				$scope.selectedRange = {};
			$scope.createNextPlanningPeriod = function() {
				var selectedRange = $scope.suggestions[$scope.selectedRange];
				var range = { Number: selectedRange.Number, PeriodType: selectedRange.PeriodType, DateFrom: selectedRange.StartDate };
				PlanningPeriodNewSvrc.planningperiod.update({}, JSON.stringify(range)).$promise.then(function(result) {
					$scope.planningPeriod = result;
					$state.go('resourceplanner', $scope.planningPeriod);
				});
			};
		}
		]);
})();
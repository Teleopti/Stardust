(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourcePlannerCtrl', [
			'$scope', '$state', 'planningPeriodService', function ($scope, $state, planningPeriodService) {
	            $scope.planningPeriods = [];
	            $scope.isEnabled = false;

	            planningPeriodService.getPlanningPeriods().$promise.then(function (result) {
                    $scope.planningPeriods = result;
                    $scope.isEnabled = true;
                });
				
				$scope.selectedPlanningPeriod = function(p) {
					if ($scope.isValid === false) return;
					$state.go('resourceplanner.planningperiod', { id: p.Id });
				};
				$scope.startNextPlanningPeriod = function() {
					planningPeriodService.nextPlanningPeriodDeprecated().$promise.then(function (result) {
						$state.go('resourceplanner.planningperiod', { id: result.Id });
						$scope.planningPeriods.push(result);
					});
				};
			}
		]);
})();

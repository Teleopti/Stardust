(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourcePlannerCtrl', [
			'$scope', '$state', 'ResourcePlannerSvrc', 'PlanningPeriodNewSvrc', function($scope, $state, ResourcePlannerSvrc, PlanningPeriodNewSvrc) {
	            $scope.planningPeriods = [];
	            $scope.isEnabled = false;

                ResourcePlannerSvrc.getPlanningPeriod.query().$promise.then(function (result) {
                    $scope.planningPeriods = result;
                    $scope.isEnabled = true;
                });
				
				$scope.selectedPlanningPeriod = function(p) {
					if ($scope.isValid === false) return;
					$state.go('resourceplanner.planningperiod', { id: p.Id });
				};
				$scope.startNextPlanningPeriod = function() {
					PlanningPeriodNewSvrc.planningperiod.update().$promise.then(function(result) {
						$state.go('resourceplanner.planningperiod', { id: result.Id });
						$scope.planningPeriods.push(result);
					});
				};
			}
		]);
})();

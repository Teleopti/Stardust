(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourcePlannerCtrl', [
			'$scope', '$state', 'ResourcePlannerSvrc', 'PlanningPeriodNewSvrc', function ($scope, $state, ResourcePlannerSvrc, PlanningPeriodNewSvrc) {
				$scope.planningPeriods = ResourcePlannerSvrc.getPlanningPeriod.query();
				$scope.selectedPlanningPeriod = function(p) {
					$state.go('resourceplanner.planningperiod', { id: p.Id });
				};
			$scope.startNextPlanningPeriod = function() {
				PlanningPeriodNewSvrc.planningperiod.update().$promise.then(function (result) {
					$state.go('resourceplanner.planningperiod', { id: result.Id });
					$scope.planningPeriods.push(result);
				});
			};
		}
		]).controller('PlanningPeriodNewCtrl', [
			'$scope', '$state', 'PlanningPeriodNewSvrc', function($scope, $state, PlanningPeriodNewSvrc) {
				$scope.error = false;
				$scope.suggestions = {};
				PlanningPeriodNewSvrc.suggestions.query().$promise.then(function(result) {
					$scope.suggestions = result;
				}, function() {
					$scope.error = true;
				});
				
				$scope.selectedRange = {};
				$scope.createNextPlanningPeriod = function(selectedRange) {
					var s = $scope.suggestions[selectedRange];
					var range = { Number: s.Number, PeriodType: s.PeriodType, DateFrom: s.StartDate };
					PlanningPeriodNewSvrc.planningperiod.update({}, JSON.stringify(range)).$promise.then(function(result) {
						$scope.planningPeriod = result;
						$state.go('resourceplanner.planningperiod', { id: $scope.planningPeriod.Id });
					});
				};
			}
		]);
})();
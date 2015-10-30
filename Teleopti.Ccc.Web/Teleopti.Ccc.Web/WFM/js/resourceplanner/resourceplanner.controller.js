(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourcePlannerCtrl', [
			'$scope', '$state', 'ResourcePlannerSvrc', 'PlanningPeriodNewSvrc', function($scope, $state, ResourcePlannerSvrc, PlanningPeriodNewSvrc) {
				$scope.planningPeriods = ResourcePlannerSvrc.getPlanningPeriod.query();
				$scope.isValid = false;
				ResourcePlannerSvrc.getDayoffRules.query().$promise.then(function (result){
					$scope.dayoffRules = result;
					$scope.optionForm.rules = result;

					$scope.isValid = true;
				});

				$scope.optionForm = {};

				//refactor me
				var workruleComposer = function(data){
					if ($scope.isValid === false) return;
					data.Default = true;
					data.Id = $scope.dayoffRules.Id
					ResourcePlannerSvrc.saveDayoffRules.update(data);
				};

				$scope.validateInput = function(node){
					if (node.MinConsecutiveDayOffs > node.MaxConsecutiveDayOffs ||
						node.MinConsecutiveWorkdays > node.MaxConsecutiveWorkdays ||
						node.MinDayOffsPerWeek > node.MaxDayOffsPerWeek) {
						$scope.isValid = false;
					}
					else {
						$scope.isValid = true;
					}
					workruleComposer(node);

				};
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

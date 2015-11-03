(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourcePlannerCtrl', [
			'$scope', '$state', 'ResourcePlannerSvrc', 'PlanningPeriodNewSvrc', function($scope, $state, ResourcePlannerSvrc, PlanningPeriodNewSvrc) {
				$scope.planningPeriods = ResourcePlannerSvrc.getPlanningPeriod.query();
				$scope.isValid = false;
				$scope.isEnabled = false;

				ResourcePlannerSvrc.getDayoffRules.query().$promise.then(function (result){
					$scope.dayoffRules = result;
					$scope.optionForm.rules = result;
					$scope.isValid = true;
					$scope.isEnabled = true;
				});

				$scope.optionForm = {};

				var workruleComposer = function(data){
					if ($scope.isValid === false) return;
					data.Default = true;
					data.Id = $scope.dayoffRules.Id
					ResourcePlannerSvrc.saveDayoffRules.update(data);
				};

				var validateInput= function(node){
					return node.MinConsecutiveDayOffs <= node.MaxConsecutiveDayOffs &&
						node.MinConsecutiveWorkdays <= node.MaxConsecutiveWorkdays &&
						node.MinDayOffsPerWeek <= node.MaxDayOffsPerWeek;
				}

				$scope.validateInputAndSend = function(node){
					$scope.isValid = validateInput(node);
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

(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourcePlannerCtrl', [
			'$scope', '$state', 'ResourcePlannerSvrc', 'PlanningPeriodNewSvrc', function($scope, $state, ResourcePlannerSvrc, PlanningPeriodNewSvrc) {
				$scope.planningPeriods = ResourcePlannerSvrc.getPlanningPeriod.query();
				ResourcePlannerSvrc.getDayoffRules.query().$promise.then(function (result){
					$scope.dayoffRules = result;
					$scope.optionForm.rules = result
				});
				$scope.isbroken = false;
				$scope.optionForm = {};

				//refactor me
				var workruleComposer = function(data){
					if ($scope.isbroken === true) return;
					data.Default = true;
					data.Id = $scope.dayoffRules.Id
					ResourcePlannerSvrc.saveDayoffRules.update(data);
				};

				$scope.validateInput = function(node){
					//refactor me
					for (var i = 0; i < Object.keys(node).length; i++) {
							if (node.MinConsecutiveDayOffs > node.MaxConsecutiveDayOffs) {
								$scope.isbroken = true;
							}
							else if (node.MinConsecutiveWorkdays > node.MaxConsecutiveWorkdays) {
								$scope.isbroken = true;
							}
							else if (node.MinDayOffsPerWeek > node.MaxDayOffsPerWeek) {
								$scope.isbroken = true;
							}
							else {
								$scope.isbroken = false;
							}

					}
					workruleComposer(node);

				};
				$scope.selectedPlanningPeriod = function(p) {
					if ($scope.isbroken === true) return;
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

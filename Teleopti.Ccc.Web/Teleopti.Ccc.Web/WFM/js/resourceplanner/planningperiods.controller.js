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
			$scope.suggestions = {};
			PlanningPeriodNewSvrc.suggestions.query().$promise.then(function(result) {
				$scope.suggestions = result;
				
			},function() {
				$scope.error = true;
			});


				$scope.selectedRange = {};
				$scope.createNextPlanningPeriod = function (selectedRange) {
				var s = $scope.suggestions[selectedRange];
				var range = { Number: s.Number, PeriodType: s.PeriodType, DateFrom: s.StartDate };
				PlanningPeriodNewSvrc.planningperiod.update({}, JSON.stringify(range)).$promise.then(function(result) {
					$scope.planningPeriod = result;
					$state.go('resourceplanner', $scope.planningPeriod);
				});
			};
		}
		]);
})();
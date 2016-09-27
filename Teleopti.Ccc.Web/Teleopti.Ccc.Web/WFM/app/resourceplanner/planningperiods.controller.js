
(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('PlanningPeriodsCtrl', [
			'$scope', '$state', '$stateParams', '$interval', 'PlanningPeriodSvrc', 'Toggle', '$translate',
			function($scope, $state, $stateParams, $interval, PlanningPeriodSvrc, toggleService, $translate) {
				//schedulings
				$scope.status = '';
				$scope.schedulingPerformed = false;
				$scope.scheduleClicked = false;
				$scope.initialized = false;
				$scope.isEnabled = false;
				$scope.dayoffRules = [];
				$scope.planningPeriod = {};
				$scope.disableSchedule = function() {
					return !$scope.initialized || $scope.scheduleClicked || !$scope.planningPeriod.StartDate;
				};

				function handleScheduleOrOptimizeError() {
					$scope.errorMessage = "An error occurred. Please try again.";
					$scope.schedulingPerformed = false;
					$scope.status = '';
					$scope.scheduleClicked = false;
				}

				PlanningPeriodSvrc.getDayOffRules().then(function(result) {
					$scope.dayoffRules = result.data;
				}, handleScheduleOrOptimizeError);

				var tenMinutes = 1000 * 60 * 10;
				var keepAliveRef = $interval(function() {
					PlanningPeriodSvrc.keepAlive();
				}, tenMinutes);

				$scope.launchSchedule = function(p) {
					$scope.errorMessage = undefined;
					$scope.schedulingPerformed = false;
					$scope.scheduleClicked = true;

					var planningPeriod = {
						StartDate: p.StartDate,
						EndDate: p.EndDate
					};
					$scope.status = $translate.instant('PresentTenseSchedule');
					PlanningPeriodSvrc.launchScheduling.query(JSON.stringify(planningPeriod)).$promise.then(function(scheduleResult) {
						$scope.status = $translate.instant('OptimizingDaysOff');
						//to make sure long optimization request doesn't create a new cookie based on current time
						//we call keepAlive here again
						PlanningPeriodSvrc.keepAlive().then(function() {
							PlanningPeriodSvrc.launchOptimization.query({id: p.Id}).$promise.then(function(result) {
								$scope.schedulingPerformed = true;
								$state.go('resourceplanner.report', {
									id:p.Id,
									result: scheduleResult,
									interResult: result,
									planningperiod: p
								});
							}, handleScheduleOrOptimizeError);
						}, handleScheduleOrOptimizeError);
					}, handleScheduleOrOptimizeError);
				};

				toggleService.togglesLoaded.then(function() {
					$scope.isEnabled = toggleService.Wfm_ChangePlanningPeriod_33043;
				});

				PlanningPeriodSvrc.getPlanningPeriod.query({
					id: $stateParams.id
				}).$promise.then(function(result) {
					$scope.planningPeriod = result;
					$scope.initialized = true;
				});

				$scope.suggestions = function(id) {
					$scope.suggestedPlanningPeriods = [];
					PlanningPeriodSvrc.getSuggestions.query({
						id: id
					}).$promise.then(function(result) {
						result.forEach(function(suggestion) {
							if (suggestion.PeriodType === "Week") {
								suggestion.PeriodType = $translate.instant('SchedulePeriodTypeWeek');
							}
							if (suggestion.PeriodType === "Day") {
								suggestion.PeriodType = $translate.instant('SchedulePeriodTypeDay');
							}
							if (suggestion.PeriodType === "Month") {
								suggestion.PeriodType = $translate.instant('SchedulePeriodTypeMonth');
							}
						});
						$scope.suggestedPlanningPeriods = result;
					});
				};
				$scope.rangeUpdated = function(id, rangeDetails) {
					$scope.rangeDisabled = true;
					var planningPeriodChangeRangeModel = {
						Number: rangeDetails.Number,
						PeriodType: rangeDetails.PeriodType,
						DateFrom: rangeDetails.StartDate
					};
					PlanningPeriodSvrc.changeRange.update({
						id: id
					}, JSON.stringify(planningPeriodChangeRangeModel)).$promise.then(function(result) {
						$scope.rangeDisabled = false;
						$scope.planningPeriod = result;
					});
				};
				$scope.editRuleset = function(filter) {
					$state.go('resourceplanner.filter', {
						filterId: filter.Id,
						periodId: $stateParams.id,
						isDefault: filter.Default
					});
				};
				$scope.createRuleset = function() {
					$state.go('resourceplanner.filter', {
						periodId: $stateParams.id
					});
				};
				$scope.destoryRuleset = function(node) {
					PlanningPeriodSvrc.destroyDayOffRule.remove({
						id: node.Id
					}).$promise.then(function() {
						PlanningPeriodSvrc.getDayOffRules().then(function(result) {
							$scope.dayoffRules = result.data;
						});
					});
				};

				$scope.$on('$destroy', function() {
					$interval.cancel(keepAliveRef);
				});
			}
		]);
})();

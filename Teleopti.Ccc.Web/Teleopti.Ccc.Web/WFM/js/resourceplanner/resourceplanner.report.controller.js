
(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerReportCtrl', [
			'$scope', '$state', '$translate', '$stateParams', 'ResourcePlannerReportSrvc', 'PlanningPeriodSvrc', 'growl', 'Toggle', '$interval',
			function($scope, $state, $translate, $stateParams, ResourcePlannerReportSrvc, PlanningPeriodSvrc, growl, toggleService, $interval) {
				var toggledOptimization = false;
				var scheduleResult = $stateParams.interResult.SkillResultList ? $stateParams.interResult.SkillResultList : [];
				$scope.issues = $stateParams.result.BusinessRulesValidationResults ? $stateParams.result.BusinessRulesValidationResults : [];
				$scope.scheduledAgents = $stateParams.result.ScheduledAgentsCount ? $stateParams.result.ScheduledAgentsCount : 0;
				$scope.planningPeriod = $stateParams.planningperiod;
				$scope.hasIssues = $scope.issues.length > 0;
				$scope.isDataAvailable = scheduleResult.length > 0;
				$scope.dayNodes = scheduleResult;
				$scope.optimizeRunning = false;
				$scope.optimizationHasBeenRun = false;

				toggleService.togglesLoaded.then(function() {
					toggledOptimization = toggleService.Scheduler_IntradayOptimization_36617;
				});
				$scope.optimizeDayOffIsEnabled = function() {
					return (toggledOptimization && $stateParams.id !== "");
				}
				$scope.$watch(function() {
						return $scope.dayNodes;
					},
					function(newData) {
						if (newData.length !== 0) {
							parseRelativeDifference(newData);
							parseWeekends(newData)
						}
					}
				);

				$scope.intraOptimize = function() {
					$scope.optimizeRunning = true;
					//to make sure long optimization request doesn't create a new cookie based on current time
					//we call keepAlive here again
					PlanningPeriodSvrc.keepAlive().then(function() {
						ResourcePlannerReportSrvc.intraOptimize.save({
							id: $stateParams.id
						}).$promise.then(function(result) {
							$scope.optimizeRunning = false;
							$scope.optimizationHasBeenRun = true;
							//Untill further notice the intraOptimize results will be disabled
							//$scope.dayNodes = result.SkillResultList;
							notifyOptimizationDone();
						}, function(reason) {
							$scope.optimizeRunning = false;
						});
					});
				};

				var tenMinutes = 1000 * 60 * 10;
				var keepAliveRef = $interval(function() {
					PlanningPeriodSvrc.keepAlive();
				}, tenMinutes);

				var notifyOptimizationDone = function() {
					growl.success("<i class='mdi mdi-thumb-up'></i>" + $translate.instant('Done'), {
						ttl: 0,
						disableCountDown: true
					});
				};
				var parseRelativeDifference = function(period) {
					ResourcePlannerReportSrvc.parseRelDif(period);
				};
				var parseWeekends = function(period) {
					ResourcePlannerReportSrvc.parseWeek(period);
				};
				parseRelativeDifference($scope.dayNodes);
				parseWeekends($scope.dayNodes)
				$scope.gridOptions = {
					columnDefs: [{
						name: 'Agent',
						field: 'Name',
						enableColumnMenu: false
					}, {
						name: 'Detail',
						field: 'Message',
						enableColumnMenu: false
					}, {
						name: 'Issue-type',
						field: 'BusinessRuleCategoryText',
						enableColumnMenu: false
					}],
					data: $scope.issues
				};

				$scope.publishSchedule = function() {
					//Translate me better
					if ($scope.publishedClicked === true) {
						growl.warning("<i class='mdi mdi-warning'></i>" + $translate.instant('Error'), {
							ttl: 5000,
							disableCountDown: true
						});
						return;
					}
					$scope.publishedClicked = true;
					PlanningPeriodSvrc.publishPeriod.query({
						id: $stateParams.id
					}).$promise.then(function() {
						growl.success("<i class='mdi mdi-thumb-up'></i>" + $translate.instant('Done'), {
							ttl: 5000,
							disableCountDown: true
						});
					});
				};
				$scope.$on('$destroy', function() {
					$interval.cancel(keepAliveRef);
				});
			}
		]);
})();

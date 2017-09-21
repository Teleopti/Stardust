
(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerReportCtrl', [
			'$scope', '$state', '$translate', '$stateParams', 'ResourcePlannerReportSrvc', 'planningPeriodService', 'NoticeService', 'Toggle', '$interval',
			function ($scope, $state, $translate, $stateParams, ResourcePlannerReportSrvc, planningPeriodService, NoticeService, toggleService, $interval) {
				var toggledOptimization = false;
				var toggledSchedulingOnStardust = false;
				var tenMinutes = 1000 * 60 * 10;
				var planningPeriodId = $stateParams.id ? $stateParams.id : $scope.id;
				var checkProgressRef;
				$scope.issues = [];
				$scope.scheduledAgents = 0;
				$scope.totalAgents = 0;
				$scope.hasIssues = false;
				$scope.isDataAvailable = false;
				$scope.dayNodes = [];
				$scope.optimizeRunning = false;
				$scope.optimizationHasBeenRun = false;
				$scope.gridOptions = {};
				$scope.optimizeDayOffIsEnabled = optimizeDayOffIsEnabled;
				$scope.intraOptimize = intraOptimize;
				$scope.publishSchedule = publishSchedule;

				var keepAliveRef = $interval(function () {
					planningPeriodService.keepAlive();
				}, tenMinutes);

				toggleService.togglesLoaded.then(function () {
					toggledOptimization = toggleService.Scheduler_IntradayOptimization_36617;
					toggledSchedulingOnStardust = toggleService.Wfm_ResourcePlanner_SchedulingOnStardust_42874;
					initLoad();
				});

				$scope.$watch(function () { return $scope.dayNodes; },
					function (newData) {
						if (newData.length !== 0) {
							parseRelativeDifference(newData);
							parseWeekends(newData);
						}
					}
				);

				$scope.$on('$destroy', function () {
					$interval.cancel(keepAliveRef);
					if (checkProgressRef)
						$interval.cancel(checkProgressRef);
				});

				function initResult(interResult, result, planningPeriod) {
					if (planningPeriod != undefined) {
						planningPeriodService.getPlanningPeriod({ id: planningPeriodId })
							.$promise.then(function (data) {
								$scope.totalAgents = data.TotalAgents;
							});
					}

					$scope.planningPeriod = planningPeriod;
					var scheduleResult = interResult.SkillResultList ? interResult.SkillResultList : [];
					$scope.issues = result.BusinessRulesValidationResults ? result.BusinessRulesValidationResults : [];
					$scope.scheduledAgents = result.ScheduledAgentsCount ? result.ScheduledAgentsCount : 0;
					$scope.hasIssues = $scope.issues.length > 0;
					$scope.isDataAvailable = scheduleResult.length > 0;
					$scope.dayNodes = scheduleResult;
					$scope.optimizeRunning = false;
					$scope.optimizationHasBeenRun = false;

					parseRelativeDifference($scope.dayNodes);
					parseWeekends($scope.dayNodes);

					displayGrid();
				}

				function initLoad() {
					if (toggledSchedulingOnStardust && !$stateParams.ranSynchronously) {
						planningPeriodService.lastJobResult({ id: planningPeriodId })
							.$promise.then(function (data) {
								initResult(data.OptimizationResult, data.ScheduleResult, data.PlanningPeriod);
							});
						checkIntradayOptimizationProgress();
						checkProgressRef = $interval(function () {
							checkIntradayOptimizationProgress();
						}, 10000);
					} else {
						initResult($stateParams.interResult, $stateParams.result, $stateParams.planningperiod);
					}
				}

				function publishSchedule() {
					//Translate me better
					if ($scope.publishedClicked === true) {
						NoticeService.warning($translate.instant('Error'), null, true);
						return;
					}
					$scope.publishedClicked = true;
					planningPeriodService.publishPeriod({
						id: planningPeriodId
					}).$promise.then(function () {
						NoticeService.success($translate.instant('Done'), null, true);
					});
				};

				function optimizeDayOffIsEnabled() {
					return (toggledOptimization && planningPeriodId !== "");
				}

				function notifyOptimizationDone() {
					NoticeService.success($translate.instant('Done'), null, true);
				}
				function parseRelativeDifference(period) {
					ResourcePlannerReportSrvc.parseRelDif(period);
				}
				function parseWeekends(period) {
					ResourcePlannerReportSrvc.parseWeek(period);
				}

				function intraOptimize() {
					$scope.optimizeRunning = true;
					if (toggledSchedulingOnStardust) {
						ResourcePlannerReportSrvc.intraOptimize({
							id: planningPeriodId,
							runAsynchronously: true
						}).$promise.then(checkIntradayOptimizationProgress, intradayOptimizationFailedHandler);
					} else {
						//to make sure long optimization request doesn't create a new cookie based on current time
						//we call keepAlive here again
						planningPeriodService.keepAlive().then(function () {
							ResourcePlannerReportSrvc.intraOptimize({
								id: planningPeriodId,
								runAsynchronously: false
							}).$promise.then(intradayOptimizationDone, intradayOptimizationFailedHandler);
						});
					}
				};

				function checkIntradayOptimizationProgress() {
					planningPeriodService.lastIntradayOptimizationJobStatus({ id: planningPeriodId }).$promise.then(function (result) {
						if (result.HasJob === false) {
							$scope.optimizeRunning = false;
						} else if (result.Successful === true) {
							intradayOptimizationDone();
						} else if (result.Failed) {
							intradayOptimizationFailedHandler();
						}
					});
				}

				function intradayOptimizationDone() {
					//Untill further notice the intraOptimize results will be disabled
					//$scope.dayNodes = result.SkillResultList;
					if ($scope.optimizeRunning && !$scope.optimizationHasBeenRun)
						notifyOptimizationDone();
					$scope.optimizeRunning = false;
					$scope.optimizationHasBeenRun = true;
				}
				function intradayOptimizationFailedHandler() {
					$scope.optimizeRunning = false;
				}
				function displayGrid() {
					$scope.gridOptions = {
						columnDefs: [
							{
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
							}
						],
						data: $scope.issues
					};
				}
			}
		])
})();

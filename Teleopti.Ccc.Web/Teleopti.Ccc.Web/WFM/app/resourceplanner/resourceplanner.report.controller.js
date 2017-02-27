﻿
(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerReportCtrl', [
			'$scope', '$state', '$translate', '$stateParams', 'ResourcePlannerReportSrvc', 'PlanningPeriodSvrc', 'NoticeService', 'Toggle', '$interval',
			function($scope, $state, $translate, $stateParams, ResourcePlannerReportSrvc, PlanningPeriodSvrc, NoticeService, toggleService, $interval) {
				var toggledOptimization = false;
				var toggledSchedulingOnStardust = false;
				$scope.issues = [];
				$scope.scheduledAgents = 0;
				$scope.planningPeriod = $stateParams.planningperiod;
				$scope.hasIssues = false;
				$scope.isDataAvailable = false;
				$scope.dayNodes = [];
				$scope.optimizeRunning = false;
				$scope.optimizationHasBeenRun = false;


				var initResult = function (interResult, result) {
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

					displayGird();
				}

				var initLoad = function() {
					if (toggledSchedulingOnStardust) {
						PlanningPeriodSvrc.lastJobResult.query({ id: $stateParams.id })
							.$promise.then(function (data) {
								var interResult = data.OptimizationResult;
								var result = data.ScheduleResult;
								initResult(interResult, result);
							});
					} else {
						var interResult = $stateParams.interResult;
						var result = $stateParams.result;
						initResult(interResult, result);
					}
				}

				toggleService.togglesLoaded.then(function() {
					toggledOptimization = toggleService.Scheduler_IntradayOptimization_36617;
					toggledSchedulingOnStardust = toggleService.Wfm_ResourcePlanner_SchedulingOnStardust_42874;
					initLoad();
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
							parseWeekends(newData);
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
					NoticeService.success($translate.instant('Done'), null, true);
				};
				var parseRelativeDifference = function(period) {
					ResourcePlannerReportSrvc.parseRelDif(period);
				};
				var parseWeekends = function(period) {
					ResourcePlannerReportSrvc.parseWeek(period);
				};

				var displayGird = function() {
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

				$scope.publishSchedule = function() {
					//Translate me better
					if ($scope.publishedClicked === true) {
						NoticeService.warning($translate.instant('Error'), null, true);
						return;
					}
					$scope.publishedClicked = true;
					PlanningPeriodSvrc.publishPeriod.query({
						id: $stateParams.id
					}).$promise.then(function() {
						NoticeService.success( $translate.instant('Done'), null, true);
					});
				};
				$scope.$on('$destroy', function() {
					$interval.cancel(keepAliveRef);
				});
			}
		]);
})();

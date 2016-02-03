
(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerReportCtrl', [
			'$scope', '$state', '$stateParams', 'ResourcePlannerReportSrvc', 'PlanningPeriodSvrc', 'growl', 'Toggle',
			function($scope, $state, $stateParams, ResourcePlannerReportSrvc, PlanningPeriodSvrc, growl, toggleService) {
				var toggledOptimization = false;
				var optimizationState = true;
				var scheduleResult = $stateParams.interResult.SkillResultList ? $stateParams.interResult.SkillResultList : [];
				var issues = $stateParams.result.BusinessRulesValidationResults ? $stateParams.result.BusinessRulesValidationResults : [];
				$scope.scheduledAgents = $stateParams.result.ScheduledAgentsCount ? $stateParams.result.ScheduledAgentsCount : 0;
				$scope.planningPeriod = $stateParams.planningperiod;
				$scope.hasIssues = issues.length > 0;
				$scope.dayNodes = scheduleResult;
				$scope.optimizeRunning = false;
				toggleService.togglesLoaded.then(function() {
					toggledOptimization = toggleService.Scheduler_IntradayOptimization_36617;
				});
				$scope.optimizeDayOffIsEnabled = function() {
					return (toggledOptimization && optimizationState && $stateParams.id !== "");
				}
				$scope.intraOptimize = function() {
					$scope.optimizeRunning = true;
					optimizationState = false
					ResourcePlannerReportSrvc.intraOptimize.save({
						id: $stateParams.id
					}).$promise.then(function(result) {
						optimizationState = true;
						$scope.optimizeRunning = false;
						$scope.dayNodes = result.SkillResultList;
						notifyOptimization('successfull');
					}, function(reason) {
						notifyOptimization('error');
					});
				};
				var notifyOptimization = function(status) {
					//translate me better
					if (status === 'successfull') {
						growl.success("<i class='mdi mdi-thumb-up'></i>" + $translate.instant('Done'), {
							ttl: 0,
							disableCountDown: true
						});
					} else {
						growl.warning("<i class='mdi mdi-warning'></i>" + $translate.instant('Error'), {
							ttl: 0,
							disableCountDown: true
						});
					}

				};
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
					data: issues
				};
				var parseRelDif = function(period) {
					ResourcePlannerReportSrvc.parseRelDif(period);
				}(scheduleResult);
				var parseWeekends = function(period) {
					ResourcePlannerReportSrvc.parseWeek(period);
				}(scheduleResult);
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
			}
		]);
})();

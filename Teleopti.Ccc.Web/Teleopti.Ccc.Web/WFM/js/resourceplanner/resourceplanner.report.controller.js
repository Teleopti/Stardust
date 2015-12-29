(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerReportCtrl', [
			'$scope','$filter','$state', '$stateParams','ResourcePlannerReportSrvc','PlanningPeriodSvrc','growl', function($scope, $filter, $state, $stateParams, ResourcePlannerReportSrvc, PlanningPeriodSvrc, growl) {
	            $scope.issues = $stateParams.result.BusinessRulesValidationResults;
	            $scope.hasIssues = $scope.issues.length > 0;
	            $scope.scheduledAgents = $stateParams.result.ScheduledAgentsCount;
	            var scheduleResult = $stateParams.interResult.SkillResultList;
	            $scope.planningPeriod = $stateParams.planningperiod;
	            $scope.dayNodes = scheduleResult;
	            $scope.gridOptions = {
	                columnDefs: [
	                    { name: 'Agent', field: 'Name', enableColumnMenu: false },
	                    { name: 'Detail', field: 'Message', enableColumnMenu: false },
	                    { name: 'Issue-type', field: 'BusinessRuleCategoryText', enableColumnMenu: false }
	                ],
	                data: $scope.issues
	            };
	            var parseRelDif = function(period) {
	                ResourcePlannerReportSrvc.parseRelDif(period);
	            }(scheduleResult);
	            var parseWeekends = function(period) {
	                ResourcePlannerReportSrvc.parseWeek(period);
	            }(scheduleResult);
	            $scope.publishSchedule = function() {
	                if ($scope.publishedClicked === true) {
	                    growl.warning("<i class='mdi mdi-warning'></i> Scheduled agents have already been published", {
	                        ttl: 5000,
	                        disableCountDown: true
	                    });
	                    return;
	                }
	                $scope.publishedClicked = true;
	                PlanningPeriodSvrc.publishPeriod.query({ id: $scope.planningPeriod.Id }).$promise.then(function() {
	                    growl.success("<i class='mdi mdi-thumb-up'></i> Scheduled agents have been successfully published", {
	                        ttl: 5000,
	                        disableCountDown: true
	                    });
	                });
	            };
	        }
	    ]);
})();

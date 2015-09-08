﻿(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('PlanningPeriodsCtrl', [
			'$scope', '$state', '$stateParams', '$interval', 'PlanningPeriodSvrc', function ($scope, $state, $stateParams, $interval, PlanningPeriodSvrc) {
	        //schedulings
	        $scope.status = '';
	        $scope.scheduledDays = 0;
	        $scope.schedulingPerformed = false;

	        $scope.isScheduleRunning = false;

	        function updateRunningStatus() {
	            PlanningPeriodSvrc.status.get().$promise.then(function(result) {
	                $scope.isScheduleRunning = result.IsRunning;
	            });
	        }

	        updateRunningStatus();
	        var stopPolling = $interval(updateRunningStatus, 30 * 1000);

	        function cancelPoll() {
	            $scope.isScheduleRunning = false;
	            if (angular.isDefined(stopPolling)) {
	                $interval.cancel(stopPolling);
	            }
	        }

	        $scope.launchSchedule = function (p) {
	            $scope.schedulingPerformed = false;
	            var planningPeriod = { StartDate: p.StartDate, EndDate: p.EndDate };
	            cancelPoll();
	            $scope.status = 'Scheduling';
	            PlanningPeriodSvrc.launchScheduling.query(JSON.stringify(planningPeriod)).$promise.then(function (scheduleResult) {
	                //if not success
	                $scope.scheduledDays = scheduleResult.DaysScheduled;
	                //else
	                $scope.status = 'Optimizing days off';
	                PlanningPeriodSvrc.launchOptimization.query({ id: p.Id }, JSON.stringify(scheduleResult.ThrottleToken)).$promise.then(function() {
	                    $scope.schedulingPerformed = true;
	                    $state.go('resourceplanner.report', { result: scheduleResult });
	                });
	            });
	        };

	        //toggle
	        PlanningPeriodSvrc.isEnabled.query({ toggle: 'Wfm_ChangePlanningPeriod_33043' }).$promise.then(function(result) {
	            $scope.isEnabled = result.IsEnabled;
	        });
	        //planningperiod
	        $scope.planningPeriod = PlanningPeriodSvrc.getPlanningPeriod.query({ id: $stateParams.id });
	        $scope.suggestions = function(id) {
	            $scope.suggestedPlanningPeriods = [];
	            PlanningPeriodSvrc.getSuggestions.query({ id: id }).$promise.then(function(result) {
	                $scope.suggestedPlanningPeriods = result;
	            });
	        };
	        $scope.rangeUpdated = function(id, rangeDetails) {
	            var planningPeriodChangeRangeModel = { Number: rangeDetails.Number, PeriodType: rangeDetails.PeriodType, DateFrom: rangeDetails.StartDate };
	            PlanningPeriodSvrc.changeRange.update({ id: id }, JSON.stringify(planningPeriodChangeRangeModel)).$promise.then(function(result) {
	                $scope.planningPeriod = result;
	            });
	        };

	        $scope.$on('$destroy', function() {
	            // Make sure that the interval is destroyed too
	            cancelPoll();
	        });
	    }
	]);
})();
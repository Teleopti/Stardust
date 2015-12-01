(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('PlanningPeriodsCtrl', [
			'$scope', '$state', '$stateParams', '$interval', 'PlanningPeriodSvrc', function ($scope, $state, $stateParams, $interval, PlanningPeriodSvrc) {
	        //schedulings
	        $scope.status = '';
	        $scope.scheduledDays = 0;
	        $scope.schedulingPerformed = false;

	        $scope.isScheduleRunning = false;
	        $scope.scheduleClicked = false;
	        $scope.disableSchedule = function () {
		        return $scope.isScheduleRunning || $scope.scheduleClicked || !$scope.planningPeriod.StartDate;
	        };
				$scope.dayoffRules=[];

				PlanningPeriodSvrc.getDayOffRules().then(function (result) {
					$scope.dayoffRules = result.data;
				});


	        function updateRunningStatus() {
	            PlanningPeriodSvrc.status.get().$promise.then(function(result) {
	                $scope.isScheduleRunning = result.IsRunning;
	            });
	        }

	        var stopPolling;
	        function startPoll() {
	            updateRunningStatus();
	            stopPolling = $interval(updateRunningStatus, 10 * 1000);
	        }
	        startPoll();

	        function cancelPoll() {
	            $scope.isScheduleRunning = false;
	            if (angular.isDefined(stopPolling)) {
	                $interval.cancel(stopPolling);
	            }
	        }

	        function handleScheduleOrOptimizeError(error) {
	            startPoll();

	            $scope.errorMessage = "An error occurred. Please try again.";
	            $scope.schedulingPerformed = false;
	            $scope.status = '';
	            $scope.scheduleClicked = false;
	        }
	        $scope.launchSchedule = function (p) {
	            $scope.errorMessage = undefined;
	            $scope.schedulingPerformed = false;
	            $scope.scheduleClicked = true;

	            var planningPeriod = { StartDate: p.StartDate, EndDate: p.EndDate };
	            cancelPoll();
	            $scope.status = 'Scheduling';
	            PlanningPeriodSvrc.launchScheduling.query(JSON.stringify(planningPeriod)).$promise.then(function (scheduleResult) {
	                //if not success
	                $scope.scheduledDays = scheduleResult.DaysScheduled;
	                //else
	                $scope.status = 'Optimizing days off';
	                PlanningPeriodSvrc.launchOptimization.query({ id: p.Id }, JSON.stringify(scheduleResult.ThrottleToken)).$promise.then(function(result) {
	                    $scope.schedulingPerformed = true;
							$state.go('resourceplanner.report', { result: scheduleResult, interResult:result, planningperiod:p});
	                }, handleScheduleOrOptimizeError);
	            }, handleScheduleOrOptimizeError);
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
			$scope.editRuleset = function(filter){
				$state.go('resourceplanner.filter',{filterId:filter.Id, periodId:$stateParams.id,isDefault:filter.Default})
			};
			$scope.createRuleset = function(){
				$state.go('resourceplanner.filter',{periodId:$stateParams.id})
			};
			$scope.destoryRuleset = function(node){
				PlanningPeriodSvrc.destroyDayOffRule.remove({id:node.Id}).$promise.then(function(){
					PlanningPeriodSvrc.getDayOffRules().then(function (result) {
						$scope.dayoffRules = result.data;
					});
				});
			};

	        $scope.$on('$destroy', function() {
	            // Make sure that the interval is destroyed too
	            cancelPoll();
	        });
	    }
	]);
})();

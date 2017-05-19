
(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('PlanningPeriodsCtrl', [
			'$scope', '$state', '$stateParams', '$interval', 'planningPeriodService', 'Toggle', '$translate', 'dayOffRuleService',
			function ($scope, $state, $stateParams, $interval, planningPeriodService, toggleService, $translate, dayOffRuleService) {
				var runAsynchronously = false;
				//schedulings
				$scope.status = '';
				$scope.schedulingPerformed = false;
				$scope.scheduleClicked = false;
				$scope.initialized = false;
				$scope.isEnabled = false;
				$scope.dayOffRules = [];
				$scope.planningPeriod = {};
				$scope.lastJobSuccessful = false;
				$scope.disableSchedule = function () {
					return !$scope.initialized || $scope.scheduleClicked || !$scope.planningPeriod.StartDate;
				};
				$scope.disableReport = function () {
					return !($scope.schedulingPerformed && $scope.lastJobSuccessful && runAsynchronously);
				};

				function getDayOffRules() {
					var agentGroupId = $scope.planningPeriod.AgentGroupId;
					if (agentGroupId != undefined) {
						dayOffRuleService.getDayOffRulesByAgentGroupId({ agentGroupId: agentGroupId }).$promise.then(handleGetDayOffRulesSuccess, handleGetDayOffRulesFail);
					} else {
						dayOffRuleService.getDayOffRules().$promise.then(handleGetDayOffRulesSuccess, handleGetDayOffRulesFail);
					}

					function handleGetDayOffRulesSuccess(result) {
						$scope.dayOffRules = result;
					}
					function handleGetDayOffRulesFail() {
						handleScheduleOrOptimizeError($translate.instant('FailedToLoadDayoffRules'));
					}
				}

				function handleScheduleOrOptimizeError(message) {
					if (!message)
					{ message = "An error occurred. Please try again."; }
					$scope.errorMessage = message;
					$scope.schedulingPerformed = false;
					$scope.status = '';
					$scope.scheduleClicked = false;
				}

				var tenMinutes = 1000 * 60 * 10;
				var keepAliveRef = $interval(function () {
					planningPeriodService.keepAlive();
				}, tenMinutes);


				var checkProgress = function (planningPeriodId) {
					planningPeriodService.lastJobStatus({ id: planningPeriodId })
						.$promise.then(function (result) {
							if (!result.HasJob) {
								$scope.schedulingPerformed = false;
								$scope.scheduleClicked = false;
								$scope.lastJobSuccessful = false;
							} else {
								if (result.Successful) {
									$scope.scheduleClicked = false;
									$scope.schedulingPerformed = true;
									$scope.lastJobSuccessful = true;
								} else if (result.Failed) {
									if (result.CurrentStep === 0) {
										handleScheduleOrOptimizeError($translate.instant('FailedToScheduleForSelectedPlanningPeriodDueToTechnicalError'));
									} else if (result.CurrentStep === 1) {
										handleScheduleOrOptimizeError($translate.instant('FailedToScheduleForSelectedPlanningPeriodDueToTechnicalError'));
									} else if (result.CurrentStep === 2) {
										handleScheduleOrOptimizeError($translate.instant('FailedToOptimizeDayoffForSelectedPlanningPeriodDueToTechnicalError'));
									}
									$scope.lastJobSuccessful = false;
								} else {
									$scope.scheduleClicked = true;
									$scope.schedulingPerformed = false;
									if (result.CurrentStep === 0) {
										$scope.status = $translate.instant('PresentTenseSchedule');
									} else if (result.CurrentStep === 1) {
										$scope.status = $translate.instant('OptimizingDaysOff');
									}
									$scope.lastJobSuccessful = false;
								}
							}
						});
				}


				var launchScheduleNew = function (p) {
					$scope.errorMessage = undefined;
					$scope.schedulingPerformed = false;
					$scope.scheduleClicked = true;

					planningPeriodService.launchScheduling({ id: p.Id, runAsynchronously: true }).$promise.then(function () {
						checkProgress(p.Id);
					}, function () {
						handleScheduleOrOptimizeError($translate.instant('FailedToCreateSchedulingJobForSelectedPlanningPeriod'));
					});
				}

				var launchScheduleOld = function (p) {
					$scope.errorMessage = undefined;
					$scope.schedulingPerformed = false;
					$scope.scheduleClicked = true;

					$scope.status = $translate.instant('PresentTenseSchedule');
					planningPeriodService.launchScheduling({ id: p.Id, runAsynchronously: false }).$promise.then(function (scheduleResult) {
						$scope.status = $translate.instant('OptimizingDaysOff');
						//to make sure long optimization request doesn't create a new cookie based on current time
						//we call keepAlive here again
						planningPeriodService.keepAlive()
							.then(function () {
								planningPeriodService.launchOptimization({ id: p.Id })
									.$promise.then(function (result) {
										$scope.schedulingPerformed = true;
										$state.go('resourceplanner.report',
											{
												id: p.Id,
												result: scheduleResult,
												interResult: result,
												planningperiod: p,
												ranSynchronously: true
											});
									},
									handleScheduleOrOptimizeError);
							},
							handleScheduleOrOptimizeError);
					}, handleScheduleOrOptimizeError);
				}

				$scope.launchSchedule = function (p) {
					if (runAsynchronously) {
						launchScheduleNew(p);
					} else {
						launchScheduleOld(p);
					}
				};

				$scope.goToReport = function (p) {
					$state.go('resourceplanner.report',
						{
							id: p.Id
						});
				};

				$scope.shouldShowValidationErrors = function (planningPeriod) {
					return planningPeriod.ValidationResult != undefined &&
						planningPeriod.ValidationResult.InvalidResources.length > 0
						&& planningPeriod.State === 'New';
				}

				toggleService.togglesLoaded.then(function () {
					$scope.isEnabled = toggleService.Wfm_ChangePlanningPeriod_33043;
					runAsynchronously = toggleService.Wfm_ResourcePlanner_SchedulingOnStardust_42874 && !$stateParams.runForTest;
				});

				var checkProgressRef;

				planningPeriodService.getPlanningPeriod({ id: $stateParams.id })
					.$promise.then(function (result) {
						$scope.planningPeriod = result;
						getDayOffRules();
						$scope.initialized = true;
						if (runAsynchronously) {
							checkProgress($stateParams.id);
							checkProgressRef = $interval(function () {
								checkProgress($stateParams.id);
							}, 10000);
						}
					});

				$scope.suggestions = function (id) {
					$scope.suggestedPlanningPeriods = [];
					planningPeriodService.getSuggestions({ id: id })
						.$promise.then(function (result) {
							result.forEach(function (suggestion) {
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
				$scope.rangeUpdated = function (id, rangeDetails) {
					$scope.rangeDisabled = true;
					var planningPeriodChangeRangeModel = {
						Number: rangeDetails.Number,
						PeriodType: rangeDetails.PeriodType,
						DateFrom: rangeDetails.StartDate
					};
					planningPeriodService.changeRange({ id: id }, JSON.stringify(planningPeriodChangeRangeModel))
						.$promise.then(function (result) {
							$scope.rangeDisabled = false;
							$scope.planningPeriod = result;
						});
				};
				$scope.editRuleset = function (filter) {
					$state.go('resourceplanner.filter', {
						filterId: filter.Id,
						periodId: $stateParams.id,
						isDefault: filter.Default,
						groupId: $scope.planningPeriod.AgentGroupId
					});
				};
				$scope.createRuleset = function () {
					$state.go('resourceplanner.filter', {
						periodId: $stateParams.id,
						groupId: $scope.planningPeriod.AgentGroupId
					});
				};
				$scope.destoryRuleset = function (node) {
					dayOffRuleService.removeDayOffRule({ id: node.Id })
						.$promise.then(getDayOffRules);
				};

				$scope.$on('$destroy', function () {
					$interval.cancel(keepAliveRef);
					if (checkProgressRef)
						$interval.cancel(checkProgressRef);
				});
			}
		]);
})();

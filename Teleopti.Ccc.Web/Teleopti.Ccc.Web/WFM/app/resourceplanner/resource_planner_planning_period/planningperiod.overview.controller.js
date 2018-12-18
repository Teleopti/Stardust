(function() {
	'use strict';

	angular.module('wfm.resourceplanner').controller('planningPeriodOverviewController', Controller);

	Controller.$inject = [
		'$stateParams',
		'$state',
		'planningPeriodServiceNew',
		'NoticeService',
		'$translate',
		'$interval',
		'$scope',
		'$timeout',
		'selectedPp',
		'planningGroupInfo',
		'localeLanguageSortingService'
	];

	function Controller(
		$stateParams,
		$state,
		planningPeriodServiceNew,
		NoticeService,
		$translate,
		$interval,
		$scope,
		$timeout,
		selectedPp,
		planningGroupInfo,
		localeLanguageSortingService
	) {
		var vm = this;
		var selectedPpId = $stateParams.ppId ? $stateParams.ppId : null;
		var checkProgressRef;
		var lastJobId = '';
		vm.planningGroup = planningGroupInfo ? planningGroupInfo : null;
		vm.selectedPp = selectedPp ? selectedPp : {};
		vm.totalAgents = selectedPp ? selectedPp.TotalAgents : 0;
		vm.scheduledAgents = 0;
		vm.validationRunning = false;
		vm.publishRunning = false;
		vm.schedulingPerformed = false;
		vm.optimizeRunning = false;
		vm.dayNodes = undefined;
		vm.clearRunning = false;
		vm.isScheduled = false;
		vm.status = '';
		vm.gridOptions = {};
		vm.valData = {
			totalValNum: 0,
			totalPreValNum: 0,
			totalLastValNum: 0,
			scheduleIssues: [],
			preValidation: []
		};
		vm.launchSchedule = launchSchedule;
		vm.intraOptimize = intraOptimize;
		vm.publishSchedule = publishSchedule;
		vm.clearSchedules = clearSchedules;
		vm.isDisable = isDisable;
		vm.openModal = openModal;
		vm.valNumber = getTotalValidationErrorsNumber;
		vm.goDoRulesSetting = goDoRulesSetting;
		vm.textForClearPp = $translate
			.instant('AreYouSureYouWantToClearPlanningPeriodData')
			.replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
			.replace('{1}', moment(vm.selectedPp.EndDate).format('L'));

		$scope.$on('$destroy', function() {
			$interval.cancel(checkProgressRef);
		});

		checkState();
		loadLastResult();

		function checkState(pp) {
			checkProgress();
			checkProgressRef = $interval(function() {
				checkProgress();
			}, 10000);
		}

		function isDisable() {
			if (vm.schedulingPerformed || vm.optimizeRunning || vm.clearRunning || vm.publishRunning) return true;
		}

		function openModal() {
			if (isDisable() || vm.dayNodes == null) {
				return;
			}
			return (vm.confirmModal = true);
		}

		function clearSchedules() {
			if (selectedPpId == null || vm.clearRunning) return;
			vm.clearRunning = true;
			vm.status = $translate.instant('ClearScheduleResultAndHistoryData');
			return planningPeriodServiceNew.clearSchedules({ id: selectedPpId }).$promise.then(function() {
				checkProgress();
			});
		}

		function launchSchedule() {
			if (isDisable() || selectedPpId == null) {
				return;
			}
			vm.schedulingPerformed = true;
			return planningPeriodServiceNew.launchScheduling({ id: selectedPpId }).$promise.then(function() {
				checkProgress();
			});
		}

		function checkProgress() {
			if (selectedPpId !== null) {
				planningPeriodServiceNew.lastJobStatus({ id: selectedPpId }).$promise.then(function(result) {
					var schedulingStatus = result.SchedulingStatus;
					if (!schedulingStatus || !schedulingStatus.HasJob) {
						vm.schedulingPerformed = false;
					} else {
						if (!schedulingStatus.Successful && !schedulingStatus.Failed) {
							vm.schedulingPerformed = true;
							vm.status = $translate.instant('PresentTenseSchedule');
							return;
						}
						if (schedulingStatus.Failed) {
							vm.schedulingPerformed = false;
							if (schedulingStatus.LastJobId !== lastJobId) {
								lastJobId = schedulingStatus.LastJobId;
								handleScheduleOrOptimizeError(
									$translate
										.instant('FailedToScheduleForSelectedPlanningPeriodDueToTechnicalError')
										.replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
										.replace('{1}', moment(vm.selectedPp.EndDate).format('L'))
								);
								return;
							}
						}
						if (schedulingStatus.Successful && vm.schedulingPerformed) {
							vm.schedulingPerformed = false;
							NoticeService.success(
								$translate
									.instant('SuccessfullyScheduledPlanningPeriodFromTo')
									.replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
									.replace('{1}', moment(vm.selectedPp.EndDate).format('L')),
								null,
								true
							);
							return loadLastResult();
						}
					}

					var intradayOptimizationStatus = result.IntradayOptimizationStatus;
					if (!intradayOptimizationStatus || !intradayOptimizationStatus.HasJob) {
						vm.optimizeRunning = false;
					} else {
						if (!intradayOptimizationStatus.Successful && !intradayOptimizationStatus.Failed) {
							vm.optimizeRunning = true;
							vm.status = $translate.instant('RunningIntradayOptimization');
							return;
						}
						if (intradayOptimizationStatus.Successful && vm.optimizeRunning) {
							vm.optimizeRunning = false;
							vm.status = '';
							NoticeService.success(
								$translate
									.instant('SuccessfullyIntradayOptimizationPlanningPeriodFromTo')
									.replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
									.replace('{1}', moment(vm.selectedPp.EndDate).format('L')),
								null,
								true
							);
							return loadLastResult();
						}
						if (intradayOptimizationStatus.Failed) {
							vm.optimizeRunning = false;
							vm.status = '';
							return handleScheduleOrOptimizeError(
								$translate
									.instant('FailedToIntradayOptimizeForSelectedPlanningPeriodDueToTechnicalError')
									.replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
									.replace('{1}', moment(vm.selectedPp.EndDate).format('L'))
							);
						}
					}

					var clearScheduleStatus = result.ClearScheduleStatus;
					if (!clearScheduleStatus || !clearScheduleStatus.HasJob) {
						vm.clearRunning = false;
					} else {
						if (!clearScheduleStatus.Successful && !clearScheduleStatus.Failed) {
							vm.clearRunning = true;
							vm.status = $translate.instant('ClearScheduleResultAndHistoryData');
							return;
						}
						if (clearScheduleStatus.Successful && vm.clearRunning) {
							vm.clearRunning = false;
							vm.isScheduled = false;
							vm.scheduledAgents = 0;
							vm.dayNodes = undefined;
							vm.status = '';
							NoticeService.success(
								$translate
									.instant('SuccessClearPlanningPeriodData')
									.replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
									.replace('{1}', moment(vm.selectedPp.EndDate).format('L')),
								20000,
								true
							);
							return;
						}
						if (clearScheduleStatus.Failed) {
							vm.clearRunning = false;
							vm.status = '';
							return handleScheduleOrOptimizeError(
								$translate
									.instant('FailedToClearScheduleForSelectedPlanningPeriodDueToTechnicalError')
									.replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
									.replace('{1}', moment(vm.selectedPp.EndDate).format('L'))
							);
						}
					}
				});
			}
		}

		function handleScheduleOrOptimizeError(message) {
			if (!message) message = $translate.instant('AnErrorOccurredPleaseTryAgain');
			NoticeService.warning(message, null, true);
		}

		function intraOptimize() {
			if (isDisable() || selectedPpId == null) {
				return;
			}
			vm.optimizeRunning = true;
			return planningPeriodServiceNew.launchIntraOptimize({ id: selectedPpId }).$promise.then(function() {
				checkProgress();
			});
		}

		function publishSchedule() {
			if (isDisable() || selectedPpId == null) {
				return;
			}
			if (vm.publishRunning === true) {
				NoticeService.warning(
					$translate
						.instant('PublishingScheduleSuccess')
						.replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
						.replace('{1}', moment(vm.selectedPp.EndDate).format('L')),
					null,
					true
				);
				return;
			}
			vm.publishRunning = true;
			return planningPeriodServiceNew.publishPeriod({ id: selectedPpId }).$promise.then(function() {
				NoticeService.success(
					$translate
						.instant('PublishScheduleSucessForSelectedPlanningPeriod')
						.replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
						.replace('{1}', moment(vm.selectedPp.EndDate).format('L')),
					null,
					true
				);
				vm.publishRunning = false;
			});
		}

		function loadLastResult() {
			if (selectedPpId == null) return;
			return planningPeriodServiceNew.lastJobResult({ id: selectedPpId }).$promise.then(function(data) {
				if (data.FullSchedulingResult) {
					vm.isScheduled = true;
					vm.scheduledAgents = data.FullSchedulingResult.ScheduledAgentsCount;
					vm.valData.scheduleIssues = data.FullSchedulingResult.BusinessRulesValidationResults;
					vm.valData.scheduleIssues.sort(localeLanguageSortingService.localeSort('+ResourceName'));
					getTotalValidationErrorsNumber();
					initResult(data.FullSchedulingResult);
					return data;
				} else {
					return (vm.isScheduled = false);
				}
			});
		}

		function getTotalValidationErrorsNumber() {
			vm.valData.totalValNum = 0;
			vm.valData.totalPreValNum = 0;
			vm.valData.totalLastValNum = 0;
			var pre = vm.valData.preValidation;
			var after = vm.valData.scheduleIssues;
			if (pre.length > 0) {
				angular.forEach(pre, function(item) {
					if (item.ValidationErrors !== null) vm.valData.totalPreValNum += item.ValidationErrors.length;
				});
			}
			if (after.length > 0) {
				angular.forEach(after, function(item) {
					if (item.ValidationErrors !== null) vm.valData.totalLastValNum += item.ValidationErrors.length;
				});
			}
			return (vm.valData.totalValNum = vm.valData.totalPreValNum + vm.valData.totalLastValNum);
		}

		function initResult(interResult) {
			if (!interResult) return;
			vm.dayNodes = interResult.SkillResultList ? interResult.SkillResultList : undefined;
			vm.dayNodes.sort(localeLanguageSortingService.localeSort('+SkillName'));
			parseRelativeDifference(vm.dayNodes);
			parseWeekends(vm.dayNodes);
			displayGrid();
		}

		function parseRelativeDifference(nodes) {
			nodes.forEach(function(node) {
				node.SkillDetails.forEach(function(subnode) {
					if (isNaN(subnode.RelativeDifference)) {
						subnode.ColorId = 3;
					}
					var tempParseDif = (subnode.RelativeDifference * 100).toFixed(1);
					return (subnode.parseDif = tempParseDif);
				});
			});
		}

		function parseWeekends(nodes) {
			var culturalDaysOff = {};
			culturalDaysOff.a = 6; //saturday
			culturalDaysOff.b = 0; //sunday
			culturalDaysOff.start = 1;
			nodes.forEach(function(node) {
				node.SkillDetails.forEach(function(subnode) {
					var day = new Date(subnode.Date).getDay();
					if (day === culturalDaysOff.a || day === culturalDaysOff.b) {
						return (subnode.weekend = true);
					}
					if (day === culturalDaysOff.start) {
						return (subnode.weekstart = true);
					}
				});
			});
		}

		function displayGrid() {
			vm.gridOptions = {
				columnDefs: [
					{
						name: 'Agent',
						field: 'Name',
						enableColumnMenu: false
					},
					{
						name: 'Detail',
						field: 'Message',
						enableColumnMenu: false
					},
					{
						name: 'Issue-type',
						field: 'BusinessRuleCategoryText',
						enableColumnMenu: false
					}
				]
			};
		}

		function goDoRulesSetting() {
			$state.go('resourceplanner.editplanninggroup', {
				groupId: $stateParams.groupId,
				planningPeriodId: $stateParams.ppId
			});
		}
	}
})();

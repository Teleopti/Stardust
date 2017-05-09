(function () {
  'use strict';

  angular
    .module('wfm.resourceplanner')
    .controller('planningPeriodOverviewController', Controller);

  Controller.$inject = ['$stateParams', 'planningPeriodServiceNew', 'Toggle', 'NoticeService', '$translate', '$interval', '$scope'];

  function Controller($stateParams, planningPeriodServiceNew, toggleService, NoticeService, $translate, $interval, $scope) {
    var vm = this;
    var agentGroupId = $stateParams.groupId ? $stateParams.groupId : null;
    var selectedPpId = $stateParams.ppId ? $stateParams.ppId : null;
    var toggledOptimization = false;
    var toggledSchedulingOnStardust = false;
    var checkProgressRef;
    var keepAliveRef;
    var preMessage = '';
    var publishing = false;
    vm.agentGroup = {};
    vm.selectedPp = {};
    vm.schedulingPerformed = false;
    vm.optimizeRunning = false;
    vm.status = '';
    vm.dayNodes = undefined;
    vm.gridOptions = {};
    vm.totalAgents = null;
    vm.scheduledAgents = 0;
    vm.isDisableDo = true;
    vm.isClearing = false;
    vm.launchSchedule = launchSchedule;
    vm.intraOptimize = intraOptimize;
    vm.publishSchedule = publishSchedule;
    vm.clearSchedules = clearSchedules;
    vm.isDisable = isDisable;
    vm.openModal = openModal;
    vm.valData = {
      totalValNum: 0,
      totalPreValNum: 0,
      scheduleIssues: [],
      preValidation: [],
      selectedPpId: selectedPpId
    };

    checkToggle();
    destroyCheckState();
    getAgentGroupById();
    getPlanningPeriodByPpId();

    function checkToggle() {
      toggleService.togglesLoaded.then(function () {
        toggledOptimization = toggleService.Scheduler_IntradayOptimization_36617;
        toggledSchedulingOnStardust = toggleService.Wfm_ResourcePlanner_SchedulingOnStardust_42874;
      });
    }

    function getAgentGroupById() {
      if (agentGroupId !== null) {
        var getAgentGroup = planningPeriodServiceNew.getAgentGroupById({ agentGroupId: agentGroupId });
        return getAgentGroup.$promise.then(function (data) {
          vm.agentGroup = data;
          return vm.agentGroup;
        });
      }
    }

    function getPlanningPeriodByPpId() {
      if (selectedPpId !== null) {
        var planningPeriod = planningPeriodServiceNew.getPlanningPeriod({ id: selectedPpId });
        return planningPeriod.$promise.then(function (data) {
          vm.selectedPp = data;
          vm.valData.preValidation = data.ValidationResult.InvalidResources;
          init();
          return vm.selectedPp;
        });
      }
    }

    function init() {
      checkState();
      getTotalAgents();
      loadLastResult();
    }

    $scope.$on('$destroy', function () {
      destroyCheckState();
    });

    function destroyCheckState() {
      $interval.cancel(checkProgressRef);
      $interval.cancel(keepAliveRef);
    }

    function checkState(pp) {
      checkProgress();
      checkProgressRef = $interval(function () {
        checkProgress();
      }, 10000);

      checkIntradayOptimizationProgress();
      keepAliveRef = $interval(function () {
        checkIntradayOptimizationProgress();
      }, 10000);
    }

    function isDisable() {
      if (vm.schedulingPerformed || vm.optimizeRunning || vm.totalAgents == 0 || vm.isClearing) {
        return true;
      }
    }

    function openModal() {
      vm.textForClearPp = $translate.instant("AreYouSureYouWantToClearPlanningPeriodData")
        .replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
        .replace('{1}', moment(vm.selectedPp.EndDate).format('L'));
      return vm.confirmModal = true;
    }

    function clearSchedules() {
      if (selectedPpId !== null) {
        vm.isClearing = true;
        planningPeriodServiceNew.clearSchedules({ id: selectedPpId }).$promise.then(function () {
          vm.isClearing = false;
          init();
          NoticeService.success($translate.instant('SuccessClearPlanningPeriodData')
            .replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
            .replace('{1}', moment(vm.selectedPp.EndDate).format('L')), 20000, true);
        });
      }
    }

    function launchSchedule() {
      if (selectedPpId !== null) {
        planningPeriodServiceNew.launchScheduling({ id: selectedPpId, runAsynchronously: true }).$promise.then(function () {
          checkProgress();
        });
      }
    }

    function checkProgress() {
      if (selectedPpId !== null) {
        planningPeriodServiceNew.lastJobStatus({ id: selectedPpId })
          .$promise.then(function (result) {
            if (!result.HasJob) {
              vm.schedulingPerformed = false;
            } else {
              if (result.Successful === true) {
                if (vm.schedulingPerformed === true) {
                  vm.schedulingPerformed = false;
                  NoticeService.success($translate.instant('SuccessfullyScheduledPlanningPeriodFromTo')
                    .replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
                    .replace('{1}', moment(vm.selectedPp.EndDate).format('L')), null, true);
                  loadLastResult();
                }
              } else if (result.Failed === true) {
                vm.schedulingPerformed = false;
                vm.lastJobFail = true;
                if (result.CurrentStep === 0) {
                  handleScheduleOrOptimizeError($translate.instant('FailedToScheduleForSelectedPlanningPeriodDueToTechnicalError'));
                } else if (result.CurrentStep === 1) {
                  handleScheduleOrOptimizeError($translate.instant('FailedToScheduleForSelectedPlanningPeriodDueToTechnicalError'));
                } else if (result.CurrentStep === 2) {
                  handleScheduleOrOptimizeError($translate.instant('FailedToOptimizeDayoffForSelectedPlanningPeriodDueToTechnicalError'));
                }
              } else {
                vm.schedulingPerformed = true;
                if (result.CurrentStep === 0) {
                  vm.status = $translate.instant('PresentTenseSchedule');
                } else if (result.CurrentStep === 1) {
                  vm.status = $translate.instant('OptimizingDaysOff');
                }
              }
            }
          });
      }
    }

    function handleScheduleOrOptimizeError(message) {
      if (!message)
        message = $translate.instant('AnErrorOccurredPleaseTryAgain');
      if (message === preMessage) {
        return
      } else {
        NoticeService.warning(message, null, true);
        preMessage = message;
      }
    }

    function intraOptimize() {
      if (selectedPpId !== null) {
        vm.optimizeRunning = true;
        planningPeriodServiceNew.launchIntraOptimize({ id: selectedPpId, runAsynchronously: true }).$promise.then(function () {
          checkIntradayOptimizationProgress();
        });
      }
    }

    function checkIntradayOptimizationProgress() {
      if (selectedPpId !== null) {
        planningPeriodServiceNew.lastIntradayOptimizationJobStatus({ id: selectedPpId }).$promise.then(function (result) {
          if (!result.HasJob) {
            vm.optimizeRunning = false;
          } else {
            if (result.Successful) {
              if (vm.optimizeRunning) {
                loadLastResult();
                vm.optimizeRunning = false;
              }
            } else if (result.Failed) {
              vm.optimizeRunning = false;
              vm.status = '';
            } else {
              vm.optimizeRunning = true;
              vm.status = $translate.instant('RunningIntradayOptimization');
            }
          }
        });
      }
    }

    function publishSchedule() {
      if (publishing === true) {
        NoticeService.warning($translate.instant('PublishingSchedule'), null, true);
        return;
      }
      if (selectedPpId !== null) {
        publishing = true;
        planningPeriodServiceNew.publishPeriod({ id: selectedPpId }).$promise.then(function () {
          NoticeService.success($translate.instant('PublishScheduleSuccess'), null, true);
          publishing = false;
        });
      }
    };

    function getTotalAgents() {
      if (vm.selectedPp !== null) {
        planningPeriodServiceNew.getNumberOfAgents({ id: vm.selectedPp.Id, startDate: vm.selectedPp.StartDate, endDate: vm.selectedPp.EndDate })
          .$promise.then(function (data) {
            vm.totalAgents = data.TotalAgents ? data.TotalAgents : 0;
          });
      }
    }

    function loadLastResult() {
      if (selectedPpId !== null) {
        vm.dayNodes = undefined;
        vm.valData.scheduleIssues = [];
        vm.scheduledAgents = 0;
        planningPeriodServiceNew.lastJobResult({ id: selectedPpId })
          .$promise.then(function (data) {
            if (data.OptimizationResult) {
              initResult(data.OptimizationResult, data.ScheduleResult);
              vm.valData.scheduleIssues = data.ScheduleResult.BusinessRulesValidationResults ? data.ScheduleResult.BusinessRulesValidationResults : [];
            }
            getTotalValidationErrorsNumber(vm.valData.preValidation, vm.valData.scheduleIssues);
          });
      }
    }

    function getTotalValidationErrorsNumber(pre, after) {
      vm.valData.totalValNum = 0;
      vm.valData.totalPreValNum = 0;
      if (pre.length > 0) {
        angular.forEach(pre, function (item) {
          vm.valData.totalPreValNum += item.ValidationErrors.length;
        });
      }
      if (after.length > 0) {
        vm.valData.totalValNum += vm.valData.scheduleIssues.length;
      }
      return vm.valData.totalValNum += vm.valData.totalPreValNum;
    }

    function initResult(interResult, result) {
      vm.scheduledAgents = result.ScheduledAgentsCount ? result.ScheduledAgentsCount : 0;
      vm.dayNodes = interResult.SkillResultList ? interResult.SkillResultList : undefined;
      parseRelativeDifference(vm.dayNodes);
      parseWeekends(vm.dayNodes);
      displayGrid();
    }

    function parseRelativeDifference(nodes) {
      nodes.forEach(function (node) {
        node.SkillDetails.forEach(function (subnode) {
          if (isNaN(subnode.RelativeDifference)) {
            subnode.ColorId = 3;
          }
          var tempParseDif = (subnode.RelativeDifference * 100).toFixed(1);
          return subnode.parseDif = tempParseDif;
        });
      });
    }

    function parseWeekends(nodes) {
      var culturalDaysOff = {};
      culturalDaysOff.a = 6; //saturday
      culturalDaysOff.b = 0; //sunday
      culturalDaysOff.start = 1;
      nodes.forEach(function (node) {
        node.SkillDetails.forEach(function (subnode) {
          var day = new Date(subnode.Date).getDay();
          if (day === culturalDaysOff.a || day === culturalDaysOff.b) {
            return subnode.weekend = true;
          }
          if (day === culturalDaysOff.start) {
            return subnode.weekstart = true;
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
          }, {
            name: 'Detail',
            field: 'Message',
            enableColumnMenu: false
          }, {
            name: 'Issue-type',
            field: 'BusinessRuleCategoryText',
            enableColumnMenu: false
          }
        ]
      };
    }
  }
})();

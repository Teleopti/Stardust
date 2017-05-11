(function () {
  'use strict';

  angular
    .module('wfm.resourceplanner')
    .controller('planningPeriodOverviewController', Controller);

  Controller.$inject = ['$stateParams', 'planningPeriodServiceNew', 'NoticeService', '$translate', '$interval', '$scope'];

  function Controller($stateParams, planningPeriodServiceNew, NoticeService, $translate, $interval, $scope) {
    var vm = this;
    var agentGroupId = $stateParams.groupId ? $stateParams.groupId : null;
    var selectedPpId = $stateParams.ppId ? $stateParams.ppId : null;
    var checkProgressRef;
    var keepAliveRef;
    var preMessage = '';
    vm.publishRunning = false;
    vm.agentGroup = {}; //get data from dayOff directive 
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

    destroyCheckState();
    getPlanningPeriodByPpId();

    function getPlanningPeriodByPpId() {
      if (selectedPpId !== null) {
        var planningPeriod = planningPeriodServiceNew.getPlanningPeriod({ id: selectedPpId });
        return planningPeriod.$promise.then(function (data) {
          vm.selectedPp = data;
          vm.valData.preValidation = data.ValidationResult.InvalidResources ? data.ValidationResult.InvalidResources : [];
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
      if (vm.schedulingPerformed || vm.optimizeRunning || vm.totalAgents == 0 || vm.isClearing || vm.publishRunning) {
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
                  handleScheduleOrOptimizeError(
                    $translate.instant('FailedToScheduleForSelectedPlanningPeriodDueToTechnicalError')
                      .replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
                      .replace('{1}', moment(vm.selectedPp.EndDate).format('L'))
                  );
                } else if (result.CurrentStep === 1) {
                  handleScheduleOrOptimizeError(
                    $translate.instant('FailedToOptimizeDayoffForSelectedPlanningPeriodDueToTechnicalError')
                      .replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
                      .replace('{1}', moment(vm.selectedPp.EndDate).format('L'))
                  );
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
                NoticeService.success($translate.instant('SuccessfullyIntradayOptimizationPlanningPeriodFromTo')
                  .replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
                  .replace('{1}', moment(vm.selectedPp.EndDate).format('L')), null, true);
                loadLastResult();
              }
            } else if (result.Failed) {
              vm.optimizeRunning = false;
              vm.status = '';
              handleScheduleOrOptimizeError(
                $translate.instant('FailedToIntradayOptimizeForSelectedPlanningPeriodDueToTechnicalError')
                  .replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
                  .replace('{1}', moment(vm.selectedPp.EndDate).format('L'))
              );
            } else {
              vm.optimizeRunning = true;
              vm.status = $translate.instant('RunningIntradayOptimization');
            }
          }
        });
      }
    }

    function publishSchedule() {
      if (vm.publishRunning === true) {
        NoticeService.warning(
          $translate.instant('PublishingScheduleSuccess')
            .replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
            .replace('{1}', moment(vm.selectedPp.EndDate).format('L')), null, true);
        return;
      }
      if (selectedPpId !== null) {
        vm.publishRunning = true;
        planningPeriodServiceNew.publishPeriod({ id: selectedPpId }).$promise.then(function () {
          NoticeService.success($translate.instant('PublishScheduleSucessForSelectedPlanningPeriod')
            .replace('{0}', moment(vm.selectedPp.StartDate).format('L'))
            .replace('{1}', moment(vm.selectedPp.EndDate).format('L')), null, true);
          vm.publishRunning = false;
        });
      }
    }

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
        planningPeriodServiceNew.lastJobResult({ id: selectedPpId })
          .$promise.then(function (data) {
            if (data.ScheduleResult) {
              vm.scheduledAgents = data.ScheduleResult.ScheduledAgentsCount;
              vm.valData.scheduleIssues = data.ScheduleResult.BusinessRulesValidationResults;
              initResult(data.OptimizationResult);
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

    function initResult(interResult) {
      if (interResult) {
        vm.dayNodes = interResult.SkillResultList ? interResult.SkillResultList : undefined;
        parseRelativeDifference(vm.dayNodes);
        parseWeekends(vm.dayNodes);
        displayGrid();
      }
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

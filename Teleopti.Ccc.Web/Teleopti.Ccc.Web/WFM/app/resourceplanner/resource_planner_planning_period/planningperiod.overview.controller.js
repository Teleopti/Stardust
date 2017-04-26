(function () {
  'use strict';

  angular
    .module('wfm.resourceplanner')
    .controller('planningPeriodOverviewController', Controller);

  Controller.$inject = ['$stateParams', '$state', 'planningPeriodService', 'Toggle', 'NoticeService', '$translate', '$interval', '$scope', 'ResourcePlannerReportSrvc'];

  function Controller($stateParams, $state, planningPeriodService, toggleService, NoticeService, $translate, $interval, $scope, ResourcePlannerReportSrvc) {
    var vm = this;
    var agentGroupId = $stateParams.groupId;
    var selectedPpId = $stateParams.ppId;
    var toggledOptimization = false;
    var toggledSchedulingOnStardust = false;
    var checkProgressRef;
    var keepAliveRef;
    var preMessage = "";
    vm.agentGroup = {};
    vm.selectedPp = $stateParams.selectedPp ? $stateParams.selectedPp : null;
    vm.schedulingPerformed = false;
    vm.optimizeRunning = false;
    vm.status = "";
    vm.dayNodes = undefined;
    vm.gridOptions = {};
    vm.totalAgents = null;
    vm.scheduledAgents = 0;
    vm.isDisableDo = true;
    vm.launchSchedule = launchSchedule;
    vm.intraOptimize = intraOptimize;
    vm.publishSchedule = publishSchedule;
    vm.isDisable = isDisable;
    vm.totalValNum = 0;
    vm.valData = {
      totalPreValNum: 0,
      scheduleIssues: [],
      preValidation: [],
      selectedPp: $stateParams.selectedPp ? $stateParams.selectedPp : null,
      getPlanningPeriodByPpId: getPlanningPeriodByPpId
    };

    checkToggle();
    destroyCheckState();
    getAgentGroupbyId();
    getPlanningPeriodByPpId(selectedPpId);

    function checkToggle() {
      toggleService.togglesLoaded.then(function () {
        toggledOptimization = toggleService.Scheduler_IntradayOptimization_36617;
        toggledSchedulingOnStardust = toggleService.Wfm_ResourcePlanner_SchedulingOnStardust_42874;
      });
    }

    function getAgentGroupbyId() {
      if (agentGroupId !== null) {
        var getAgentGroup = planningPeriodService.getAgentGroupById({ agentGroupId: agentGroupId });
        return getAgentGroup.$promise.then(function (data) {
          vm.agentGroup = data;
          return vm.agentGroup;
        });
      }
    }

    function getPlanningPeriodByPpId(id) {
      var planningPeriod = planningPeriodService.getPlanningPeriod({ id: id });
      return planningPeriod.$promise.then(function (data) {
        vm.selectedPp = data;
        vm.valData.preValidation = data.ValidationResult.InvalidResources;
        init();
        return vm.selectedPp;
      });
    }

    function init() {
      checkState(vm.selectedPp);
      getTotalAgents(vm.selectedPp);
      loadLastResult(vm.selectedPp);
    }

    $scope.$on('$destroy', function () {
      destroyCheckState();
    });

    function destroyCheckState() {
      $interval.cancel(checkProgressRef);
      $interval.cancel(keepAliveRef);
    }

    function checkState(pp) {
      checkProgress(pp);
      checkProgressRef = $interval(function () {
        checkProgress(pp);
      }, 10000);

      checkIntradayOptimizationProgress(pp);
      keepAliveRef = $interval(function () {
        checkIntradayOptimizationProgress(pp);
      }, 10000);
    }

    function isDisable() {
      if (vm.schedulingPerformed || vm.optimizeRunning || vm.totalAgents == 0) {
        return true;
      }
    }

    function launchSchedule(pp) {
      planningPeriodService.launchScheduling({ id: pp.Id, runAsynchronously: true }).$promise.then(function () {
        checkProgress(pp);
      });
    }

    function checkProgress(pp) {
      planningPeriodService.lastJobStatus({ id: pp.Id })
        .$promise.then(function (result) {
          if (!result.HasJob) {
            vm.schedulingPerformed = false;
          } else {
            if (result.Successful === true) {
              if (vm.schedulingPerformed === true) {
                vm.schedulingPerformed = false;
                NoticeService.success("Successfully scheduled planning period:" + moment(vm.selectedPp.StartDate).format('DD/MM/YYYY') + "-" + moment(vm.selectedPp.EndDate).format('DD/MM/YYYY'), null, true);
                loadLastResult(pp);
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

    function handleScheduleOrOptimizeError(message) {
      if (!message)
        message = "An error occurred. Please try again.";
      if (message === preMessage) {
        return
      } else {
        NoticeService.warning(message, null, true);
        preMessage = message;
      }
    }

    function intraOptimize(pp) {
      if (pp) {
        vm.optimizeRunning = true;
        ResourcePlannerReportSrvc.intraOptimize({ id: pp.Id, runAsynchronously: true }).$promise.then(function () {
          checkIntradayOptimizationProgress(pp);
        });
      }
    }

    function checkIntradayOptimizationProgress(pp) {
      planningPeriodService.lastIntradayOptimizationJobStatus({ id: pp.Id }).$promise.then(function (result) {
        if (!result.HasJob) {
          vm.optimizeRunning = false;
        } else {
          if (result.Successful) {
            if (vm.optimizeRunning) {
              loadLastResult(pp);
              vm.optimizeRunning = false;
            }
          } else if (result.Failed) {
            vm.optimizeRunning = false;
            vm.status = '';
          } else {
            vm.optimizeRunning = true;
            vm.status = "Running Intraday Optimization";
          }
        }
      });
    }

    function publishSchedule(pp) {
      if (vm.publishedClicked === true) {
        NoticeService.warning($translate.instant('Publishing schedule.'), null, true);
        return;
      }
      vm.publishedClicked = true;
      planningPeriodService.publishPeriod({
        id: pp.Id
      }).$promise.then(function () {
        NoticeService.success($translate.instant('Published schedule sucess.'), null, true);
      });
    };

    function getTotalAgents(pp) {
      planningPeriodService.getNumberOfAgents({ id: pp.Id, startDate: pp.StartDate, endDate: pp.EndDate })
        .$promise.then(function (data) {
          vm.totalAgents = data.TotalAgents ? data.TotalAgents : 0;
        });
    }

    function loadLastResult(pp) {
      vm.dayNodes = undefined;
      vm.valData.scheduleIssues = [];
      vm.scheduledAgents = 0;
      planningPeriodService.lastJobResult({ id: pp.Id })
        .$promise.then(function (data) {
          if (data.OptimizationResult) {
            initResult(data.OptimizationResult, data.ScheduleResult, pp);
            vm.valData.scheduleIssues = data.ScheduleResult.BusinessRulesValidationResults ? data.ScheduleResult.BusinessRulesValidationResults : [];
          }
          getTotalValidationErrorsNumber(vm.valData.preValidation, vm.valData.scheduleIssues);
        });
    }

    function getTotalValidationErrorsNumber(pre, after) {
      vm.totalValNum = 0;
      vm.valData.totalPreValNum = 0;
      if (pre.length > 0) {
        angular.forEach(pre, function (item) {
          vm.valData.totalPreValNum += item.ValidationErrors.length;
        });
      }
      if (after.length > 0) {
        vm.totalValNum += vm.valData.scheduleIssues.length;
      }
      return vm.totalValNum += vm.valData.totalPreValNum;
    }

    function initResult(interResult, result, pp) {
      if (pp != undefined) {
        vm.scheduledAgents = result.ScheduledAgentsCount ? result.ScheduledAgentsCount : 0;
        vm.dayNodes = interResult.SkillResultList ? interResult.SkillResultList : undefined;
        parseRelativeDifference(vm.dayNodes);
        parseWeekends(vm.dayNodes);
        displayGrid();
      }
    }

    function parseRelativeDifference(period) {
      ResourcePlannerReportSrvc.parseRelDif(period);
    }

    function parseWeekends(period) {
      ResourcePlannerReportSrvc.parseWeek(period);
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

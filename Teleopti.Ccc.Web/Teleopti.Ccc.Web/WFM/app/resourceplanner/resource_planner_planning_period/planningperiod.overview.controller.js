(function () {
  'use strict';

  angular
    .module('wfm.resourceplanner')
    .controller('planningPeriodOverviewController', Controller)
    .directive('planningPeriods', planingPeriodsDirective);

  Controller.$inject = ['$stateParams', '$state', 'planningPeriodService', 'Toggle', 'NoticeService', '$translate', '$interval', '$scope', 'ResourcePlannerReportSrvc'];

  function Controller($stateParams, $state, planningPeriodService, toggleService, NoticeService, $translate, $interval, $scope, ResourcePlannerReportSrvc) {
    var vm = this;
    var agentGroupId = $stateParams.groupId;
    var toggledOptimization = false;
    var toggledSchedulingOnStardust = false;
    var checkProgressRef;
    var keepAliveRef;
    vm.selectedPp = null;
    vm.show = vm.selectedPp ? false : true;
    vm.schedulingPerformed = false;
    vm.optimizeRunning = false;
    vm.status = "";
    vm.planningPeriods = [];
    vm.preValidation = [];
    vm.scheduleIssues = [];
    vm.dayNodes = [];
    vm.gridOptions = {};
    vm.totalAgents = null;
    vm.scheduledAgents = 0;
    vm.totalValidationNumbers = 0;
    vm.selectPp = selectPp;
    vm.launchSchedule = launchSchedule;
    vm.startNextPlanningPeriod = startNextPlanningPeriod;
    vm.intraOptimize = intraOptimize;
    vm.publishSchedule = publishSchedule;
    vm.isDisable = isDisable;

    checkToggle();
    destroyCheckState();
    getPlanningPeriod(agentGroupId);
    getStorePpFromlocal();
    selectPp(vm.selectedPp);

    function checkToggle() {
      toggleService.togglesLoaded.then(function () {
        toggledOptimization = toggleService.Scheduler_IntradayOptimization_36617;
        toggledSchedulingOnStardust = toggleService.Wfm_ResourcePlanner_SchedulingOnStardust_42874;
      });
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

    function destroyCheckState() {
      $interval.cancel(checkProgressRef);
      $interval.cancel(keepAliveRef);
    }

    $scope.$on('$destroy', function () {
      destroyCheckState();
    });

    function getStorePpFromlocal() {
      var pp = sessionStorage.getItem('selectedPp') ? JSON.parse(sessionStorage.getItem('selectedPp')) : null;
      if (pp && pp.AgentGroupId === agentGroupId) {
        vm.selectedPp = pp;
      }
    }

    function getPlanningPeriod(id) {
      if (id) {
        var query = planningPeriodService.getPlanningPeriodsForAgentGroup({ agentGroupId: id });
        return query.$promise.then(function (data) {
          vm.planningPeriods = data;
          return vm.planningPeriods;
        });
      }
    }

    function startNextPlanningPeriod() {
      var nextPlanningPeriod = planningPeriodService.nextPlanningPeriod({ agentGroupId: agentGroupId });
      return nextPlanningPeriod.$promise.then(function (data) {
        vm.planningPeriods.push(data);
        return vm.planningPeriods;
      });
    }

    function selectPp(pp) {
      if (pp) {
        vm.show = false;
        vm.selectedPp = {};
        vm.selectedPp = pp;
        sessionStorage.clear();
        sessionStorage.setItem('selectedPp', JSON.stringify(pp));
        destroyCheckState();
        checkState(vm.selectedPp);
        getTotalAgents(vm.selectedPp);
        loadLastResult(vm.selectedPp);
        getPrevalidationByPpId(vm.selectedPp);
        return vm.selectedPp;
      }
    }

    function getPrevalidationByPpId(pp) {
      var planningPeriod = planningPeriodService.getPlanningPeriod({ id: pp.Id });
      return planningPeriod.$promise.then(function (data) {
        vm.preValidation = data.ValidationResult.InvalidResources;
        getTotalValidationErrorsNumber(vm.preValidation, vm.scheduleIssues);
        return vm.preValidation;
      });
    }

    function getTotalValidationErrorsNumber(pre, after) {
      vm.totalValidationNumbers = 0;
      if (pre.length > 0) {
        angular.forEach(pre, function (item) {
          vm.totalValidationNumbers += item.ValidationErrors.length;
        });
      }
      if (after.length > 0) {
        vm.totalValidationNumbers += vm.scheduleIssues.length;
      }
      return vm.totalValidationNumbers;
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
            if (result.Successful) {
              if (vm.schedulingPerformed === true) {
                vm.schedulingPerformed = false;
                loadLastResult(pp);
              }
            } else if (result.Failed) {
              vm.schedulingPerformed = false;
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
      vm.schedulingPerformed = false;
      vm.status = '';
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

    function getTotalAgents (pp) {
      planningPeriodService.getNumberOfAgents({ id: pp.Id, startDate: pp.StartDate, endDate: pp.EndDate })
        .$promise.then(function (data) {
          vm.totalAgents = data.TotalAgents ? data.TotalAgents : 0;
        });
    }

    function loadLastResult(pp) {
      vm.dayNodes = [];
      vm.scheduleIssues = [];
      vm.scheduledAgents = 0;
      planningPeriodService.lastJobResult({ id: pp.Id })
        .$promise.then(function (data) {
          if (data.OptimizationResult) {
            initResult(data.OptimizationResult, data.ScheduleResult, pp);
            vm.scheduleIssues = data.ScheduleResult.BusinessRulesValidationResults ? data.ScheduleResult.BusinessRulesValidationResults : [];
          }
        });
    }

    function initResult(interResult, result, pp) {
      if (pp != undefined) {
        vm.scheduledAgents = result.ScheduledAgentsCount ? result.ScheduledAgentsCount : 0;
        vm.dayNodes = interResult.SkillResultList ? interResult.SkillResultList : [];
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

  function planingPeriodsDirective() {
    var directive = {
      restrict: 'EA',
      scope: {},
      templateUrl: 'app/resourceplanner/resource_planner_planning_period/planningperiod.overview.html',
      controller: 'planningPeriodOverviewController as vm',
      bindToController: true
    };
    return directive;
  }
})();

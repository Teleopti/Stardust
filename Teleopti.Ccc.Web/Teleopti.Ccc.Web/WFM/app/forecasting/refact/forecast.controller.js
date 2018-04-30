(function() {
  'use strict';

  angular.module('wfm.forecasting')
  .controller('ForecastRefactController', ForecastCtrl);

  ForecastCtrl.$inject = ['forecastingService', '$interval', '$state', '$stateParams',  'NoticeService', '$translate'];

  function ForecastCtrl(forecastingService, $interval, $state, $stateParams, NoticeService, $translate) {
    var vm = this;
    vm.forecastModal = false;
    vm.selectedDayCount = [];
    vm.forecastModalObj = {};
    vm.currentWorkload = {
      Days: [],
      Id: ''
    };
    vm.selectedScenario = {};
    vm.forecastPeriod = {
		  startDate: {},
		  endDate: {}
	  };
    vm.skillMaster = {
      IsPermittedToModify: false,
      IsForecastRunning: false,
      Skills: [],
      Scenarios: []
    };
    vm.noForecastedDaysFound = false;

    vm.forecastingModal = forecastingModal;
    vm.disableMoreThanOneYear = disableMoreThanOneYear;
    vm.getWorkloadForecastData = getWorkloadForecastData;
    vm.pointClick = pointClick;
    vm.loadChart = loadChart;
    vm.forecastWorkload = forecastWorkload;
    vm.exportToFile = exportToFile;

    vm.getSkills = getSkills;
    vm.isForecastRunning = isForecastRunning;
    vm.getScenarios = getScenarios;

    vm.goToModify = goToModify;
    vm.goStatistics = goStatistics;


    function init() {
      resetForecastPeriod();
      vm.isForecastRunning();
      vm.getSkills();
      vm.getScenarios();

      var message = $translate.instant('WFMReleaseNotificationWithoutOldModuleLink')
          .replace('{0}', $translate.instant('Forecasts'))
          .replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx' target='_blank' rel='noopener'>")
          .replace('{2}', '</a>');
      NoticeService.info(message, null, true);
    }
    init();

    function isForecastRunning() {
      forecastingService.status.get().$promise.then(function(data) {
        vm.skillMaster.IsForecastRunning = data.IsRunning;
        if (vm.skillMaster.IsForecastRunning) {
          var pollRunningStatus = $interval(function () {
            isForecastRunning();
          }, 60000);
        }else{
          $interval.cancel(pollRunningStatus);
        }
      });
    }

    function getSkills() {
      forecastingService.skills.query().$promise.then(function (result) {
        result.Skills.forEach(function(s){
          s.Workloads.forEach(function(w){
            var temp = {
              Workload: w,
              SkillId: s.Id,
              ChartId: "chart" + w.Id
            }
            vm.skillMaster.Skills.push(temp)
          });
        });
        vm.skillMaster.IsPermittedToModify = result.IsPermittedToModifySkill;
      });
    }

    function getScenarios() {
      forecastingService.scenarios.query().$promise.then(function (result) {
        result.forEach(function(s) {
          vm.skillMaster.Scenarios.push(s);
        });
        vm.selectedScenario = vm.skillMaster.Scenarios[0];
      });
    }

    function loadChart() {
      return;
    }

    function getWorkloadForecastData(skill) {
      vm.forecastModalObj = skill;
      vm.skillMaster.isForecastRunning = true;

      var resultStartDate = moment().utc();
      var resultEndDate = moment(resultStartDate).add(6, 'months');

      var wl = {
        ForecastStart: resultStartDate.toDate(),
        ForecastEnd: resultEndDate.toDate(),
        WorkloadId: skill.Workload.Id,
        ScenarioId: vm.selectedScenario.Id
      };

      forecastingService.result(
        wl,
        function(data, status, headers, config) {
          vm.noForecastedDaysFound = (data.Days.length < 1 ? true : false);
          vm.skillMaster.isForecastRunning = false;
          vm.currentWorkload.Days = data.Days;
          vm.currentWorkload.Id = data.WorkloadId;
          vm.loadChart('chart'+ data.WorkloadId, data.Days);
        },
        function(data, status, headers, config) {
          vm.skillMaster.isForecastRunning = false;
        },
        function(data, status, headers, config) {
          vm.skillMaster.isForecastRunning = false;
        }
      )
    }

    function forecastWorkload (blockToken) {
      if (disableMoreThanOneYear()) {
        return;
      }

      vm.skillMaster.IsForecastRunning = true;
      var temp = {
        WorkloadId: vm.forecastModalObj.Workload.Id,
        ForecastMethodId: -1
      }

      vm.forecastModal = false;
      forecastingService.forecast(angular.toJson(
        {
          ForecastStart: vm.forecastPeriod.startDate,
          ForecastEnd: vm.forecastPeriod.endDate,
          WorkloadId: temp,
          ScenarioId: vm.selectedScenario.Id,
          BlockToken: blockToken,
          IsLastWorkload: true
        }), function(data, status, headers, config) {
          vm.skillMaster.IsForecastRunning = false;
          blockToken = data.BlockToken;
          getWorkloadForecastData(vm.forecastModalObj);
        }, function(data, status, headers, config) {
          vm.skillMaster.IsForecastRunning = false;
        });
      }

      function pointClick(days) {
        vm.selectedDayCount = days;
      }

      function resetForecastPeriod() {
        vm.forecastPeriod = {
          startDate:  moment().utc().add(1, 'months').startOf('month').toDate(),
          endDate: moment().utc().add(2, 'months').startOf('month').toDate()
        };
      }

      function exportToFile() {
        vm.skillMaster.isForecastRunning = true;
        forecastingService.exportForecast(angular.toJson(
          {
            ForecastStart: vm.forecastPeriod.startDate,
            ForecastEnd: vm.forecastPeriod.endDate,
            ScenarioId: vm.selectedScenario.Id,
            SkillId: vm.forecastModalObj.SkillId,
            WorkloadId: vm.currentWorkload.Id
          }), function (data, status, headers, config) {
            var blob = new Blob([data], {
              type: headers['content-type']
            });
            var fileName = moment(vm.forecastPeriod.startDate).format('YYYY-MM-DD') +
              ' - ' +
              moment(vm.forecastPeriod.endDate).format('YYYY-MM-DD') +
              '.xlsx';
            saveAs(blob, fileName);
            vm.skillMaster.isForecastRunning = false;
            vm.exportModal = false;
          }, function(data, status, headers, config) {
            vm.skillMaster.isForecastRunning = false;
          }
        );
      };

      function disableMoreThanOneYear() {
        if (vm.forecastPeriod.endDate && vm.forecastPeriod.startDate) {
          return moment(vm.forecastPeriod.endDate).diff(moment(vm.forecastPeriod.startDate), 'years') >= 1;
        } else
          return false;
      };

      function goToModify(skillData) {
        $state.go("modify", {workloadId:skillData.Workload.Id, skill:skillData, scenario:vm.selectedScenario, days:vm.currentWorkload.Days})
      }

      function goStatistics(workload) {
        $state.go('statistics', { workloadId: workload });
      };

      function forecastingModal(forecast, exporting) {
        if (vm.skillMaster.isForecastRunning) {
          return;
        }

        resetForecastPeriod();
        vm.exportModal = exporting;
        vm.forecastModal =  forecast;

        if (!forecast && !exporting) {
          vm.exportModal = false;
          vm.forecastModal = false;
        }
      }

    }
  })();

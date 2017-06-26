(function() {
  'use strict';

  angular
  .module('wfm.forecasting')
  .controller('ForecastRefactCtrl', ForecastCtrl);

  ForecastCtrl.$inject = ['forecastingService', '$interval'];

  function ForecastCtrl(forecastingService, $interval) {
    var vm = this;
    vm.forecastModal = false;
    vm.selectedDayCount = [];
    vm.forecastModalObj = {};
    vm.currentWorkload = {
      Days: []
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

    vm.forecastingModal = forecastingModal;
    vm.getWorkloadForecastData = getWorkloadForecastData;
    vm.pointClick = pointClick;
    vm.loadChart = loadChart;
    vm.forecastWorkload = forecastWorkload;
    vm.exportToFile = exportToFile;

    vm.getSkills = getSkills;
    vm.isForecastRunning = isForecastRunning;
    vm.getScenarios = getScenarios;
    
    function init() {
      resetForecastPeriod();
      vm.isForecastRunning();
      vm.getSkills();
      vm.getScenarios();
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

    function loadChart(chartId) {
      console.log('FAILED GEN');
      return;
    }

    function getWorkloadForecastData(skill) {
      vm.forecastModalObj = skill;
      vm.skillMaster.isForecastRunning = true;
      var wl = {
        ForecastStart: vm.forecastPeriod.startDate,
        ForecastEnd: vm.forecastPeriod.endDate,
        WorkloadId: skill.Workload.Id,
        ScenarioId: vm.selectedScenario.Id
      };

      forecastingService.result(
        wl,
        function(data, status, headers, config) {
          vm.skillMaster.isForecastRunning = false;
          vm.currentWorkload.Days = data.Days;
          vm.loadChart('chart'+ data.WorkloadId, data.Days);
        },
        function(data, status, headers, config) {
          vm.skillMaster.isForecastRunning = false;
        }
      )
    }

    function forecastWorkload (blockToken) {
      vm.skillMaster.IsForecastRunning = true;
      var temp = {
        WorkloadId: vm.forecastModalObj.Workload.Id,
        ForecastMethodId: -1
      }

      vm.forecastModal = false;
      forecastingService.forecast(JSON.stringify(
        {
          ForecastStart: vm.forecastPeriod.startDate,
          ForecastEnd: vm.forecastPeriod.endDate,
          Workloads: [temp],
          ScenarioId: vm.selectedScenario.Id,
          BlockToken: blockToken,
          IsLastWorkload: true
        }), function(data, status, headers, config) {
          // console.log(data);
          vm.skillMaster.IsForecastRunning = false;
          blockToken = data.BlockToken;
          getWorkloadForecastData(vm.forecastModalObj);
        }, function(data, status, headers, config) {

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
        forecastingService.exportForecast(JSON.stringify(
          {
            ForecastStart: vm.forecastPeriod.startDate,
            ForecastEnd: vm.forecastPeriod.endDate,
            ScenarioId: vm.selectedScenario.Id,
            SkillId: vm.forecastModalObj.SkillId
          }), function(data, status, headers, config) {
            if (status !== 200) {
              console.log(data, 'Export failed');
            }
            var blob = new Blob([data], {
              type: headers['content-type']
            });
            var d = moment().format('L');
            saveAs(blob, d + ".xlsx");
            vm.skillMaster.isForecastRunning = false;
            vm.exportModal = false;
          }, function(data, status, headers, config) {
            vm.skillMaster.isForecastRunning = false;
          }
        );
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

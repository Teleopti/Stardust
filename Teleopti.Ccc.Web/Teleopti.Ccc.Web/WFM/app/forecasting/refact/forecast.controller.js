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


    vm.init = init;
    vm.forecastingModal = forecastingModal;
    vm.getWorkloadForecastData = getWorkloadForecastData;
    vm.pointClick = pointClick;
    vm.loadChart = loadChart;
    vm.forecastWorkload = forecastWorkload;

    vm.init();

    function init() {
      resetForecastPeriod();
      isForecastRunning();
      getSkills();
      getScenarios();
    }

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
      //Used as a placeholder for refresh component
    }

    function getWorkloadForecastData(workloadId) {
      vm.skillMaster.isForecastRunning = true;
      var wl = {
        ForecastStart: vm.forecastPeriod.startDate,
        ForecastEnd: vm.forecastPeriod.endDate,
        WorkloadId: workloadId,
        ScenarioId: vm.selectedScenario.Id
      };
      forecastingService.result(
        wl,
        function(data, status, headers, config) {
          if (data.Days.length === 0) {
            console.log('no data');
            vm.skillMaster.isForecastRunning = false;
            return;
          }
          else{
            console.log(data);
            vm.currentWorkload.Days = data.Days;
            vm.skillMaster.isForecastRunning = false;
          }
          forecastingModal();
        },
        function(data, status, headers, config) {
          vm.skillMaster.isForecastRunning = false;
        }
      )
    }

    function forecastWorkload (blockToken) {
      vm.skillMaster.IsForecastRunning = true;
      var temp = {
        WorkloadId: vm.forecastModalObj.Id,
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
          getWorkloadForecastData(vm.forecastModalObj.Id);
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


      function forecastingModal(workload) {
        if (vm.skillMaster.isForecastRunning) {
          return;
        }
        resetForecastPeriod();
        vm.forecastModalObj = {};
        if (workload) {
          vm.forecastModalObj = workload;
          vm.forecastModal = true;
        }
        else {
          vm.forecastModal = false;
        }
      }

    }
  })();

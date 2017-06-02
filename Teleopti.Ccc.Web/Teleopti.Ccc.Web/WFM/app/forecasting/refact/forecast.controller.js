(function() {
  'use strict';

  angular
  .module('wfm.forecasting')
  .controller('ForecastRefactCtrl', ForecastCtrl);

  ForecastCtrl.$inject = ['forecastingService', '$interval'];

  function ForecastCtrl(forecastingService, $interval) {
    var vm = this;
    vm.forecastModal = false;
    vm.forecastModalObj = {};
    vm.selectedScenario = {};
    vm.forecastPeriod = {
      startDate:  moment().utc().add(1, 'months').startOf('month').toDate(),
      endDate: moment().utc().add(2, 'months').startOf('month').toDate()
    };
    vm.skillMaster = {
      IsPermittedToModify: false,
      IsForecastRunning: false,
      Skills: [],
      Scenarios: []
    };

    vm.init = init;
    vm.getForecastResult = getForecastResult;
    vm.forecastingModal = forecastingModal;
    vm.generateForecast = generateForecast;

    vm.init();

    function init() {
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

    function generateForecast() {
      if (!vm.forecastModalObj) {
        //forecast all
        return;
      }
      else{
        vm.skillMaster.isForecastRunning = true;

        forecastingService.result(JSON.stringify(
          {
            ForecastStart: vm.forecastPeriod.startDate,
            ForecastEnd: vm.forecastPeriod.endDate,
            WorkloadId: vm.forecastModalObj.Id,
            ScenarioId: vm.selectedScenario.Id
          }), function(data) {
            // console.log('response', data);
          });

        }
        forecastingModal(null, true);
      }

      function getForecastResult(skill) {
        //open
      };

      function forecastingModal(workload, cancel) {
        vm.forecastModalObj = {};
        if (vm.skillMaster.isForecastRunning) {
          return;
        }

        if (cancel === true) {
          vm.forecastPeriod = {
            startDate:  moment().utc().add(1, 'months').startOf('month').toDate(),
            endDate: moment().utc().add(2, 'months').startOf('month').toDate()
          };
          vm.forecastModal = false;
        }

        else if (workload) {
          vm.forecastModalObj = workload;
          vm.forecastModal = true;
        }else{
          // forecast all
        }
      }

    }
  })();

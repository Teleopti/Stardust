(function () {
  'use strict';

  angular
  .module('adminApp')
  .controller('etlController', etlController, ['$http', '$timeout']);

  function etlController($http, tokenHeaderService, $timeout) {
    var vm = this;

    vm.state = 'manual';
    vm.jobs = [];
    vm.tenants = [];
    vm.selectedTenant = '';
    vm.selectedJob = null;
    vm.dataSources = null;
    vm.masterTenantConfigured = false;
    vm.selectDataSource = null;

    vm.getConfigStatus = getConfigStatus;
    vm.getJobs = getJobs;
    vm.getTenants = getTenants;
    vm.sendTenant = sendTenant;
    vm.selectedTenantChanged = selectedTenantChanged;
    vm.selectJob = selectJob;
    vm.encueueJob = encueueJob;
    vm.sendBaseConfig = sendBaseConfig;



    var today = new Date();

    vm.dataSources = [];

    //manual inputs
    vm.manualInitial = {
      StartDate: null,
      EndDate: null
    }
    vm.manualQueueStats = {
      StartDate: null,
      EndDate: null
    }
    vm.manualAgentStats = {
      StartDate: null,
      EndDate: null
    }
    vm.manualSchedule = {
      StartDate: null,
      EndDate: null
    }
    vm.manualForecast = {
      StartDate: null,
      EndDate: null
    }

    vm.applyToAll = function (param, input) {
      if (vm.selectedJob.Initial) {
        vm.manualInitial[param] = input;
      }

      if (vm.selectedJob.QueueStatistics) {
        vm.manualQueueStats[param] = input;
      }

      if (vm.selectedJob.AgentStatistics) {
        vm.manualAgentStats[param] = input;
      }

      if (vm.selectedJob.Schedule) {
        vm.manualSchedule[param] = input;
      }

      if (vm.selectedJob.Forecast) {
        vm.manualForecast[param] = input;
      }
    }

    //init
    vm.getManualData = function () {
      vm.getTenants();
      vm.getConfigStatus();
    }
    vm.getManualData();

    function selectJob(job) {
      vm.selectedJob = job;
      for (var i = 0; i < vm.selectedJob.NeededDatePeriod.length; i++) {
        vm.selectedJob[vm.selectedJob.NeededDatePeriod[i]] = true;
      }
      if (!vm.selectedJob.NeedsParameterDataSource) {
        vm.selectDataSource = null;
      } else {
        vm.selectDataSource = vm.dataSources[0].Id;
      }

      if (!vm.selectedJob.Initial) {
        setDateInput(vm.manualInitial, null);
      } else {
        setDateInput(vm.manualInitial, today);
      }
      if (!vm.selectedJob.QueueStatistics) {
        setDateInput(vm.manualQueueStats, null);
      } else {
        setDateInput(vm.manualQueueStats, today);
      }
      if (!vm.selectedJob.AgentStatistics) {
        setDateInput(vm.manualAgentStats, null);
      } else {
        setDateInput(vm.manualAgentStats, today);
      }
      if (!vm.selectedJob.Schedule) {
        setDateInput(vm.manualSchedule, null);
      } else {
        setDateInput(vm.manualSchedule, today);
      }
      if (!vm.selectedJob.Forecast) {
        setDateInput(vm.manualForecast, null);
      } else {
        setDateInput(vm.manualForecast, today);
      }
    }

    function setDateInput(data, value) {
      data.StartDate = value;
      data.EndDate = value;
    }

    function getJobs(data) {
      // $http.post("./Etl/Jobs", JSON.stringify(data), tokenHeaderService.getHeaders())
      // .success(function (data) {
      //   vm.jobs = data;
      // })
      // .error(function (data) {
      //   vm.jobs = [];
      //   console.log(data, 'failed to get jobs');
      // });
    }

    function getTenants() {
      $http.get("./AllTenants", tokenHeaderService.getHeaders())
      .success(function (data) {
        vm.tenants = data;
        vm.tenants.unshift({
          Name: '<All>'
        });
        vm.selectedTenant = vm.tenants[0].Name;
        vm.sendTenant(vm.selectedTenant);
        vm.getJobs(vm.selectedTenant);
      });
    }

    function getConfigStatus() {
      $http.get("./Etl/IsBaseConfigurationAvailable", tokenHeaderService.getHeaders())
      .success(function (data) {
        vm.masterTenant  = {
          IsBaseConfigured: data.IsBaseConfigured,
          ConnectionString: data.ConnectionString,
          TenantName: data.TenantName
        };
      });
    }

    function selectedTenantChanged() {
      vm.sendTenant(vm.selectedTenant);
      vm.getJobs(vm.selectedTenant);
      vm.selectDataSource = null;
    }

    function sendTenant(data) {
      $http.post("./Etl/TenantLogDataSources", JSON.stringify(data), tokenHeaderService.getHeaders())
      .success(function (data) {
        vm.dataSources = data;
      });
    }

    function sendBaseConfig(nonMasterConnectionString) {
      var baseObj = {
        ConnectionString: '',
        BaseConfig: {
          CultureId: vm.baseConfig.culture,
          IntervalLength: vm.baseConfig.interval,
          TimeZoneCode: vm.baseConfig.timezone
        }
      }

      if (nonMasterConnectionString) {
        baseObj.ConnectionString = nonMasterConnectionString;
      } else if(vm.masterTenant.ConnectionString){
        baseObj.ConnectionString = vm.masterTenant.ConnectionString
      }

      $http.post("./Etl/SaveConfigurationForTenant", baseObj, tokenHeaderService.getHeaders())
      .success(function (data) {
        vm.masterTenant.IsBaseConfigured = true;
      });
    }

    function encueueJob(job) {
      var data = {
        JobName: job.JobName,
        JobPeriods: [],
        LogDataSourceId: vm.selectDataSource,
        TenantName: vm.selectedTenant
      };

      for (var i = 0; i < job.NeededDatePeriod.length; i++) {
        var dates = {
          Start: null,
          End: null
        }

        if (job.NeededDatePeriod[i] === 'Initial') {
          dates.Start = vm.manualInitial.StartDate;
          dates.End = vm.manualInitial.EndDate;
        }
        if (job.NeededDatePeriod[i] === 'QueueStatistics') {
          dates.Start = vm.manualQueueStats.StartDate;
          dates.End = vm.manualQueueStats.EndDate;
        }
        if (job.NeededDatePeriod[i] === 'AgentStatistics') {
          dates.Start = vm.manualAgentStats.StartDate;
          dates.End = vm.manualAgentStats.EndDate;
        }
        if (job.NeededDatePeriod[i] === 'Schedule') {
          dates.Start = vm.manualSchedule.StartDate;
          dates.End = vm.manualSchedule.EndDate;
        }
        if (job.NeededDatePeriod[i] === 'Forecast') {
          dates.Start = vm.manualForecast.StartDate;
          dates.End = vm.manualForecast.EndDate;
        }
        data.JobPeriods.push({
          Start: dates.Start,
          End: dates.End,
          JobCategoryName: job.NeededDatePeriod[i]
        });
      };

      $http.post("./Etl/EnqueueJob", data, tokenHeaderService.getHeaders())
      .success(function() {
        job.Status = 'Job enqueued';
        $timeout(function () {
          job.Status = null;
        }, 5000);
      }).error(function () {
        job.Status = 'Failed. Check inputs and network';
        $timeout(function () {
          job.Status = null;
        }, 5000);
      });
    }

    //history inputs
    vm.historyWorkPeriod = {
      StartDate: new Date().toLocaleDateString("sv-se"),
      EndDate: new Date().toLocaleDateString("sv-se")
    }

    //schedule inputs
    vm.scheduleNameEnabled = true;

    vm.schedules = [
      {
        Name: 'My main job',
        Jname: 'Nightly',
        Enabled: true,
        Description: 'Occurs every day at 15:58. Using the log data'
      },
      {
        Name: 'My secondary job',
        Jname: 'Nightly',
        Enabled: true,
        Description: 'Occurs some days at 15:58. Who knows?'
      }
    ];
  }

})();

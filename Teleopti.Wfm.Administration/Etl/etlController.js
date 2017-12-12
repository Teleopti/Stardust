(function () {
    'use strict';

    angular
        .module('adminApp')
        .controller('etlController', etlController, ['$http']);

    function etlController($http, tokenHeaderService) {
        var vm = this;

        vm.state = 'manual';
        vm.jobs = [];
        vm.tenants = [];
        vm.selectedTenant = '';
        vm.selectedJob = null;

        vm.getJobs = getJobs;
        vm.getTennants = getTennants;
        vm.sendTennant = sendTennant;
        vm.selectJob = selectJob;
        vm.encueueJob = encueueJob;

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

        vm.applyToAll = function (input) {
            vm.manualInitial = {
                StartDate: input,
                EndDate: input
            }
            vm.manualQueueStats = {
                StartDate: input,
                EndDate: input
            }
            vm.manualAgentStats = {
                StartDate: input,
                EndDate: input
            }
            vm.manualSchedule = {
                StartDate: input,
                EndDate: input
            }
            vm.manualForecast = {
                StartDate: input,
                EndDate: input
            }
        }

        //init

        vm.getManualData = function () {

            vm.getTennants();
        }
        vm.getManualData();

        function getJobs(data) {
            $http.post("./Etl/Jobs", JSON.stringify(data), tokenHeaderService.getHeaders())
                .success(function (data) {
                    vm.jobs = data;
                })
                .error(function (data) {
                    vm.jobs = [];
                    console.log(data, 'failed to get jobs');
                });
        }

        function getTennants() {
            $http.get("./AllTenants", tokenHeaderService.getHeaders())
                .success(function (data) {
                    vm.tenants = data;
                    vm.selectedTenant = vm.tenants[0].Name;
                    //vm.sendTennant(vm.selectedTenant);
                    vm.getJobs(vm.selectedTenant);
                });
        }

        function sendTennant(data) {
            $http.post("./Etl/TenantLogDataSources", data, tokenHeaderService.getHeaders())
                .success(function (data) {
                    console.log('success ', data);
                })
                .error(function (data) {
                    console.log(data, 'fail');
                });
        }

        function encueueJob(job) {
            console.log(job);

            var data = {
                JobName: job.JobName,
                JobPeriods: [],
                LogDataSourceId: "-2",
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

	        console.log(data);

	        $http.post("./Etl/EnqueueJob", data, tokenHeaderService.getHeaders())
		        .success(function (data) {
			        console.log('success ', data);
		        })
		        .error(function (data) {
			        console.log(data, 'fail');
		        });
        }

        function selectJob(job) {
            vm.selectedJob = job;
            for (var i = 0; i < vm.selectedJob.NeededDatePeriod.length; i++) {
                vm.selectedJob[vm.selectedJob.NeededDatePeriod[i]] = true;
            }

            if (!vm.selectedJob.Initial) {
                resetDateInput(vm.manualInitial);
            }
            if (!vm.selectedJob.QueueStatistics) {
                resetDateInput(vm.manualQueueStats);
            }
            if (!vm.selectedJob.AgentStatistics) {
                resetDateInput(vm.manualAgentStats);
            }
            if (!vm.selectedJob.Schedule) {
                resetDateInput(vm.manualSchedule);
            }
            if (!vm.selectedJob.Forecast) {
                resetDateInput(vm.manualForecast);
            }

        }

        function resetDateInput(data) {
            console.log(data);
            data.StartDate = null;
            data.EndDate = null;
        }

        //history inputs
        vm.historyWorkPeriod = {
            StartDate: new Date().toLocaleDateString("en-US"),
            EndDate: new Date().toLocaleDateString("en-US")
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

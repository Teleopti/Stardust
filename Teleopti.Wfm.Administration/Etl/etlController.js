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

        vm.getJobs = getJobs;
	    vm.getTennants = getTennants;

		//init
	    vm.getManualData = function () { 
            vm.getJobs();
	        vm.getTennants();
        }
	    vm.getManualData();

        function getJobs() {
            $http.get("./Etl/Jobs", tokenHeaderService.getHeaders())
                .success(function (data) {
                    vm.jobs = data;
                });
        }

        function getTennants() {
            $http.get("./AllTenants", tokenHeaderService.getHeaders())
                .success(function (data) {
                    vm.tenants = data;
	            });
        }



        //manual inputs
        vm.manualInitial = {
            StartDate: new Date().toLocaleDateString("en-US"),
            EndDate: new Date().toLocaleDateString("en-US")
        }
        vm.manualQueueStats = {
            StartDate: new Date().toLocaleDateString("en-US"),
            EndDate: new Date().toLocaleDateString("en-US")
        }
        vm.manualAgentStats = {
            StartDate: new Date().toLocaleDateString("en-US"),
            EndDate: new Date().toLocaleDateString("en-US")
        }
        vm.manualSchedule = {
            StartDate: new Date().toLocaleDateString("en-US"),
            EndDate: new Date().toLocaleDateString("en-US")
        }
        vm.manualForecast = {
            StartDate: new Date().toLocaleDateString("en-US"),
            EndDate: new Date().toLocaleDateString("en-US")
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

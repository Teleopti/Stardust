(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('etlController', etlController, []);

	function etlController() {
		var vm = this;

        vm.state = 'manual';

		//manual inputs
        vm.manualInitial = {
            StartDate: new Date(),
            EndDate: new Date()
        }
		vm.manualQueueStats = {
			StartDate: new Date(),
			EndDate: new Date()
        }
		vm.manualAgentStats = {
			StartDate: new Date(),
			EndDate: new Date()
		}
		vm.manualSchedule = {
			StartDate: new Date(),
			EndDate: new Date()
		}
		vm.manualForecast = {
			StartDate: new Date(),
			EndDate: new Date()
        }

		//history inputs
		vm.historyWorkPeriod = {
			StartDate: new Date(),
			EndDate: new Date()
		}

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

		vm.jobs = [
			{
                Name: 'Forecast',
                Tasks: [
	                {
		                Name: 'stg_business_unit'
	                },
	                {
		                Name: 'stg_date'
	                },
	                {
		                Name: 'stg_skill'
	                },
	                {
		                Name: 'stg_workload'
	                },
	                {
		                Name: 'stg_queue_workload'
	                }
                ],
				Open: false
            },
			{
				Name: 'Permission',
				Tasks: [
					{
						Name: 'stg_business_unit'
					},
					{
						Name: 'stg_date'
					},
					{
						Name: 'stg_skill'
					},
					{
						Name: 'stg_workload'
					},
					{
						Name: 'stg_queue_workload'
					}
				],
				Open: false
			}
        ];


	}

})();

(function () {
	'use strict';
	angular.module('scheduleManipulationService', ['ngResource'])
		.service('ManageScheduleSrvc', ['$resource', function ($resource) {
			this.scenarios = $resource('../api/Global/Scenario', {}, {
				query: { method: 'GET', params: {}, isArray: true }
			});

			this.runArchiving = $resource('../api/ResourcePlanner/Archiving/Run', {}, {
				post: { method: 'POST', params: {}, isArray: false }
			});

			this.runImporting = $resource('../api/ResourcePlanner/Importing/Run', {}, {
				post: { method: 'POST', params: {}, isArray: false }
			});

			this.getStatus = $resource('../api/ResourcePlanner/JobStatus/:id', { id: "@id" },
			{
				query: { method: 'GET', params: {}, isArray: false }
			});
		}
		]);
})();

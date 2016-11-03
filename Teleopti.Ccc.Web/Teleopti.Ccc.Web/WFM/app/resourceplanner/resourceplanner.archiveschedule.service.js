(function() {
	'use strict';
	angular.module('scheduleManipulationService', ['ngResource'])
		.service('ArchiveScheduleSrvc', ['$resource', function($resource) {
				this.scenarios = $resource('../api/Global/Scenario', {}, {
					query: { method: 'GET', params: {}, isArray: true }
				});
			}
		]);
})();

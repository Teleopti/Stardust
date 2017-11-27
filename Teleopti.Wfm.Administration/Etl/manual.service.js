(function () {
	'use strict';

    angular.module('adminApp')
		.service('manualEtlService', ['$resource', function ($resource) {

			    this.jobs = $resource('./ETL/Jobs', {}, {
				    get: { method: 'GET', params: {}, isArray: false }
			    });

			}
		]);
})();

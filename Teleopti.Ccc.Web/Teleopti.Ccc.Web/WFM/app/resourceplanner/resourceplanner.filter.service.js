(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.service('ResourcePlannerFilterSrvc', [
			'$http', '$q', function ($http, $q) {
				var canceler = $q.defer();

				this.getData = function(params) {
					canceler.resolve();
					canceler = $q.defer();
					if (params) {
						return $http({
							method: 'GET',
							url: '../api/filters',
							timeout: canceler.promise,
							params: params
						});
					}
				};
			}
		]);
})();

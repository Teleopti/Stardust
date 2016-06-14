(function () {
	'use strict';

	angular.module('wfm.jalaali', [])
	  .service('JalaaliService', ['$resource', function ($resource) {
	  	this.getJalaaliDate = function (data) {
	  		return $resource('../api/Schedule/FetchData', {}, {
	  			query: {
	  				method: 'GET',
	  				isArray: false
	  			}
	  		}).query().$promise;
	  	};
	  }]);
})();
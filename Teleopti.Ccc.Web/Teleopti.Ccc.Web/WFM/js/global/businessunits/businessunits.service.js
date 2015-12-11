(function () {
	'use strict';

	angular.module('wfm.businessunits')
		.service('BusinessUnitsService', ['$sessionStorage', '$q', '$resource', '$http',BusinessUnitsService]);

	function BusinessUnitsService($sessionStorage, $q, $resource, $http) {
		var service = {
			getAllBusinessUnits: getAllBusinessUnits,
			setBusinessUnit: setBusinessUnit,
			setBusinessUnitInSessionStorage: setBusinessUnitInSessionStorage,
			getBusinessUnitFromSessionStorage: getBusinessUnitFromSessionStorage,
			setBusinessUnitInHeaders: setBusinessUnitInHeaders,
			initBusinessUnit: initBusinessUnit
	};

		var getBusinessUnits = $resource('../api/BusinessUnit', {}, {
			get: { method: 'GET', params: {}, isArray: true }
		});

		var allBusinessUnits = [];

		function initBusinessUnit() {
			var buid = getBusinessUnitFromSessionStorage();
			setBusinessUnitInHeaders(buid);
			getAllBusinessUnits().then(function(result) {
				if (!buid) {
					buid = result[0].Id;
				}
				setBusinessUnit(buid);

			});
		};

		function getAllBusinessUnits() {
			var deferred = $q.defer();
			if (!allBusinessUnits.length) {
				getBusinessUnits.get().$promise.then(function(result) {
					allBusinessUnits = result;
					deferred.resolve(result);
				});
			} else {
				deferred.resolve(allBusinessUnits);
			}
			return deferred.promise;
		};

		function setBusinessUnit(buid) {
			setBusinessUnitInSessionStorage(buid);
			setBusinessUnitInHeaders(buid);
		};

		function setBusinessUnitInSessionStorage(buid) {
			$sessionStorage.buid = buid;
		};

		function getBusinessUnitFromSessionStorage() {
			return $sessionStorage.buid;
		};

		function setBusinessUnitInHeaders(buid) {
			$http.defaults.headers.common['X-Business-Unit-Filter'] = buid;
		};

		return service;
	};

})();

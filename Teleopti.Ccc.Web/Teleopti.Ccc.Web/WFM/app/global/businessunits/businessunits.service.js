(function() {
    'use strict';

    angular
        .module('wfm.businessunits')
        .factory('BusinessUnitsService', BusinessUnitsService);

    BusinessUnitsService.$inject = ['$window', '$q', '$resource', '$http'];

    function BusinessUnitsService($window, $q, $resource, $http) {
        var service = {
					getAllBusinessUnits: getAllBusinessUnits,
					setBusinessUnit: setBusinessUnit,
					setBusinessUnitInSessionStorage: setBusinessUnitInSessionStorage,
					getBusinessUnitFromSessionStorage: getBusinessUnitFromSessionStorage,
					setBusinessUnitInHeaders: setBusinessUnitInHeaders,
					initBusinessUnit: initBusinessUnit
        };

        return service;

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
					var getBusinessUnits = $resource('../api/BusinessUnit', {}, {
						get: { method: 'GET', params: {}, isArray: true }
					});

					var allBusinessUnits = [];

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
					$window.sessionStorage.setItem('buid', buid);
				};

				function getBusinessUnitFromSessionStorage() {
					return $window.sessionStorage.buid;
				};

				function setBusinessUnitInHeaders(buid) {
					$http.defaults.headers.common['X-Business-Unit-Filter'] = buid;
				};
    }
})();

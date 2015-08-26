(function() {
	'use strict';
	angular.module('wfm.businessunits', ['ngResource'])
		.controller('BusinessUnitsCtrl', [
			'$scope', '$resource', '$location', '$http', '$filter', '$state',
			function ($scope, $resource, $location, $http, $filter, $state) {

				$scope.data = {
					selectedBu: null
				};

				$scope.changeBusinessUnit = function (selectedBu) {
					$http.defaults.headers.common['X-Business-Unit-Filter'] = selectedBu.Id;
					$location.search({ buid: selectedBu.Id });
				};
				
				var getBusinessUnits = $resource('../BusinessUnit', {}, {
					get: { method: 'GET', params: {}, isArray: true }
				});

				$scope.loadBusinessUnits = function() {
					getBusinessUnits.get({}).$promise.then(function (result) {
						$scope.data.businessUnits = result;
						var buid = $location.search().buid;
						if (buid) {
							var businessUnit = $filter('filter')(result, function (d) { return d.Id === buid; })[0];
							$scope.data.selectedBu = businessUnit;
							$http.defaults.headers.common['X-Business-Unit-Filter'] = businessUnit.Id;
						} else {
							$scope.data.selectedBu = result[0];
						}
					});
				};
			}
		]);
})();
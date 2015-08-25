(function() {
	'use strict';
	angular.module('wfm.businessunits', ['ngResource'])
		.controller('BusinessUnitsCtrl', [
			'$scope', '$resource', '$location', '$http', '$filter', '$state',
			function ($scope, $resource, $location, $http, $filter, $state) {

				$scope.data = {
					selectedBu: null
				};

				$scope.changeBusinessUnit = function (selectedBuId) {
					$http.defaults.headers.common['X-Business-Unit-Filter'] = selectedBuId;
					$state.go($state.current, { buid: selectedBuId });
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
							$scope.data.selectedBu = businessUnit.Name;
							$http.defaults.headers.common['X-Business-Unit-Filter'] = businessUnit.Id;
						} else {
							$scope.data.selectedBu = result[0].Name;
						}
					});
				};
			}
		]);
})();
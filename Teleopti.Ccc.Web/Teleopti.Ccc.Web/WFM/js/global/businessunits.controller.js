(function() {
	'use strict';
	angular.module('wfm.businessunits', ['ngResource', 'ngStorage'])
		.controller('BusinessUnitsCtrl', [
			'$scope', '$resource', '$http', '$filter', '$state', '$sessionStorage', '$window',
			function ($scope, $resource, $http, $filter, $state, $sessionStorage, $window) {

				$scope.show = false;

				$scope.data = {
					selectedBu: null
				};

				$scope.changeBusinessUnit = function (selectedBu) {
					$http.defaults.headers.common['X-Business-Unit-Filter'] = selectedBu.Id;
					$sessionStorage.buid = selectedBu.Id;
					$window.location.reload();
				};
				
				var getBusinessUnits = $resource('../BusinessUnit', {}, {
					get: { method: 'GET', params: {}, isArray: true }
				});

				$scope.loadBusinessUnits = function() {
					getBusinessUnits.get({}).$promise.then(function (result) {
						$scope.data.businessUnits = result;
						$scope.show = (result.length > 1);
						var buid = $sessionStorage.buid;
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
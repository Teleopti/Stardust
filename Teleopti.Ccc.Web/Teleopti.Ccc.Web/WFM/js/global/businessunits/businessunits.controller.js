(function() {
	'use strict';
	angular.module('wfm.businessunits')
		.controller('BusinessUnitsCtrl', [
			'$scope', '$resource', '$http', '$filter', '$state', '$sessionStorage', '$window', 'BusinessUnitsService',
			function ($scope, $resource, $http, $filter, $state, $sessionStorage, $window, BusinessUnitsService) {

				$scope.show = false;

				$scope.data = {
					selectedBu: null
				};

				$scope.changeBusinessUnit = function (selectedBu) {
					BusinessUnitsService.setBusinessUnit(selectedBu.Id);
					$window.location.reload();
				};
				
				

				$scope.loadBusinessUnits = function() {
					BusinessUnitsService.getAllBusinessUnits().then(function (result) {
						$scope.data.businessUnits = result;
						$scope.show = (result.length > 1);
						var buid = BusinessUnitsService.getBusinessUnitFromSessionStorage();
						if (buid) {
							var businessUnit = $filter('filter')(result, function (d) { return d.Id === buid; })[0];
							$scope.data.selectedBu = businessUnit;
						} else {
							$scope.data.selectedBu = result[0];
						}
					});
				};
			}
		]);
})();
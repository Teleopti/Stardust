(function () {
	'use strict';

	angular.module('wfm.permissions').controller('RoleDataController', [
		'$scope', '$filter', 'RoleDataService',
		function ($scope, $filter, RoleDataService) {
			$scope.organization = {};

			$scope.$watch(function () { return RoleDataService.organization; },
				function () {
					$scope.organization = RoleDataService.organization;
				}
			);

			$scope.toggleOrganizationSelection = function (node) {
				if (node.selected) {
					RoleDataService.deleteAvailableData($scope.selectedRole, node.Type, node.Id).then(function () {
						node.selected = false;
					});
				} else {
					RoleDataService.assignOrganizationSelection($scope.selectedRole, node.Type, node.Id).then(function () {
						node.selected = true;
					});

				}
			};

			$scope.changeOption = function (option) {
				RoleDataService.assignAuthorizationLevel($scope.selectedRole, option);
			}
		}
	]);

})();
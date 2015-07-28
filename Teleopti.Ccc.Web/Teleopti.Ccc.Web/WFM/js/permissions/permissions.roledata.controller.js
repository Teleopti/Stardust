(function () {
	'use strict';

	angular.module('wfm.permissions').controller('RoleDataController', [
		'$scope', '$filter', 'RoleDataService','Roles',
		function ($scope, $filter, RoleDataService, Roles) {
			$scope.organization = {};

			$scope.$watch(function () { return RoleDataService.organization; },
				function () {
					$scope.organization = RoleDataService.organization;
				}
			);
			$scope.toggleOrganizationSelection = function (node) {
			    if (Roles.selectedRole.BuiltIn === false) {
			        if (node.selected) {
			            node.selected = false;
			            RoleDataService.deleteAvailableData($scope.selectedRole, node.Type, node.Id);
			        } else {
			            node.selected = true;
			            RoleDataService.assignOrganizationSelection($scope.selectedRole, node.Type, node.Id).then(function () {
			            });
			        }
			    }
			};
			$scope.changeOption = function (option) {
			    RoleDataService.assignAuthorizationLevel($scope.selectedRole, option);
			}
		}
	]);

})();
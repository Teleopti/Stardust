(function () {
	'use strict';

	angular.module('wfm.permissions').controller('RoleFunctionsController', [
		'$scope', '$filter', 'RolesFunctionsService', 'Roles',
		function ($scope, $filter, RolesFunctionsService, Roles) {
			$scope.unselectedFunctionsToggle = false;
			$scope.selectedFunctionsToggle = false;
			$scope.rolesService = Roles;
			$scope.rolesFunctionsService = RolesFunctionsService;
			$scope.selectedRole = $scope.rolesService.selectedRole;
			$scope.functionsDisplayed = [];

			$scope.$watch(function () { return Roles.selectedRole; },
				function (newSelectedRole) {
					if (!newSelectedRole.Id) return;
					$scope.selectedRole = newSelectedRole;
					RolesFunctionsService.refreshFunctions(newSelectedRole.Id);
				}
			);

			$scope.$watch(function () { return RolesFunctionsService.functionsDisplayed; },
					function (rolesFunctionsData) {
						$scope.functionsDisplayed = rolesFunctionsData;
					}
			);

			$scope.toggleFunctionForRole = function (node) {
				var functionNode = node.$modelValue;

				

				if (functionNode.selected) { 
					RolesFunctionsService.unselectFunction(functionNode.FunctionId, $scope.selectedRole).then(function () {
						functionNode.selected = false;
						increaseParentNumberOfSelectedNodes(node);

					});
				} else {
					RolesFunctionsService.selectFunction(functionNode.FunctionId, $scope.selectedRole).then(function () {
						functionNode.selected = true;
						decreaseParentNumberOfSelectedNodes(node);
						if (node.$parentNodeScope !== null) {
							var parentNode = node.$parentNodeScope.$modelValue;
							parentNode.selected = true;

						} 
					});
				}
			};

			var increaseParentNumberOfSelectedNodes = function(node) {
				if (node.$parentNodeScope) node.$parentNodeScope.$modelValue.nmbSelectedChildren--;
			};

			var decreaseParentNumberOfSelectedNodes = function (node) {
				if (node.$parentNodeScope) node.$parentNodeScope.$modelValue.nmbSelectedChildren++;
			}
		}
	]);

})();
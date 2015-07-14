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
				var nodes = [];

				if (functionNode.selected) {

					RolesFunctionsService.unselectFunction(functionNode.FunctionId, $scope.selectedRole).then(function () {
						console.log(nodes);
						$scope.deselectNode = function (childNode) {
							childNode.selected = false;
						}
						$scope.push = function (nodeTarget) {
							nodes.push(nodeTarget.FunctionId);
						}
						$scope.loopAround = function (a) {
							var b = a.ChildFunctions;
							for (var i = 0; i < b.length; i++) {

								if (b[i].ChildFunctions.length === 0) {
									$scope.deselectNode(b[i]);
								} else {
									$scope.deselectNode(b[i]);
									$scope.loopAround(b[i]);
								}
							}
						}
						functionNode.selected = false;
						nodes.push(functionNode.FunctionId);

						var a = node.$nodeScope.$modelValue.ChildFunctions;
						for (var i = 0; i < a.length; i++) {
							if (a[i].ChildFunctions.length === 0) {
								$scope.deselectNode(a[i]);

							} else {
								$scope.deselectNode(a[i]);
								$scope.loopAround(a[i]);
							}
						}
						
						increaseParentNumberOfSelectedNodes(node);
					});
				} else {
					nodes.push(functionNode.FunctionId);
					functionNode.selected = true;
					decreaseParentNumberOfSelectedNodes(node);
					if (node.$nodeScope.$parentNodeScope !== null) {
						var parent = node.$nodeScope.$parentNodeScope;
						while (parent !== null) {
							if (parent.$modelValue.selected === false) {
								nodes.push(parent.$modelValue.FunctionId);
								parent.$modelValue.selected = true;
							}
							parent = parent.$parentNodeScope;
						}
					}
					RolesFunctionsService.selectFunction(nodes, $scope.selectedRole).then(function () {
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
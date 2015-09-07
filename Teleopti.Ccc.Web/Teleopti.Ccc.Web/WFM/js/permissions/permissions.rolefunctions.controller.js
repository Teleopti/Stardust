(function () {
	'use strict';

	angular.module('wfm.permissions').controller('RoleFunctionsController', [
		'$scope', '$filter', 'RolesFunctionsService', 'Roles', 'growl',
		function ($scope, $filter, RolesFunctionsService, Roles, growl) {
			$scope.unselectedFunctionsToggle = false;
			$scope.selectedFunctionsToggle = false;
			$scope.rolesService = Roles;
			$scope.rolesFunctionsService = RolesFunctionsService;
			$scope.selectedRole = $scope.rolesService.selectedRole;
			$scope.functionsDisplayed = [];
			$scope.nodeAllFunction = [];
			$scope.toggleState = false;
			$scope.allToggleElement = {is: false};
			$scope.functionNodes = [];

			var message;
			$scope.$watch(function () { return Roles.selectedRole; },
				function (newSelectedRole) {
					if (!newSelectedRole.Id) return;
					$scope.selectedRole = newSelectedRole;
					if ($scope.selectedRole.BuiltIn) {
						message = growl.warning("<i class='mdi mdi-alert'></i>Changes are disabled for predefined roles.", {
							disableCountDown: true
						});
					}
					else if (message) {
						message.destroy();
					} 

					RolesFunctionsService.refreshFunctions(newSelectedRole.Id).then(function () {
						$scope.allToggleElement.is = RolesFunctionsService.allFunctions;
					});
				}
			);

			$scope.$watch(function () { return RolesFunctionsService.functionsDisplayed; },
					function (rolesFunctionsData) { 
						$scope.functionsDisplayed = rolesFunctionsData;
					}
			);
  
			var traverseNodes = function (node) {
				for (var i = 0; i < node.length; i++) {
					if (node[i].ChildFunctions.length === 0)
						node[i].selected = false; 
					else {
						node[i].selected = false;
						traverseNodes(node[i].ChildFunctions);
					}
				}
			}

			var toggleParentNode = function (node) {
				$scope.functionNodes.push(node.$modelValue.FunctionId);
				if (node.$nodeScope.$parentNodeScope !== null) {
					var parent = node.$nodeScope.$parentNodeScope;
					while (parent !== null) {
						if (!parent.$modelValue.selected) {
							parent.$modelValue.selected = true;
							$scope.functionNodes.push(parent.$modelValue.FunctionId);
						}
						parent = parent.$parentNodeScope;
					}
				}
			}
	
			$scope.toggleFunctionForRole = function (node) {
				if ($scope.selectedRole.BuiltIn) return;
				var functionNode = node.$modelValue;
					
				if (functionNode.selected) {
					RolesFunctionsService.unselectFunction(functionNode.FunctionId, $scope.selectedRole).then(function () {
						functionNode.selected = false;
						traverseNodes(functionNode.ChildFunctions);
						increaseParentNumberOfSelectedNodes(node);

					});
				} else {
					functionNode.selected = true;
					decreaseParentNumberOfSelectedNodes(node);
					toggleParentNode(node);
					RolesFunctionsService.selectFunction($scope.functionNodes, $scope.selectedRole);
					//console.log('Function nodes: ', $scope.functionNodes);
				}
			};

			$scope.toggleAllNode = function (state) {

				if (!state.is) {
    
					RolesFunctionsService.unselectAllFunctions($scope.selectedRole);
					growl.warning("<i class='mdi mdi-alert'></i> All functions are disabled.", {
						ttl: 5000,
						disableCountDown: true
					});
					return;
				}

			
				RolesFunctionsService.selectAllFunctions($scope.selectedRole);
				$scope.allToggleElement.is = state.is;
				
				growl.info("<i class='mdi mdi-thumb-up'></i> All functions are enabled.", {
					ttl: 5000,
					disableCountDown: true
				});
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
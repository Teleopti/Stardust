﻿(function () {
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
			$scope.multiDeselectModal = false;
			$scope.tempActiveNode = {};

			var message;
			$scope.$watch(function () { return Roles.selectedRole; },
				function (newSelectedRole) {
					if (!newSelectedRole.Id) return;
					$scope.selectedRole = newSelectedRole;
					if ($scope.selectedRole.BuiltIn) {
						message = growl.warning("<i class='mdi mdi-alert' ></i><span>{{'ChangesAreDisabled'|translate}}</span>", {
							ttl: 5000,
							disableCountDown: true
						});
					}

					else if ($scope.selectedRole.IsMyRole) {
						message = growl.warning("<i class='mdi mdi-alert' ></i><span>{{'CanNotModifyMyRole'|translate}}</span>", {
							ttl: 5000,
							disableCountDown: true
						});
					}

					else if (message) {
						message.destroy();
					}

					RolesFunctionsService.refreshFunctions(newSelectedRole.Id).then(function () {
						$scope.allToggleElement.is = RolesFunctionsService.allFunctions;
						$scope.functionsDisplayed = RolesFunctionsService.functionsDisplayed;
					});
				}
			);

			$scope.$watch(function () { return RolesFunctionsService.functionsDisplayed; },
					function (rolesFunctionsData) {
						$scope.functionsDisplayed = rolesFunctionsData;
						openTopNode($scope.functionsDisplayed)
					}
			);

			var openTopNode = function(tree){
				if (tree.length > 0){
						tree[1].show = true;
						}
			}

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
					$scope.tempActiveNode = node;
					if ($scope.tempActiveNode.childNodes().length > 0) {
						$scope.multiDeselectModal = true;
					} else {
						RolesFunctionsService.unselectFunction(functionNode.FunctionId, $scope.selectedRole).then(function () {
							functionNode.selected = false;
							traverseNodes(functionNode.ChildFunctions);
							increaseParentNumberOfSelectedNodes(functionNode);

						});
					}

				} else {
					functionNode.selected = true;
					decreaseParentNumberOfSelectedNodes(node);
					toggleParentNode(node);
					RolesFunctionsService.selectFunction($scope.functionNodes, $scope.selectedRole).then(function(){
						$scope.functionNodes = [];
					});
				}
			};

			$scope.deselectFunctionNodes = function () {
				var functionNode = $scope.tempActiveNode.$modelValue;
				RolesFunctionsService.unselectFunction(functionNode.FunctionId, $scope.selectedRole).then(function () {
					functionNode.selected = false;
					traverseNodes(functionNode.ChildFunctions);
					increaseParentNumberOfSelectedNodes(functionNode);

				});

				$scope.multiDeselectModal = false;
			}


			$scope.closemultiDeselectModal = function () {
				$scope.multiDeselectModal = false;
			}

			$scope.toggleAllNode = function (state) {

				if (state.is) {
					$scope.allToggleElement.is = !state.is;
					RolesFunctionsService.unselectAllFunctions($scope.selectedRole);
					growl.warning("<i class='mdi mdi-alert'></i> All functions are disabled.", {
						ttl: 5000,
						disableCountDown: true
					});
					return;
				}
					$scope.allToggleElement.is = !state.is;
					RolesFunctionsService.selectAllFunctions($scope.selectedRole);

					growl.info("<i class='mdi mdi-thumb-up'></i> All functions are enabled.", {
						ttl: 5000,
						disableCountDown: true
					});
			};

		    $scope.isAllNode = function(node) {
		        return node.FunctionCode === 'All';
		    };

			var increaseParentNumberOfSelectedNodes = function(node) {
				if (node.$parentNodeScope) node.$parentNodeScope.$modelValue.nmbSelectedChildren--;
			};

			var decreaseParentNumberOfSelectedNodes = function (node) {
				if (node.$parentNodeScope) node.$parentNodeScope.$modelValue.nmbSelectedChildren++;
			}

			$scope.disablePermissionForMe = function (node) {
				if ( $scope.selectedRole.IsMyRole)
				{
					return true;
				}
			}

		}
	]);

})();

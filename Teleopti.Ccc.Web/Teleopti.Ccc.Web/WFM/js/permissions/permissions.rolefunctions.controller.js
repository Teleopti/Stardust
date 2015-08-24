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
			$scope.allToggleElement = false;
			$scope.functionNodes = [];
			
			$scope.$watch(function () { return Roles.selectedRole; },
				function (newSelectedRole) {
					if (!newSelectedRole.Id) return;
					$scope.selectedRole = newSelectedRole;
					RolesFunctionsService.refreshFunctions(newSelectedRole.Id).then(function() {
						if ($scope.functionsDisplayed.length > 0) {
							$scope.functionsDisplayed.forEach(function (nodeTree) {
								console.log('nodetree', nodeTree);
								if (nodeTree.FunctionCode === 'All') {
									var i = $scope.functionsDisplayed.indexOf(nodeTree);
									$scope.nodeAllFunction = $scope.functionsDisplayed.slice(i, 1);
									$scope.allToggleElement = $scope.nodeAllFunction[0].selected;
								}
							});
						}
					});
					console.log('watch2');
		
				}
			);

			$scope.$watch(function () { return RolesFunctionsService.functionsDisplayed; },
					function (rolesFunctionsData) { 
						$scope.functionsDisplayed = rolesFunctionsData;
						
						console.log('watch',rolesFunctionsData);
						if ($scope.functionsDisplayed.length > 0) {
							$scope.functionsDisplayed.forEach(function(nodeTree) {
								if (nodeTree.FunctionCode === 'All') {
									var i = $scope.functionsDisplayed.indexOf(nodeTree);
									console.log(i);
									$scope.nodeAllFunction = $scope.functionsDisplayed.splice(i, 1);
									$scope.allToggleElement = $scope.nodeAllFunction[0].selected;
								}
								
							});
						}
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
				functionNodes.push(node.$modelValue.FunctionId);
				if (node.$nodeScope.$parentNodeScope !== null) {
					var parent = node.$nodeScope.$parentNodeScope;
					while (parent !== null) {
						if (!parent.$modelValue.selected) {
							parent.$modelValue.selected = true;
							functionNodes.push(parent.$modelValue.FunctionId);
						}
						parent = parent.$parentNodeScope;
					}
				}
				
			}
	
			$scope.toggleFunctionForRole = function (node) {
				console.log(node, "hello, this is a test!");
				if ($scope.selectedRole.BuiltIn === false) {
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
						RolesFunctionsService.selectFunction(functionNodes, $scope.selectedRole);
					}


				}
	
				

			};

			$scope.toggleAllNode = function(state) {
				$scope.toggleState = state;
				if (!state) { return };
				console.log(state);	        	

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
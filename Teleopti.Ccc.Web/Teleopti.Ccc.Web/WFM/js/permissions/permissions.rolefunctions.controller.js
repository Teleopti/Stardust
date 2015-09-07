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
            var functionNodes = [];

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
					//console.log('Function nodes: ', $scope.functionNodes);
                    }


				if (!state.is) {
    
					RolesFunctionsService.unselectAllFunctions($scope.selectedRole);
					growl.warning("<i class='mdi mdi-alert'></i> All functions are disabled.", {
						ttl: 5000,
						disableCountDown: true
					});
					return;
				}


				RolesFunctionsService.selectAllFunctions($scope.selectedRole);

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
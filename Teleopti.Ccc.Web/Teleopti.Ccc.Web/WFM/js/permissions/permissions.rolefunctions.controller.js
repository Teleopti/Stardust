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
            var traverseNodes = function (node) {
                for (var i = 0; i < node.length; i++) {
                    if (node[i].ChildFunctions.length === 0)
                        $scope.deselecteNode(node[i]);
                    else {
                        $scope.deselecteNode(node[i]);
                        $scope.loopAround(node[i]);
                    }
                }
                
            }
            $scope.toggleFunctionForRole = function (node) {
                
                if ($scope.selectedRole.BuiltIn === false) {
                    var functionNode = node.$modelValue;
                    $scope.deselecteNode = function (childNode) {
                        childNode.selected = false;
                    }
                    if (functionNode.selected) {
                        RolesFunctionsService.unselectFunction(functionNode.FunctionId, $scope.selectedRole).then(function () {
                            functionNode.selected = false;
                            
                            $scope.loopAround = function (a) {
                                var childs = a.ChildFunctions;
                                traverseNodes(childs);

                            }
                            var rootNode = node.$nodeScope.$modelValue.ChildFunctions;
                            traverseNodes(rootNode);
                            increaseParentNumberOfSelectedNodes(node);
                        });
                    } else {
                        var nodes = [];
                        functionNode.selected = true;
                        nodes.push(functionNode.FunctionId);
                        decreaseParentNumberOfSelectedNodes(node);
                        if (node.$nodeScope.$parentNodeScope !== null) {
                            var parent = node.$nodeScope.$parentNodeScope;
                            while (parent !== null) {
                                if (!parent.$modelValue.selected) {
                                    nodes.push(parent.$modelValue.FunctionId);
                                    parent.$modelValue.selected = true;
                                }
                                parent = parent.$parentNodeScope;
                            }
                        }
                        RolesFunctionsService.selectFunction(nodes, $scope.selectedRole);
                    }

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
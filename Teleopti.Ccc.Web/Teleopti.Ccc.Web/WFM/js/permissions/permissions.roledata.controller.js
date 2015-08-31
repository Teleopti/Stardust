﻿(function () {
	'use strict';

	angular.module('wfm.permissions').controller('RoleDataController', [
		'$scope', '$filter', 'RoleDataService', 'Roles',
			function ($scope, $filter, RoleDataService, Roles) {
				$scope.organization = { DynamicOptions: [] };
				$scope.dynamicOptionSelected = { data: 0, isBuiltIn: false };

				
				$scope.$watch(function () { return Roles.selectedRole; },
					function (newSelectedRole) {
						if (!newSelectedRole.Id) return;
						RoleDataService.refreshpermissions(newSelectedRole.Id);
						$scope.dynamicOptionSelected.isBuiltIn = Roles.selectedRole.BuiltIn;
					}
				);

				$scope.$watch(function () { return RoleDataService.organization; },
				   function (organization) {
				       $scope.organization = organization;
					   $scope.dynamicOptionSelected.data = RoleDataService.dynamicOptionSelected;

				   }
				);

				$scope.$watch(function () { return RoleDataService.dynamicOptionSelected; },
					function (option) {
						$scope.dynamicOptionSelected.data = option;
					
					}
				);
				
				var uiTree = {};
				uiTree.deselectParent = function (node) {
				    if (!node.$parentNodeScope) return;

				    var childrenOfParent = uiTree.getChildrenOfParent(node);
					if (childrenOfParent.length > 0) {
						var bu = $filter("filter")(childrenOfParent, { Type: "BusinessUnit" });
						var site = $filter("filter")(childrenOfParent, { Type: "Site" });
						if (bu.length > 0) {
							RoleDataService.deleteAvailableData($scope.selectedRole, bu[0].Type, bu[0].Id);
						}
						else if (site.length > 0) {
							RoleDataService.deleteAvailableData($scope.selectedRole, site[0].Type, site[0].Id);
						}
					}
				};

				uiTree.getChildrenOfParent = function (node) {
				    var highestLevelNode = [];
					var selectedChildren = [];
					var parent = node.$parentNodeScope;
				    parent.$modelValue.ChildNodes.forEach(function(child) {
				        if (child.selected) {
				            selectedChildren.push(parent);
				        }
				    });
					if (selectedChildren.length === 0) {
						parent.$modelValue.selected = false;
						highestLevelNode.push({ Type: parent.$modelValue.Type, Id: parent.$modelValue.Id });
					}
					if (parent.$parentNodeScope) {
						highestLevelNode.concat(uiTree.getChildrenOfParent(parent));
					}
				    return highestLevelNode;
				};

				uiTree.toggleChildSelection = function (node, state) {
				    node.forEach(function(child) {
				        child.selected = state;
				        if (child.ChildNodes.length > 0){			            
				            uiTree.toggleChildSelection(child.ChildNodes, state);
				        }
				    });
				};

				uiTree.toggleParentSelection = function (node) {
					var dataNodes = [];
					dataNodes.push({ type: node.$modelValue.Type, id: node.$modelValue.Id });
					if (node.$parentNodeScope) {
						var parent = node.$parentNodeScope;
						while (parent) {
							if (!parent.$modelValue.selected) {
								parent.$modelValue.selected = true;
								dataNodes.push({ type: parent.$modelValue.Type, id: parent.$modelValue.Id });
							}
							parent = parent.$parentNodeScope;
						}
					}
					RoleDataService.assignOrganizationSelection($scope.selectedRole, dataNodes);
				};

				$scope.toggleOrganizationSelection = function (node) {
					if (Roles.selectedRole.BuiltIn) return;
					var dataNode = node.$modelValue;
					if (dataNode.selected) {
						RoleDataService.deleteAvailableData($scope.selectedRole, dataNode.Type, dataNode.Id).
						then(function () {
							dataNode.selected = false;
							uiTree.toggleChildSelection(dataNode.ChildNodes, false);
							uiTree.deselectParent(node);
						});
					} else {
						dataNode.selected = true;
						uiTree.toggleChildSelection(dataNode.ChildNodes, true);
						uiTree.toggleParentSelection(node);
					}
				};

				$scope.changeOption = function (option) {
					RoleDataService.assignAuthorizationLevel($scope.selectedRole, option);
				};

				RoleDataService.refreshOrganizationSelection();
			}
	]);

})();
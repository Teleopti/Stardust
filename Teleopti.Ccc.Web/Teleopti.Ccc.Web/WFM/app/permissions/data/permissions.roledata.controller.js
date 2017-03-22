(function () {
	'use strict';

	angular.module('wfm.permissions').controller('RoleDataController', [
		'$scope', '$filter', 'RoleDataService', 'Roles',
			function ($scope, $filter, RoleDataService, Roles) {
				$scope.organization = { DynamicOptions: [] };
				$scope.dynamicOptionSelected = { data: 0, isBuiltIn: false, isMyRole: false };
				$scope.tempDataNode = {};
				//	$scope.multiDeselectDataModal = false;

				$scope.$watch(function () { return Roles.selectedRole; },
					function (newSelectedRole) {
						if (!newSelectedRole.Id) return;
						RoleDataService.refreshpermissions(newSelectedRole.Id);
						$scope.dynamicOptionSelected.isBuiltIn = Roles.selectedRole.BuiltIn;
					}
				);

				$scope.$watch(function () { return $scope.$parent.deselectedDataNodes; },
					function () {
						if (!$scope.$parent.deselectedDataNodes) return;
						$scope.multiDeslectConfirmed();
						$scope.$parent.deselectedDataNodes = false;
					})

				$scope.$watch(function () { return Roles.selectedRole; },
					function (newSelectedRole) {
						if (!newSelectedRole.Id) return;
						RoleDataService.refreshpermissions(newSelectedRole.Id);
						$scope.dynamicOptionSelected.isMyRole = Roles.selectedRole.IsMyRole;
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
				uiTree.countChildren = function (node) {
					var nodeCount = 0;
					var selectedCount = 0;
					var unselectedCount = 0;
					count(node);

					function count(node) {
						if (node.childNodes() && node.childNodes().length > 0) {
							node.childNodes().forEach(function (child) {
								nodeCount++;
								if (child.$modelValue.selected) {
									selectedCount++;
								} else {
									unselectedCount++;
								}
								count(child);
							});
						}
					}
					return {
						nodeCount: nodeCount,
						selectedCount: selectedCount,
						unselectedCount: unselectedCount
					}
				}

				uiTree.sameSelectionToChildren = function(node, result) {
					var children = node.childNodes();
					if (children && children.length > 0) {
						children.forEach(function(child) {
							var childData = child.$modelValue;
							childData.selected = node.$modelValue.selected;
							childData.semiSelected = false;
							result.nodes.push(child);
							uiTree.sameSelectionToChildren(child, result);
						});
					};
				}

				uiTree.selectParents = function(node, result) {
					if (node.$parentNodeScope) {
						var parent = node.$parentNodeScope;
						while (parent) {
							var children = uiTree.countChildren(parent);
							var shouldSemiSelect = (children.nodeCount !== children.selectedCount) &&
								(children.nodeCount !== children.unselectedCount);
							var shouldSelect = children.nodeCount === children.selectedCount;
							var shouldUnselect = children.nodeCount === children.unselectedCount;

							if (shouldSemiSelect) {
								if (!parent.$modelValue.semiSelected) {
									parent.$modelValue.selected = false;
									parent.$modelValue.semiSelected = true;
									if (!node.$modelValue.selected) {
										result.nodes.push(parent);
										result.toSave = false;
									}
								}
							}
							if (shouldSelect) {
								parent.$modelValue.selected = true;
								parent.$modelValue.semiSelected = false;
								result.nodes.push(parent);
							}
							if (shouldUnselect) {
								parent.$modelValue.selected = false;
								parent.$modelValue.semiSelected = false;
								result.nodes.push(parent);
							}
							parent = parent.$parentNodeScope;
						}
					}
				}

				uiTree.selectNodes = function (node, cb) {
					var nodeData = node.$modelValue;
					nodeData.selected = !nodeData.selected;
					nodeData.semiSelected = false;
					var result = {
						toSave: nodeData.selected,
						nodes: [node]
					}

					uiTree.sameSelectionToChildren(node, result);
					uiTree.selectParents(node, result);
					cb(result);
				};

				function handleSelectionResult(result) {
					if (result.nodes.length > 0) {
						var formatedNodes = [];
						result.nodes.forEach(function (node) {
							formatedNodes.push({ id: node.$modelValue.Id, type: node.$modelValue.Type });
						});

						if (result.toSave) {
							RoleDataService.assignOrganizationSelection($scope.selectedRole, formatedNodes);
						} else {
							RoleDataService.deleteAllNodes($scope.selectedRole, formatedNodes);
						}
					}
				}

				$scope.toggleOrganizationSelection = function (node) {
					if (Roles.selectedRole.BuiltIn) return;
					uiTree.selectNodes(node, handleSelectionResult);
				}

				$scope.changeOption = function (option) {
					RoleDataService.assignAuthorizationLevel($scope.selectedRole, option);
				};

				$scope.launchDeselectConfirmModal = function (node) {
					$scope.tempDataNode = node;
					if (node.$modelValue.ChildNodes.length > 0 && node.$modelValue.selected) {
						$scope.$parent.multiDeselectDataModal = true;
					}
					else {
						$scope.toggleOrganizationSelection($scope.tempDataNode);
					}
				}

				$scope.closeDeselectConfirmModal = function () {
					$scope.$parent.multiDeselectDataModal = false;
				}

				$scope.multiDeslectConfirmed = function () {
					$scope.toggleOrganizationSelection($scope.tempDataNode);
					$scope.closeDeselectConfirmModal();
				}

				RoleDataService.refreshOrganizationSelection();
			}
	]);

})();

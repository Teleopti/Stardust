(function () {
	'use strict';

	angular.module('wfm.resourceplanner').controller('HierarchyController', [
		'$scope', '$filter', 'HierarchyService',
			function ($scope, $filter, HierarchyService) {
				$scope.organization = {  };

				var formatNode = function (dataNode) {
					return { id: dataNode.Id, type: dataNode.Type, name: dataNode.Name, selected: dataNode.selected };
				}

				$scope.$watch(function() { return $scope.$parent.deselectedDataNodes; },
					function() {
						if (!$scope.$parent.deselectedDataNodes) return;
						//$scope.multiDeslectConfirmed();
						$scope.$parent.deselectedDataNodes = false;
					}
				);

				$scope.$watch(function () { return HierarchyService.organization; },
				   function (organization) {
				   	$scope.organization = organization;
				   }
				);

				var uiTree = {};
				uiTree.deselectParent = function (node) {
					if (!node.$parentNodeScope) return;
					var childrenOfParent = uiTree.getChildrenOfParent(node);
					if (childrenOfParent.length > 0) {
						var bu = $filter("filter")(childrenOfParent, { Type: "BusinessUnit" });
						var site = $filter("filter")(childrenOfParent, { Type: "Site" });
						//if (bu.length > 0) {
						//	HierarchyService.deleteAvailableData($scope.selectedRole, bu[0].Type, bu[0].Id);
						//}
						//else if (site.length > 0) {
						//	HierarchyService.deleteAvailableData($scope.selectedRole, site[0].Type, site[0].Id);
						//}
					}
				};

				uiTree.getChildrenOfParent = function (node) {
					var highestLevelNode = [];
					var selectedChildren = [];
					var parent = node.$parentNodeScope;
					parent.$modelValue.ChildNodes.forEach(function (child) {
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
					var children = [];
					node.forEach(function (child) {
						child.selected = state;
						children.push(formatNode(child));

						if (child.ChildNodes.length > 0) {
							children = children.concat(uiTree.toggleChildSelection(child.ChildNodes, state));
						}

					});
					return children;
				};

				uiTree.toggleParentSelection = function (node) {
					if (node.$parentNodeScope) {
						var parent = node.$parentNodeScope;
						while (parent) {
							if (!parent.$modelValue.selected) {
								parent.$modelValue.selected = true;
							}
							parent = parent.$parentNodeScope;
						}
					}
				};

				$scope.toggleOrganizationSelection = function (node) {
					var dataNode = node.$modelValue;
					dataNode.selected = !dataNode.selected;
					var formattedNodes = [formatNode(dataNode)];
					var children = uiTree.toggleChildSelection(dataNode.ChildNodes, dataNode.selected);
					formattedNodes = children.concat(formattedNodes);
					$scope.$emit('teamSelectionChanged', formattedNodes);
					if (dataNode.selected) {
						uiTree.toggleParentSelection(node);
					}
				};
				
				HierarchyService.refreshOrganizationSelection();
			}
	]);

})();

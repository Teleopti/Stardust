(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.filter('nameFilter', [
			function () {
				return function (nodes, name) {
					if (!name) return nodes;
					return filterNodes(nodes, name, 'nameFilter');
				};
			}
		])
		.controller('HierarchyController', [
		'$scope', 'HierarchyService',
			function ($scope, HierarchyService) {
				$scope.organization = {};

				var formatNode = function (dataNode) {
					return { id: dataNode.Id, type: dataNode.Type, name: dataNode.Name, selected: dataNode.selected };
				}

				$scope.$watch(function () { return $scope.$parent.deselectedDataNodes; },
					function () {
						if (!$scope.$parent.deselectedDataNodes) return;
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
					uiTree.getChildrenOfParent(node);
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
						highestLevelNode.push(formatNode(parent.$modelValue));
					}
					if (parent.$parentNodeScope) {
						highestLevelNode.concat(uiTree.getChildrenOfParent(parent));
					}
					return highestLevelNode;
				};

				uiTree.toggleChildSelection = function (node, state) {
					var children = [];
					node.forEach(function (child) {
						if (child.Choosable) {
							child.selected = state;
						}
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
					if (dataNode.Choosable) {
						dataNode.selected = !dataNode.selected;
					}
					var formattedNodes = [formatNode(dataNode)];
					var children = uiTree.toggleChildSelection(dataNode.ChildNodes, dataNode.selected);
					formattedNodes = children.concat(formattedNodes);
					$scope.$emit('teamSelectionChanged', formattedNodes);

					if (dataNode.selected) {
						uiTree.toggleParentSelection(node);
					}
					else if (dataNode.ChildNodes.length === 0) {
						uiTree.deselectParent(node);
					}
				};

				HierarchyService.refreshOrganizationSelection();
			}
	]);

})();

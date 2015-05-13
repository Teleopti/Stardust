(function() {
	'use strict';
	var permissionsFilters = angular.module('permissionsFilters', []);
	permissionsFilters.filter('selectedFunctions', [
		function() {
			return function(nodes, selectedFunctionToggle) {
				if (!selectedFunctionToggle) return nodes;

				var filteredNodes = [];
				console.log(nodes);
				nodes.forEach(function(node) {
					if (node.selected || node.nmbSelectedChildren > 0) {
						filteredNodes.push(node);
					}
				});
				return filteredNodes;
			}
		}
	]);

	permissionsFilters.filter('unselectedFunctions', [
		function() {
			return function(nodes, unselectedFunctionToggle) {
				if (!unselectedFunctionToggle) return nodes;
				var filteredNodes = [];
				nodes.forEach(function (node) {
					var hasUnselectedChildren = false;
					if (node.ChildFunctions && node.ChildFunctions.length > 0
						&& node.nmbSelectedChildren < node.ChildFunctions.length)
						hasUnselectedChildren = true;

					if (!node.selected || hasUnselectedChildren) {
						filteredNodes.push(node);
					}
				});
				return filteredNodes;
			}
		}
	]);

	permissionsFilters.filter('unselectedFunctions2', [
		function () {
			return function (node, unselectedFunctionToggle) {
				if (!unselectedFunctionToggle) return node;
				var filteredNode = null;
					var hasUnselectedChildren = false;
					if (node.ChildFunctions && node.ChildFunctions.length > 0
						&& node.nmbSelectedChildren < node.ChildFunctions.length)
						hasUnselectedChildren = true;

					if (!node.selected || hasUnselectedChildren) {
						filteredNode=node;
					}
				return filteredNode;
			}
		}
	]);
})();
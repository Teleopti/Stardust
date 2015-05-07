(function() {
	'use strict';
	var permissionsFilters = angular.module('permissionsFilters', []);
	permissionsFilters.filter('selectedFunctions', [
		function() {
			return function(nodes, selectedFunctionToggle) {
				if (!selectedFunctionToggle) return nodes;

				var filteredNodes = [];
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
				nodes.forEach(function(node) {
					if (!node.selected) {
						filteredNodes.push(node);
					}
				});
				return filteredNodes;
			}
		}
	]);
})();
(function() {
	'use strict';
	var permissionsFilters = angular.module('wfm.permissions');
	permissionsFilters.filter('selectedFunctions', [
		function() {
		    return function (nodes, selectedFunctionToggle) {
		        if (!selectedFunctionToggle) return nodes;

		        var filteredNodes = [];
		        nodes.forEach(function (node) {
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
		    return function (nodes, unselectedFunctionToggle) {
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

	permissionsFilters.filter('selectedData', [
    function () {
        return function (nodes, selectedDataToggle) {
            if (!selectedDataToggle) return nodes;
            var filteredNodes = [];

            var checkChild = function (node) {
                
                node.ChildNodes.forEach(function(subnode) {
                    if (subnode.ChildNodes.length > 0) {
                        checkChild(subnode);
                    }
                    if (subnode.selected) {
                        node.show = true;
                        filteredNodes.push(node);
                       
                        
                    }
                });
            }
                nodes.forEach(function(node) {
                    if (node.ChildNodes.length > 0) {
                        checkChild(node);

                    }
                    if (node.selected) {
                        node.show = true;
                        filteredNodes.push(node);
                        
                    }
                });
            return filteredNodes;
        }
    }
	]);

	permissionsFilters.filter('unselectedData', [
		function () {
		    return function (nodes, unselectedDataToggle) {
		        if (!unselectedDataToggle) return nodes;
		        var filteredNodes = [];
		        nodes.forEach(function (node) {
		            var hasUnselectedChildren = false;
		            if (node.ChildNodes && node.ChildNodes.length > 0
						&& node.nmbSelectedChildren < node.ChildNodes.length)
		                hasUnselectedChildren = true;

		            if (node.nmbSelectedChildren > 0) { node.selected }

		            if (!node.selected || hasUnselectedChildren) {
		                filteredNodes.push(node);
		            }
		        });
		        return filteredNodes;
		    }
		}
	]);
})();
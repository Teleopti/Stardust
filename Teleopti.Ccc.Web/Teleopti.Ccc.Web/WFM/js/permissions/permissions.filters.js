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

    var getChildren = function(child, filterType) {
        if (filterType === 'descriptionFilter') {
            return child.ChildFunctions;
        } else {
            return child.ChildNodes;
        }
    }

    var matchChild = function (node, reg, filterType) {
        if (filterType === 'descriptionFilter') {
            return node.LocalizedFunctionDescription.match(reg);
        } else {
            return node.Name.match(reg);
        }
    }

	var checkChild = function (child, name, filterType) {
	    var hasChildWithName = 0;
	    var reg = new RegExp(name, "i");
	    var thosechildren = getChildren(child, filterType);
	    if (thosechildren && thosechildren.length > 0) {
	        thosechildren.forEach(function (node) {
	            if (checkChild(node, name, filterType))
	                hasChildWithName++;
	        });
	        return (hasChildWithName > 0 || matchChild(child,reg, filterType));
	    } else {
	        return matchChild(child, reg, filterType);
	    }
	};

    var filterNodes= function(nodes, name, filterType) {
        var filteredNodes = [];

        nodes.forEach(function (node) {
            if (checkChild(node, name, filterType)) {
                filteredNodes.push(node);
            }
        });

        return filteredNodes;
    }

    permissionsFilters.filter('descriptionFilter', [
		function () {
		    return function (nodes, name) {
		        if (!name) return nodes;
		        return filterNodes(nodes, name, 'descriptionFilter');
		    };
		}
    ]);

    permissionsFilters.filter('nameFilter', [
		function () {
		    return function (nodes, name) {
		        if (!name) return nodes;
		        return filterNodes(nodes, name, 'nameFilter');
		    };
		}
    ]);

    permissionsFilters.filter('selectedData', [
    function () {
	    return function(nodes, selectedDataToggle) {
		    if (!selectedDataToggle) return nodes;
		    var filteredNodes = [];

		    var checkChild = function(node) {

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
	    };
    }
	]);

    permissionsFilters.filter('unselectedData', [
		function () {
			return function(nodes, unselectedDataToggle) {
				if (!unselectedDataToggle) return nodes;
				var filteredNodes = [];
				nodes.forEach(function(node) {
					var hasUnselectedChildren = false;
					if (node.ChildNodes && node.ChildNodes.length > 0
						&& node.nmbSelectedChildren < node.ChildNodes.length)
						hasUnselectedChildren = true;

					if (node.nmbSelectedChildren > 0) {
						node.selected
					}

					if (!node.selected || hasUnselectedChildren) {
						filteredNodes.push(node);
					}
				});
				return filteredNodes;
			};
		}
	]);
})();
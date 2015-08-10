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

	var checkChild = function (child, name) {
		var hasChildWithName = 0;
		var reg = new RegExp(name, "i");
		if (child.ChildNodes && child.ChildNodes.length > 0) {
			child.ChildNodes.forEach(function(node) {
				if (checkChild(node, name))
					hasChildWithName ++;
			});
			if (hasChildWithName > 0 || child.Name.match(reg)) {
				return true;
			} else {
				return false;
			}
		} else {
			if (child.Name.match(reg)) {
				return true;
			} else {
				return false;
			}
		}
	};


	permissionsFilters.filter('nameFilter', [
		function () {
			return function (nodes, name) {
				if (!name) return nodes;
				var filteredNodes = [];

				nodes.forEach(function(node) {
					var check = checkChild(node, name);

					if (check) {
						filteredNodes.push(node);
					}
				});

				return filteredNodes;
			};
		}
	]);

})();
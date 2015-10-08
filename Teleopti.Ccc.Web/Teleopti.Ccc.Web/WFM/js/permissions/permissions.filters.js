(function () {
    'use strict';
    var permissionsFilters = angular.module('wfm.permissions');

    var getChildren = function (child, filterType) {
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

    var checkName = function (child, name, filterType) {
        var hasChildWithName = 0;
        var reg = new RegExp(name, "i");
        var thoseChildren = getChildren(child, filterType);
        if (thoseChildren && thoseChildren.length > 0) {
            thoseChildren.forEach(function (node) {
                if (checkName(node, name, filterType))
                    hasChildWithName++;
            });
            return (hasChildWithName > 0 || matchChild(child, reg, filterType));
        } else {
            return matchChild(child, reg, filterType);
        }
    };

    var filterNodes = function (nodes, name, filterType) {
        var filteredNodes = [];

        nodes.forEach(function (node) {
            if (checkName(node, name, filterType)) {
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

    permissionsFilters.filter('selectedFunctions', [
		function () {
		    return function (nodes, selectedFunctionToggle) {
		        if (!selectedFunctionToggle) return nodes;
		        var filteredNodes = [];

		        nodes.forEach(function (node) {
		            if (node.selected || node.nmbSelectedChildren > 0) {
		                filteredNodes.push(node);
		            }
		        });
		        return filteredNodes;
		    };
		}
    ]);

    var checkChild = function(node) {
        var selectedChildren = 0;
        node.ChildNodes.forEach(function(subnode) {
            if (subnode.ChildNodes && subnode.ChildNodes.length > 0) {
                selectedChildren += checkChild(subnode);
            }
            if (subnode.selected) {
                node.show = true;
                selectedChildren ++;
            }
        });
        return selectedChildren;
    };

    var checkUnselectedChild = function (node, getChild) {
        var unselectedChildren = 0;
        var children = getChild(node);
        children.forEach(function (subnode) {
            var child = getChild(subnode);
            if (child && child.length > 0) {
                unselectedChildren += checkUnselectedChild(subnode, getChild);
            }
            if (!subnode.selected) {
                node.show = true;
                unselectedChildren++;
            }
        });
        return unselectedChildren;
    };

    permissionsFilters.filter('selectedData', [
        function () {
            return function (nodes, selectedDataToggle) {
                if (!selectedDataToggle) return nodes;
                var filteredNodes = [];

                nodes.forEach(function (node) {
                    var hasSelectedChildren = 0;
                    if (node.ChildNodes && node.ChildNodes.length > 0) {
                        hasSelectedChildren = checkChild(node);
                    }
                    if (node.selected || hasSelectedChildren) {
                        node.show = true;
                        filteredNodes.push(node);
                    }
                });
                return filteredNodes;
            };
        }
    ]);

    var unselect = function (nodes, toggle, getChild) {
        if (!toggle) return nodes;
        var filteredNodes = [];
        nodes.forEach(function (node) {
            var hasUnselectedChildren = 0;
            var child = getChild(node);
            if (child && child.length > 0) {
                hasUnselectedChildren = checkUnselectedChild(node, getChild);
            }
            if (!node.selected || hasUnselectedChildren) {
                filteredNodes.push(node);
            }
        });
        return filteredNodes;
    };

    permissionsFilters.filter('unselectedFunctions', [
        function () {
            return function (nodes, unselectedFunctionsToggle) {
                return unselect(nodes, unselectedFunctionsToggle, function (node) {
                    return node.ChildFunctions;
                });
            };
        }
    ]);

    permissionsFilters.filter('unselectedData', [
        function () {
            return function (nodes, unselectedDataToggle) {
                return unselect(nodes, unselectedDataToggle, function (node) {
                    return node.ChildNodes;
                });
            };
        }
    ]);
})();

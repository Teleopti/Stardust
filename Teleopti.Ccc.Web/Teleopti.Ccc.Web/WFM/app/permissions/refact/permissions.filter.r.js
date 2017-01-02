(function() {
	'use strict';

	angular
		.module('wfm.permissions')
		.filter('functionsFilter', functionsFilter)
		.filter('dataFilter', dataFilter);

	function dataFilter() {
		var filter = this;

		function deleteStuff(temp, selectedOrNot) {
			for (var i = 0; i < temp.length; i++) {
				if (temp[i].IsSelected == selectedOrNot) {
					temp.splice(i, 1);
					i--;
				} else {
					if (temp[i].ChildNodes != null && temp[i].ChildNodes.length > 0) {
						deleteStuff(temp[i].ChildNodes, selectedOrNot)
					}
				}
			}
		}

		filter.unselected = function(orgData) {
			function reduce(nodes) {
				return nodes.reduce(function(ret, node) {
					if (node.ChildNodes != null) {
						node.ChildNodes = reduce(node.ChildNodes);
					}
					if (!node.IsSelected || (node.ChildNodes != null && node.ChildNodes.length > 0)) {
						return ret.concat(node);
					}

					return ret;
				}, []);
			}

			var ret = $.extend(true, {}, orgData);
			ret.BusinessUnit.ChildNodes = reduce(ret.BusinessUnit.ChildNodes);

			return ret;

			// var selectedBu = {};
			//
			// if (orgData.IsSelected) {
			//   selectedBu = Object.assign({}, orgData);
			//   deleteStuff(selectedBu.ChildNodes, true);
			//   return selectedBu;
			// } else {
			//   return orgData;
			// }
		}

		filter.selected = function(orgData) {
			if (!orgData.BusinessUnit.IsSelected) {
				var ret = $.extend(true, {}, orgData);
				ret.BusinessUnit = {};
				return ret;
			}

			function reduce(nodes) {
				return nodes.reduce(function(ret, node) {
					if (node.ChildNodes != null) {
						node.ChildNodes = reduce(node.ChildNodes);
					}
					if (node.IsSelected) {
						return ret.concat(node);
					}

					return ret;
				}, []);
			}

			var ret = $.extend(true, {}, orgData);
			ret.BusinessUnit.ChildNodes = reduce(ret.BusinessUnit.ChildNodes);

			return ret;

			// var selectedBu = {};
			//
			// if (orgData.IsSelected) {
			//   selectedBu = Object.assign({}, orgData);
			//   deleteStuff(selectedBu.ChildNodes, false);
			//   return selectedBu;
			// } else {
			//   return selectedBu;
			// }
		}

		return filter;
	}

	function functionsFilter() {
		var filter = this;

		function isFunctionSelected(selectedFunctions, func) {
			return selectedFunctions[func.FunctionId];
		}

		filter.selected = function(appFunctions, selectedFunctions) {

			return appFunctions.reduce(function(filteredFunctions, func) {
				if (!isFunctionSelected(selectedFunctions, func)) {
					return filteredFunctions;
				}
				var f = $.extend(true, {}, func);
				f.ChildFunctions = filter.selected(f.ChildFunctions, selectedFunctions);
				return filteredFunctions.concat(f);
			}, []);
		};

		filter.unselected = function(appFunctions, selectedFunctions) {
			return appFunctions.reduce(function(filteredFunctions, func) {

				var f = $.extend(true, {}, func);
				f.ChildFunctions = filter.unselected(f.ChildFunctions, selectedFunctions);

        if (isFunctionSelected(selectedFunctions, func) && f.ChildFunctions.length === 0) {
          return filteredFunctions;
        }

				return filteredFunctions.concat(f);
			}, []);
		};

		return filter;
	}

})();

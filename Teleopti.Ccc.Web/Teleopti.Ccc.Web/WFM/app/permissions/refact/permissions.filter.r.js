(function() {
	'use strict';

	angular
		.module('wfm.permissions')
		.filter('newDescriptionFilter', descriptionFilter)
		.filter('functionsFilter', functionsFilter)
		.filter('dataFilter', dataFilter);

	function dataFilter() {
		var filter = this;

		filter.unselected = function(orgData, selectedOrgData) {
			function reduce(nodes) {
				return nodes.reduce(function(ret, node) {
					if (node.ChildNodes != null) {
						node.ChildNodes = reduce(node.ChildNodes);
					}
					if (!selectedOrgData[node.Id] || (node.ChildNodes != null && node.ChildNodes.length > 0)) {
						return ret.concat(node);
					}

					return ret;
				}, []);
			}

			var ret = $.extend(true, {}, orgData);
			ret.BusinessUnit.ChildNodes = reduce(ret.BusinessUnit.ChildNodes);

			return ret;
		}

		filter.selected = function(orgData, selectedOrgData) {
			if (Object.keys(selectedOrgData).length === 0 && selectedOrgData.constructor === Object) {
				var ret = $.extend(true, {}, orgData);
				ret.BusinessUnit = {};
				return null;
			}
			function reduce(nodes) {
				return nodes.reduce(function(ret, node) {
					if (node.ChildNodes != null) {
						node.ChildNodes = reduce(node.ChildNodes);
					}
					if (selectedOrgData[node.Id]) {
						return ret.concat(node);
					}

					return ret;
				}, []);
			}

			var ret = $.extend(true, {}, orgData);
			ret.BusinessUnit.ChildNodes = reduce(ret.BusinessUnit.ChildNodes);

			return ret;
		}

		return filter;
	}

	function descriptionFilter() {
		var filter = this;

		filter.filterOrgData = function(orgData, searhString) {
			var noCaseSensitiveSearchString = new RegExp(searhString, "i");

			if (orgData.BusinessUnit != null && orgData.BusinessUnit.Name.match(noCaseSensitiveSearchString)) {
				return orgData;
			}

			function reduce(nodes) {
				return nodes.reduce(function(ret, node) {

					if (!node.Name.match(noCaseSensitiveSearchString) && (node.ChildNodes != null && node.ChildNodes.length > 0)) {
						node.ChildNodes = reduce(node.ChildNodes);
					}
					if (node.Name.match(noCaseSensitiveSearchString) || (node.ChildNodes != null && node.ChildNodes.length > 0))	{
						return ret.concat(node);
					}

					return ret;
				}, []);
			}

			var ret = $.extend(true, {}, orgData);
			ret.BusinessUnit.ChildNodes = reduce(ret.BusinessUnit.ChildNodes);

			return ret;
		}

		filter.filterFunctions = function(appFunctions, searhString) {
			var noCaseSensitiveSearchString = new RegExp(searhString, "i");

			return appFunctions.reduce(function(filteredFunctions, func) {

				var f = $.extend(true, {}, func);

				if (!func.LocalizedFunctionDescription.match(noCaseSensitiveSearchString) && f.ChildFunctions.length > 0) {
					f.ChildFunctions = filter.filterFunctions(f.ChildFunctions, searhString);
				}

				if (!func.LocalizedFunctionDescription.match(noCaseSensitiveSearchString) && f.ChildFunctions.length === 0) {
					return filteredFunctions;
				}

				return filteredFunctions.concat(f);
			}, []);

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

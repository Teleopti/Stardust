(function() {
	'use strict';

	angular.module('wfm.requests').filter('filterShiftTradeDetailDisplay', ['$filter', filterShiftTradeDetailDisplay]);

	function filterShiftTradeDetailDisplay($filter) {
		return function(shiftTradeDays, day) {
			if (!shiftTradeDays) {
				return true;
			}

			return shiftTradeDays.filter(function(element) {
				var date = $filter('date')(moment(element.Date).toDate(), 'shortDate');
				if (date === day) {
					return true;
				}
			});
		};
	}

	angular.module('wfm.requests').service('RequestsFilter', function() {
		var svc = this;

		svc.filters = {};
		svc.removeFilter = function(filterName, tabName) {
			if (!svc.filters[tabName]) return;

			for (var i = 0; i < svc.filters[tabName].length; i++) {
				var filter = svc.filters[tabName][i];
				if (filter.hasOwnProperty(filterName)) {
					svc.filters[tabName].splice(i, 1);
				}
			}
		};

		svc.setFilter = function(name, filter, tabName) {
			var expectedFilterNames = ['Status', 'Subject', 'Message', 'Type'];
			var index;

			angular.forEach(svc.filters[tabName], function(f, i) {
				if (Object.keys(f)[0] == name) index = i;
			});

			if (expectedFilterNames.indexOf(name) > -1) {
				if (svc.filters[tabName] && index > -1) {
					if (filter && filter.length > 0) svc.filters[tabName][index][name] = filter;
					else svc.removeFilter(name, tabName);
				} else {
					if (angular.isUndefined(filter) || filter.trim().length === 0) return;

					var filterObj = {};
					filterObj[name] = filter.trim();

					if (angular.isUndefined(svc.filters[tabName])) {
						svc.filters[tabName] = [];
					}

					svc.filters[tabName].push(filterObj);
				}
			}
		};

		svc.resetFilter = function(tabName) {
			if (svc.filters[tabName]) svc.filters[tabName].length = 0;
		};

		svc.resetAllFilters = function() {
			svc.filters = {};
		};

		return svc;
	});
})();

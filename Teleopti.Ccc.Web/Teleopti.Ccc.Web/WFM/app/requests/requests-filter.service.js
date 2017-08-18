(function() {
	'use strict';

	angular.module('wfm.requests')
		.filter('filterShiftTradeDetailDisplay', ['$filter', filterShiftTradeDetailDisplay]);

	function filterShiftTradeDetailDisplay($filter) {

		return function(shiftTradeDays, day) {
			if (!shiftTradeDays) {
				return true;
			}

			return shiftTradeDays.filter(function(element) {
				var date = ($filter('date')(moment(element.Date).toDate(), 'shortDate'));
				if (date === day) {
					return true;
				}
			});
		}
	};

	angular.module('wfm.requests').service('RequestsFilter', function() {
		var svc = this;

		svc.filters = {};
		svc.removeFilter = function(filterName, tabName) {
			if(!svc.filters[tabName]) return;

			for (var i = 0; i < svc.filters[tabName].length; i++) {
				var filter = svc.filters[tabName][i];
				if (filter.hasOwnProperty(filterName)) {
					svc.filters[tabName].splice(i, 1);
				}
			}
		};

		svc.setFilter = function(name, filter, tabName) {
			var nameInLowerCase = name.trim().toLowerCase();
			var expectedFilterNames = ['status', 'subject', 'message', 'type'];

			if (expectedFilterNames.indexOf(nameInLowerCase) > -1) {
				var filterName = nameInLowerCase.charAt(0).toUpperCase() + nameInLowerCase.slice(1);
				svc.removeFilter(filterName, tabName);

				if (filter == undefined || filter.trim().length === 0) return;

				var filterObj = {};
				filterObj[filterName] = filter.trim();

				if(svc.filters[tabName] == undefined){
					svc.filters[tabName] = [];
				}

				svc.filters[tabName].push(filterObj);
			}
		};

		svc.resetFilter = function(tabName) {
			if(svc.filters[tabName])
				svc.filters[tabName].length = 0;
		};

		return svc;
	});
})();
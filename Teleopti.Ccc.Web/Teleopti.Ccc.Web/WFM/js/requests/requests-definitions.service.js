(function() {

	'use strict';

	angular.module('wfm.requests').service('requestsDefinitions', requestsDefinitionsService);

	function requestsDefinitionsService() {
		var self = this;

		this.REQUEST_TYPES = {
			TEXT: 0,
			ABSENCE: 1
		};

		this.REQUEST_SORTING_ORDERS = {
			AgentNameAsc: 0,
			AgentNameDesc: 1,
			UpdatedOnAsc: 2,
			UpdatedOnDesc: 3,
			CreatedOnAsc: 4,
			CreatedOnDesc: 5
		};

		this.normalizeRequestsFilter = function(filter, sortingOrders) {
			return {
				StartDate: { Date: filter.period.startDate },
				EndDate: { Date: filter.period.endDate },
				SortingOrders: sortingOrders
			};
		};

		this.translateSingleSortingOrder = function(sortColumn) {
			
			var Orders = self.REQUEST_SORTING_ORDERS;

			if (sortColumn.name === 'AgentName') {
				return sortColumn.sort.direction === 'asc' ? Orders.AgentNameAsc : Orders.AgentNameDesc;
			} else if (sortColumn.name == 'UpdatedTime') {
				return sortColumn.sort.direction === 'asc' ? Orders.UpdatedOnAsc : Orders.UpdatedOnDesc;
			} else if (sortColumn.name == 'CreatedTime') {
				return sortColumn.sort.direction === 'asc' ? Orders.CreatedOnAsc : Orders.CreatedOnDesc;
			} else {
				return null;
			}
		};
	}

	

})();
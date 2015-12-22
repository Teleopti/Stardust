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

		this.normalizeRequestsFilter = function(filter, sortingOrders, paging) {
			var target = {
				StartDate: filter.period.startDate,
				EndDate: filter.period.endDate,
				SortingOrders: sortingOrders.join(','),
				AgentSearchTerm: filter.agentSearchTerm
			};

			if (paging != null) {
				target.Paging = {
					Skip: Math.max((paging.pageNumber - 1), 0) * paging.pageSize,
					Take: paging.pageSize
				};
				target.Skip = target.Paging.Skip;
				target.Take = target.Paging.Take;
			}
		
			return target;
		};

		this.normalizeRequestsFilter_old = function (filter, sortingOrders, paging) {
			var target = {
				StartDate: filter.period.startDate,
				EndDate: filter.period.endDate,
				SortingOrders: sortingOrders,
				AgentSearchTerm: filter.agentSearchTerm
			};

			if (paging != null) {
				target.Paging = {
					Skip: Math.max((paging.pageNumber - 1), 0) * paging.pageSize,
					Take: paging.pageSize
				};
				target.Skip = target.Paging.Skip;
				target.Take = target.Paging.Take;
			}

			return target;
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
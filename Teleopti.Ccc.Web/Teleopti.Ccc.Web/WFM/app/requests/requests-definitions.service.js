(function() {
	'use strict';

	angular.module('wfm.requests').service('requestsDefinitions', requestsDefinitionsService);

	function requestsDefinitionsService() {
		var self = this;

		this.REQUEST_TYPES = {
			TEXT: 0,
			ABSENCE: 1
		};

		this.SHIFTTRADE_COLUMN_WIDTH = 40;

		this.REQUEST_SORTING_ORDERS = {
			AgentNameAsc	: 0,
			AgentNameDesc	: 1,
			CreatedOnAsc	: 2,
			CreatedOnDesc	: 3,
			DenyReasonAsc	: 4,
			DenyReasonDesc	: 5,
			MessageAsc		: 6,
			MessageDesc		: 7,
			PeriodStartAsc	: 8,
			PeriodStartDesc: 9,
			PeriodEndAsc	: 10,
			PeriodEndDesc	: 11,
			SeniorityAsc	: 12,
			SeniorityDesc	: 13,
			SubjectAsc		: 14,
			SubjectDesc		: 15,
			TeamAsc			: 16,
			TeamDesc	      : 17,
			UpdatedOnAsc	: 18,
			UpdatedOnDesc	: 19
		};

		this.SHIFT_OBJECT_TYPE = {
			PersonAssignment: 1,
			DayOff: 2
		}

		this.REQUEST_COMMANDS = {
			Approve: 'approve',
			Deny: 'deny',
			Cancel: 'cancel',
			ProcessWaitlist: 'processWaitlist',
			ApproveBasedOnBusinessRules: 'approveBasedOnBusinessRules',
			Reply: 'reply'
		};

		// Refer to Teleopti.Ccc.Domain.ApplicationLayer.Commands.RequestValidatorsFlag
		this.REQUEST_VALIDATORS = {
			None: 0,
			BudgetAllotmentValidator: 1,
			IntradayValidator: 2,
			ExpirationValidator: 4
		};

		this.formatFilters = function(filters) {
			var formated = {};
			for (var i in filters) {
				if (filters.hasOwnProperty(i)) {
					if (filters[i].Status !== undefined) formated['Status'] = filters[i].Status;
					if (filters[i].Type !== undefined) formated['Type'] = filters[i].Type;
					if (filters[i].Subject !== undefined) formated['Subject'] = filters[i].Subject;
					if (filters[i].Message !== undefined) formated['Message'] = filters[i].Message;
				}
			}
			return formated;
		}

		this.fillTermItem = function(key, item, output) {
			output[key] = item.substring(item.indexOf(":") + 2, item.length);
		}
		this.formatAgentSearchTerm = function(terms) {
			var formated = {};
			var strlist = terms.split(";");
			strlist.pop();
			for (var i in strlist) {
				if (strlist.hasOwnProperty(i)) {
					if (strlist[i].includes("FirstName")) this.fillTermItem('FirstName', strlist[i], formated);
					if (strlist[i].includes("LastName")) this.fillTermItem('LastName', strlist[i], formated);
					if (strlist[i].includes("EmploymentNumber")) this.fillTermItem('EmploymentNumber', strlist[i], formated);
					if (strlist[i].includes("Organization")) this.fillTermItem('Organization', strlist[i], formated);
					if (strlist[i].includes("Role")) this.fillTermItem('Role', strlist[i], formated);
					if (strlist[i].includes("Contract")) this.fillTermItem('Contract', strlist[i], formated);
					if (strlist[i].includes("ContractSchedule")) this.fillTermItem('ContractSchedule', strlist[i], formated);
					if (strlist[i].includes("ShiftBags")) this.fillTermItem('ShiftBag', strlist[i], formated);
					if (strlist[i].includes("PartTimePercentage")) this.fillTermItem('PartTimePercentage', strlist[i], formated);
					if (strlist[i].includes("Skill")) this.fillTermItem('Skill', strlist[i], formated);
					if (strlist[i].includes("BudgetGroup")) this.fillTermItem('BudgetGroup', strlist[i], formated);
					if (strlist[i].includes("Note")) this.fillTermItem('Note', strlist[i], formated);
				}
			}
			return formated;
		}

		this.normalizeRequestsFilter = function (filter, sortingOrders, paging) {
			var filters = this.formatFilters(filter.filters);
			var terms = this.formatAgentSearchTerm(filter.agentSearchTerm);

			var target = {
				StartDate: moment(filter.period.startDate).format('YYYY-MM-DD'),
				EndDate: moment(filter.period.endDate).format('YYYY-MM-DD'),
				SortingOrders: sortingOrders,
				AgentSearchTerm: terms,
				SelectedTeamIds: filter.selectedTeamIds,
				Filters: filters
			};

			if (paging !== null) {
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
				AgentSearchTerm: filter.agentSearchTerm,
				SelectedTeamIds: filter.selectedTeamIds
			};

			if (paging !== null) {
				target.Paging = {
					Skip: Math.max((paging.pageNumber - 1), 0) * paging.pageSize,
					Take: paging.pageSize
				};
				target.Skip = target.Paging.Skip;
				target.Take = target.Paging.Take;
			}

			return target;
		};

		this.translateSingleSortingOrder = function (sortColumn) {
			var Orders = self.REQUEST_SORTING_ORDERS;

			if (sortColumn.displayName === 'AgentName') {
				return sortColumn.sort.direction === 'asc' ? Orders.AgentNameAsc : Orders.AgentNameDesc;
			} else if (sortColumn.displayName === 'StartTime') {
				return sortColumn.sort.direction === 'asc' ? Orders.PeriodStartAsc : Orders.PeriodStartDesc;
			} else if (sortColumn.displayName === 'EndTime') {
				return sortColumn.sort.direction === 'asc' ? Orders.PeriodEndAsc : Orders.PeriodEndDesc;
			} else if (sortColumn.displayName === 'UpdatedOn') {
				return sortColumn.sort.direction === 'asc' ? Orders.UpdatedOnAsc : Orders.UpdatedOnDesc;
			} else if (sortColumn.displayName === 'CreatedOn') {
				return sortColumn.sort.direction === 'asc' ? Orders.CreatedOnAsc : Orders.CreatedOnDesc;
			} else if (sortColumn.displayName === 'DenyReason') {
				return sortColumn.sort.direction === 'asc' ? Orders.DenyReasonAsc : Orders.DenyReasonDesc;
			} else if (sortColumn.displayName === 'Message') {
				return sortColumn.sort.direction === 'asc' ? Orders.MessageAsc : Orders.MessageDesc;
			} else if (sortColumn.displayName === 'Subject') {
				return sortColumn.sort.direction === 'asc' ? Orders.SubjectAsc : Orders.SubjectDesc;
			} else if (sortColumn.displayName === 'Seniority') {
				return sortColumn.sort.direction === 'asc' ? Orders.SeniorityAsc : Orders.SeniorityDesc;
			} else if (sortColumn.displayName === 'Team') {
				return sortColumn.sort.direction === 'asc' ? Orders.TeamAsc : Orders.TeamDesc;
			} else {
				return null;
			}
		};
	}
})();
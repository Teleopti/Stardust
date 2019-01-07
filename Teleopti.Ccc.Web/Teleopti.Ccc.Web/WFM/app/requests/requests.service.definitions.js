(function() {
	'use strict';

	angular.module('wfm.requests').service('requestsDefinitions', ['$translate',requestsDefinitionsService]);

	function requestsDefinitionsService($translate) {
		var svc = this;

		svc.REQUEST_TYPES = {
			TEXT: 0,
			ABSENCE: 1,
			SHIFTTRADE: 2,
			OVERTIME: 3
		};

		svc.SHIFTTRADE_COLUMN_WIDTH = 40;

		svc.REQUEST_SORTING_ORDERS = {
			AgentNameAsc: 0,
			AgentNameDesc: 1,
			CreatedOnAsc: 2,
			CreatedOnDesc: 3,
			DenyReasonAsc: 4,
			DenyReasonDesc: 5,
			MessageAsc: 6,
			MessageDesc: 7,
			PeriodStartAsc: 8,
			PeriodStartDesc: 9,
			PeriodEndAsc: 10,
			PeriodEndDesc: 11,
			SeniorityAsc: 12,
			SeniorityDesc: 13,
			SubjectAsc: 14,
			SubjectDesc: 15,
			TeamAsc: 16,
			TeamDesc: 17,
			UpdatedOnAsc: 18,
			UpdatedOnDesc: 19
		};

		svc.SHIFT_OBJECT_TYPE = {
			PersonAssignment: 1,
			DayOff: 2
		};

		svc.REQUEST_COMMANDS = {
			Approve: 'approve',
			Deny: 'deny',
			Cancel: 'cancel',
			ProcessWaitlist: 'processWaitlist',
			ApproveBasedOnBusinessRules: 'approveBasedOnBusinessRules',
			Reply: 'reply'
		};

		// Refer to Teleopti.Ccc.Domain.ApplicationLayer.Commands.RequestValidatorsFlag
		svc.REQUEST_VALIDATORS = {
			None: 0,
			BudgetAllotmentValidator: 1,
			IntradayValidator: 2,
			ExpirationValidator: 4
		};

		svc.formatFilters = function(filters) {
			var formated = {};
			for (var i in filters) {
				if (filters.hasOwnProperty(i)) {
					if (angular.isDefined(filters[i].Status)) formated['Status'] = filters[i].Status;
					if (angular.isDefined(filters[i].Type)) formated['Type'] = filters[i].Type;
					if (angular.isDefined(filters[i].Subject)) formated['Subject'] = filters[i].Subject;
					if (angular.isDefined(filters[i].Message)) formated['Message'] = filters[i].Message;
				}
			}
			return formated;
		};

		svc.fillTermItem = function(key, item, output) {
			output[key] = item.indexOf(':') > -1 ? item.substring(item.indexOf(':') + 2, item.length) : item;
		};

		svc.formatAgentSearchTerm = function(terms) {
			var formated = {};
			var strlist = terms.indexOf(';') > -1 ? terms.split(';') : [terms];
			if (strlist.indexOf('') > -1) strlist.pop();

			for (var i in strlist) {
				if (strlist.hasOwnProperty(i)) {
					if (strlist[i].indexOf('FirstName:') > -1) svc.fillTermItem('FirstName', strlist[i], formated);
					else if (strlist[i].indexOf('LastName:') > -1) svc.fillTermItem('LastName', strlist[i], formated);
					else if (strlist[i].indexOf('EmploymentNumber:') > -1)
						svc.fillTermItem('EmploymentNumber', strlist[i], formated);
					else if (strlist[i].indexOf('Organization:') > -1)
						svc.fillTermItem('Organization', strlist[i], formated);
					else if (strlist[i].indexOf('Role:') > -1) svc.fillTermItem('Role', strlist[i], formated);
					else if (strlist[i].indexOf('Contract:') > -1) svc.fillTermItem('Contract', strlist[i], formated);
					else if (strlist[i].indexOf('ContractSchedule:') > -1)
						svc.fillTermItem('ContractSchedule', strlist[i], formated);
					else if (strlist[i].indexOf('ShiftBag:') > -1) svc.fillTermItem('ShiftBag', strlist[i], formated);
					else if (strlist[i].indexOf('PartTimePercentage:') > -1)
						svc.fillTermItem('PartTimePercentage', strlist[i], formated);
					else if (strlist[i].indexOf('Skill:') > -1) svc.fillTermItem('Skill', strlist[i], formated);
					else if (strlist[i].indexOf('BudgetGroup:') > -1)
						svc.fillTermItem('BudgetGroup', strlist[i], formated);
					else if (strlist[i].indexOf('Note:') > -1) svc.fillTermItem('Note', strlist[i], formated);
					else svc.fillTermItem('All', strlist[i], formated);
				}
			}
			return formated;
		};

		svc.normalizeRequestsFilter = function(filter, sortingOrders, paging) {
			var filters = svc.formatFilters(filter.filters);
			var terms = svc.formatAgentSearchTerm(filter.agentSearchTerm);
			var target = {
				StartDate: toShortDate(filter.period.startDate),
				EndDate: toShortDate(filter.period.endDate),
				SortingOrders: sortingOrders,
				AgentSearchTerm: terms,
				SelectedGroupIds: filter.selectedGroupIds,
				SelectedGroupPageId: filter.selectedGroupPageId,
				Filters: filters
			};

			if (paging !== null) {
				target.Paging = {
					Skip: Math.max(paging.pageNumber - 1, 0) * paging.pageSize,
					Take: paging.pageSize
				};
				target.Skip = target.Paging.Skip;
				target.Take = target.Paging.Take;
			}

			return target;
		};

		svc.normalizeRequestsFilter_old = function(filter, sortingOrders) {
			var target = {
				StartDate: filter.period.startDate,
				EndDate: filter.period.endDate,
				SortingOrders: sortingOrders,
				AgentSearchTerm: filter.agentSearchTerm,
				SelectedTeamIds: filter.selectedTeamIds
			};

			return target;
		};

		svc.translateSingleSortingOrder = function(sortColumn) {
			if (!sortColumn) return;

			var Orders = svc.REQUEST_SORTING_ORDERS;

			if (sortColumn.displayName === $translate.instant('AgentName')) {
				return sortColumn.sort.direction === 'asc' ? Orders.AgentNameAsc : Orders.AgentNameDesc;
			} else if (sortColumn.displayName === $translate.instant('StartTime')) {
				return sortColumn.sort.direction === 'asc' ? Orders.PeriodStartAsc : Orders.PeriodStartDesc;
			} else if (sortColumn.displayName === $translate.instant('EndTime')) {
				return sortColumn.sort.direction === 'asc' ? Orders.PeriodEndAsc : Orders.PeriodEndDesc;
			} else if (sortColumn.displayName === $translate.instant('UpdatedOn')) {
				return sortColumn.sort.direction === 'asc' ? Orders.UpdatedOnAsc : Orders.UpdatedOnDesc;
			} else if (sortColumn.displayName === $translate.instant('CreatedOn')) {
				return sortColumn.sort.direction === 'asc' ? Orders.CreatedOnAsc : Orders.CreatedOnDesc;
			} else if (sortColumn.displayName === $translate.instant('DenyReason')) {
				return sortColumn.sort.direction === 'asc' ? Orders.DenyReasonAsc : Orders.DenyReasonDesc;
			} else if (sortColumn.displayName === $translate.instant('Message')) {
				return sortColumn.sort.direction === 'asc' ? Orders.MessageAsc : Orders.MessageDesc;
			} else if (sortColumn.displayName === $translate.instant('Subject')) {
				return sortColumn.sort.direction === 'asc' ? Orders.SubjectAsc : Orders.SubjectDesc;
			} else if (sortColumn.displayName === $translate.instant('Seniority')) {
				return sortColumn.sort.direction === 'asc' ? Orders.SeniorityAsc : Orders.SeniorityDesc;
			} else if (sortColumn.displayName === $translate.instant('Team')) {
				return sortColumn.sort.direction === 'asc' ? Orders.TeamAsc : Orders.TeamDesc;
			} else {
				return null;
			}
		};

		function toShortDate(datetime) {
			return datetime.getFullYear() + '-' + (datetime.getMonth() + 1) + '-' + datetime.getDate();
		}
	}
})();

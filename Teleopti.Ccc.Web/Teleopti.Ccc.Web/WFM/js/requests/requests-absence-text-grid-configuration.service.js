﻿'use strict';

(function () {

	angular.module('wfm.requests')
		.factory('TextAndAbsenceGridConfiguration', function () {

			var columns = [];

			var service = {
				columnDefinitions: columnDefinitions,
				categories: getCategories
			}

			function getCategories() {

				return null;
			}

			function setupColumns() {

				columns = [
				{ displayName: 'StartTime', field: 'FormatedPeriodStartTime()', headerCellFilter: 'translate', cellClass: 'request-period-start-time', headerCellClass: 'request-period-start-time-header' },
				{ displayName: 'EndTime', field: 'FormatedPeriodEndTime()', headerCellFilter: 'translate', cellClass: 'request-period-end-time', headerCellClass: 'request-period-end-time-header' },
				{ displayName: 'TimeZone', field: 'TimeZone', headerCellFilter: 'translate', cellClass: 'request-time-zone', headerCellClass: 'request-time-zone-header', visible: false, enableSorting: false },
				{ displayName: 'Duration', field: 'GetDuration()', headerCellFilter: 'translate', cellClass: 'request-period-duration', enableSorting: false, visible: false, headerCellClass: 'request-period-duration-header' },
				{ displayName: 'AgentName', field: 'AgentName', headerCellFilter: 'translate', cellClass: 'request-agent-name', headerCellClass: 'request-agent-name-header' },
				{ displayName: 'Team', field: 'Team', headerCellFilter: 'translate', cellClass: 'request-team', headerCellClass: 'request-team-header' },
				{ displayName: 'Seniority', field: 'Seniority', headerCellFilter: 'translate', cellClass: 'request-seniority', headerCellClass: 'request-seniority-header', visible: false },
				{
					displayName: 'Type',
					field: 'GetType()',
					headerCellFilter: 'translate',
					cellClass: 'request-type',
					headerCellClass: 'request-type-header',
					enableSorting: false,
					visible: true,
					filterHeaderTemplate: '<div class=\"ui-grid-filter-container\" ng-repeat=\"colFilter in col.filters\">'
						+ '<div isteven-multi-select input-model=\"grid.appScope.AllRequestableAbsences\" output-model=\"grid.appScope.SelectedAbsences\" '
						+ 'button-label=\"Name\" item-label=\"Name\" on-close=\"grid.appScope.absenceFilterClose()\" '
						+ 'tick-property=\"Selected\" max-labels=\"1\" helper-elements=\"\"></div>'
						+ '</div>'
				},
				{
					displayName: 'Subject',
					field: 'Subject',
					headerCellFilter: 'translate',
					cellClass: 'request-subject',
					headerCellClass: 'request-subject-header',
					filter: {
						disableCancelFilterButton: true,
						placeholder: 'Filter...'
					},
					filterHeaderTemplate: '<div class=\"ui-grid-filter-container\" ng-repeat=\"colFilter in col.filters\">' +
						'<input xng-enter=\"enter()\" style=\"background-color:#FFF\" type=\"text\" class=\"ui-grid-filter-input ui-grid-filter-input-{{$index}}\" ng-model=\"colFilter.term\" ng-attr-placeholder=\"{{colFilter.placeholder || \'\'}}\" aria-label=\"{{colFilter.ariaLabel || aria.defaultFilterLabel}}\" /></div>'
				},
				{
					displayName: 'Message',
					field: 'Message',
					headerCellFilter: 'translate',
					cellClass: 'request-message',
					headerCellClass: 'request-message-header',
					visible: false,
					filter: {
						disableCancelFilterButton: true,
						placeholder: 'Filter...'
					},
					filterHeaderTemplate: '<div class=\"ui-grid-filter-container\" ng-repeat=\"colFilter in col.filters\" > <input ng-enter=\"enter()\" style=\"background-color:#FFF\" type=\"text\" class=\"ui-grid-filter-input ui-grid-filter-input-{{$index}}\" ng-model=\"colFilter.term\" ng-attr-placeholder=\"{{colFilter.placeholder || \'\'}}\" aria-label=\"{{colFilter.ariaLabel || aria.defaultFilterLabel}}\" /></div>'
				},
				{ displayName: 'DenyReason', field: 'DenyReason', headerCellFilter: 'translate', cellClass: 'request-deny-reason', headerCellClass: 'request-deny-reason-header', visible: false },
				{
					displayName: 'Status',
					field: 'StatusText',
					headerCellFilter: 'translate',
					cellClass: 'request-status',
					headerCellClass: 'request-status-header',
					enableSorting: false,
					filterHeaderTemplate: '<div class=\"ui-grid-filter-container\" ng-repeat=\"colFilter in col.filters\">'
						+ '<div isteven-multi-select input-model=\"grid.appScope.AllRequestStatuses\" output-model=\"grid.appScope.SelectedRequestStatuses\" '
						+ 'button-label=\"Name\" item-label=\"Name\" on-close=\"grid.appScope.statusFilterClose()\" '
						+ 'tick-property=\"Selected\" max-labels=\"1\" helper-elements=\"\"></div>'
						+ '</div>'
				},
				{ displayName: 'CreatedOn', field: 'FormatedCreatedTime()', headerCellFilter: 'translate', cellClass: 'request-created-time', headerCellClass: 'request-created-time-header' },
				{ displayName: 'Id', field: 'Id', headerCellFilter: 'translate', cellClass: 'request-id', visible: false, headerCellClass: 'request-id-header' },
				{ displayName: 'UpdatedOn', field: 'FormatedUpdatedTime()', headerCellFilter: 'translate', cellClass: 'request-updated-time', visible: false, headerCellClass: 'request-updated-time-header' }
				];


			}

			function columnDefinitions() {
				
				if (columns.length == 0) {
					setupColumns();
				}
				
				return columns;
			}

			return service;

		});

}());

'use strict';

(function() {
	angular
		.module('wfm.requests')
		.factory('TextAndAbsenceGridConfiguration', ['$translate', TextAndAbsenceGridConfiguration]);

	function TextAndAbsenceGridConfiguration($translate) {
		var columns = [];

		var service = {
			columnDefinitions: columnDefinitions
		};

		function setupColumns() {
			columns = [
				{
					displayName: $translate.instant('StartTime'),
					field: 'FormatedPeriodStartTime()',
					cellClass: 'request-period-start-time',
					headerCellClass: 'request-period-start-time-header',
					enableFiltering: false
				},
				{
					displayName: $translate.instant('EndTime'),
					field: 'FormatedPeriodEndTime()',
					cellClass: 'request-period-end-time',
					headerCellClass: 'request-period-end-time-header',
					enableFiltering: false
				},
				{
					displayName: $translate.instant('TimeZone'),
					field: 'TimeZone',
					cellClass: 'request-time-zone',
					headerCellClass: 'request-time-zone-header',
					visible: false,
					enableSorting: false,
					enableFiltering: false
				},
				{
					displayName: $translate.instant('Duration'),
					field: 'GetDuration()',
					cellClass: 'request-period-duration',
					enableSorting: false,
					enableFiltering: false,
					visible: false,
					headerCellClass: 'request-period-duration-header'
				},
				{
					displayName: $translate.instant('AgentName'),
					field: 'AgentName',
					cellClass: 'request-agent-name',
					headerCellClass: 'request-agent-name-header',
					enableFiltering: false
				},
				{
					displayName: $translate.instant('Team'),
					field: 'Team',
					cellClass: 'request-team',
					headerCellClass: 'request-team-header',
					enableFiltering: false
				},
				{
					displayName: $translate.instant('Seniority'),
					field: 'Seniority',
					cellClass: 'request-seniority',
					headerCellClass: 'request-seniority-header',
					visible: false,
					enableFiltering: false
				},
				{
					displayName: $translate.instant('Type'),
					field: 'GetType()',
					cellClass: 'request-type',
					headerCellClass: 'request-type-header',
					enableSorting: false,
					enableFiltering: true,
					visible: true,
					filterHeaderTemplate:
						'<div class="ui-grid-filter-container" ng-repeat="colFilter in col.filters">' +
						'<md-select md-on-close="grid.appScope.typeFilterClose()" ng-model-options="{trackBy: \'$value.Id\'}"' +
						'multiple ng-model="grid.appScope.selectedTypes" placeholder="{{\'FilterColon\' | translate}} {{\'Type\' | translate}}" aria-label="{{\'Type\' | translate}}">' +
						'<md-option ng-repeat="item in grid.appScope.AllRequestTypes" ng-value="item">' +
						'<span>{{item.Name}}</span>' +
						'</md-option>' +
						'</md-select>' +
						'</div>'
				},
				{
					displayName: $translate.instant('Subject'),
					field: 'Subject',
					cellClass: 'request-subject',
					cellTooltip: true,
					headerCellClass: 'request-subject-header',
					enableFiltering: true,
					filter: {
						disableCancelFilterButton: true,
						placeholder: $translate.instant('FilterThreeDots')
					},
					filterHeaderTemplate:
						'<div class="ui-grid-filter-container" ng-repeat="colFilter in col.filters">' +
						'<input ng-enter="enter()" type="text" ng-change="grid.appScope.subjectFilterChanged()" ' +
						'ng-model="grid.appScope.subjectFilter" ng-attr-placeholder="{{colFilter.placeholder || \'\'}}" ' +
						'ng-model-options="{ debounce: 500 }" aria-label="{{colFilter.ariaLabel || aria.defaultFilterLabel}}" />' +
						'</div>'
				},
				{
					displayName: $translate.instant('Message'),
					field: 'Message',
					cellClass: 'request-message',
					headerCellClass: 'request-message-header',
					visible: false,
					cellTooltip: true,
					enableFiltering: true,
					filter: {
						disableCancelFilterButton: true,
						placeholder: $translate.instant('FilterThreeDots')
					},
					filterHeaderTemplate:
						'<div class="ui-grid-filter-container" ng-repeat="colFilter in col.filters" >' +
						'<input ng-enter="enter()" type="text" ng-change="grid.appScope.messageFilterChanged()"' +
						'ng-model="grid.appScope.messageFilter" ng-model-options="{ debounce: 500 }"' +
						'ng-attr-placeholder="{{colFilter.placeholder || \'\'}}"' +
						'ria-label="{{colFilter.ariaLabel || aria.defaultFilterLabel}}" />' +
						'</div>'
				},
				{
					displayName: $translate.instant('DenyReason'),
					field: 'DenyReason',
					cellClass: 'request-deny-reason',
					headerCellClass: 'request-deny-reason-header',
					visible: false,
					enableFiltering: false,
					cellTooltip: true
				},
				{
					displayName: $translate.instant('Status'),
					field: 'StatusText',
					cellClass: 'request-status',
					headerCellClass: 'request-status-header',
					enableSorting: false,
					enableFiltering: true,
					filterHeaderTemplate:
						'<div class="ui-grid-filter-container" ng-repeat="colFilter in col.filters">' +
						'<md-select class="test-status-selector" ng-model-options="{trackBy: \'$value.Id\'}" md-on-close="grid.appScope.statusFilterClose()"' +
						'multiple ng-model="grid.appScope.selectedRequestStatuses" placeholder="{{\'FilterColon\' | translate}} {{\'Status\' | translate}}" aria-label="{{\'Status\' | translate}}">' +
						'<md-option ng-repeat="item in grid.appScope.allRequestStatuses" ng-value="item">' +
						'<span>{{item.Name | translate}}</span>' +
						'</md-option>' +
						'</md-select>' +
						'</div>'
				},
				{
					displayName: $translate.instant('CreatedOn'),
					field: 'FormatedCreatedTime()',
					cellClass: 'request-created-time',
					enableFiltering: false,
					headerCellClass: 'request-created-time-header'
				},
				{
					displayName: $translate.instant('UpdatedOn'),
					field: 'FormatedUpdatedTime()',
					cellClass: 'request-updated-time',
					visible: false,
					enableFiltering: false,
					headerCellClass: 'request-updated-time-header'
				}
			];

			var accountColumn = {
				displayName: $translate.instant('Account'),
				field: 'PersonAccountSummaryViewModel',
				enableFiltering: false,
				cellTemplate: 'app/requests/html/requests-absence-person-account-overview.html',
				enableSorting: false
			};

			columns.splice(12, 0, accountColumn);
		}

		function columnDefinitions() {
			if (columns.length === 0) {
				setupColumns();
			}

			// since upgrading to ui-grid 3.2.6, require copy of columns array
			return angular.copy(columns);
		}

		return service;
	}
})();

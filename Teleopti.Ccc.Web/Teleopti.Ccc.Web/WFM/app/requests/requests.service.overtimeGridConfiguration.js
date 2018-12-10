'use strict';

(function() {
	angular.module('wfm.requests').factory('OvertimeGridConfiguration', ['$translate', OvertimeGridConfiguration]);

	function OvertimeGridConfiguration($translate) {
		var svc = this,
			columns = [];

		function setupColumns() {
			columns = [
				{
					displayName: $translate.instant('StartTime'),
					enableFiltering: false,
					field: 'FormatedPeriodStartTime()',
					cellClass: 'request-period-start-time',
					headerCellClass: 'request-period-start-time-header'
				},
				{
					displayName: $translate.instant('EndTime'),
					enableFiltering: false,
					field: 'FormatedPeriodEndTime()',
					cellClass: 'request-period-end-time',
					headerCellClass: 'request-period-end-time-header'
				},
				{
					displayName: $translate.instant('TimeZone'),
					enableFiltering: false,
					field: 'TimeZone',
					cellClass: 'request-time-zone',
					headerCellClass: 'request-time-zone-header',
					visible: false,
					enableSorting: false
				},
				{
					displayName: $translate.instant('Duration'),
					enableFiltering: false,
					field: 'GetDuration()',
					cellClass: 'request-period-duration',
					enableSorting: false,
					visible: false,
					headerCellClass: 'request-period-duration-header'
				},
				{
					displayName: $translate.instant('AgentName'),
					enableFiltering: false,
					field: 'AgentName',
					cellClass: 'request-agent-name',
					headerCellClass: 'request-agent-name-header'
				},
				{
					displayName: $translate.instant('Team'),
					enableFiltering: false,
					field: 'Team',
					cellClass: 'request-team',
					headerCellClass: 'request-team-header'
				},
				{
					displayName: $translate.instant('Seniority'),
					enableFiltering: false,
					field: 'Seniority',
					cellClass: 'request-seniority',
					headerCellClass: 'request-seniority-header',
					visible: false
				},
				{
					displayName: $translate.instant('Type'),
					field: 'OvertimeTypeDescription',
					cellClass: 'request-type',
					headerCellClass: 'request-type-header',
					enableSorting: false,
					enableFiltering: true,
					visible: true,
					filterHeaderTemplate:
						'<div class="ui-grid-filter-container" ng-repeat="colFilter in col.filters">' +
						'<md-select md-on-close="grid.appScope.typeFilterClose()" ng-model-options="{trackBy: \'$value.Id\'}"' +
						'multiple ng-model="grid.appScope.selectedTypes" placeholder="{{\'FilterColon\' | translate}} {{\'Type\' | translate}}" aria-label="{{\'Type\' | translate}}">' +
						'<md-option ng-repeat="item in grid.appScope.overtimeTypes" ng-value="item">' +
						'<span>{{ item.Name | translate}}</span>' +
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
					enableFiltering: false,
					field: 'DenyReason',
					cellClass: 'request-deny-reason',
					headerCellClass: 'request-deny-reason-header',
					visible: false,
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
						'<md-select ng-model-options="{trackBy: \'$value.Id\'}" md-on-close="grid.appScope.statusFilterClose()"' +
						'multiple ng-model="grid.appScope.selectedRequestStatuses" placeholder="{{\'FilterColon\' | translate}} {{\'Status\' | translate}}" aria-label="{{\'Status\' | translate}}">' +
						'<md-option ng-repeat="item in grid.appScope.allRequestStatuses" ng-value="item">' +
						'<span>{{item.Name | translate}}</span>' +
						'</md-option>' +
						'</md-select>' +
						'</div>'
				},
				{
					displayName: $translate.instant('CreatedOn'),
					enableFiltering: false,
					field: 'FormatedCreatedTime()',
					cellClass: 'request-created-time',
					headerCellClass: 'request-created-time-header'
				},
				{
					displayName: $translate.instant('UpdatedOn'),
					enableFiltering: false,
					field: 'FormatedUpdatedTime()',
					cellClass: 'request-updated-time',
					visible: false,
					headerCellClass: 'request-updated-time-header'
				}
			];
		}

		svc.columnDefinitions = function() {
			if (columns.length === 0) {
				setupColumns();

				var brokenRulesColumn = {
					displayName: $translate.instant('BrokenRules'),
					enableFiltering: false,
					field: 'GetBrokenRules()',
					cellTooltip: true,
					visible: true,
					minWidth: 111
				};
				columns.splice(11, 0, brokenRulesColumn);
			}
			// since upgrading to ui-grid 3.2.6, require copy of columns array
			return angular.copy(columns);
		};

		return svc;
	}
})();

'use strict';

(function () {

	angular.module('wfm.requests')
		.factory('TextAndAbsenceGridConfiguration', ['Toggle', '$translate', function (toggleSvc, $translate) {
			var columns = [];

			var service = {
				columnDefinitions: columnDefinitions
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
					filterHeaderTemplate: '<md-select ng-repeat=\"colFilter in col.filters\" md-on-close=\"grid.appScope.typeFilterClose()\"'
					+ 'multiple ng-model=\"grid.appScope.SelectedTypes\" placeholder=\"{{\'FilterColon\' | translate}} {{\'Type\' | translate}}\" aria-label=\"{{\'Type\' | translate}}\">'
					+ '<md-option ng-repeat=\"item in grid.appScope.AllRequestTypes\" ng-value=\"item\">'
					+ '<span>{{ item.Name | translate}}</span>'
					+ '</md-option>'
					+ '</md-select>'
				},
				{
					displayName: 'Subject',
					field: 'Subject',
					headerCellFilter: 'translate',
					cellClass: 'request-subject',
					cellTooltip: true,
					headerCellClass: 'request-subject-header',
					filter: {
						disableCancelFilterButton: true,
						placeholder: $translate.instant('FilterThreeDots')
					},
					filterHeaderTemplate: '<div class=\"ui-grid-filter-container\" ng-repeat=\"colFilter in col.filters\">' +
						'<input ng-enter=\"enter()\" type=\"text\" ng-change=\"grid.appScope.subjectFilterChanged()\" ' +
						'ng-model=\"grid.appScope.subjectFilter\" ng-attr-placeholder=\"{{colFilter.placeholder || \'\'}}\" ' +
						'ng-model-options=\"{ debounce: 500 }\" aria-label=\"{{colFilter.ariaLabel || aria.defaultFilterLabel}}\" />' +
						'</div>'
				},
				{
					displayName: 'Message',
					field: 'Message',
					headerCellFilter: 'translate',
					cellClass: 'request-message',
					headerCellClass: 'request-message-header',
					visible: false,
					cellTooltip: true,
					filter: {
						disableCancelFilterButton: true,
						placeholder: $translate.instant('FilterThreeDots')
					},
					filterHeaderTemplate: '<div class=\"ui-grid-filter-container\" ng-repeat=\"colFilter in col.filters\" >' +
						'<input ng-enter=\"enter()\" type=\"text\" ng-change=\"grid.appScope.messageFilterChanged()\"' +
						'ng-model=\"grid.appScope.messageFilter\" ng-model-options=\"{ debounce: 500 }\"' +
						'ng-attr-placeholder=\"{{colFilter.placeholder || \'\'}}\"' +
						'ria-label=\"{{colFilter.ariaLabel || aria.defaultFilterLabel}}\" />' +
						'</div>'
				},
				{ displayName: 'DenyReason', field: 'DenyReason', headerCellFilter: 'translate', cellClass: 'request-deny-reason', headerCellClass: 'request-deny-reason-header', visible: false, cellTooltip: true },
				{
					displayName: 'Status',
					field: 'StatusText',
					headerCellFilter: 'translate',
					cellClass: 'request-status',
					headerCellClass: 'request-status-header',
					enableSorting: false,
					filterHeaderTemplate: '<md-select class=\"test-status-selector\" ng-model-options=\"{trackBy: \'$value.Id\'}\" ng-repeat=\"colFilter in col.filters\" md-on-close=\"grid.appScope.statusFilterClose()\"'
					+ 'multiple ng-model=\"grid.appScope.selectedRequestStatuses\" placeholder=\"{{\'FilterColon\' | translate}} {{\'Status\' | translate}}\" aria-label=\"{{\'Status\' | translate}}\">'
					+ '<md-option ng-repeat=\"item in grid.appScope.allRequestStatuses\" ng-value=\"item\">'
					+ '<span>{{item.Name | translate}}</span>'
					+ '</md-option>'
					+ '</md-select>'
				},
				{ displayName: 'CreatedOn', field: 'FormatedCreatedTime()', headerCellFilter: 'translate', cellClass: 'request-created-time', headerCellClass: 'request-created-time-header' },
				{ displayName: 'UpdatedOn', field: 'FormatedUpdatedTime()', headerCellFilter: 'translate', cellClass: 'request-updated-time', visible: false, headerCellClass: 'request-updated-time-header' }
				];

				if (toggleSvc.Wfm_Requests_Show_Personal_Account_39628) {
					var accountColumn = {
						displayName: 'Account',
						field: 'PersonAccountSummaryViewModel',
						headerCellFilter: 'translate',
						cellTemplate: 'requests-absence-person-account-overview.html',
						enableSorting: false
					};

					columns.splice(12, 0, accountColumn);
				}
			}

			function columnDefinitions() {
				if (columns.length === 0) {
					setupColumns();
				}

				// since upgrading to ui-grid 3.2.6, require copy of columns array
				return angular.copy(columns);
			}

			return service;
		}]);
}());

(function () {

    'use strict';

    angular.module('wfm.requests')
		.controller('requestsTableContainerCtrl', requestsTableContainerController)
		.directive('requestsTableContainer', requestsTableContainerDirective);


    requestsTableContainerController.$inject = ['$scope', '$translate', '$filter', '$timeout', 'Toggle', 'requestsDefinitions', 'requestCommandParamsHolder', 'CurrentUserInfo', 'RequestsFilter', 'requestsDataService', 'uiGridConstants'];

    function requestsTableContainerController($scope, $translate, $filter, $timeout, toggleSvc, requestsDefinitions, requestCommandParamsHolder, CurrentUserInfo, requestFilterSvc, requestsDataSvc, uiGridConstants) {
        var vm = this;
        vm.gridOptions = getGridOptions([]);
        vm.getGridOptions = getGridOptions;
        vm.prepareComputedColumns = prepareComputedColumns;
        vm.clearSelection = clearSelection;
        vm.reselectRequests = reselectRequests;
        vm.enter = function () { };
        vm.toggleFiltering = toggleFiltering;
        vm.filterToggleEnabled = toggleSvc.Wfm_Requests_Filtering_37748;

        function toggleFiltering() {
            vm.gridOptions.enableFiltering =
                !vm.gridOptions.enableFiltering;
            vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
        }

        function clearAllFilters () {
            angular.forEach(vm.gridApi.grid.columns, function (column) {
                column.filters[0].term = undefined;
            });

            angular.forEach(vm.AllRequestableAbsences, function (absence) {
                absence.Selected = false;
            });
            vm.SelectedAbsences = [];

            angular.forEach(vm.AllRequestStatuses, function (status) {
                status.Selected = false;
            });
            vm.SelectedRequestStatuses = [];

            requestFilterSvc.ResetFilter();
            vm.filters = requestFilterSvc.Filters;
        };

        vm.clearFilters = clearAllFilters;

        requestsDataSvc.getRequestableAbsences().then(function (result) {
            vm.AllRequestableAbsences = result.data;
            angular.forEach(vm.AllRequestableAbsences, function (absence) {
                absence.Selected = false;
            });
        });
        vm.AllRequestStatuses = requestsDataSvc.getAllRequestStatuses();

        vm.absenceFilterClose = function () {
            var filters = '';
            angular.forEach(vm.SelectedAbsences, function (absence) {
                filters += absence.Id + ' ';
            });
            requestFilterSvc.SetFilter("Absence", filters.trim());

            vm.filters = requestFilterSvc.Filters;
        };

        vm.statusFilterClose = function () {
            var filters = '';
            angular.forEach(vm.SelectedRequestStatuses, function (status) {
                filters += status.Id + ' ';
            });
            requestFilterSvc.SetFilter("Status", filters.trim());

            vm.filters = requestFilterSvc.Filters;
        };

        function getGridOptions(requests) {
            var options = {
                appScopeProvider: vm,
                enableGridMenu: true,
                enableHorizontalScrollbar: 2,
                useExternalSorting: true,
                data: requests,
                gridMenuTitleFilter: $translate,
                columnDefs: [
					{ displayName: 'StartTime', field: 'FormatedPeriodStartTime()', headerCellFilter: 'translate', cellClass: 'request-period-start-time', headerCellClass: 'request-period-start-time-header' },
					{ displayName: 'EndTime', field: 'FormatedPeriodEndTime()', headerCellFilter: 'translate', cellClass: 'request-period-end-time', headerCellClass: 'request-period-end-time-header' },
					{ displayName: 'TimeZone', field: 'TimeZone', headerCellFilter: 'translate', cellClass: 'request-time-zone', headerCellClass: 'request-time-zone-header', visible: false, enableSorting: false },
					{ displayName: 'Duration', field: 'GetDuration()', headerCellFilter: 'translate', cellClass: 'request-period-duration', enableSorting: false, visible: false, headerCellClass: 'request-period-duration-header' },
					{ displayName: 'AgentName', field: 'AgentName', headerCellFilter: 'translate', cellClass: 'request-agent-name', headerCellClass: 'request-agent-name-header' },
					{ displayName: 'Team', field: 'Team', headerCellFilter: 'translate', cellClass: 'request-team', headerCellClass: 'request-team-header' },
					{ displayName: 'Seniority', field: 'Seniority', headerCellFilter: 'translate', cellClass: 'request-seniority', headerCellClass: 'request-seniority-header', visible: false },
					{
					    displayName: 'Type', field: 'GetType()', headerCellFilter: 'translate', cellClass: 'request-type', headerCellClass: 'request-type-header', enableSorting: false, visible: true,
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
					    filterHeaderTemplate: '<div class=\"ui-grid-filter-container\" ng-repeat=\"colFilter in col.filters\" > <input ng-enter=\"enter()\" style=\"background-color:#FFF\" type=\"text\" class=\"ui-grid-filter-input ui-grid-filter-input-{{$index}}\" ng-model=\"colFilter.term\" ng-attr-placeholder=\"{{colFilter.placeholder || \'\'}}\" aria-label=\"{{colFilter.ariaLabel || aria.defaultFilterLabel}}\" /></div>'
					},
                    {
                        displayName: 'Message', field: 'Message', headerCellFilter: 'translate', cellClass: 'request-message', headerCellClass: 'request-message-header', visible: false,
                        filter: {
                            disableCancelFilterButton: true,
                            placeholder: 'Filter...'
                        },
                        filterHeaderTemplate: '<div class=\"ui-grid-filter-container\" ng-repeat=\"colFilter in col.filters\" > <input ng-enter=\"enter()\" style=\"background-color:#FFF\" type=\"text\" class=\"ui-grid-filter-input ui-grid-filter-input-{{$index}}\" ng-model=\"colFilter.term\" ng-attr-placeholder=\"{{colFilter.placeholder || \'\'}}\" aria-label=\"{{colFilter.ariaLabel || aria.defaultFilterLabel}}\" /></div>'
                    },
					{ displayName: 'DenyReason', field: 'DenyReason', headerCellFilter: 'translate', cellClass: 'request-deny-reason', headerCellClass: 'request-deny-reason-header', visible: false },
					{
					    displayName: 'Status', field: 'StatusText', headerCellFilter: 'translate', cellClass: 'request-status', headerCellClass: 'request-status-header', enableSorting: false,
					    filterHeaderTemplate: '<div class=\"ui-grid-filter-container\" ng-repeat=\"colFilter in col.filters\">'
							+ '<div isteven-multi-select input-model=\"grid.appScope.AllRequestStatuses\" output-model=\"grid.appScope.SelectedRequestStatuses\" '
							+ 'button-label=\"Name\" item-label=\"Name\" on-close=\"grid.appScope.statusFilterClose()\" '
							+ 'tick-property=\"Selected\" max-labels=\"1\" helper-elements=\"\"></div>'
							+ '</div>'
					},
					{ displayName: 'CreatedOn', field: 'FormatedCreatedTime()', headerCellFilter: 'translate', cellClass: 'request-created-time', headerCellClass: 'request-created-time-header' },
					{ displayName: 'Id', field: 'Id', headerCellFilter: 'translate', cellClass: 'request-id', visible: false, headerCellClass: 'request-id-header' },
					{ displayName: 'UpdatedOn', field: 'FormatedUpdatedTime()', headerCellFilter: 'translate', cellClass: 'request-updated-time', visible: false, headerCellClass: 'request-updated-time-header' }
                ],
                onRegisterApi: function (gridApi) {
                    vm.gridApi = gridApi;
                    gridApi.core.on.sortChanged($scope, function (grid, sortColumns) {
                        vm.sortingOrders = sortColumns.map(requestsDefinitions.translateSingleSortingOrder).filter(function (x) { return x !== null; });
                    });
                    gridApi.grid.clearAllFilters = clearAllFilters;
                    gridApi.selection.on.rowSelectionChanged($scope, onSelectionChanged);
                    gridApi.selection.on.rowSelectionChangedBatch($scope, onSelectionChanged);

                    var filterHandlingTimeout = null;
                    gridApi.core.on.filterChanged($scope, function () {
                        var grid = this.grid;

                        if (filterHandlingTimeout != null) {
                            $timeout.cancel(filterHandlingTimeout);
                        }

                        filterHandlingTimeout = $timeout(function () {
                            angular.forEach(grid.columns, function (column) {
                                var term = column.filters[0].term;
                                if (term != undefined) {
                                    requestFilterSvc.SetFilter(column.colDef.displayName, term.trim());
                                }
                            });

                            vm.filters = requestFilterSvc.Filters;
                        }, 500);
                    });
                }
            };

            var filteringEnabled = vm.filterToggleEnabled;
            options.enableFiltering = filteringEnabled;
            options.useExternalFiltering = filteringEnabled;
            angular.forEach(options.columnDefs, function (col) {
                var columnsWithFilterEnabled = ["Subject", "Message", "Type", "Status"];
                col.enableFiltering = filteringEnabled && columnsWithFilterEnabled.indexOf(col.displayName) > -1;
            });

            return options;
        }

        function onSelectionChanged() {
            var visibleRequestsIds = vm.gridOptions.data.map(function (row) { return row.Id; });
            var visibleSelectedRequestsIds = vm.gridApi.selection.getSelectedRows().map(function (row) { return row.Id; });
            var visibleNotSelectedRequestsIds = visibleRequestsIds.filter(function (id) {
                return visibleSelectedRequestsIds.indexOf(id) < 0;
            });

            var allSelectedRequestsIds = requestCommandParamsHolder.getSelectedRequestsIds();
            var newAllSelectedRequestsId = [];

            angular.forEach(allSelectedRequestsIds, function (id) {
                if (visibleNotSelectedRequestsIds.indexOf(id) < 0)
                    newAllSelectedRequestsId.push(id);
            });

            angular.forEach(visibleSelectedRequestsIds, function (id) {
                if (newAllSelectedRequestsId.indexOf(id) < 0)
                    newAllSelectedRequestsId.push(id);
            });

            requestCommandParamsHolder.setSelectedRequestIds(newAllSelectedRequestsId);

            if (vm.requests && (vm.requests.length === visibleSelectedRequestsIds.length) && vm.requests.length > 0) {
                vm.gridApi.grid.selection.selectAll = true;
            } else {
                vm.gridApi.grid.selection.selectAll = false;
            }
        }

        function getVisibleSelectedRequestsRows() {
            if (!vm.gridOptions.data) return [];
            var allSelectedRequestsIds = requestCommandParamsHolder.getSelectedRequestsIds();
            return vm.gridOptions.data.filter(function (row) {
                return allSelectedRequestsIds.indexOf(row.Id) > -1;
            });
        }

        function formatToTimespan(length, isFullDay) {
            var days = moment.duration(length, 'seconds')._data.days;
            var hours = moment.duration(length, 'seconds')._data.hours;
            var minutes = moment.duration(length, 'seconds')._data.minutes == 0 ? '00' : moment.duration(length, 'seconds')._data.minutes;
            var totalHours = hours + days * 24 == 0 ? '00' : hours + days * 24;
            if (isFullDay) return totalHours + 1 + ":" + "00";
            else return totalHours + ":" + minutes;
        }

        function prepareComputedColumns(requests) {
            vm.userTimeZone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;

            function formatedDateTime(dateTime, timezone, displayDateOnly) {
                var angularTimezone = moment.tz(vm.isUsingRequestSubmitterTimeZone == true ? timezone : vm.userTimeZone).format("Z");
                angularTimezone = angularTimezone.replace(":", "");
                var _dateTime = moment.tz(dateTime, timezone).toDate();
                if (displayDateOnly && vm.isUsingRequestSubmitterTimeZone) return $filter('date')(_dateTime, "shortDate", angularTimezone);
                else return $filter('date')(_dateTime, "short", angularTimezone);
            }

            angular.forEach(requests, function (row) {
                row.GetDuration = function () {
                    var length = moment(row.PeriodEndTime).diff(moment(row.PeriodStartTime), 'seconds');
                    return formatToTimespan(length, row.IsFullDay);
                };
                row.FormatedPeriodStartTime = function () { return formatedDateTime(row.PeriodStartTime, row.TimeZone, row.IsFullDay); };
                row.FormatedPeriodEndTime = function () { return formatedDateTime(row.PeriodEndTime, row.TimeZone, row.IsFullDay); };

                row.FormatedCreatedTime = function () { return formatedDateTime(row.CreatedTime, row.TimeZone, false); };
                row.FormatedUpdatedTime = function () { return formatedDateTime(row.UpdatedTime, row.TimeZone, false); };

                row.GetType = function () {
                    var typeText = row.TypeText;
                    if (row.Type == requestsDefinitions.REQUEST_TYPES.ABSENCE) {
                        typeText += ' (' + row.Payload.Name + ')';
                    }
                    return typeText;
                }
            });

            return requests;
        }

        function clearSelection() {
            if (vm.gridApi.clearSelectedRows) {
                vm.gridApi.clearSelectedRows();
            }
            vm.gridApi.grid.selection.selectAll = false;
            requestCommandParamsHolder.resetSelectedRequestIds();
            vm.gridApi.selection.clearSelectedRows();

        }

        function reselectRequests() {
            if (!vm.gridOptions.data) return;

            var rows = getVisibleSelectedRequestsRows();
            vm.gridApi.grid.modifyRows(vm.gridOptions.data);
            angular.forEach(rows, function (row) {
                vm.gridApi.selection.selectRow(row);
            });

            vm.gridApi.grid.selection.selectAll =
				vm.requests && vm.requests.length > 0 && vm.requests.length === rows.length;
        }
    }

    function requestsTableContainerDirective() {
        return {
            controller: 'requestsTableContainerCtrl',
            controllerAs: 'requestsTableContainer',
            bindToController: true,
            restrict: 'E',
            scope: {
                requests: '=',
                sortingOrders: '=',
                filters: '=?'
            },
            require: ['requestsTableContainer'],
            templateUrl: 'js/requests/html/requests-table-container.tpl.html',
            link: postlink
        };

        function postlink(scope, elem, attrs, ctrls) {
            var requestsTableContainerCtrl = ctrls[0];
            scope.requestsTableContainer.gridOptions = requestsTableContainerCtrl.getGridOptions([]);
            scope.requestsTableContainer.isUsingRequestSubmitterTimeZone = true;

            scope.$watch(function () {
                return scope.requestsTableContainer.requests;
            }, function (v) {
                scope.requestsTableContainer.gridOptions.data = requestsTableContainerCtrl.prepareComputedColumns(v);
                requestsTableContainerCtrl.reselectRequests();
            }, true);

            scope.$on('reload.requests.without.selection', function () {
                requestsTableContainerCtrl.clearSelection();
            });

            scope.$on('reload.requests.with.selection', function () {
                requestsTableContainerCtrl.reselectRequests();
            });
        }
    }
})();
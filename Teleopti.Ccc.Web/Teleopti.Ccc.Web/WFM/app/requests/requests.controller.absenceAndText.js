(function () {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsAbsenceAndTextCtrl', requestsAbsenceAndTextCtrl);

	requestsAbsenceAndTextCtrl.$inject = ['$scope', '$filter', '$injector', '$translate', '$timeout', '$stateParams', 'requestsDataService', 'Toggle', 'requestsNotificationService', 'uiGridConstants', 'requestsDefinitions', 'requestCommandParamsHolder', 'CurrentUserInfo', 'RequestsFilter', 'RequestGridStateService', 'TextAndAbsenceGridConfiguration'];

	function requestsAbsenceAndTextCtrl($scope, $filter, $injector, $translate, $timeout, $stateParams, requestsDataService, toggleService, requestsNotificationService, uiGridConstants, requestsDefinitions, requestCommandParamsHolder, CurrentUserInfo, requestFilterSvc, requestGridStateService, textAndAbsenceGridConfigurationService) {
		var vm = this;

		vm.requests = [];
		vm.period = {};

		vm.filters = [];
		vm.isLoading = false;
		vm.sortingOrders = [];
		vm.agentSearchTerm = '';
		vm.selectedTeamIds = [];
		vm.paging = {};
		vm.initialized = false;
		vm.onInitCallBack = undefined;
		vm.requestFiltersMgr = new requestFilterSvc.RequestsFilter();

		var columnsWithFilterEnabled = ['Subject', 'Message', 'Type', 'Status'];

		vm.setDefaultStatuses = function () {
			if (!vm.showRequestsInDefaultStatus || vm.defaultStatusesLoaded) {
				return;
			}

			if (vm.filters && vm.filters.length > 0) {
				vm.SelectedRequestStatuses = [];
				var defaultStatusFilter = vm.filters[0].Status;
				vm.requestFiltersMgr.SetFilter('status', defaultStatusFilter);
				angular.forEach(defaultStatusFilter.split(' '), function (value) {
					if (value.trim() !== '') {
						vm.SelectedRequestStatuses.push({ Id: value.trim() });
					}
				});

				vm.defaultStatusesLoaded = true;
			}
		}

		vm.subjectFilterChanged = function () {
			vm.requestFiltersMgr.SetFilter('Subject', vm.subjectFilter);
			vm.filters = vm.requestFiltersMgr.Filters;
		}

		vm.messageFilterChanged = function () {
			vm.requestFiltersMgr.SetFilter('Message', vm.messageFilter);
			vm.filters = vm.requestFiltersMgr.Filters;
		}

		vm.statusFilterClose = function () {

			var filters = '';
			angular.forEach(vm.SelectedRequestStatuses, function (status) {
				filters += status.Id + ' ';
			});
			vm.requestFiltersMgr.SetFilter('Status', filters.trim());

			vm.filters = vm.requestFiltersMgr.Filters;

		};

		vm.typeFilterClose = function () {
			var filters = '';
			angular.forEach(vm.SelectedTypes, function (absence) {
				filters += absence.Id + ' ';
			});
			vm.requestFiltersMgr.SetFilter('Type', filters.trim());

			vm.filters = vm.requestFiltersMgr.Filters;
		};

		vm.reload = function (params) {
			if (params) {
				vm.agentSearchTerm = params.agentSearchTerm || vm.agentSearchTerm;
				vm.selectedTeamIds = params.selectedTeamIds || vm.selectedTeamIds;
				vm.paging = params.paging || vm.paging;
				vm.period = params.period || vm.period;
			}

			var requestsFilter = {
				period: vm.period,
				agentSearchTerm: vm.agentSearchTerm,
				selectedTeamIds: vm.selectedTeamIds,
				filters: vm.filters
			};

			getRequests(requestsFilter, vm.sortingOrders, vm.paging);
		};

		vm.init = function () {
			vm.isUsingRequestSubmitterTimeZone = $stateParams.isUsingRequestSubmitterTimeZone;
			vm.onInitCallBack = $stateParams.onInitCallBack;
			vm.userTimeZone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
			vm.gridOptions = getGridOptions();
			vm.saveGridColumnState = toggleService.Wfm_Requests_Save_Grid_Columns_37976;
			vm.requestsPromise = requestsDataService.getAllRequestsPromise;
			if (toggleService.Wfm_Requests_Default_Status_Filter_39472) {
				vm.filters = [{ 'Status': '0 5' }];
			}
			vm.initialized = true;
			vm.filterEnabled = $stateParams.filterEnabled;
			getRequestTypes();

			vm.AllRequestStatuses = requestsDataService.getAllRequestStatuses(false);
			vm.reload($stateParams);
		};

		$scope.$on('reload.requests.with.selection', function (event, data) {
			vm.initialized && vm.reload(data);
		});

		$scope.$on('requests.filterEnabled.changed', function (event, data) {
			vm.filterEnabled = data;
			vm.gridOptions.enableFiltering = vm.filterEnabled;
			vm.gridOptions.useExternalFiltering = vm.filterEnabled;
			applyColumnFilters(vm.gridOptions.columnDefs);
			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
		});

		$scope.$on('requests.isUsingRequestSubmitterTimeZone.changed', function (event, data) {
			vm.isUsingRequestSubmitterTimeZone = data;
			prepareComputedColumns(vm.requests);
		});

		$scope.$watchCollection(function () {
			return vm.filters;
		}, function () {
			vm.initialized && vm.reload();
		});

		toggleService.togglesLoaded.then(vm.init);

		function getRequestTypes() {
			requestsDataService.getRequestTypes().then(function (result) {
				vm.AllRequestTypes = result.data;
				angular.forEach(vm.AllRequestTypes, function (type) {
					type.Selected = false;
				});
			});
		}

		function initialiseGridStateHandling() {
			requestGridStateService.restoreState(vm);

			// delay the setup of these handlers a little to let the table load
			$timeout(function () {
				requestGridStateService.setupGridEventHandlers($scope, vm);
			}, 500);
		}

		function formatedDateTime(dateTime, timezone, displayDateOnly) {
			var angularTimezone = moment.tz(dateTime, vm.isUsingRequestSubmitterTimeZone ? timezone : vm.userTimeZone).format('Z');
			angularTimezone = angularTimezone.replace(':', '');
			var _dateTime = moment.tz(dateTime, timezone).toDate();
			if (displayDateOnly && vm.isUsingRequestSubmitterTimeZone) return $filter('date')(_dateTime, 'shortDate', angularTimezone);
			else return $filter('date')(_dateTime, 'short', angularTimezone);
		}

		function getGridOptions() {
			var options = {
				appScopeProvider: vm,
				enableGridMenu: true,
				enableFullRowSelection: true,
				enableRowHeaderSelection: true,
				enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
				enableVerticalScrollbar: uiGridConstants.scrollbars.WHEN_NEEDED,
				useExternalSorting: true,
				headerTemplate: 'shift-trade-header-template.html',
				gridMenuTitleFilter: $translate,
				columnVirtualizationThreshold: 200,
				rowHeight: 35,

				onRegisterApi: function (gridApi) {

					vm.gridApi = gridApi;
					gridApi.core.on.sortChanged($scope, function (grid, sortColumns) {
						vm.sortingOrders = sortColumns.map(requestsDefinitions.translateSingleSortingOrder)
							.filter(function (x) { return x !== null; });
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
									vm.requestFiltersMgr.SetFilter(column.colDef.displayName, term.trim());
								}
							});

							vm.filters = vm.requestFiltersMgr.Filters;
						}, 500);
					});
				}
			};

			return options;
		}

		function clearAllFilters() {
			angular.forEach(vm.gridApi.grid.columns, function (column) {
				column.filters[0].term = undefined;
			});

			angular.forEach(vm.AllRequestableAbsences, function (absence) {
				absence.Selected = false;
			});
			vm.SelectedTypes = [];

			angular.forEach(vm.AllRequestStatuses, function (status) {
				status.Selected = false;
			});
			vm.SelectedRequestStatuses = [];

			vm.requestFiltersMgr.ResetFilter();
			vm.filters = vm.requestFiltersMgr.Filters;
			vm.subjectFilter = undefined;
			vm.messageFilter = undefined;
		};

		function onSelectionChanged() {
			var visibleRequestsIds = vm.gridOptions.data.map(function (row) { return row.Id; });
			var visibleSelectedRequestsIds = vm.gridApi.selection.getSelectedRows().map(function (row) { return row.Id; });
			if (visibleSelectedRequestsIds.length === 1) {
				var message = vm.gridApi.selection.getSelectedRows().map(function (row) { return row.Message; });
				requestCommandParamsHolder.setSelectedIdAndMessage(visibleSelectedRequestsIds, message);
			}

			var visibleNotSelectedRequestsIds = visibleRequestsIds.filter(function (id) {
				return visibleSelectedRequestsIds.indexOf(id) < 0;
			});

			var allSelectedRequestsIds = requestCommandParamsHolder.getSelectedRequestsIds(false);
			var newAllSelectedRequestsId = [];

			angular.forEach(allSelectedRequestsIds, function (id) {
				if (visibleNotSelectedRequestsIds.indexOf(id) < 0)
					newAllSelectedRequestsId.push(id);
			});

			angular.forEach(visibleSelectedRequestsIds, function (id) {
				if (newAllSelectedRequestsId.indexOf(id) < 0)
					newAllSelectedRequestsId.push(id);
			});

			requestCommandParamsHolder.setSelectedRequestIds(newAllSelectedRequestsId, false);

			if (vm.requests && (vm.requests.length === visibleSelectedRequestsIds.length) && vm.requests.length > 0) {
				vm.gridApi.grid.selection.selectAll = true;
			} else {
				vm.gridApi.grid.selection.selectAll = false;
			}
		}

		function getRequests(requestsFilter, sortingOrders, paging) {
			vm.isLoading = true;
			vm.requestsPromise(requestsFilter, sortingOrders, paging).then(function (requests) {
				if (!requests) {
					vm.isLoading = false;
					return;
				}

				vm.requests = requests.data.Requests;
				prepareComputedColumns(vm.requests);

				if (requests.data.IsSearchPersonCountExceeded) {
					vm.requests = [];
					requestsNotificationService.notifyMaxSearchPersonCountExceeded(requests.data.MaxSearchPersonCount);
				}
				vm.onInitCallBack && vm.onInitCallBack(requests.data.TotalCount);
				vm.isLoading = false;
			});
		}

		function prepareComputedColumns(requests) {
			angular.forEach(requests, function (row) {
				row.GetDuration = function () {
					var length = moment(row.PeriodEndTime).diff(moment(row.PeriodStartTime), 'seconds');
					return formatToTimespan(length, row.IsFullDay);
				};
				row.FormatedPeriodStartTime = function () {
					return formatedDateTime(row.PeriodStartTime, row.TimeZone, row.IsFullDay);
				};
				row.FormatedPeriodEndTime = function () {
					return formatedDateTime(row.PeriodEndTime, row.TimeZone, row.IsFullDay);
				};

				row.FormatedCreatedTime = function () {
					return formatedDateTime(row.CreatedTime, row.TimeZone, false);
				};

				row.FormatedUpdatedTime = function () {
					return formatedDateTime(row.UpdatedTime, row.TimeZone, false);
				};

				row.GetType = function () {
					var typeText = row.TypeText;
					if (row.Type == requestsDefinitions.REQUEST_TYPES.ABSENCE) {
						typeText += ' (' + row.Payload.Name + ')';
					}
					return typeText;
				};

				row.GetBrokenRules = function () {
					var translatedBrokenRules = new Array();
					var brokenRules = row.BrokenRules;
					if (brokenRules) {
						angular.forEach(brokenRules,
							function (value) {
								translatedBrokenRules.push($translate.instant(value));
							});
						return translatedBrokenRules.join(', ');
					}
					return brokenRules;
				};
			});

			setupColumnDefinitions();

			if (vm.saveGridColumnState) {
				initialiseGridStateHandling();
				vm.setDefaultStatuses();
			}

			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
			vm.gridOptions.data = vm.requests;
		}

		function setupColumnDefinitions() {
			vm.gridOptions.columnDefs = textAndAbsenceGridConfigurationService.columnDefinitions();
			applyColumnFilters(vm.gridOptions.columnDefs, columnsWithFilterEnabled);

			vm.gridOptions.enableFiltering = vm.filterEnabled;
			vm.gridOptions.useExternalFiltering = vm.filterEnabled;
			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
		}

		function applyColumnFilters(columnDefs, columnsWithFilterEnabled) {
			angular.forEach(columnDefs, function (col) {
				col.enableFiltering = vm.filterEnabled && columnsWithFilterEnabled.indexOf(col.displayName) > -1;
			});
		}

		function formatToTimespan(lengthInSecond, isFullDay) {
			var durationData = moment.duration(lengthInSecond, "seconds")._data;
			var days = durationData.days;
			var hours = durationData.hours;
			var minutes = durationData.minutes === 0 ? "00" : durationData.minutes;
			var totalHours = hours + days * 24 === 0 ? "00" : hours + days * 24;
			return isFullDay
				? totalHours + 1 + ":" + "00"
				: totalHours + ":" + minutes;
		}
	}
})();

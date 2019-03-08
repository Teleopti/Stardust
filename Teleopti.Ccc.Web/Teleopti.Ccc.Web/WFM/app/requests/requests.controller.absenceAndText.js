(function() {
	'use strict';

	angular.module('wfm.requests').controller('requestsAbsenceAndTextController', requestsAbsenceAndTextController);

	requestsAbsenceAndTextController.$inject = [
		'$scope',
		'$translate',
		'$timeout',
		'$stateParams',
		'requestsDataService',
		'Toggle',
		'requestsNotificationService',
		'uiGridConstants',
		'requestsDefinitions',
		'CurrentUserInfo',
		'RequestsFilter',
		'RequestGridStateService',
		'TextAndAbsenceGridConfiguration',
		'UIGridUtilitiesService',
		'REQUESTS_TAB_NAMES',
		'REQUESTS_TYPES',
		'REQUESTS_STATUS',
		'requestCommandParamsHolder',
		'uiGridFixService',
		'requestScheduleService'
	];

	function requestsAbsenceAndTextController(
		$scope,
		$translate,
		$timeout,
		$stateParams,
		requestsDataService,
		toggleService,
		requestsNotificationService,
		uiGridConstants,
		requestsDefinitions,
		currentUserInfo,
		requestFilterSvc,
		requestGridStateService,
		textAndAbsenceGridConfigurationService,
		uiGridUtilitiesService,
		requestsTabNames,
		requestsTypes,
		requestsStatus,
		requestCommandParamsHolder,
		uiGridFixService,
		requestScheduleService
	) {
		var vm = this;

		vm.requests = [];
		vm.period = {};
		vm.filters = [];
		vm.selectedTypes = [];
		vm.subjectFilter = undefined;
		vm.messageFilter = undefined;
		vm.isLoading = false;
		vm.sortingOrders = [];
		vm.agentSearchTerm = '';
		vm.selectedGroupIds = [];
		vm.selectedGroupPageId = undefined;
		vm.paging = {};
		vm.initialized = false;
		vm.absence = {};
		vm.absenceRequestType = requestsTypes.AbsenceRequest;
		vm.enabledRequestStausesForShowingScheduleIcon = [requestsStatus.Pending, requestsStatus.Waitlisted];
		vm.showingAbsenceSchedules = false;

		var onInitCallBack = undefined;

		vm.setDefaultStatuses = function() {
			if (vm.defaultStatusesLoaded) return;

			vm.selectedRequestStatuses = uiGridUtilitiesService.getDefaultStatus(
				vm.filters,
				requestsTabNames.absenceAndText
			);
			vm.defaultStatusesLoaded = true;
		};

		vm.setSelectedTypes = function() {
			if (!vm.filters || vm.filters.length == 0) return;

			var typeElement = vm.filters.filter(function(f) {
				return Object.keys(f)[0] == 'Type';
			})[0];
			if (!typeElement) return;

			requestFilterSvc.setFilter('Type', typeElement.Type, requestsTabNames.absenceAndText);
			angular.forEach(typeElement.Type.split(' '), function(value) {
				if (value.trim() !== '') {
					vm.selectedTypes.push({
						Id: value.trim()
					});
				}
			});
		};

		vm.subjectFilterChanged = function() {
			requestFilterSvc.setFilter('Subject', vm.subjectFilter, requestsTabNames.absenceAndText);
			vm.filters = requestFilterSvc.filters[requestsTabNames.absenceAndText];
		};

		vm.messageFilterChanged = function() {
			requestFilterSvc.setFilter('Message', vm.messageFilter, requestsTabNames.absenceAndText);
			vm.filters = requestFilterSvc.filters[requestsTabNames.absenceAndText];
		};

		vm.statusFilterClose = function() {
			setFilters(vm.selectedRequestStatuses, 'Status');
		};

		vm.typeFilterClose = function() {
			setFilters(vm.selectedTypes, 'Type');
		};

		vm.reload = function(params) {
			if (params) {
				if (angular.isDefined(params.agentSearchTerm)) vm.agentSearchTerm = params.agentSearchTerm;
				if (angular.isDefined(params.selectedGroupIds)) vm.selectedGroupIds = params.selectedGroupIds;
				if (angular.isDefined(params.selectedGroupPageId)) vm.selectedGroupPageId = params.selectedGroupPageId;
				if (angular.isDefined(params.paging)) vm.paging = params.paging;
			}

			var requestsFilter = {
				period: vm.period,
				agentSearchTerm: vm.agentSearchTerm,
				selectedGroupIds: vm.selectedGroupIds,
				selectedGroupPageId: vm.selectedGroupPageId,
				filters: vm.filters
			};
			getRequests(requestsFilter, vm.sortingOrders, vm.paging);
		};

		vm.clearAllFilters = function() {
			angular.forEach(vm.gridApi.grid.columns, function(column) {
				column.filters[0].term = undefined;
			});

			angular.forEach(vm.AllRequestableAbsences, function(absence) {
				absence.Selected = false;
			});
			vm.selectedTypes = [];

			angular.forEach(vm.allRequestStatuses, function(status) {
				status.Selected = false;
			});
			vm.selectedRequestStatuses = [];

			requestFilterSvc.resetFilter(requestsTabNames.absenceAndText);
			vm.filters = requestFilterSvc.filters[requestsTabNames.absenceAndText];
			vm.subjectFilter = undefined;
			vm.messageFilter = undefined;
		};

		vm.toggleAbsenceSchedules = function(absence, $event) {
			$event.preventDefault();
			$event.stopPropagation();

			if (!absence || !$event) return;

			if (vm.absence && absence.Id === vm.showingScheduleAbsenceId) {
				vm.showingAbsenceSchedules = !vm.showingAbsenceSchedules;
				absence.activeShowSchedule = !absence.activeShowSchedule;
			} else {
				vm.showingAbsenceSchedules = true;
				absence.activeShowSchedule = true;
			}

			deactiveOtherShowScheduleIcons(absence.Id);

			vm.showingScheduleAbsenceId = absence.Id;
			vm.showingScheduleAgentName = absence.AgentName;

			var shifts = [];

			absence.Shifts.forEach(function(s) {
				var currentTimeZone = vm.userTimeZone;
				var targetTimeZone = vm.isUsingRequestSubmitterTimeZone ? absence.TimeZone : vm.userTimeZone;

				shifts.push(requestScheduleService.buildShiftData(s, currentTimeZone, targetTimeZone));
			});

			vm.shifts = shifts;
			vm.schedulesContainerStyle = requestScheduleService.buildScheduleContainerStyle(vm.shifts.length, $event);
		};

		vm.hideAbsenceSchedules = function() {
			vm.showingAbsenceSchedules = false;
			deactiveAllShowScheduleIcons();
		};

		vm.init = function() {
			vm.defaultStatusesLoaded = false;
			vm.userTimeZone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;

			var sortingOrder = requestsDefinitions.translateSingleSortingOrder(
				requestGridStateService.getAbsenceAndTextSorting()
			);
			if (sortingOrder) vm.sortingOrders.push(sortingOrder);

			vm.gridOptions = getGridOptions();
			vm.allRequestStatuses = requestsDataService.getAbsenceAndTextRequestsStatuses();

			var params = $stateParams.getParams && $stateParams.getParams();
			if (!params) return;

			vm.filterEnabled = params.filterEnabled;
			onInitCallBack = params.onInitCallBack;
			vm.isUsingRequestSubmitterTimeZone = params.isUsingRequestSubmitterTimeZone;

			if (requestFilterSvc.filters[requestsTabNames.absenceAndText]) {
				vm.filters = requestFilterSvc.filters[requestsTabNames.absenceAndText];

				var subjectFilter = vm.filters.filter(function(f) {
					return Object.keys(f)[0] == 'Subject';
				})[0];
				var messageFilter = vm.filters.filter(function(f) {
					return Object.keys(f)[0] == 'Message';
				})[0];

				if (subjectFilter) vm.subjectFilter = subjectFilter['Subject'];
				if (messageFilter) vm.messageFilter = messageFilter['Message'];
			} else {
				vm.filters = [{ Status: requestsStatus.Pending + ' ' + requestsStatus.Waitlisted }];
			}

			getRequestTypes();
			applyGridColumns();

			vm.initialized = true;
			setupWatch();
		};

		toggleService.togglesLoaded.then(vm.init);

		$scope.$on('reload.requests.with.selection', function(event, data) {
			vm.initialized && vm.reload(data);
		});

		$scope.$on('reload.requests.without.selection', function() {
			clearSelection();
			vm.initialized && vm.reload();
		});

		$scope.$on('requests.filterEnabled.changed', function(event, data) {
			if (!vm.initialized) return;

			vm.filterEnabled = data;
			vm.gridOptions.enableFiltering = vm.filterEnabled;
			vm.gridOptions.useExternalFiltering = vm.filterEnabled;

			if (vm.gridApi && vm.gridApi.core) vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
		});

		$scope.$on('requests.isUsingRequestSubmitterTimeZone.changed', function(event, data) {
			vm.isUsingRequestSubmitterTimeZone = data;
			prepareComputedColumns(vm.requests);
		});

		function setFilters(filtersList, displayName) {
			var filters = '';
			angular.forEach(filtersList, function(filter) {
				filters += filter.Id + ' ';
			});
			requestFilterSvc.setFilter(displayName, filters.trim(), requestsTabNames.absenceAndText);
			vm.filters = requestFilterSvc.filters[requestsTabNames.absenceAndText];
		}

		function setupWatch() {
			$scope.$watch(
				function() {
					return {
						period: $stateParams.getParams().getPeriod(),
						filters: vm.filters,
						sortOrder: vm.sortingOrders
					};
				},
				function(newVal) {
					if (!newVal || !vm.initialized) return;
					if (newVal.period && validateDateParameters(newVal.period.startDate, newVal.period.endDate)) {
						vm.period = newVal.period;
						vm.reload($stateParams.getParams());
					}
				},
				true
			);
		}

		function getRequestTypes() {
			requestsDataService.getRequestTypes().then(function(result) {
				vm.AllRequestTypes = result.data;

				angular.forEach(vm.AllRequestTypes, function(type) {
					type.Selected = false;
				});
				vm.setSelectedTypes();
			});
		}

		function initialiseGridStateHandling() {
			vm.shiftTradeView = false;

			$timeout(function() {
				requestGridStateService.restoreState(vm, requestsDefinitions.REQUEST_TYPES.ABSENCE);
			}, 0);

			// delay the setup of these handlers a little to let the table load
			$timeout(function() {
				requestGridStateService.setupGridEventHandlers($scope, vm, requestsDefinitions.REQUEST_TYPES.ABSENCE);
			}, 500);
		}

		function validateDateParameters(startDate, endDate) {
			if (endDate === null || startDate === null) return false;
			return (
				!moment(endDate).isBefore(startDate, 'day') &&
				moment(startDate).year() > 1969 &&
				moment(endDate).year() > 1969
			);
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
				headerTemplate: 'app/requests/html/requests-absence-and-text-header.html',
				gridMenuTitleFilter: $translate,
				columnVirtualizationThreshold: 200,
				rowHeight: 30,
				saveSelection: false,

				onRegisterApi: function(gridApi) {
					vm.gridApi = gridApi;
					gridApi.core.on.sortChanged($scope, function(grid, sortColumns) {
						vm.sortingOrders = sortColumns
							.map(requestsDefinitions.translateSingleSortingOrder)
							.filter(function(x) {
								return x !== null;
							});
					});
					gridApi.grid.clearAllFilters = vm.clearAllFilters;
					gridApi.selection.on.rowSelectionChanged($scope, onSelectionChanged);
					gridApi.selection.on.rowSelectionChangedBatch($scope, onSelectionChanged);

					var filterHandlingTimeout = null;
					gridApi.core.on.filterChanged($scope, function() {
						var grid = this.grid;

						if (filterHandlingTimeout != null) {
							$timeout.cancel(filterHandlingTimeout);
						}

						filterHandlingTimeout = $timeout(function() {
							angular.forEach(grid.columns, function(column) {
								var term = column.filters[0].term;
								if (angular.isDefined(term)) {
									requestFilterSvc.setFilter(
										column.colDef.displayName,
										term.trim(),
										requestsTabNames.absenceAndText
									);
								}
							});
							vm.filters = requestFilterSvc.filters[requestsTabNames.absenceAndText];
						}, 500);
					});
					$timeout(function() {
						uiGridFixService.fixColumneMenuToggling();
					}, 500);
				}
			};

			return options;
		}

		function applyGridColumns() {
			vm.gridOptions.columnDefs = textAndAbsenceGridConfigurationService.columnDefinitions();
		}

		function onSelectionChanged() {
			var visibleRequestsIds = vm.gridOptions.data.map(function(row) {
				return row.Id;
			});
			var visibleSelectedRequestsIds = vm.gridApi.selection.getSelectedRows().map(function(row) {
				return row.Id;
			});
			var messages = vm.gridApi.selection.getSelectedRows().map(function(row) {
				return row.Message;
			});
			uiGridUtilitiesService.setAbsenceAndTextSelectedRequestIds(
				visibleSelectedRequestsIds,
				visibleRequestsIds,
				messages
			);
			vm.gridApi.grid.selection.selectAll =
				vm.requests && vm.requests.length === visibleSelectedRequestsIds.length && vm.requests.length > 0;
		}

		function getRequests(requestsFilter, sortingOrders, paging) {
			if (requestsFilter.selectedGroupIds.length === 0) {
				getRequestsCallback({
					data: {
						Requests: []
					}
				});
			} else {
				vm.isLoading = true;
				requestsDataService
					.getAbsenceAndTextRequestsPromise(requestsFilter, sortingOrders, paging)
					.then(getRequestsCallback);
			}
		}

		function getRequestsCallback(requests) {
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
			onInitCallBack && onInitCallBack(requests.data.TotalCount);

			if (vm.gridApi) vm.gridApi.grid.selection.selectAll = false;

			vm.isLoading = false;
			$timeout(function() {
				reselectRequests();
			}, 0);
		}

		function reselectRequests() {
			if (!vm.gridOptions.data) return;

			var rows = getVisibleSelectedRequestsRows();

			vm.gridApi.grid.modifyRows(vm.gridOptions.data);
			angular.forEach(rows, function(row) {
				vm.gridApi.selection.selectRow(row);
			});

			vm.gridApi.grid.selection.selectAll =
				vm.requests && vm.requests.length > 0 && vm.requests.length === rows.length;
		}

		function getVisibleSelectedRequestsRows() {
			if (!vm.gridOptions.data) return [];

			var allSelectedRequestsIds = requestCommandParamsHolder.getSelectedRequestsIds(false);
			if (!allSelectedRequestsIds) return [];

			return vm.gridOptions.data.filter(function(row) {
				return allSelectedRequestsIds.indexOf(row.Id) > -1;
			});
		}

		function prepareComputedColumns(requests) {
			uiGridUtilitiesService.prepareComputedColumns(
				requests,
				vm.userTimeZone,
				vm.isUsingRequestSubmitterTimeZone
			);

			vm.gridOptions.enableFiltering = vm.filterEnabled;
			vm.gridOptions.useExternalFiltering = vm.filterEnabled;

			if (vm.gridApi && vm.gridApi.core) vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);

			initialiseGridStateHandling();
			vm.setDefaultStatuses();
			if (vm.gridApi && vm.gridApi.core) vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
			vm.gridOptions.data = vm.requests;
		}

		function clearSelection() {
			if (vm.gridApi) {
				if (vm.gridApi.clearSelectedRows) {
					vm.gridApi.clearSelectedRows();
				}
				vm.gridApi.grid.selection.selectAll = false;
				vm.gridApi.selection.clearSelectedRows();
			}
			requestCommandParamsHolder.resetSelectedRequestIds(false);
		}

		function deactiveOtherShowScheduleIcons(absenceId) {
			if (
				!vm.gridOptions ||
				!vm.gridOptions.data ||
				Object.prototype.toString.call(vm.gridOptions.data) != '[object Array]'
			)
				return;

			vm.gridOptions.data.forEach(function(d) {
				if (d.Id !== absenceId) d.activeShowSchedule = false;
			});
		}

		function deactiveAllShowScheduleIcons() {
			if (
				!vm.gridOptions ||
				!vm.gridOptions.data ||
				Object.prototype.toString.call(vm.gridOptions.data) != '[object Array]'
			)
				return;

			vm.gridOptions.data.forEach(function(d) {
				d.activeShowSchedule = false;
			});
		}
	}
})();

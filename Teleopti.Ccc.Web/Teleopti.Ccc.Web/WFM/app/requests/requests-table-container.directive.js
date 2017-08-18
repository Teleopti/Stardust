'use strict';

(function () {
	angular.module('wfm.requests')
		.controller('requestsTableContainerCtrl', requestsTableContainerController)
		.directive('requestsTableContainer', requestsTableContainerDirective);

	requestsTableContainerController.$inject = ['$scope', '$translate', '$filter', '$timeout', 'Toggle', 'requestsDefinitions',
		'requestCommandParamsHolder', 'CurrentUserInfo', 'RequestsFilter', 'requestsDataService', 'uiGridConstants', '$injector',
		'TeamSchedule', 'GroupScheduleFactory', '$window', 'RequestGridStateService'];

	function requestsTableContainerController($scope, $translate, $filter, $timeout, toggleSvc, requestsDefinitions,
		requestCommandParamsHolder, CurrentUserInfo, requestFilterSvc, requestsDataSvc, uiGridConstants, $injector,
		teamScheduleSvc, groupScheduleFactory, $window, requestGridStateService) {
		var vm = this;

		vm.getGridOptions = getGridOptions;
		vm.prepareComputedColumns = prepareComputedColumns;
		vm.clearSelection = clearSelection;
		vm.reselectRequests = reselectRequests;
		vm.getShiftTradeHeaderClass = getShiftTradeHeaderClass;
		vm.thereIsRequest = thereIsRequest;
		vm.isDayOff = isDayOff;
		vm.shouldDisplayShiftTradeDayDetail = shouldDisplayShiftTradeDayDetail;
		vm.showRequestsInDefaultStatus = toggleSvc.Wfm_Requests_Default_Status_Filter_39472;
		vm.saveGridColumnState = toggleSvc.Wfm_Requests_Save_Grid_Columns_37976;
		vm.setFilterEnabled = setFilterEnabled;
		vm.hideShiftDetail = hideShiftDetail;
		vm.shiftDetailStyleJson = shiftDetailStyleJson;
		vm.requestFiltersMgr = new requestFilterSvc.RequestsFilter();
		vm.showShiftDetail = showShiftDetail;
		vm.setupShiftTradeVisualisation = setupShiftTradeVisualisation;

		vm.defaultStatusesLoaded = false;
		vm.definitionsLoadComplete = false;

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

		if (!vm.saveGridColumnState) {
			vm.setDefaultStatuses();
		}

		function showShiftDetail(params) {
			vm.schedules = params.schedules;
			vm.shiftDetailTop = params.top;
			vm.shiftDetailLeft = params.left;
			vm.displayShiftDetail = true;
		}

		function hideShiftDetail() {
			vm.displayShiftDetail = false;
		}

		function shiftDetailStyleJson() {
			return {
				top: vm.shiftDetailTop + 'px',
				left: vm.shiftDetailLeft + 'px',
				position: 'fixed'
			};
		}

		function isDayOff(scheduleDayDetail) {
			return (scheduleDayDetail && (scheduleDayDetail.Type === requestsDefinitions.SHIFT_OBJECT_TYPE.DayOff));
		}

		function shouldDisplayShiftTradeDayDetail(shiftTradeDayDetail) {
			return (shiftTradeDayDetail && (shiftTradeDayDetail.ShortName != null && shiftTradeDayDetail.ShortName.length !== 0));
		}

		function getShiftTradeHeaderClass() {
			return vm.shiftTradeView || vm.filterEnabled ? 'request-header-full-height' : '';
		}

		function applyColumnFilters(columnDefs) {
			angular.forEach(columnDefs, function (col) {
				var columnsWithFilterEnabled = ['Subject', 'Message', 'Type', 'Status'];
				col.enableFiltering = vm.filterEnabled && columnsWithFilterEnabled.indexOf(col.displayName) > -1;
			});
		}

		function setFilterEnabled(enabled) {
			vm.gridOptions.enableFiltering = enabled;
			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
		}

		function setupShiftTradeVisualisation(requests) {
			if (!vm.shiftTradeView || !vm.shiftTradeRequestDateSummary) return;

			vm.shiftTradeDayViewModels = vm.gridConfigurationService.getDayViewModels(requests,
				vm.shiftTradeRequestDateSummary, vm.isUsingRequestSubmitterTimeZone);

			vm.shiftTradeScheduleViewModels = vm.gridConfigurationService.getShiftTradeScheduleViewModels(requests,
				vm.shiftTradeRequestDateSummary, vm.isUsingRequestSubmitterTimeZone);
		}

		function setupColumnDefinitions(requests) {
			if (!vm.gridConfigurationService) {
				var configurationService = vm.shiftTradeView ? 'ShiftTradeGridConfiguration' : 'TextAndAbsenceGridConfiguration';
				vm.gridConfigurationService = $injector.get(configurationService);
			}

			setupShiftTradeVisualisation(requests);

			vm.gridOptions.columnDefs = vm.gridConfigurationService.columnDefinitions(vm.shiftTradeRequestDateSummary);
			vm.gridOptions.enablePinning = vm.shiftTradeView;

			applyColumnFilters(vm.gridOptions.columnDefs);

			var filteringEnabled = vm.filterEnabled;
			vm.gridOptions.enableFiltering = filteringEnabled;
			vm.gridOptions.useExternalFiltering = filteringEnabled;

			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL); // ROBTODO really needed?
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

		vm.clearFilters = clearAllFilters;

		if (!vm.shiftTradeView) {
			requestsDataSvc.getRequestTypes().then(function (result) {
				vm.AllRequestTypes = result.data;
				angular.forEach(vm.AllRequestTypes, function (type) {
					type.Selected = false;
				});
			});
		}

		vm.AllRequestStatuses = requestsDataSvc.getAllRequestStatuses(vm.shiftTradeView);

		vm.typeFilterClose = function () {
			var filters = '';
			angular.forEach(vm.SelectedTypes, function (absence) {
				filters += absence.Id + ' ';
			});
			vm.requestFiltersMgr.SetFilter('Type', filters.trim());

			vm.filters = vm.requestFiltersMgr.Filters;
		};

		vm.statusFilterClose = function () {

			var filters = '';
			angular.forEach(vm.SelectedRequestStatuses, function (status) {
				filters += status.Id + ' ';
			});
			vm.requestFiltersMgr.SetFilter('Status', filters.trim());

			vm.filters = vm.requestFiltersMgr.Filters;
		};

		vm.subjectFilterChanged = function () {
			vm.requestFiltersMgr.SetFilter('Subject', vm.subjectFilter);
			vm.filters = vm.requestFiltersMgr.Filters;
		}

		vm.messageFilterChanged = function () {
			vm.requestFiltersMgr.SetFilter('Message', vm.messageFilter);
			vm.filters = vm.requestFiltersMgr.Filters;
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
				rowHeight: vm.shiftTradeView ? 60 : 35,

				onRegisterApi: function (gridApi) {

					vm.gridApi = gridApi;
					gridApi.core.on.sortChanged($scope, function (grid, sortColumns) {
						vm.sortingOrders = sortColumns.map(requestsDefinitions.translateSingleSortingOrder)
							.filter(function(x) { return x !== null; });
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

			var allSelectedRequestsIds = requestCommandParamsHolder.getSelectedRequestsIds(vm.shiftTradeView);
			var newAllSelectedRequestsId = [];

			angular.forEach(allSelectedRequestsIds, function (id) {
				if (visibleNotSelectedRequestsIds.indexOf(id) < 0)
					newAllSelectedRequestsId.push(id);
			});

			angular.forEach(visibleSelectedRequestsIds, function (id) {
				if (newAllSelectedRequestsId.indexOf(id) < 0)
					newAllSelectedRequestsId.push(id);
			});

			requestCommandParamsHolder.setSelectedRequestIds(newAllSelectedRequestsId, vm.shiftTradeView);

			if (vm.requests && (vm.requests.length === visibleSelectedRequestsIds.length) && vm.requests.length > 0) {
				vm.gridApi.grid.selection.selectAll = true;
			} else {
				vm.gridApi.grid.selection.selectAll = false;
			}
		}

		function getVisibleSelectedRequestsRows() {
			if (!vm.gridOptions.data) return [];
			var allSelectedRequestsIds = requestCommandParamsHolder.getSelectedRequestsIds(vm.shiftTradeView);
			return vm.gridOptions.data.filter(function (row) {
				return allSelectedRequestsIds.indexOf(row.Id) > -1;
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

		function initialiseGridStateHandling() {
			requestGridStateService.restoreState(vm);
			
			// delay the setup of these handlers a little to let the table load
			$timeout(function () {
				requestGridStateService.setupGridEventHandlers($scope,vm);
			}, 500);
		}

		function prepareComputedColumns(requests) {
			vm.definitionsLoadComplete = false;

			vm.userTimeZone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;

			function formatedDateTime(dateTime, timezone, displayDateOnly) {
				var angularTimezone = moment.tz(dateTime, vm.isUsingRequestSubmitterTimeZone ? timezone : vm.userTimeZone).format('Z');
				angularTimezone = angularTimezone.replace(':', '');
				var _dateTime = moment.tz(dateTime, timezone).toDate();
				if (displayDateOnly && vm.isUsingRequestSubmitterTimeZone) return $filter('date')(_dateTime, 'shortDate', angularTimezone);
				else return $filter('date')(_dateTime, 'short', angularTimezone);
			}

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
				}

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
				}
			});

			setupColumnDefinitions(requests);

			vm.gridOptions.data = vm.requests;
			vm.definitionsLoadComplete = true;

			if (vm.saveGridColumnState) {
				initialiseGridStateHandling();
				
				vm.setDefaultStatuses();
			}

			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);

			return requests;
		}

		function thereIsRequest() {
			return vm.requests && vm.requests.length > 0;
		}

		function clearSelection() {

			if (vm.gridApi && vm.gridApi != null) {
				if (vm.gridApi.clearSelectedRows) {
					vm.gridApi.clearSelectedRows();
				}
				vm.gridApi.grid.selection.selectAll = false;
				vm.gridApi.selection.clearSelectedRows();
			}

			requestCommandParamsHolder.resetSelectedRequestIds(vm.shiftTradeView);
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
				shiftTradeRequestDateSummary: '=',
				sortingOrders: '=',
				filterEnabled: '=',
				filters: '=?',
				shiftTradeView: '=?',
				isUsingRequestSubmitterTimeZone: '=?'
			},
			require: ['requestsTableContainer'],
			templateUrl: 'app/requests/html/requests-table-container.tpl.html',
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrls) {
			var requestsTableContainerCtrl = ctrls[0];
			scope.requestsTableContainer.gridOptions = requestsTableContainerCtrl.getGridOptions([]);

			scope.$watch(function () {
				return {
					requests: scope.requestsTableContainer.requests,
					shiftTradeRequestDateSummary: scope.requestsTableContainer.shiftTradeRequestDateSummary
				}
			}, function (requestWatch) {
				var requests = requestWatch.requests;
				requestsTableContainerCtrl.prepareComputedColumns(requests);
				requestsTableContainerCtrl.reselectRequests();
			}, true);

			scope.$watch(function () {
				return {
					isUsingRequestSubmitterTimeZone: scope.requestsTableContainer.isUsingRequestSubmitterTimeZone
				}
			}, function () {
				var ctrl = requestsTableContainerCtrl;
				if (ctrl.shiftTradeView && ctrl.shiftTradeRequestDateSummary) {
					var requests = ctrl.requests;
					ctrl.setupShiftTradeVisualisation(requests);
				}

			}, true);

			scope.$on('reload.requests.without.selection', function () {
				requestsTableContainerCtrl.clearSelection();
			});

			scope.$on('reload.requests.with.selection', function () {
				requestsTableContainerCtrl.reselectRequests();
			});

			scope.$watch('requestsTableContainer.filterEnabled',
				function handleFilterEnabledChanged(newValue, oldValue) {
					requestsTableContainerCtrl.setFilterEnabled(newValue);
				});
		}
	}
})();

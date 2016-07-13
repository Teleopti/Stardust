'use strict';

(function() {

	angular.module('wfm.requests')
		.controller('requestsTableContainerCtrl', requestsTableContainerController)
		.directive('requestsTableContainer', requestsTableContainerDirective);

	requestsTableContainerController.$inject = ['$scope', '$translate', '$filter', '$timeout', 'Toggle', 'requestsDefinitions', 'requestCommandParamsHolder', 'CurrentUserInfo', 'RequestsFilter', 'requestsDataService', 'uiGridConstants', '$injector', 'TeamSchedule', 'GroupScheduleFactory'];

	function requestsTableContainerController($scope, $translate, $filter, $timeout, toggleSvc, requestsDefinitions, requestCommandParamsHolder, CurrentUserInfo, requestFilterSvc, requestsDataSvc, uiGridConstants, $injector, teamScheduleSvc, groupScheduleFactory) {
		var vm = this;

		vm.getGridOptions = getGridOptions;
		vm.prepareComputedColumns = prepareComputedColumns;
		vm.clearSelection = clearSelection;
		vm.reselectRequests = reselectRequests;
		vm.getShiftTradeHeaderClass = getShiftTradeHeaderClass;
		vm.thereIsRequest = thereIsRequest;
		vm.isDayOff = isDayOff;
		vm.shouldDisplayShiftTradeDayDetail = shouldDisplayShiftTradeDayDetail;
		vm.showRelevantInfo = toggleSvc.Wfm_Requests_ShiftTrade_More_Relevant_Information_38492;
		vm.showRequestsInDefaultStatus = toggleSvc.Wfm_Requests_Default_Status_Filter_39472;
		vm.setFilterEnabled = setFilterEnabled;
		vm.showShiftDetail = showShiftDetail;
		vm.hideShiftDetail = hideShiftDetail;
		vm.shiftDetailStyleJson = shiftDetailStyleJson;
		vm.shiftDetailLeft;
		vm.shiftDetailTop;
		vm.displayShiftDetail;

		vm.init = function() {
			if (!vm.showRequestsInDefaultStatus) return;
			if (vm.filters && vm.filters.length > 0) {
				vm.SelectedRequestStatuses = [];
				var defaultStatusFilter = vm.filters[0].Status;
				angular.forEach(defaultStatusFilter.split(' '), function(value) {
					if (value.trim() !== '') {
						vm.SelectedRequestStatuses.push({ Id: value.trim() });
					}
				});
			}
		}

		vm.init();

		function updateShiftStatusForSelectedPerson(scheduleDate) {
			var selectedPersonIdList = vm.personIds;
			if (selectedPersonIdList.length === 0) {
				return;
			}

			var currentDate = scheduleDate.format('YYYY-MM-DD');

			teamScheduleSvc.getSchedules(currentDate, selectedPersonIdList).then(function(result) {
				vm.schedules = groupScheduleFactory.Create(result.Schedules, scheduleDate);
				vm.displayShiftDetail = true;
			});
		}

		function showShiftDetail(oEvent, personFromId, personToId, scheduleDate) {
			if (!vm.showRelevantInfo) return;
			vm.personIds = [personFromId, personToId];
			vm.shiftDetailLeft = oEvent.pageX - 5;
			vm.shiftDetailTop = oEvent.pageY - 5;
			if ((document.body.clientWidth - oEvent.pageX) < 710) vm.shiftDetailLeft = document.body.clientWidth - 710;
			if ((document.body.clientHeight - oEvent.pageY) < 145) vm.shiftDetailTop = document.body.clientHeight - 145;
			updateShiftStatusForSelectedPerson(moment(scheduleDate));
		}

		function hideShiftDetail() {
			if (!vm.showRelevantInfo) return;
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
			angular.forEach(columnDefs, function(col) {
				var columnsWithFilterEnabled = ['Subject', 'Message', 'Type', 'Status'];
				col.enableFiltering = vm.filterEnabled && columnsWithFilterEnabled.indexOf(col.displayName) > -1;
			});
		}

		function setFilterEnabled(enabled) {
			vm.gridOptions.enableFiltering = enabled;
			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
		}

		function setupShiftTradeVisualisation(requests) {

			if (vm.shiftTradeView && vm.shiftTradeRequestDateSummary) {
				vm.shiftTradeDayViewModels = vm.gridConfigurationService.getDayViewModels(requests, vm.shiftTradeRequestDateSummary);
				vm.shiftTradeScheduleViewModels = vm.gridConfigurationService.getShiftTradeScheduleViewModels(requests, vm.shiftTradeRequestDateSummary);
			}
		}

		function setupColumnDefinitions(requests) {
			if (!vm.gridConfigurationService) {
				var configurationService = vm.shiftTradeView ? 'ShiftTradeGridConfiguration' : 'TextAndAbsenceGridConfiguration';
				vm.gridConfigurationService = $injector.get(configurationService);
			}
			
			setupShiftTradeVisualisation(requests);

			vm.gridOptions.columnDefs = vm.gridConfigurationService.columnDefinitions(vm.shiftTradeRequestDateSummary, requests);
			vm.gridOptions.enablePinning = vm.shiftTradeView;
			vm.gridOptions.enableVerticalScrollbar = 0;
			applyColumnFilters(vm.gridOptions.columnDefs);

			var filteringEnabled = vm.filterEnabled;
			vm.gridOptions.enableFiltering = filteringEnabled;
			vm.gridOptions.useExternalFiltering = filteringEnabled;

			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
		}

		function clearAllFilters() {
			angular.forEach(vm.gridApi.grid.columns, function(column) {
				column.filters[0].term = undefined;
			});

			angular.forEach(vm.AllRequestableAbsences, function(absence) {
				absence.Selected = false;
			});
			vm.SelectedAbsences = [];

			angular.forEach(vm.AllRequestStatuses, function(status) {
				status.Selected = false;
			});
			vm.SelectedRequestStatuses = [];

			requestFilterSvc.ResetFilter();
			vm.filters = requestFilterSvc.Filters;
		};

		vm.clearFilters = clearAllFilters;

		requestsDataSvc.getRequestableAbsences().then(function(result) {
			vm.AllRequestableAbsences = result.data;
			angular.forEach(vm.AllRequestableAbsences, function(absence) {
				absence.Selected = false;
			});
		});
		vm.AllRequestStatuses = requestsDataSvc.getAllRequestStatuses(vm.shiftTradeView);

		vm.absenceFilterClose = function() {
			var filters = '';
			angular.forEach(vm.SelectedAbsences, function(absence) {
				filters += absence.Id + ' ';
			});
			requestFilterSvc.SetFilter('Absence', filters.trim());

			vm.filters = requestFilterSvc.Filters;
		};

		vm.statusFilterClose = function() {
			var filters = '';
			angular.forEach(vm.SelectedRequestStatuses, function(status) {
				filters += status.Id + ' ';
			});
			requestFilterSvc.SetFilter('Status', filters.trim());

			vm.filters = requestFilterSvc.Filters;
		};

		function getGridOptions() {
			var options = {
				appScopeProvider: vm,
				enableGridMenu: true,
				enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
				enableVerticalScrollbar: uiGridConstants.scrollbars.NEVER,
				useExternalSorting: true,
				headerTemplate: 'shift-trade-header-template.html',
				gridMenuTitleFilter: $translate,
				columnVirtualizationThreshold: 200,
				rowHeight: vm.shiftTradeView ? 60 : 30,

				onRegisterApi: function(gridApi) {

					vm.gridApi = gridApi;
					gridApi.core.on.sortChanged($scope, function(grid, sortColumns) {
						vm.sortingOrders = sortColumns.map(requestsDefinitions.translateSingleSortingOrder).filter(function(x) { return x !== null; });
					});
					gridApi.grid.clearAllFilters = clearAllFilters;
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
								if (term != undefined) {
									requestFilterSvc.SetFilter(column.colDef.displayName, term.trim());
								}
							});

							vm.filters = requestFilterSvc.Filters;
						}, 500);
					});
				}
			};

			return options;
		}

		function onSelectionChanged() {
			var visibleRequestsIds = vm.gridOptions.data.map(function(row) { return row.Id; });
			var visibleSelectedRequestsIds = vm.gridApi.selection.getSelectedRows().map(function(row) { return row.Id; });
			var visibleNotSelectedRequestsIds = visibleRequestsIds.filter(function(id) {
				return visibleSelectedRequestsIds.indexOf(id) < 0;
			});

			var allSelectedRequestsIds = requestCommandParamsHolder.getSelectedRequestsIds(vm.shiftTradeView);
			var newAllSelectedRequestsId = [];

			angular.forEach(allSelectedRequestsIds, function(id) {
				if (visibleNotSelectedRequestsIds.indexOf(id) < 0)
					newAllSelectedRequestsId.push(id);
			});

			angular.forEach(visibleSelectedRequestsIds, function(id) {
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
			return vm.gridOptions.data.filter(function(row) {
				return allSelectedRequestsIds.indexOf(row.Id) > -1;
			});
		}

		function formatToTimespan(length, isFullDay) {
			var days = moment.duration(length, 'seconds')._data.days;
			var hours = moment.duration(length, 'seconds')._data.hours;
			var minutes = moment.duration(length, 'seconds')._data.minutes == 0 ? '00' : moment.duration(length, 'seconds')._data.minutes;
			var totalHours = hours + days * 24 == 0 ? '00' : hours + days * 24;
			if (isFullDay) return totalHours + 1 + ':' + '00';
			else return totalHours + ':' + minutes;
		}

		function prepareComputedColumns(requests) {
			vm.userTimeZone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;

			function formatedDateTime(dateTime, timezone, displayDateOnly) {
				var angularTimezone = moment.tz(vm.isUsingRequestSubmitterTimeZone ? timezone : vm.userTimeZone).format('Z');
				angularTimezone = angularTimezone.replace(':', '');
				var _dateTime = moment.tz(dateTime, timezone).toDate();
				if (displayDateOnly && vm.isUsingRequestSubmitterTimeZone) return $filter('date')(_dateTime, 'shortDate', angularTimezone);
				else return $filter('date')(_dateTime, 'short', angularTimezone);
			}

			angular.forEach(requests, function(row) {
				row.GetDuration = function() {
					var length = moment(row.PeriodEndTime).diff(moment(row.PeriodStartTime), 'seconds');
					return formatToTimespan(length, row.IsFullDay);
				};
				row.FormatedPeriodStartTime = function() {
					return formatedDateTime(row.PeriodStartTime, row.TimeZone, row.IsFullDay);
				};
				row.FormatedPeriodEndTime = function() {
					return formatedDateTime(row.PeriodEndTime, row.TimeZone, row.IsFullDay);
				};

				row.FormatedCreatedTime = function() {
					return formatedDateTime(row.CreatedTime, row.TimeZone, false);
				};

				row.FormatedUpdatedTime = function() {
					return formatedDateTime(row.UpdatedTime, row.TimeZone, false);
				};

				row.GetType = function() {
					var typeText = row.TypeText;
					if (row.Type == requestsDefinitions.REQUEST_TYPES.ABSENCE) {
						typeText += ' (' + row.Payload.Name + ')';
					}
					return typeText;
				}

				row.GetBrokenRules = function() {
					var translatedBrokenRules = new Array();
					var brokenRules = row.BrokenRules;
					if (brokenRules) {
						angular.forEach(brokenRules,
							function(value) {
								translatedBrokenRules.push($translate.instant(value));
							});
						return translatedBrokenRules.join(', ');
					}
					return brokenRules;
				}
			});

			setupColumnDefinitions(requests);

			vm.gridOptions.data = requests;

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
			angular.forEach(rows, function(row) {
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
				shiftTradeView: '=?'
			},
			require: ['requestsTableContainer'],
			templateUrl: 'js/requests/html/requests-table-container.tpl.html',
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrls) {
			var requestsTableContainerCtrl = ctrls[0];

			scope.requestsTableContainer.gridOptions = requestsTableContainerCtrl.getGridOptions([]);
			scope.requestsTableContainer.isUsingRequestSubmitterTimeZone = true;
			
			scope.$watch(function() {
				return {
					requests: scope.requestsTableContainer.requests
				}
			}, function (requestWatch) {
				
				var requests = requestWatch.requests;
				requestsTableContainerCtrl.prepareComputedColumns(requests);
				requestsTableContainerCtrl.reselectRequests();

				//ROBTODO: review - do we really need this??
				//var shiftTradeDayView = '.shift-trade-view .ui-grid-render-container-body .ui-grid-viewport';
				//if ($(shiftTradeDayView).length && scope.requestsTableContainer.requests.length > 0) {
				//	(function() {
				//		function thereIsScrollBar() {
				//			return $(shiftTradeDayView)[0].scrollWidth > $(shiftTradeDayView).width();
				//		};

				//		scope.$watch(function() {
				//			return $(shiftTradeDayView).width();
				//		}, function() {
				//			thereIsScrollBar() ?
				//				$(shiftTradeDayView).css('height', requestsTableContainerCtrl.gridApi.grid.gridHeight - 65 + 18) :
				//				$(shiftTradeDayView).css('height', requestsTableContainerCtrl.gridApi.grid.gridHeight - 65);
				//		});
				//	})();
				//}
			}, true);

			scope.$on('reload.requests.without.selection', function() {
				requestsTableContainerCtrl.clearSelection();
			});

			scope.$on('reload.requests.with.selection', function() {
				requestsTableContainerCtrl.reselectRequests();
			});

			scope.$watch('requestsTableContainer.filterEnabled',
				function handleFilterEnabledChanged(newValue, oldValue) {
					requestsTableContainerCtrl.setFilterEnabled(newValue);
			});


		}
	}
})();

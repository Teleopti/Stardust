'use strict';

(function () {

	angular.module('wfm.requests')
		.factory('RequestGridStateService', ['$timeout','$window', 'requestsDefinitions', function ($timeout, $window, requestsDefinitions) {
			var svc = this,
				columnsToExcludeFromSave = ['AgentName2'],
				shiftTradeViewGridStateName = 'shiftTradeViewGridState',
				absenceAndTextViewGridStateName = 'absenceRequestViewGridState',
				overtimeViewGridStateName = 'overtimeViewGridState';

			svc.hasSavedState = function(isShiftTradeView) {
				var localStorageKeyName = getGridStateKey(isShiftTradeView);
				var state = $window.localStorage.getItem(localStorageKeyName);
				return state != null;
			};

			svc.restoreState = function(vm, requestType) {
				var localStorageKeyName = getGridStateKey(requestType);
				var state = $window.localStorage.getItem(localStorageKeyName);
				if (state) vm.gridApi.saveState.restore(vm, JSON.parse(state));
			};

			svc.setupGridEventHandlers = function($scope, vm, requestType) {
				vm.gridApi.core.on.columnVisibilityChanged($scope, function() { saveState(vm, requestType) });
				vm.gridApi.core.on.sortChanged($scope, function() {
					saveState(vm, requestType);
				});

				if (vm.gridApi.colResizable) {
					vm.gridApi.colResizable.on.columnSizeChanged($scope, function() { saveState(vm, requestType) });
				};
			};

			svc.getAbsenceAndTextSorting = function() {
				return getSortingColumn(absenceAndTextViewGridStateName);
			};

			svc.getOvertimeSorting = function() {
				return getSortingColumn(overtimeViewGridStateName);
			};

			svc.getShiftTradeSorting = function() {
				return getSortingColumn(shiftTradeViewGridStateName);
			};

			function getSortingColumn(name){
				var state = JSON.parse($window.localStorage.getItem(name));

				if (!state || !state.columns)
					return;

				var sortingColumn = state.columns.filter(function(column){
					return column.sort.direction
				})[0];

				if(sortingColumn)
					sortingColumn.displayName = sortingColumn.name;

				return sortingColumn;
			}

			function getGridStateKey(requestType) {
				switch (requestType) {
					case requestsDefinitions.REQUEST_TYPES.TEXT:
						return absenceAndTextViewGridStateName;
					case requestsDefinitions.REQUEST_TYPES.ABSENCE:
						return absenceAndTextViewGridStateName;

					case requestsDefinitions.REQUEST_TYPES.SHIFTTRADE:
						return shiftTradeViewGridStateName;

					case requestsDefinitions.REQUEST_TYPES.OVERTIME:
						return overtimeViewGridStateName;
				}
			}

			function excludeColumns(vm, state) {
				if (!vm.shiftTradeView ){
					return;
				}

				var columnsToInclude =  state.columns.filter(function (col) {
					return (columnsToExcludeFromSave.indexOf(col.name) != 0);
				});

				state.columns = columnsToInclude;
			}

			function saveState(vm, requestType) {
				if (vm.definitionsLoadComplete === false) {
					return;
				}
				var localStorageKeyName = getGridStateKey(requestType);
				var state = vm.gridApi.saveState.save();

				excludeColumns(vm, state);
				$window.localStorage.setItem(localStorageKeyName, JSON.stringify(state));
			}

			return svc;
		}]);
}());
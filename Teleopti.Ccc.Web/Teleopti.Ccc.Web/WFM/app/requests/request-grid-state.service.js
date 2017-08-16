'use strict';

(function () {

	angular.module('wfm.requests')
		.factory('RequestGridStateService', ['$timeout','$window', function ($timeout, $window) {
			var svc = this,
				columnsToExcludeFromSave = ['AgentName2'],
				shiftTradeViewGridStateName = 'shiftTradeViewGridState',
				absenceAndTextViewGridStateName = 'absenceRequestViewGridState';

			svc.hasSavedState = function(isShiftTradeView) {
				var localStorageKeyName = getGridStateKey(isShiftTradeView);
				var state = $window.localStorage.getItem(localStorageKeyName);
				return state != null;
			};

			svc.restoreState = function(vm) {
				var localStorageKeyName = getGridStateKey(vm.shiftTradeView);
				var state = $window.localStorage.getItem(localStorageKeyName);
				if (state) vm.gridApi.saveState.restore(vm, JSON.parse(state));
			};

			svc.setupGridEventHandlers = function($scope, vm) {
				vm.gridApi.core.on.columnVisibilityChanged($scope, function() { saveState(vm) });
				vm.gridApi.core.on.sortChanged($scope, function() {
					saveState(vm);
				});

				if (vm.gridApi.colResizable) {
					vm.gridApi.colResizable.on.columnSizeChanged($scope, function() { saveState(vm) });
				};
			};

			svc.getAbsenceAndTextSorting = function() {
				return getSortingColumn(absenceAndTextViewGridStateName);
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
				sortingColumn.displayName = sortingColumn.name;

				return sortingColumn;
			}

			function getGridStateKey(isShiftTradeView) {
				return isShiftTradeView ? shiftTradeViewGridStateName: absenceAndTextViewGridStateName;
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

			function saveState(vm) {
				if (vm.definitionsLoadComplete === false) {
					return;
				}
				var localStorageKeyName = getGridStateKey(vm.shiftTradeView);
				var state = vm.gridApi.saveState.save();

				excludeColumns(vm, state);
				$window.localStorage.setItem(localStorageKeyName, JSON.stringify(state));
			}

			return svc;
		}]);
}());
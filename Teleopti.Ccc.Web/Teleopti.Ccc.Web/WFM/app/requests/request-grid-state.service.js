'use strict';

(function () {

	angular.module('wfm.requests')
		.factory('RequestGridStateService', ['$timeout','$window', function ($timeout, $window) {

			var columnsToExcludeFromSave = ['AgentName2'];

			var service = {
				hasSavedState: hasSavedState,
				restoreState: restoreState,
				setupGridEventHandlers: setupGridEventHandlers
			}

			function getGridStateKey(isShiftTradeView) {
				return isShiftTradeView ? 'shiftTradeViewGridState' : 'absenceRequestViewGridState';
			}

			function hasSavedState(isShiftTradeView) {

				var localStorageKeyName = getGridStateKey(isShiftTradeView);
				var state = $window.localStorage.getItem(localStorageKeyName);
				return state != null;
			}

			function excludeColumns(vm, state) {

				if (!vm.shiftTradeView ) {
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

			function restoreState(vm) {
				var localStorageKeyName = getGridStateKey(vm.shiftTradeView);
				var state = $window.localStorage.getItem(localStorageKeyName);
				if (state) vm.gridApi.saveState.restore(vm, JSON.parse(state));
			}

			function setupGridEventHandlers($scope, vm) {
				vm.gridApi.core.on.columnVisibilityChanged($scope, function() { saveState(vm) });
				vm.gridApi.core.on.sortChanged($scope, function() {
					saveState(vm);
				});

				if (vm.gridApi.colResizable) {
					vm.gridApi.colResizable.on.columnSizeChanged($scope, function() { saveState(vm) });
				};
			}
			
			return service;
		}]);
}());
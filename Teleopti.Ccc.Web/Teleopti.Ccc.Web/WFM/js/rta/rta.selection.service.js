(function () {
	'use strict';

	angular.module('wfm.rta').service('RtaSelectionService', ['$state',
		function ($state) {

			this.toggleSelection = function (id, selectedItemIds) {
				var index = selectedItemIds.indexOf(id);
				if (index > -1) {
					selectedItemIds.splice(index, 1);
				} else {
					selectedItemIds.push(id);
				}
				return selectedItemIds;
			};

			this.openSelection = function (state, stateParams) {
				$state.go(state, stateParams);
			}
		}
	]);
})();

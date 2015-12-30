(function() {
	'use strict';

	angular.module('wfm.requests')
	.service('requestCommandParamsHolder', requestCommandParamsHolderService)

	function requestCommandParamsHolderService() {

		var self = this;

		this._state = {
			selectedRequestIds: []
		};

		this.setSelectedRequestIds = function(selectedRequestIds) {
			self._state.selectedRequestIds = selectedRequestIds;
		};

		this.resetSelectedRequestIds = function() {
			self._state.selectedRequestIds = [];
		};

		this.getSelectedRequestsIds = function() {
			return self._state.selectedRequestIds;
		}


	}

})();
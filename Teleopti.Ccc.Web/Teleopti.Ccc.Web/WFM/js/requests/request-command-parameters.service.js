(function () {
	'use strict';

	angular.module('wfm.requests')
		.service('requestCommandParamsHolder', requestCommandParamsHolderService);

	function requestCommandParamsHolderService() {

		var self = this;

		this._state = {
			selectedTextAndAbsenceRequestIds: [],
			selectedShiftTradeRequestIds: []
		};

		this.setSelectedRequestIds = function (selectedRequestIds, isShiftTrade) {
			if (isShiftTrade) {
				self._state.selectedShiftTradeRequestIds = selectedRequestIds;
			} else {
				self._state.selectedTextAndAbsenceRequestIds = selectedRequestIds;
			};
		};

		this.resetSelectedRequestIds = function (isShiftTrade) {

			if (isShiftTrade) {
				self._state.selectedShiftTradeRequestIds = [];
			} else {
				self._state.selectedTextAndAbsenceRequestIds = [];
			}
		};

		this.getSelectedRequestsIds = function (isShiftTrade) {

			if (isShiftTrade) {
				return self._state.selectedShiftTradeRequestIds;
			} else {
				return self._state.selectedTextAndAbsenceRequestIds;
			}
		}


	}

})();
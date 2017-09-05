(function () {
	'use strict';

	angular.module('wfm.requests')
		.service('requestCommandParamsHolder', requestCommandParamsHolderService);

	function requestCommandParamsHolderService() {

		var self = this;

		this._state = {
			selectedTextAndAbsenceRequestIds: [],
			selectedShiftTradeRequestIds: [],
			selectedIdAndMessage: {}
		};

		this.setSelectedIdAndMessage = function (selectedRequestId, message) {
			message[0] = message[0].trim();
			self._state.selectedIdAndMessage[selectedRequestId] = message[0];
		}

		this.getSelectedIdAndMessage = function (selectedRequestId) {
			return self._state.selectedIdAndMessage[selectedRequestId];
		}

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

		this.getSelectedRequestsIds = function(isShiftTrade) {
			if (isShiftTrade) {
				return self._state.selectedShiftTradeRequestIds;
			} else {
				return self._state.selectedTextAndAbsenceRequestIds;
			}
		};
	}

})();
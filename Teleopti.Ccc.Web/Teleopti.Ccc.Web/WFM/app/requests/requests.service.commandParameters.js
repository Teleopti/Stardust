(function() {
	'use strict';

	angular.module('wfm.requests').service('requestCommandParamsHolder', requestCommandParamsHolderService);

	function requestCommandParamsHolderService() {
		var self = this;

		self._state = {
			selectedTextAndAbsenceRequestIds: [],
			selectedShiftTradeRequestIds: [],
			selectedOvertimeRequestIds: [],
			selectedIdAndMessage: {}
		};

		self.setSelectedIdAndMessage = function(selectedRequestId, message) {
			message[0] = message[0].trim();
			self._state.selectedIdAndMessage[selectedRequestId] = message[0];
		};

		self.getSelectedIdAndMessage = function(selectedRequestId) {
			return self._state.selectedIdAndMessage[selectedRequestId];
		};

		self.setSelectedRequestIds = function(selectedRequestIds, isShiftTrade) {
			if (isShiftTrade) {
				self._state.selectedShiftTradeRequestIds = selectedRequestIds;
			} else {
				self._state.selectedTextAndAbsenceRequestIds = selectedRequestIds;
			}
		};

		self.setOvertimeSelectedRequestIds = function(selectedRequestIds) {
			self._state.selectedOvertimeRequestIds = selectedRequestIds;
		};

		self.getOvertimeSelectedRequestIds = function() {
			return self._state.selectedOvertimeRequestIds;
		};

		self.resetOvertimeSelectedRequestIds = function() {
			self._state.selectedOvertimeRequestIds = [];
		};

		self.resetSelectedRequestIds = function(isShiftTrade) {
			if (isShiftTrade) {
				self._state.selectedShiftTradeRequestIds = [];
			} else {
				self._state.selectedTextAndAbsenceRequestIds = [];
			}
		};

		self.getSelectedRequestsIds = function(isShiftTrade) {
			if (isShiftTrade) {
				return self._state.selectedShiftTradeRequestIds;
			} else {
				return self._state.selectedTextAndAbsenceRequestIds;
			}
		};

		self.resetAllSelectedRequestsIds = function() {
			self._state.selectedShiftTradeRequestIds = [];
			self._state.selectedTextAndAbsenceRequestIds = [];
			self._state.selectedOvertimeRequestIds = [];
		};
	}
})();

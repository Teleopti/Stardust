(function() {
	'use strict';

	angular.module('wfm.requests')
		.service('requestCommandParamsHolder', ['requestsDefinitions', requestCommandParamsHolderService]);

	function requestCommandParamsHolderService(requestsDefinitions) {
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

		self.setSelectedRequestIds = function(selectedRequestIds, requestType) {
			switch (requestType) {
				case requestsDefinitions.REQUEST_TYPES.TEXT:
					self._state.selectedTextAndAbsenceRequestIds = selectedRequestIds;

				case requestsDefinitions.REQUEST_TYPES.ABSENCE:
					self._state.selectedTextAndAbsenceRequestIds = selectedRequestIds;
					break;

				case requestsDefinitions.REQUEST_TYPES.SHIFTTRADE:
					self._state.selectedShiftTradeRequestIds = selectedRequestIds;
					break;

				case requestsDefinitions.REQUEST_TYPES.OVERTIME:
					self._state.selectedOvertimeRequestIds = selectedRequestIds;
					break;
			}
		};

		self.resetSelectedRequestIds = function(requestType) {
			switch (requestType) {
				case requestsDefinitions.REQUEST_TYPES.TEXT:
					self._state.selectedTextAndAbsenceRequestIds = [];
					break;

				case requestsDefinitions.REQUEST_TYPES.ABSENCE:
					self._state.selectedTextAndAbsenceRequestIds = [];
					break;

				case requestsDefinitions.REQUEST_TYPES.SHIFTTRADE:
					self._state.selectedShiftTradeRequestIds = [];
					break;

				case requestsDefinitions.REQUEST_TYPES.OVERTIME:
					self._state.selectedOvertimeRequestIds = [];
					break;
			}
		};

		self.getSelectedRequestsIds = function(requestType) {
			switch (requestType) {
				case requestsDefinitions.REQUEST_TYPES.TEXT:
					return self._state.selectedTextAndAbsenceRequestIds;

				case requestsDefinitions.REQUEST_TYPES.ABSENCE:
					return self._state.selectedTextAndAbsenceRequestIds;

				case requestsDefinitions.REQUEST_TYPES.SHIFTTRADE:
					return self._state.selectedShiftTradeRequestIds;

				case requestsDefinitions.REQUEST_TYPES.OVERTIME:
					return self._state.selectedOvertimeRequestIds;
			}
		};
	}

})();
(function() {

	'use strict';

	angular.module('wfm.requests').service('requestsDefinitions', requestsDefinitionsService);

	function requestsDefinitionsService() {
		this.REQUEST_TYPES = {
			TEXT: 'TEXT',
			ABSENCE: 'ABSENCE'
		}
	}

})();
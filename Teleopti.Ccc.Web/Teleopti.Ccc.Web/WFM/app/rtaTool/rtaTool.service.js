(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('rtaToolService', rtaToolService);

	rtaToolService.$inject = ['$resource'];

	function rtaToolService($resource) {

		var service = {
			getStateCodes: getStateCodes,
			getAgents: getAgents,
			sendBatch: sendBatch
		}

		return service;

		function getStateCodes() {
			return $resource('../RtaTool/PhoneStates/For', {}, { query: { method: 'GET', isArray: true } }).query().$promise;
		}

		function getAgents() {
			return $resource('../RtaTool/Agents/For', {}, { query: { method: 'GET', isArray: true }}).query().$promise;
		}

		function sendBatch(batch) {
			return $resource('../Rta/State/Batch', {}, { query: { method: 'POST', }, }).query(batch).$promise;
		}

	};
})();

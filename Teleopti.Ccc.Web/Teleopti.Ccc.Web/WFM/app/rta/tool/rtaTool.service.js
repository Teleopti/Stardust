(function () {
	'use strict';

	angular
		.module('wfm.rtaTool')
		.factory('rtaToolService', rtaToolService);

	rtaToolService.$inject = ['$resource'];

	function rtaToolService($resource) {

		var service = {
			getStateCodes: getStateCodes,
			getAgents: getAgents,
			sendBatch: sendBatch,
			getOrganization: getOrganization
		};

		return service;

		function sendBatch(batch) {
			return $resource('../Rta/State/Batch', {}, { query: { method: 'POST', }, }).query(batch).$promise;
		}

		function getStateCodes() {
			return $resource('../api/RtaTool/PhoneStates', {}, { query: { method: 'GET', isArray: true } }).query().$promise;
		}

		function getAgents(params) {
			return $resource('../api/RtaTool/Agents', params, { query: { method: 'GET', isArray: true } }).query().$promise;
		}

		function getOrganization() {
			return $resource('../api/RtaTool/Organization', {}, { query: { method: 'GET', isArray: false } }).query().$promise;
		}

	};
})();

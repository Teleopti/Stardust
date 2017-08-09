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
			getAgentsForSites: getAgentsForSites,
			sendBatch: sendBatch,
			getOrganization: getOrganization
		}

		return service;

		function getStateCodes() {
			return $resource('../RtaTool/PhoneStates/For', {}, { query: { method: 'GET', isArray: true } }).query().$promise;
		}

		function getAgents() {
			return $resource('../RtaTool/Agents/For', {}, { query: { method: 'GET', isArray: true }}).query().$promise;
		}

		function getAgentsForSites(siteIds) {
			return $resource('../RtaTool/Agents/For', {siteIds: siteIds}, { query: { method: 'GET', isArray: true }}).query().$promise;
		}

		function sendBatch(batch) {
			return $resource('../Rta/State/Batch', {}, { query: { method: 'POST', }, }).query(batch).$promise;
		}

		function getOrganization() {
			return $resource('../RtaTool/Organization/For', {}, { query: { method: 'GET', isArray: false }}).query().$promise;
		}

	};
})();

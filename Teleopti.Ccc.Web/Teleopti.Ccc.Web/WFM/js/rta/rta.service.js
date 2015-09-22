(function () {
    'use strict';

    angular.module('RtaService', ['ngResource']).service('RtaService', [
        '$resource', function ($resource) {

        	this.getAgents = $resource('../Agents/ForTeam', {}, {
        		query: { method: 'GET', params: {}, isArray: true }
        	});

        	this.getTeams = $resource('../Teams/ForSite', {}, {
        		query: { method: 'GET', params: {}, isArray: false }
        	});

            this.getSites = $resource('../Sites', {}, {
                query: { method: 'GET', params: {}, isArray: true }
            });

            this.getAdherenceForAllSites = $resource('../Sites/GetOutOfAdherenceForAllSites', {}, {
                query: {
                    method: 'GET',
                    headers: { 'X-Business-Unit-Filter': '928dd0bc-bf40-412e-b970-9b5e015aadea' },
                    params: {}, isArray: true
                }
            });
        }]);
})();


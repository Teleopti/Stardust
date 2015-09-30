(function () {
    'use strict';

    angular.module('RtaService', ['ngResource']).service('RtaService', [
        '$resource', function ($resource) {

        	this.getAgents = $resource('../Agents/ForTeam?teamId=:teamId', {teamId: '@teamId'}, {
        		query: { method: 'GET', params: {}, isArray: true }
        	});

        	this.getTeams = $resource('../Teams/ForSite?siteId=:siteId', {siteId: '@siteId'}, {
        		query: { method: 'GET', params: {}, isArray: true }
        	});

            this.getSites = $resource('../Sites', {}, {
                query: { method: 'GET', params: {}, isArray: true }
            });

            /**this.getTeamsForSelectedSites('../Teams/ForSites?siteId=:siteId', { selecetedSites: '@selectedSites' }, {
				query: { method: 'GET', params: {}, isArray: true}
            });**/

            this.getAdherenceForTeamsOnSite = $resource('../Teams/GetOutOfAdherenceForTeamsOnSite?siteId=:siteId', {siteId: '@siteId'}, {
                query: {
                    method: 'GET',
                    headers: { 'X-Business-Unit-Filter': '928dd0bc-bf40-412e-b970-9b5e015aadea' },
                    params: {}, isArray: true
                }
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

